/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 스크립트 실행 순서 정의
[DefaultExecutionOrder(-100)] // 가장 먼저 실행
public class StartupManager : MonoBehaviour
{
    public static StartupManager Instance { get; private set; }
    
    [Header("시작 단계 설정")]
    public StartupPhase[] phases;
    
    [System.Serializable]
    public class StartupPhase
    {
        public string name;
        public UnityEngine.Events.UnityEvent onPhaseStart;
        public UnityEngine.Events.UnityEvent onPhaseComplete;
        public float maxDuration = 5f; // 타임아웃
    }
    
    private int currentPhaseIndex = 0;
    private float phaseStartTime;
    
    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartStartupSequence();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void StartStartupSequence()
    {
        Debug.Log("앱인토스 게임 시작 시퀀스 시작");
        ExecuteNextPhase();
    }
    
    void ExecuteNextPhase()
    {
        if (currentPhaseIndex >= phases.Length)
        {
            OnStartupComplete();
            return;
        }
        
        var phase = phases[currentPhaseIndex];
        Debug.Log($"시작 단계 실행: {phase.name}");
        
        phaseStartTime = Time.realtimeSinceStartup;
        phase.onPhaseStart?.Invoke();
        
        // 타임아웃 설정
        StartCoroutine(PhaseTimeout(phase));
    }
    
    IEnumerator PhaseTimeout(StartupPhase phase)
    {
        yield return new WaitForSeconds(phase.maxDuration);
        
        // 단계가 완료되지 않았다면 강제 진행
        if (currentPhaseIndex < phases.Length)
        {
            Debug.LogWarning($"시작 단계 타임아웃: {phase.name}");
            CompleteCurrentPhase();
        }
    }
    
    public void CompleteCurrentPhase()
    {
        if (currentPhaseIndex >= phases.Length) return;
        
        var phase = phases[currentPhaseIndex];
        float duration = Time.realtimeSinceStartup - phaseStartTime;
        
        Debug.Log($"시작 단계 완료: {phase.name} ({duration:F2}초)");
        
        phase.onPhaseComplete?.Invoke();
        
        // 앱인토스 분석 시스템에 전송
        SendPhaseMetric(phase.name, duration);
        
        currentPhaseIndex++;
        ExecuteNextPhase();
    }
    
    void OnStartupComplete()
    {
        float totalTime = Time.realtimeSinceStartup;
        Debug.Log($"게임 시작 완료! 총 시간: {totalTime:F2}초");
        
        // 앱인토스에 시작 완료 알림
        AppsInToss.ReportGameReady(totalTime);
        
        // 성능 목표 체크
        if (totalTime > 5f)
        {
            Debug.LogWarning($"시작 시간이 목표를 초과했습니다: {totalTime:F2}s > 5s");
            AppsInToss.ReportPerformanceIssue("startup_slow", totalTime);
        }
    }
    
    void SendPhaseMetric(string phaseName, float duration)
    {
        var metric = new Dictionary<string, object>
        {
            {"phase", phaseName},
            {"duration", duration},
            {"timestamp", System.DateTime.UtcNow.ToString("o")}
        };
        
        AppsInToss.SendAnalytics("startup_phase", metric);
    }
}
*/