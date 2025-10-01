using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class WaveManager : NetworkBehaviour
{
    [FormerlySerializedAs("_waveDataSO")] [SerializeField] private StageDataSO _stageDataSo;
    [SerializeField] private GameObject[] _spawnPosObj = new GameObject[30];
    [SerializeField] private float _howSpawnDistance;   // どれだけ画面外にスポーンさせるか
    [SerializeField] private AudioClip _smallEnemyBgm;
    [SerializeField] private AudioClip _bossBgm;
    [SerializeField] private ShowImageManager  _showImageManager;
     
    private GameManager _gameManager;
    private int _stageNumber = 1;
    private int _currentWave = 1;
    private int _maxWave;
    
   private CancellationToken _token;

    public override async void Spawned()
    {
        AudioSingleton.Instance.PlayBgm(_smallEnemyBgm);
        _stageNumber = StageSelectManager.StageNumber;
        if (!HasStateAuthority) { return; }
        
        _maxWave = _stageDataSo.StageDatas[_stageNumber - 1].WaveDatas.Length;
        _token = this.GetCancellationTokenOnDestroy();

        _gameManager = GameManager.Instance;
        await UniTask.WaitUntil(() => _gameManager.CurrentGameState == GameState.Playing,
                cancellationToken: _token);
        
        StartWaveLoop().Forget();
    }
    
    private async UniTask StartWaveLoop()
    {
        while (_currentWave <= _maxWave && _gameManager.CurrentGameState != GameState.GameOver)
        {
            Debug.Log($"ウェーブ{_currentWave}の開始");
            await UpdateWave(_currentWave);
            _currentWave++;
        }
        
        Debug.Log("すべてのウェーブが終了しました！");
        _gameManager.CurrentGameState = GameState.Clear;
    }

    private async UniTask UpdateWave(int waveNumber)
    {
        NetworkObject[] enemyPrefabs = _stageDataSo.StageDatas[_stageNumber - 1].WaveDatas[waveNumber - 1].Enemies;
        var enemyList = new List<EnemyBase>();
        int index = 0;

        // ボスウェーブの場合，BGMを変更し，WARNINGを表示．
        if (_stageDataSo.StageDatas[_stageNumber - 1].WaveDatas[waveNumber - 1].IsBoss)
        {
            RpcChangeBossBgm();
            await _showImageManager.ShowImage(ImageType.WaringImage, 2.0f);
        }
        
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
                var enemyBase = enemyObj.GetComponent<EnemyBase>();
                // 定位置のpositionを敵に伝える
                enemyBase.SpawnPos = _spawnPosObj[index].transform.position;
                enemyList.Add(enemyBase);
            });
            
            index++;
        }

        // 敵が全滅するまで待つ処理
        await UniTask.WhenAll(enemyList
            .Select(e => e.OnDeath.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy())));
        Debug.Log($"ウェーブ {waveNumber + 1} の敵をすべて倒しました！");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcChangeBossBgm()
    {
        AudioSingleton.Instance.PlayBgm(_bossBgm);
    }
}