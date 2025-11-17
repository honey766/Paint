using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CardImageLoader : SingletonBehaviour<CardImageLoader>
{
    [SerializeField] StageSO stageSO;
    public bool imageLoadingCompleted;

    private List<Sprite> cards = new List<Sprite>(); // 로드된 Sprite를 저장할 리스트
    private List<AsyncOperationHandle<Sprite>> loadHandles = new List<AsyncOperationHandle<Sprite>>(); // 핸들을 저장할 리스트 (해제용)

    private async void Start()
    {
        int stage = stageSO.numOfStage;
        imageLoadingCompleted = false;

        for (int i = 1; i <= stage; i++)
        {
            string address = $"Assets/Sprites/Main/back_img_{i}.png";

            try
            {
                AsyncOperationHandle<Sprite> newHandle = Addressables.LoadAssetAsync<Sprite>(address);
                await newHandle.Task;

                if (newHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    // 1. 성공 시 결과 저장 및 핸들 저장
                    cards.Add(newHandle.Result);
                    loadHandles.Add(newHandle); // 해제 시 핸들 또는 Result를 사용합니다.
                }
                else
                {
                    Logger.LogError($"로드 실패: {address}");
                    Addressables.Release(newHandle); 
                    break;
                }
            }
            catch (UnityEngine.AddressableAssets.InvalidKeyException)
            {
                Logger.LogWarning($"키 없음: {address}. 연속 로드 종료.");
                break; // 키가 없으면 로드 종료
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"예상치 못한 오류: {ex.Message}");
                break;
            }
        }

        imageLoadingCompleted = true;
    }

    public Sprite GetStageCard(int stage) => cards[stage - 1];
}