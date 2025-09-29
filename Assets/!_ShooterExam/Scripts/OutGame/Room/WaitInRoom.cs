using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitInRoom : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static int JoinedPlayerCount = 1;
    [SerializeField] private GameObject _battleStartButton;
    [SerializeField] private TextMeshProUGUI _sessionNameText;
    [SerializeField] private GameObject[] _playerPreviewPrefabs;
    [SerializeField] private string _homeSceneName;
    [SerializeField] private string _nextSceneName;
    [SerializeField] private Button _startButton;
    
    [SerializeField] private TransitionProgressController _transitionProgressController;
    [SerializeField] private AudioClip _bgmClip;
    
    // 1, 2, 3, 4の枠で，どこが空いているかを管理する．trueだと空いてる
    [Networked, Capacity(4)] private NetworkArray<bool> _hasEmpties { get; } 
        = MakeInitializer(new bool[] {true, true, true, true});
    // 1, 2, 3, 4の枠を埋めてるプレイヤーIDを管理する．
    [Networked, Capacity(4)] private NetworkArray<int> _hasEmptyPlayerIds { get; }
    [Networked, Capacity(4), UnitySerializeField] private NetworkDictionary<int, string> _playerNames => default;

    private int _emptyIndex = -1;
    private CancellationToken _token;

    private void Awake()
    {
        _transitionProgressController.Progress = 1f;
        ErrorSingleton.Instance.PlayerNames = new();
    }

    public override void Spawned()
    {
        AudioSingleton.Instance.PlayBgm(_bgmClip);
        _transitionProgressController.FadeOut().Forget();
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
            _token = this.GetCancellationTokenOnDestroy();
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
        
        RpcUpdateEmpty(_emptyIndex, false, Runner.LocalPlayer.PlayerId, PlayerInfo.PlayerName);
        PlayerInfo.PlayerColor = (PlayerColorEnum)(_emptyIndex + 1);
        
        // プレビュー(自分のジェット機の色と名前)を表示する
        var previewObj = _playerPreviewPrefabs[_emptyIndex];
        Runner.Spawn(previewObj, previewObj.transform.position, onBeforeSpawned: (_, obj) =>
        {
            Debug.Log(PlayerInfo.PlayerName);
            obj.GetComponent<ShowPlayerPreview>().MyName = PlayerInfo.PlayerName;
        });
    
    }

    /// <summary>
    /// 今の部屋から退出する．ホストなら全員が退出．
    /// </summary>
    public async void BackHome()
    {
        Runner.RemoveCallbacks(this);
        await Runner.Shutdown(); 
        await _transitionProgressController.FadeIn();
        SceneManager.LoadScene(_homeSceneName);
    }

    /// <summary>
    /// 自分が参加したことをホストに通知する．
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcUpdateEmpty(int index, bool exist, int playerId, string playerName)
    {
        _playerNames.Add(playerId, playerName);
        _hasEmpties.Set(index, exist);
        _hasEmptyPlayerIds.Set(index, playerId);
    }

    /// <summary>
    /// 次のシーンに行く．
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void RpcMoveScene()
    {
        await _transitionProgressController.FadeIn();
        GetPlayerNames();
        if (HasStateAuthority)
        {
            Runner.SessionInfo.IsVisible = false;
            JoinedPlayerCount = Runner.SessionInfo.PlayerCount;
            await UniTask.Delay(TimeSpan.FromSeconds(2.0f), cancellationToken: _token);
            await Runner.LoadScene(_nextSceneName);
        }
    }

    private void GetPlayerNames()
    {
        foreach (var player in _playerNames)
        {
            ErrorSingleton.Instance.PlayerNames.Add(player.Key, player.Value);
        }
    }
    
    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // 退出したプレイヤーがホストなら，全員が退出する
        if (player.PlayerId == 1)
        {
            Runner.Shutdown();
            SceneManager.LoadScene("Home");
            ErrorSingleton.Instance.ShowErrorPanel(ErrorType.HostDisconnected);
            return;
        }
         
        // ホストでないかつ，マッチングルームシーンなら，そのプレイヤーがいた枠に空きを作る．
        if (SceneManager.GetActiveScene().name == "MatchingRoom")
        {
            _hasEmpties.Set(_hasEmptyPlayerIds.IndexOf(player.PlayerId), true);
            _playerNames.Remove(player.PlayerId);
        }
        else
        {
            ErrorSingleton.Instance.UpdateLeftPlayerName(player.PlayerId);
        }
    }

    async void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        
        if (HasStateAuthority)
        {
            _startButton.interactable = false;
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f), cancellationToken: _token);
            _startButton.interactable = true;
        }
    }
    
    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (shutdownReason != ShutdownReason.Ok)
        {
            ErrorSingleton.Instance.ShowErrorPanel(ErrorType.NetworkConnectFailed);
            SceneManager.LoadScene("Home");
        }
         
        Debug.Log($"OnShutdown．{shutdownReason}");
    }
    
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        ErrorSingleton.Instance.ShowErrorPanel(ErrorType.DisconnectedFromServer);
        SceneManager.LoadScene("Home");
        Debug.Log($"OnDisconnectedFromServer．{reason}");
    }
    
    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress,
        NetConnectFailedReason reason)
    {
        ErrorSingleton.Instance.ShowErrorPanel(ErrorType.NetworkConnectFailed);
        SceneManager.LoadScene("Home");
        Debug.Log($"OnConnectFailed．{reason}");
    }
    
    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) {}
    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) {}
    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}
    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) {}
    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) {}
}