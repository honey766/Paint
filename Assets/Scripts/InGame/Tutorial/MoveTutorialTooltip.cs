using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class MoveTutorialTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tooltip;
    private bool isActivated;
    private bool isTutorial1;
    private TextMeshProUGUI informationText;

    private void Awake()
    {
        isTutorial1 = true;
        Settings.onLanguageChanged += SetMoveTutorialText;
    }
    private void OnEnable()
    {
        isActivated = true;
        OnInformationClicked();
    }
    public void OnInformationClicked()
    {
        isActivated = !isActivated;
        SetMoveTutorialText();
    }
    public void EnteredTutorialTwo()
    {
        isTutorial1 = false;
    }
    public void SetMoveTutorialText()
    {
        if (!isTutorial1) return;

        // bool isTileTouch = PersistentDataManager.Instance.isTileTouch;
        // string str1 = isTileTouch ? "타일을 터치" : "화면을 스와이프";
        // string str2 = isTileTouch ? "화면 스와이프" : "타일 터치";

        // tooltip.text = $"<size=110%><color=#555555>[{str1}]</color></size> 해서 이동할 수 있어요.\n"
        //                + $"설정에서 <size=110%><color=#555555>[{str2}]</color></size> 방식으로 변경할 수 있어요.";

        bool isTileTouch = PersistentDataManager.Instance.isTileTouch;
        string key = isTileTouch ? "MoveTutorialTouch" : "MoveTutorialSwipe";
        tooltip.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", key, LocalizationSettings.SelectedLocale);
    }
}
