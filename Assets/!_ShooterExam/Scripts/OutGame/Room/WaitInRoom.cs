using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitInRoom : NetworkBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private GameObject _battleStartButton;
    [SerializeField] private TextMeshProUGUI _sessionNameText;
    [SerializeField] private GameObject[] _playerPreviewPrefabs;
    [SerializeField] private string _homeSceneName;
    [SerializeField] private string _nextScenePath;
    
    // 1, 2, 3, 4の枠で，どこが空いているかを管理する．trueだと空いてる
    [Networked, Capacity(4)] private NetworkArray<bool> _hasEmpties { get; } 
        = MakeInitializer(new bool[] {true, true, true, true});
    // 1, 2, 3, 4の枠を埋めてるプレイヤーIDを管理する．
    [Networked, Capacity(4)] private NetworkArray<int> _hasEmptyPlayerIds { get; }

    private int _emptyIndex = -1;
    
    public override void Spawned()
    {
        Runner.AddCallbacks(this);
        // プライベートルームなら，部屋名を表示
        if (!Runner.SessionInfo.IsVisible)
        {
            _sessionNameText.gameObject.SetActive(true);
            _sessionNameText.text = $"SessionName: {Runner.SessionInfo.Name}";
        }
        
        // ホストのみ，バトル開始(ステージセレクトボタンを表示)
        if (HasStateAuthority)
        {
            _battleStartButton.SetActive(true);
        }
        
        // 各色のプレイヤーを，手前からスポーンさせる．
        for (int i = 0; i < _hasEmpties.Length; i++)
        {
            if (_hasEmpties[i])
            {
                _emptyIndex = i;
                break;
            }
        }
        
        RpcUpdateEmpty(_emptyIndex, false, Runner.LocalPlayer.PlayerId);
        // プレビュー(自分のジェット機の色と名前)を表示する
        var previewObj = _playerPreviewPrefabs[_emptyIndex];
        Runner.Spawn(previewObj, previewObj.transform.position, onBeforeSpawned: (_, obj) =>
        {
            obj.GetComponent<ShowPlayerPreview>().MyName = PlayerInfo.PlayerName;
        });
    
    }

    /// <summary>
    /// 今の部屋から退出する．ホストなら全員が退出．
    /// </summary>
    public async void BackHome()
    {
        await Runner.Shutdown();
        SceneManager.LoadScene(_homeSceneName);
    }

    /// <summary>
    /// 自分が参加したことをホストに通知する．
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcUpdateEmpty(int index, bool exist, int playerId)
    {
        _hasEmpties.Set(index, exist);
        _hasEmptyPlayerIds.Set(index, playerId);
    }

    /// <summary>
    /// 次のシーンに行く．OnPlayerLeftなどが呼び出されないように，コールバックは解除する．
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcMoveScene()
    {
        if (HasStateAuthority)
        {
            Runner.LoadScene(_nextScenePath);
        }
        Runner.RemoveCallbacks(this);
    }
    
    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // 退出したプレイヤーのプレイヤーIDを取得する
        int leftPlayerIndex = _hasEmptyPlayerIds.IndexOf(player.PlayerId);
         
        // 退出したプレイヤーがホストなら，全員が退出する
        if (leftPlayerIndex == 0)
        {
            BackHome();
        }
         
        // ホストでないなら，そのプレイヤーがいた枠に空きを作る．
        if (HasStateAuthority)
        {
            _hasEmpties.Set(leftPlayerIndex, true);
        }
    }
    
    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
     void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
     void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player) {}
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