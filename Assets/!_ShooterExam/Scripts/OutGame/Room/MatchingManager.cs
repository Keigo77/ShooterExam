using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MatchingManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner _networkRunnerPrefab;
    [SerializeField] private TMP_InputField _inputRoomNameField;
    [SerializeField] private int _matchingSceneIndex;
    [SerializeField] private string _matchingSceneName;

    /// <summary>
    /// ランダムマッチの開始
    /// </summary>
    public async void StartRandomMatching() {
        var networkRunner = Instantiate(_networkRunnerPrefab);
        networkRunner.AddCallbacks(this);
        var result = await networkRunner.StartGame(new StartGameArgs {
            GameMode = GameMode.Shared,
            Scene = SceneRef.FromIndex(_matchingSceneIndex),
            PlayerCount = 4,
        });
        Debug.Log(result);
    }
    
    /// <summary>
    /// プライベートマッチの開始
    /// </summary>
    public async void StartPrivateMatching() {
        var networkRunner = Instantiate(_networkRunnerPrefab);
        networkRunner.AddCallbacks(this);
        var result = await networkRunner.StartGame(new StartGameArgs {
            GameMode = GameMode.Shared,
            Scene = SceneRef.FromIndex(_matchingSceneIndex),
            SessionName = _inputRoomNameField.text,
            IsVisible = false,
            PlayerCount = 4,
        });
        Debug.Log(result);
    }
    
    // NetworkRunner関連のコールバック
    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer && runner.IsServer)
        {
            SceneManager.LoadScene(_matchingSceneName);
        }
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player) {}
    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) {}
    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        SceneManager.LoadScene("Home");
    }
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
