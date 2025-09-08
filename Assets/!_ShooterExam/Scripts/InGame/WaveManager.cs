using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

public class WaveManager : NetworkBehaviour
{
    [SerializeField] private WaveDataSO _waveDataSO;
    private int _currentWave = 0;
    private int _maxWave;
    
    private Vector2 _minCameraPos;
    private Vector2 _maxCameraPos;

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
        
        StartWaveLoop().Forget();
    }
    
    // asyncメソッド名がStartWave()とUpdateWave()で重複していたため、StartWaveLoop()に変更
    private async UniTask StartWaveLoop()
    {
        while (_currentWave < _maxWave)
        {
            Debug.Log($"ウェーブ{_currentWave}の開始");
            await UpdateWave(_currentWave);
            _currentWave++;
        }
        
        // 追加: 全てのウェーブ終了時のログ
        Debug.Log("すべてのウェーブが終了しました！");
    }

    private async UniTask UpdateWave(int waveNumber)
    {
        var enemyPrefabs = _waveDataSO.WaveDatas[waveNumber].Enemies;
        var enemyList = new List<Enemy>(); 
        
        foreach (var enemyPrefab in enemyPrefabs)
        {
            var enemy = Runner.Spawn(enemyPrefab, GetRandomPos()).GetComponent<Enemy>();
            enemyList.Add(enemy);
        }

        // 敵が全滅するまで待つ処理
        await UniTask.WhenAll(enemyList
            .Select(e => e.OnDeath.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy())));
        Debug.Log($"ウェーブ {waveNumber + 1} の敵をすべて倒しました！");
    }

    private Vector2 GetRandomPos()
    {
        return new Vector2(Random.Range(_maxCameraPos.x * 0.8f, _maxCameraPos.x - 1.0f), Random.Range(_minCameraPos.y + 1.0f, _maxCameraPos.y - 1.5f));
    }
}