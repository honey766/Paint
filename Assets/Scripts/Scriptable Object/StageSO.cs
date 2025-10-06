using UnityEngine;

[CreateAssetMenu(fileName = "Stage", menuName = "ScriptableObjects/Stage")]
public class StageSO : ScriptableObject
{
    public int numOfStage;
    public int[] numOfLevelOfStage;
    public int[] numOfExtraLevelOfStage;
    public int[] numOfStarToUnlockStage;
}