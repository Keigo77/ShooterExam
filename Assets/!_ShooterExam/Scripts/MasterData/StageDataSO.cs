using System.Collections.Generic;
using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveDataSO", menuName = "Scriptable Objects/StageDataSO")]
public class StageDataSO : ScriptableObject
{
    public List<StageData> StageDatas = new List<StageData>(); 
}

[System.Serializable]
public class StageData
{
    public int StageNumber;
    public WaveData[] WaveDatas;
}

[System.Serializable]
public struct WaveData
{
    public int WaveNumber;
    public bool IsBoss;
    public NetworkObject[] Enemies;
}