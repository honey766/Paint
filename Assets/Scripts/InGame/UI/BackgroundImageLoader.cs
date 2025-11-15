using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BackgroundImageLoader : MonoBehaviour
{
    private AsyncOperationHandle<Sprite> spriteHandle;

    async void Start()
    {
        // string address = $"Images/ingame_back_img_{PersistentDataManager.Instance.stage}";
        string address = $"Assets/Sprites/InGame/UI/ingame_back_img_{PersistentDataManager.Instance.stage}.png";
        // Addressables로 스프라이트 로드
        spriteHandle = Addressables.LoadAssetAsync<Sprite>(address);
        await spriteHandle.Task;

        // 로드 성공 확인
        if (spriteHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Sprite backgroundImg = spriteHandle.Result;
            GetComponent<Image>().sprite = backgroundImg;
        }
        else
        {
            Logger.LogWarning($"{address} 로드 실패 또는 존재하지 않음");
        }
    }

    void OnDestroy()
    {
        // 메모리 해제
        if (spriteHandle.IsValid())
        {
            Addressables.Release(spriteHandle);
        }
    }
}