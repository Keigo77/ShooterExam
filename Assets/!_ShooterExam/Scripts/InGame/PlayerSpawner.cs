using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSpawner : NetworkBehaviour
{ 
    [SerializeField] private GameObject _gameManager;
    [SerializeField] private GameObject[] _playerPrefabs;

    public override void Spawned()
    {
        // プレビューの色のジェット機を生成する
        var playerObj = Runner.Spawn(_playerPrefabs[(int)(PlayerInfo.PlayerColor) - 1]);
        Runner.SetPlayerObject(Runner.LocalPlayer, playerObj);
    }
}
