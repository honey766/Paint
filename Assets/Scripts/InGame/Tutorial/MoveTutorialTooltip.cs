using TMPro;
using UnityEngine;

public class MoveTutorialTooltip : MonoBehaviour
{
    [SerializeField] private GameObject information;
    [SerializeField] private TextMeshProUGUI tooltip;
    private bool isActivated;
    private bool isTutorial1;
    private TextMeshProUGUI informationText;

    private void Awake()
    {
        informationText = information.GetComponentInChildren<TextMeshProUGUI>();
        isTutorial1 = true;
    }
    private void OnEnable()
    {
        isActivated = true;
        OnInformationClicked();
    }
    public void OnInformationClicked()
    {
        isActivated = !isActivated;
        information.SetActive(isActivated);
        SetMoveTutorialText();
    }
    public void EnteredTutorialTwo()
    {
        isTutorial1 = false;
    }
    public void SetMoveTutorialText()
    {
        if (!isTutorial1) return;

        bool isTileTouch = PersistentDataManager.Instance.isTileTouch;
        string str1 = isTileTouch ? "타일을 터치" : "화면을 스와이프";
        string str2 = isTileTouch ? "화면 스와이프" : "타일 터치";
        string str3 = "[화면 스와이프]  화면을 상하좌우로 스와이프해서\n                            캐릭터를 움직여요.\n";
        string str4 = "    [타일 터치]     타일을 직접 터치해서\n                            해당 타일의 위치로 이동해요.";
        string str5 = "<color=#772200>";

        tooltip.text = $"<size=110%>[{str1}]</size> 해서 이동할 수 있어요.\n"
                       + $"설정에서 <size=110%>[{str2}]</size> 방식으로 변경할 수 있어요.";
        informationText.text = (isTileTouch ? str5 : "") + str4 + (isTileTouch ? "</color>" : "")
                             + "<size=40%> </size>\n"
                             + (!isTileTouch ? str5 : "") + str3 + (!isTileTouch ? "</color>" : "");
    }
}
