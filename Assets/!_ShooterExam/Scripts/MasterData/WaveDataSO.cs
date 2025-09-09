using System.Collections.Generic;
using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveDataSO", menuName = "Scriptable Objects/WaveDataSO")]
public class WaveDataSO : ScriptableObject
{
    public List<WaveData> WaveDatas = new List<WaveData>(); 
}

[System.Serializable]
public class WaveData
{
    public int WaveNumber;
    public bool IsBoss;
    public NetworkObject[] Enemies = new NetworkObject[30];
}