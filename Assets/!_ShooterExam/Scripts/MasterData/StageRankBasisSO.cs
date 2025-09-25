using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RankBasisSO", menuName = "Scriptable Objects/RankBasisSO")]
public class StageRankBasisSO : ScriptableObject
{
    public List<StageRankData> StageRankDatas = new List<StageRankData>();
}

[System.Serializable]
public class StageRankData
{
    public int MaxPointClearTime;
    public int MinPointClearTime;
}