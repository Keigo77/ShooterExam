using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

public class WaveManager : NetworkBehaviour
{
    [SerializeField] private WaveDataSO _waveDataSO;
    [SerializeField] private GameObject[] _spawnPosObj = new GameObject[30];
    [SerializeField] private float _howSpawnDistance;   // どれだけ画面外にスポーンさせるか
    private int _currentWave = 0;
    private int _maxWave;
    
    private Vector2 _minCameraPos;
    private Vector2 _maxCameraPos;

    public override void Spawned()
    {
        if (!Runner.IsSharedModeMasterClient) { return; }
        _maxWave = _waveDataSO.WaveDatas.Count;
        
        StartWaveLoop().Forget();
    }
    
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
        NetworkObject[] enemyPrefabs = _waveDataSO.WaveDatas[waveNumber].Enemies;
        var enemyList = new List<Enemy>();
        int index = 0;
        
        foreach (var enemyPrefab in enemyPrefabs)
        {
            if (enemyPrefab == null)
            {
                index++;
                continue;
            }

            // 一度画面外にスポーンさせ，そこからX軸だけDOTweenで画面内に移動させる．
            Vector2 enemySpawnPos = _spawnPosObj[index].transform.position;
            enemySpawnPos.x += _howSpawnDistance;
            Runner.Spawn(enemyPrefab, enemySpawnPos, onBeforeSpawned: (_, enemyObj) =>
            {
                var enemy = enemyObj.GetComponent<Enemy>();
                // 定位置のpositionを敵に伝える
                enemy.spawnPos = _spawnPosObj[index].transform.position;
                enemyList.Add(enemy);
            });
            
            index++;
        }

        // 敵が全滅するまで待つ処理
        await UniTask.WhenAll(enemyList
            .Select(e => e.OnDeath.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy())));
        Debug.Log($"ウェーブ {waveNumber + 1} の敵をすべて倒しました！");
    }
}