using UnityEngine;

public class TutorialAnswerButton : MonoBehaviour
{
    [SerializeField] private GameObject tutorial1_1_2Answer, tutorial1_1_3Answer;
    private int tutorialNum;
    private GameObject answerObj;

    private void Awake()
    {
        tutorialNum = 2;
        answerObj = null;
    }

    public void OnTutoAnswerButtonClicked()
    {
        if (answerObj == null)
        {
            answerObj = tutorialNum == 2 ? tutorial1_1_2Answer : tutorial1_1_3Answer;
            answerObj = Instantiate(answerObj);
        }
        else
        {
            answerObj.SetActive(!answerObj.activeSelf);
        }
    }
    public void OnTutorial3()
    {
        if (answerObj != null)
        {
            Destroy(answerObj);
            answerObj = null;
        }
        tutorialNum = 3;
    }
}