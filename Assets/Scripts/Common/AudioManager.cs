using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using DG.Tweening;

[System.Serializable]
public class SfxAudioClip
{
    public SfxType sfxType;
    public AudioClip audioClip;
}

[System.Serializable]
public class BgmAssetReference
{
    public BgmType bgmType;
    public AssetReference assetReference;
}

[System.Serializable]
public class LoopSfxAudioClip {
    public SfxType sfxType;
    public AudioClip introClip;  // 도입부
    public AudioClip loopClip;   // 반복 구간
    public AudioClip outroClip;  // 끝맺음
}

public enum SfxType
{
    // UI
    Click1 = 0,
    Click2 = 1,
    SelectCard = 2,
    FlipCard = 3,
    Transition = 4,

    // Tile
    ColorTile = 1000,
    EnterSpray = 1001,

    // Block
    PushBlock = 2000,
    MirrorActivation = 2001
}

public enum BgmType
{
    Title = 1000,
    Tutorial = 2000,
    Spring = 2001,
    Summer,
    Autumn,
    Winter,
    Desert,
    Mountain,
    Swamp
}

public class AudioManager : SingletonBehaviour<AudioManager>
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer;

    [Header("오디오 소스 (Audio Sources)")]
    [SerializeField] private AudioSource curBgmSource;
    [SerializeField] private AudioSource nextBgmSource;
    [SerializeField] private AudioSource sfxSource2D; // UI 등 2D 사운드 전용
    private Dictionary<SfxType, (AudioSource, AudioSource)> loopSfxSources = new();

    [Header("오디오 데이터 (Audio Data)")]
    [SerializeField] private BgmAssetReference[] bgmReferences;
    [SerializeField] private SfxAudioClip[] sfxClips;
    [SerializeField] private LoopSfxAudioClip[] loopSfxClips;

    // 데이터 딕셔너리
    private Dictionary<BgmType, string> bgmAddressDict = new();
    private Dictionary<SfxType, AudioClip> sfxDict = new();
    private Dictionary<SfxType, LoopSfxAudioClip> loopSfxDict = new();

    // 루프 상태 관리용
    private Dictionary<SfxType, Coroutine> loopCoroutines = new();
    private HashSet<SfxType> loopingFlags = new();
    private Dictionary<SfxType, float> lastPlaybackTimes = new Dictionary<SfxType, float>();

    // 각 오디오 소스별 핸들 추적
    private AsyncOperationHandle<AudioClip> curBgmHandle;
    private AsyncOperationHandle<AudioClip> nextBgmHandle;
    private string curBgmSourceClipAddress = "";

    // 볼륨 설정
    public float BgmVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    // 스테이지별 BGM
    private readonly BgmType[] stageBgm = new BgmType[]
    {
        BgmType.Spring, BgmType.Summer, BgmType.Autumn, BgmType.Winter,
        BgmType.Desert, BgmType.Mountain, BgmType.Swamp
    };

    protected override void Init()
    {
        base.Init();

        foreach (var bgmRef in bgmReferences)
            bgmAddressDict[bgmRef.bgmType] = bgmRef.assetReference.RuntimeKey.ToString();

        foreach (var sfx in sfxClips) sfxDict[sfx.sfxType] = sfx.audioClip;

        foreach (var loopSfx in loopSfxClips)
        {
            loopSfxDict[loopSfx.sfxType] = loopSfx;

            // 각 Loop SFX별 전용 AudioSource 생성
            loopSfxSources[loopSfx.sfxType] = (GetLoopSfxAudioSource(), GetLoopSfxAudioSource());
        }

        curBgmSource.loop = true;
        nextBgmSource.loop = true;
        sfxSource2D.loop = false;
    }
    private AudioSource GetLoopSfxAudioSource()
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.outputAudioMixerGroup = sfxSource2D.outputAudioMixerGroup;
        return src;
    }

    private void Start()
    {
        SetBGMVolume(PersistentDataManager.LoadBGM() / 100f);
        SetSFXVolume(PersistentDataManager.LoadSFX() / 100f);
        // Invoke(nameof(PlayBgmFirst), 0.3f);
    }

    private void PlayBgmFirst() => PlayBgmImmediatelyAsync(BgmType.Title, 0.5f);

    // ---------------------
    // 🎵 일반 SFX / BGM
    // ---------------------
    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Max(volume, 0.0001f);
        masterMixer.SetFloat("BGM", Mathf.Log10(volume) * 20);
        BgmVolume = volume;
    }

    public async void PlayBgmImmediatelyAsync(BgmType bgmType, float fadeInDuration)
    {
        if (!bgmAddressDict.TryGetValue(bgmType, out string address))
        {
            Debug.LogError($"Addressables 주소를 찾을 수 없습니다: {bgmType}");
            return;
        }

        // 1. 새 BGM 로드
        AsyncOperationHandle<AudioClip> newHandle = Addressables.LoadAssetAsync<AudioClip>(address);
        await newHandle.Task;

        if (newHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"BGM 로드 실패: {address}");
            return;
        }

        AudioClip clip = newHandle.Result;

        // 2. 이전 BGM 정리 (재생 중단 후 해제)
        curBgmSource.Stop();
        if (curBgmHandle.IsValid())
        {
            Addressables.Release(curBgmHandle);
            curBgmHandle = default;
        }

        // 3. 다음 BGM 소스도 정리
        nextBgmSource.Stop();
        if (nextBgmHandle.IsValid())
        {
            Addressables.Release(nextBgmHandle);
            nextBgmHandle = default;
        }

        // 4. 새 BGM 설정 및 재생
        curBgmSource.clip = clip;
        curBgmSource.volume = 0;
        curBgmSource.Play();
        curBgmSource.DOFade(1, fadeInDuration).SetEase(Ease.Linear);

        // 5. 핸들 저장
        curBgmHandle = newHandle;
        curBgmSourceClipAddress = address;
    }

    public void ChangeBgmWithTransition(BgmType bgmType)
    {
        if (bgmAddressDict.TryGetValue(bgmType, out string address))
            if (curBgmSourceClipAddress != address)
                StartCoroutine(ChangeBgmCoroutine(address));
    }
    public void ChangeBgmWithTransition(int stage)
    {
        if (stage < 1 || stage > stageBgm.Length)
            return;
        BgmType bgmType = stageBgm[stage - 1];
        if (bgmAddressDict.TryGetValue(bgmType, out string address))
            if (curBgmSourceClipAddress != address)
                StartCoroutine(ChangeBgmCoroutine(address));
    }

    private IEnumerator ChangeBgmCoroutine(string address)
    {
        curBgmSourceClipAddress = address;
        float fadeOutDuration = 1.2f;
        float fadeInDuration = 0.3f;
        float transitionDuration = UIManager.Instance.GetTransitionDuration();
    
        // 1. 현재 BGM 페이드아웃 시작
        curBgmSource.DOFade(0, fadeOutDuration).SetEase(Ease.Linear);

        // 2. nextBgmSource에 남아있던 이전 핸들 먼저 정리
        if (nextBgmHandle.IsValid())
        {
            Addressables.Release(nextBgmHandle);
            nextBgmHandle = default; // 핸들 초기화
        }

        // 3. 새 BGM 로딩 시작
        AsyncOperationHandle<AudioClip> newHandle = Addressables.LoadAssetAsync<AudioClip>(address);
    
        // 4. transitionDuration 대기
        yield return new WaitForSeconds(transitionDuration);
    
        // 5. 로드 완료 대기
        while (!newHandle.IsDone)
            yield return null; 

        if (newHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Addressables BGM 로드 실패: " + address);
            curBgmSource.DOFade(1, fadeOutDuration).SetEase(Ease.Linear);
            yield break;
        }

        AudioClip clip = newHandle.Result;
    
        // 6. 다음 BGM 소스 준비
        nextBgmSource.clip = clip;
        nextBgmSource.volume = 0f;
        nextBgmHandle = newHandle; // 새 핸들 할당
    
        // 7. 나머지 페이드아웃 시간 대기
        yield return new WaitForSeconds(Mathf.Max(0, fadeOutDuration - transitionDuration));
    
        // 8. 이전 핸들 저장 (해제용)
        AsyncOperationHandle<AudioClip> oldHandle = curBgmHandle;
    
        // 9. 소스 스왑
        curBgmSource.Stop();
        var tempSource = curBgmSource;
        curBgmSource = nextBgmSource;
        nextBgmSource = tempSource;
        
        // 10. 핸들도 스왑
        curBgmHandle = nextBgmHandle;
        nextBgmHandle = default; // nextBgmHandle 초기화 (이미 curBgmHandle로 이동)
    
        // 11. 새 BGM 재생 및 페이드인
        curBgmSource.Play();
        curBgmSource.DOFade(1, fadeInDuration).SetEase(Ease.Linear);
        
        // 12. 이전 BGM 정리
        nextBgmSource.Stop();
        nextBgmSource.clip = null;
        
        // 13. 이전 핸들 해제 (이제 안전)
        if (oldHandle.IsValid())
        {
            Addressables.Release(oldHandle);
        }
    }

    // OnDestroy에서 모든 핸들 정리
    private void OnDestroy()
    {
        if (curBgmHandle.IsValid())
        {
            Addressables.Release(curBgmHandle);
        }
        
        if (nextBgmHandle.IsValid())
        {
            Addressables.Release(nextBgmHandle);
        }
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Max(volume, 0.0001f);
        masterMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        SfxVolume = volume;
    }

     public void PlaySfx(SfxType sfxType, float volumeRate = 1)
    {
        if (sfxDict.TryGetValue(sfxType, out AudioClip clip))
        {
            float _sfxVolume = Mathf.Clamp01(volumeRate);
            if (sfxSource2D != null)
                sfxSource2D.PlayOneShot(clip, _sfxVolume);
        }
    }

    // ---------------------
    // 🔁 루프형 효과음
    // ---------------------
    public void StartLoopSfx(SfxType sfxType)
    {
        if (!loopSfxDict.ContainsKey(sfxType)) return;

        // 이미 재생 중이면 무시
        if (loopingFlags.Contains(sfxType)) return;

        var src = loopSfxSources[sfxType];
        var clip = loopSfxDict[sfxType];

        StartLoopSfx(sfxType, src.Item1, src.Item2, clip);
        // loopingFlags.Add(sfxType);
        // loopCoroutines[sfxType] = StartCoroutine(PlayLoopSfxSequence_Crossfade(sfxType, src.Item1, src.Item2, clip));
    }

    // 루프 시작 함수 (코루틴을 시작하고 loopingFlags에 추가)
    // public void StartLoopSfx(SfxType sfxType, AudioSource src1, AudioSource src2, LoopSfxAudioClip clip)
    // {
    //     // 1. 기존 코루틴 정리 (가장 중요)
    //     if (loopCoroutines.ContainsKey(sfxType))
    //     {
    //         // 기존 코루틴을 강제로 중지합니다.
    //         StopCoroutine(loopCoroutines[sfxType]); 
    //         loopCoroutines.Remove(sfxType);
            
    //         // 기존 AudioSource도 즉시 정리합니다.
    //         // DOFade 중일 수 있으므로 Kill()을 호출합니다.
    //         src1.DOKill();
    //         src2.DOKill();
    //         src1.Stop();
    //         src2.Stop();
    //         src1.volume = 0f; // 초기화
    //         src2.volume = 0f; // 초기화
            
    //         // 플래그도 안전하게 정리
    //         if (loopingFlags.Contains(sfxType))
    //             loopingFlags.Remove(sfxType);
                
    //         Logger.LogWarning($"💥 {sfxType}의 기존 루프 시퀀스가 중지되고 새로 시작됩니다.");
    //     }
        
    //     // 2. 새로운 루프 시작
    //     loopingFlags.Add(sfxType);
    //     // StartCoroutine은 MonoBehaviour에 종속되므로 this.StartCoroutine 사용
    //     Coroutine newRoutine = StartCoroutine(PlayLoopSfxSequence_Crossfade(sfxType, src1, src2, clip)); 
    //     loopCoroutines.Add(sfxType, newRoutine);
    // }

    public void StartLoopSfx(SfxType sfxType, AudioSource src1, AudioSource src2, LoopSfxAudioClip clip)
    {
        // 1. 기존 코루틴 정리 (가장 중요)
        if (loopCoroutines.ContainsKey(sfxType))
        {
            // [✨ 핵심 수정: 재생 시간 가져오기 ✨]
            float lastTime = 0f;
            // src1이 루프 클립을 재생 중이고 활성화되어 있다면 시간을 가져옵니다.
            if (src1.isPlaying && src1.clip == clip.loopClip) 
            {
                lastTime = src1.time;
            } 
            // src2가 루프 클립을 재생 중이고 활성화되어 있다면 시간을 가져옵니다.
            else if (src2.isPlaying && src2.clip == clip.loopClip) 
            {
                lastTime = src2.time;
            }
            
            // 시간을 저장소에 업데이트합니다.
            lastPlaybackTimes[sfxType] = lastTime; 

            // 기존 코루틴 강제 중지 및 정리
            StopCoroutine(loopCoroutines[sfxType]); 
            loopCoroutines.Remove(sfxType);
            
            // AudioSource 정리 및 초기화 (이전 단계와 동일)
            src1.DOKill();
            src2.DOKill();
            src1.Stop();
            src2.Stop();
            src1.volume = 0f; 
            src2.volume = 0f; 
            
            if (loopingFlags.Contains(sfxType))
                loopingFlags.Remove(sfxType);
        }
        
        // 2. 새로운 루프 시작 (이전 단계와 동일)
        loopingFlags.Add(sfxType);
        Coroutine newRoutine = StartCoroutine(PlayLoopSfxSequence_Crossfade(sfxType, src1, src2, clip)); 
        loopCoroutines.Add(sfxType, newRoutine);
    }

    public void StopLoopSfx(SfxType sfxType)
    {
        if (!loopingFlags.Contains(sfxType)) return;
        loopingFlags.Remove(sfxType);
    }

    private IEnumerator PlayLoopSfxSequence_Crossfade(SfxType sfxType, AudioSource src1, AudioSource src2, LoopSfxAudioClip clip)
    {
        // currentSrc를 src1으로 초기 설정
        AudioSource currentSrc = src1;
        AudioSource nextSrc = src2;
        float targetVolume = 1f;
        float crossfadeDuration = 0.15f;
        
        // [✨ 핵심 수정: 재개 시간 가져오기 ✨]
        float startTime = 0f;
        if (lastPlaybackTimes.ContainsKey(sfxType))
        {
            startTime = lastPlaybackTimes[sfxType];
            // 재개 후에는 즉시 0으로 초기화하여 다음 시작 시 다시 0부터 시작하게 합니다.
            lastPlaybackTimes[sfxType] = 0f;
        }

        // 1. 도입부 (clip.introClip이 있는 경우)
        // 도입부는 재개하지 않습니다. 도입부가 있다면 0부터 시작합니다.
        if (clip.introClip != null && startTime == 0f)
        {
            currentSrc.clip = clip.introClip;
            currentSrc.loop = false;
            currentSrc.volume = 0f;
            currentSrc.Play();
            currentSrc.DOFade(targetVolume, 0.1f);
            Logger.Log($"{sfxType} intro start");

            // 도입부 클립 재생 완료 대기 (짧은 페이드인 시간 포함)
            yield return new WaitForSeconds(0.1f); // 페이드인 시간
            yield return new WaitForSeconds(clip.introClip.length - 0.1f); // 남은 재생 시간 대기
        }
        // 도입부가 없거나 (clip.introClip == null) 재개 요청이 있는 경우 (startTime > 0)
        else
        {
            // 도입부 로직을 건너뛸 경우, currentSrc를 루프 클립으로 설정합니다.
            currentSrc.clip = clip.loopClip;
            currentSrc.volume = targetVolume;
            currentSrc.loop = false;
        }

        // 2. 메인 루프 시작 및 시간 적용
        if (currentSrc.clip != clip.loopClip)
        {
            currentSrc.clip = clip.loopClip;
            currentSrc.volume = targetVolume;
            currentSrc.loop = false;
        }
        
        // [✨ 핵심 수정: 재생 시간 적용 ✨]
        if (startTime > 0f)
        {
            // 클립 길이보다 길면 오류가 나므로 모듈러 연산으로 위치를 맞춥니다.
            currentSrc.time = startTime % clip.loopClip.length; 
        }
        
        currentSrc.Play();
        
        while (loopingFlags.Contains(sfxType))
        {
            // 현재 클립의 재생이 거의 끝났는지 확인합니다.
            float timeLeft = currentSrc.clip.length - currentSrc.time;

            if (timeLeft <= crossfadeDuration)
            {
                // 다음 AudioSource 설정
                nextSrc.clip = clip.loopClip;
                nextSrc.time = 0f; // 다음 클립은 처음부터 시작
                nextSrc.volume = 0f;
                nextSrc.loop = false;
                nextSrc.Play();

                // 크로스페이드: 현재 소스 볼륨 Down, 다음 소스 볼륨 Up
                currentSrc.DOFade(0f, crossfadeDuration);
                nextSrc.DOFade(targetVolume, crossfadeDuration);

                // 크로스페이드 시간만큼 대기
                yield return new WaitForSeconds(crossfadeDuration);

                // 현재 소스 정지 및 스왑
                currentSrc.Stop();
                var temp = currentSrc;
                currentSrc = nextSrc;
                nextSrc = temp; 
                
                // 크로스페이드 이후 남은 재생 시간은 nextSrc (이전 currentSrc)가 아니라 
                // 새로 currentSrc가 된 AudioSource에서 계속 진행됩니다.
            }
            
            yield return null; 
        }

        // 3. 끝맺음

        if (clip.outroClip != null)
        {
            // 아웃트로 클립이 있으면 현재 클립을 멈추고 아웃트로 재생
            currentSrc.Stop(); 
            currentSrc.clip = clip.outroClip;
            currentSrc.loop = false;
            currentSrc.volume = targetVolume; // 이전 볼륨 유지
            currentSrc.Play();

            // 아웃트로 클립 재생 시간 동안 페이드 아웃
            currentSrc.DOFade(0f, clip.outroClip.length);
            yield return new WaitForSeconds(clip.outroClip.length);
        }
        else
        {
            // 아웃트로 클립이 없으면 현재 클립을 페이드 아웃
            currentSrc.DOFade(0f, 0.1f); // 기존 코드와 동일한 0.1f 페이드 아웃
            yield return new WaitForSeconds(0.1f);
        }
        
        currentSrc.Stop();
        
        // 정리
        if (loopCoroutines.ContainsKey(sfxType))
            loopCoroutines.Remove(sfxType);
    }
}