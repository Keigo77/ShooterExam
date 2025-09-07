using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class WaveManager : NetworkBehaviour
{
    [SerializeField] private WaveDataSO _waveDataSO;
    private int _currentWave = 0;
    private int _maxWave;
    
    private Vector2 _minCameraPos;  // カメラの左下のワールド座標
    private Vector2 _maxCameraPos;  // カメラの右上のワールド座標

    public override void Spawned()
    {
        _maxWave = _waveDataSO.WaveDatas.Count;
        // カメラの描写範囲を，ワールド座標で取得
        if (Camera.main != null)
        {
            _minCameraPos = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
            _maxCameraPos = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            Debug.Log(_minCameraPos);
            Debug.Log(_maxCameraPos);
        }
        else
        {
            Debug.LogError("Can't find camera");
        }
        
        UpdateWave(_currentWave);
    }

    private void UpdateWave(int waveNumber)
    {
        foreach (var enemy in _waveDataSO.WaveDatas[_currentWave].Enemies)
        {
            Runner.Spawn(enemy, GetRandomPos());
        }
    }

    private Vector2 GetRandomPos()
    {
        return new Vector2(Random.Range(_maxCameraPos.x * 0.66f, _maxCameraPos.x - 1.0f), Random.Range(_minCameraPos.y + 1.0f, _maxCameraPos.y - 2.5f));
    }
}