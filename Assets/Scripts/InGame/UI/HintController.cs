using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HintController : MonoBehaviour
{
    [SerializeField] private GameObject notExists, backgroundImageParent;
    private HintDrawer hintDrawer;

    private void Awake()
    {
        bool isExtra = PersistentDataManager.Instance.level < 0;
        int stage = PersistentDataManager.Instance.stage;
        int level = Mathf.Abs(PersistentDataManager.Instance.level);
        List<BoardSO> boardSOs = new();
        int num = 1;
        while (true)
        {
            string boardName = isExtra ? "Extra" : "";
            boardName += $"Hint{stage}-{level}" + (num == 1 ? "" : $"-{num}");
            BoardSO boardSO = Resources.Load<BoardSO>($"ScriptableObjects/Hint/Stage{stage}/" + boardName);
            if (boardSO == null) break;
            boardSOs.Add(boardSO);
            num++;
        }

        notExists.SetActive(boardSOs.Count == 0);
        backgroundImageParent.gameObject.SetActive(boardSOs.Count != 0);
        if (boardSOs.Count == 0)
            return;

        backgroundImageParent.transform.GetChild(0).GetComponent<Image>().sprite 
            = FindAnyObjectByType<BackgroundImageLoader>().GetComponent<Image>().sprite;

        hintDrawer = GetComponentInChildren<HintDrawer>();
        hintDrawer.Draw(boardSOs.ToArray());
    }
}
