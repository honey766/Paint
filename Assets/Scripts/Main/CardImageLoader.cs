using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class CardImageLoader : SingletonBehaviour<CardImageLoader>
{
    [SerializeField] StageSO stageSO;
    public bool imageLoadingCompleted;

    private List<Sprite> cards = new();
    private List<AsyncOperationHandle<Sprite>> loadHandles = new();

    private async void Start()
    {
        await LoadImages();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale newLocale)
    {
        ReloadImages();
    }

    private async void ReloadImages()
    {
        ReleaseImages();
        await LoadImages();
    }

    private async System.Threading.Tasks.Task LoadImages()
    {
        imageLoadingCompleted = false;
        cards.Clear();
        loadHandles.Clear();

        int stage = stageSO.numOfStage;

        string local = LocalizationSettings.SelectedLocale.Identifier.Code == "en"
            ? "en"
            : "ko";

        for (int i = 1; i <= stage; i++)
        {
            string address = $"Assets/Sprites/Main/back_img_{i}_{local}.png";

            try
            {
                var handle = Addressables.LoadAssetAsync<Sprite>(address);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    cards.Add(handle.Result);
                    loadHandles.Add(handle);
                }
                else
                {
                    Addressables.Release(handle);
                    break;
                }
            }
            catch (UnityEngine.AddressableAssets.InvalidKeyException)
            {
                break;
            }
        }

        imageLoadingCompleted = true;
    }

    private void ReleaseImages()
    {
        foreach (var handle in loadHandles)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        loadHandles.Clear();
        cards.Clear();
    }

    public Sprite GetStageCard(int stage) => cards[stage - 1];
}
