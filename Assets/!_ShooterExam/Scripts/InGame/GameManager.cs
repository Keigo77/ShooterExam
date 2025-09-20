using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum GameState
{
    Stopping,
    Playing,
    Finished
}

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static GameManager Instance { get; private set; }
    [Networked] public GameState CurrentGameState { get; private set; } = GameState.Stopping;
    
    // Hp
    [Networked, OnChangedRender(nameof(UpdatePlayerHpGauge))] private float AllPlayerHP { get; set; } = 0f;
    [Networked] private float MaxPlayersHP { get; set; }
    
    // UI
    [SerializeField] private Slider _playerHpGaugeSlider;
    [SerializeField] private Slider _bossHpGaugeSlider;
    [SerializeField] private TransitionProgressController _transitionProgressController;
    
    // 始まるまでの処理
    [Networked] private int JoinedPlayerCount { get; set; } = 0;
    private int _nowPlayerCount = 0;
    private CancellationToken _token;
    private bool _isTimeOut = false;
    
    // プレイヤーは，これがtrueになってからAddPlayerHpを実行する．f
    public bool IsSpawned = false;

    private void Awake()
    {
        _transitionProgressController.Progress = 1.0f;
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public override async void Spawned()
    {
        IsSpawned = true;

        try
        {
            await _transitionProgressController.FadeOut();
        }
        catch (Exception e)
        {
            Debug.Log($"{e}　フェードアウトがキャンセルされました");
        }
        
        if (!HasStateAuthority)
        {
            return;
        }

        JoinedPlayerCount = WaitInRoom.JoinedPlayerCount;
        _token = this.GetCancellationTokenOnDestroy();
        // プレイヤー全員が揃うか，10秒経つまで待つ
        StartTimeoutCount().Forget();
        
        try
        {
            await UniTask.WaitUntil(() =>
                (_nowPlayerCount == WaitInRoom.JoinedPlayerCount || _isTimeOut), cancellationToken: _token);
        }
        catch (Exception e)
        {
            Debug.Log($"{e}　プレイヤー入室待ちがキャンセルされました");
        }
        
        Debug.Log("ゲーム開始");
        CurrentGameState = GameState.Playing;
        AllPlayerHP = MaxPlayersHP;
    }

    private async UniTask StartTimeoutCount()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(10.0f), cancellationToken: _token);
        _isTimeOut = true;
    }
    
    /// <summary>
    /// プレイヤーがスポーン時に実行．全体の最大HPを追加し，何人がバトル開始部屋まで入ってきたかチェックする．
    /// </summary>
    public void AddPlayerHP(float playerHp)
    { 
        MaxPlayersHP += playerHp;
        _nowPlayerCount++;
    }

    // ネットワークプロパティはホストしか変更できないため，プレイヤー全員がホストに通知する．
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcDecreasePlayerHpGauge(float damage)
    {
        if (HasStateAuthority)
        {
            AllPlayerHP -= damage;
        }
    }
    
    private void UpdatePlayerHpGauge()
    {
        _playerHpGaugeSlider.value = AllPlayerHP / MaxPlayersHP;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcInitializeBossHpGauge(float bossHp)
    {
        _bossHpGaugeSlider.value = 1.0f;
        _bossHpGaugeSlider.gameObject.SetActive(true);
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcUpdateBossHpGauge(float maxBossHp, float bossHp)
    {
        if (bossHp <= 0)
        {
            _bossHpGaugeSlider.gameObject.SetActive(false);
        } 
        _bossHpGaugeSlider.value = bossHp / maxBossHp;
    }
    
    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player) {}
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

