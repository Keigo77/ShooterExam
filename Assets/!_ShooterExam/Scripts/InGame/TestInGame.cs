using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Serialization;

public class TestInGame : MonoBehaviour, INetworkRunnerCallbacks
{
     [SerializeField] private NetworkRunner _networkRunnerPrefab;
     [SerializeField] private int _sceneIndex;
     [SerializeField] private GameObject _playerPrefab;
     [SerializeField] private GameObject _enemyPrefab;
     

     private async void Start() {
         var networkRunner = Instantiate(_networkRunnerPrefab);
         // GameLauncherを、NetworkRunnerのコールバック対象に追加する
         networkRunner.AddCallbacks(this);
         var result = await networkRunner.StartGame(new StartGameArgs {
             GameMode = GameMode.Shared,
             //Scene = SceneRef.FromIndex(_sceneIndex),
         });
         Debug.Log(result);
     }

     void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
     void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}

     void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
     {
         if (player == runner.LocalPlayer)
         {
             var playerObj = runner.Spawn(_playerPrefab, _playerPrefab.transform.position);
             runner.SetPlayerObject(runner.LocalPlayer, playerObj);
             if (runner.IsSharedModeMasterClient)
             {
                 runner.Spawn(_enemyPrefab, _enemyPrefab.transform.position);
             }
             
         }
     }
     void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player) {}
     void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) {}
     void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
     void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}
     void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) {}
     void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
     void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
     void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
     void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
     void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}
     void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
     void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
     void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
     void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
     void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) {}
     void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) {}
}
