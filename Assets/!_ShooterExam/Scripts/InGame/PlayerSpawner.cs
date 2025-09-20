using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSpawner : NetworkBehaviour
{ 
    [SerializeField] private GameObject _gameManager;
    [SerializeField] private GameObject[] _playerPrefabs;
    private CancellationToken _token;

    public override async void Spawned()
    {
        _token = this.GetCancellationTokenOnDestroy();

        try
        {
            await UniTask.WaitUntil(() =>
                GameManager.Instance != null && GameManager.Instance.IsSpawned, cancellationToken: _token);
            GameManager.Instance.NowPlayerCount++;
            await UniTask.WaitUntil(() => GameManager.Instance.CurrentGameState == GameState.Playing, cancellationToken: _token);
        }
        catch (Exception e)
        {
            Debug.Log($"{e}　プレイヤーのスポーン待ちをキャンセル");
        }
        
        // プレビューの色のジェット機を生成する
        var playerObj = Runner.Spawn(_playerPrefabs[(int)(PlayerInfo.PlayerColor) - 1]);
        Runner.SetPlayerObject(Runner.LocalPlayer, playerObj);
    }
}
