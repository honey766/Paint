using TMPro;
using UnityEngine;

public class MoveTutorialTooltip : MonoBehaviour
{
    [SerializeField] private GameObject information;
    private bool isActivated;
    private TextMeshProUGUI informationText;

    private void Awake()
    {
        informationText = information.GetComponentInChildren<TextMeshProUGUI>();
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
        SetInformationText();
    }
    public void SetInformationText()
    {
        string str1 = "[화면 스와이프]  화면을 상하좌우로 스와이프해서\n                            캐릭터를 움직여요.\n";
        string str2 = "    [타일 터치]     타일을 직접 터치해서\n                            해당 타일의 위치로 이동해요.";
        string str3 = "<color=#772200>";
        bool isTileTouch = PersistentDataManager.Instance.isTileTouch;

        informationText.text = (!isTileTouch ? str3 : "") + str1 + (!isTileTouch ? "</color>" : "")
                             + "<size=40%> </size>\n"
                             + (isTileTouch ? str3 : "") + str2 + (isTileTouch ? "</color>" : "");
    }
}
