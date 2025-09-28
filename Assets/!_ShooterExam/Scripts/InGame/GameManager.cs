using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum GameState
{
    Stopping,
    Playing,
    Clear,
    GameOver
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    [Networked] public GameState CurrentGameState { get; set; } = GameState.Stopping;

    // Hp
    [Networked, OnChangedRender(nameof(UpdatePlayerHpGauge))]
    private float AllPlayerHP { get; set; } = 0f;

    [Networked] private float MaxPlayersHP { get; set; }
    [SerializeField] private bool _isTestMode = false;

    // UI
    [SerializeField] private Slider _playerHpGaugeSlider;
    [SerializeField] private Slider _bossHpGaugeSlider;
    [SerializeField] private TransitionProgressController _transitionProgressController;
    [SerializeField] private ShowImageManager _showImageManager;

    // 始まるまでの処理
    [Networked] private int JoinedPlayerCount { get; set; } = 0;
    private int _nowPlayerCount = 0;
    private CancellationToken _token;
    private bool _isTimeOut = false;

    // プレイヤーは，これがtrueになってからAddPlayerHpを実行する．
    public bool IsSpawned = false;

    // リザルト
    private int _startTick;
    public static float ClearTime { get; private set; } = 0f;
    public static float RemainHpPercentage { get; private set; } = 0f;

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
        ClearTime = 0f;
        RemainHpPercentage = 0f;
        IsSpawned = true;
        _transitionProgressController.FadeOut().Forget();

        if (!HasStateAuthority)
        {
            return;
        }

        JoinedPlayerCount = WaitInRoom.JoinedPlayerCount;
        _token = this.GetCancellationTokenOnDestroy();
        // プレイヤー全員が揃うか，10秒経つまで待つ
        StartTimeoutCount().Forget();
        
        await UniTask.WaitUntil(() =>
            (_nowPlayerCount == WaitInRoom.JoinedPlayerCount || _isTimeOut), cancellationToken: _token);
        RpcDeleteTransition();
        await UniTask.WaitUntil(() => _transitionProgressController.Progress == 0f, cancellationToken: _token);
        await _showImageManager.ShowImage(ImageType.StartImage, 1.5f);

        Debug.Log("ゲーム開始");
        CurrentGameState = GameState.Playing;
        _startTick = Runner.Tick;
        AllPlayerHP = MaxPlayersHP;

        await UniTask.WaitUntil(() => CurrentGameState == GameState.Clear, cancellationToken: _token);
        
        StageClear().Forget();
    }

    public override void Render()
    {
        if (CurrentGameState == GameState.Playing)
        {
            ClearTime = (Runner.Tick - _startTick) * Runner.DeltaTime;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcDeleteTransition()
    {
        _transitionProgressController.FadeOut().Forget();
    }

    private async UniTaskVoid StartTimeoutCount()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(10.0f), cancellationToken: _token);
        _isTimeOut = true;
    }

    /// <summary>
    /// プレイヤーがスポーン時に実行．全体の最大HPを追加し，何人がバトル開始部屋まで入ってきたかチェックする．
    /// </summary>
    public void AddPlayerHp(float playerHp)
    {
        MaxPlayersHP += playerHp;
        _nowPlayerCount++;
    }

    // ネットワークプロパティはホストしか変更できないため，プレイヤー全員がホストに通知する．
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public async void RpcDecreasePlayerHpGauge(float damage)
    {
        if (HasStateAuthority && AllPlayerHP > 0)
        {
            AllPlayerHP -= damage;
            AllPlayerHP = Math.Max(AllPlayerHP, 0f);
        }
        
        if (HasStateAuthority && AllPlayerHP <= 0 && CurrentGameState == GameState.Playing && !_isTestMode)
        {
            CurrentGameState = GameState.GameOver;
            await _showImageManager.ShowImage(ImageType.GameOverImage, 1.5f);

            RpcMoveGameOverScene();
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcUpdateBossHpGauge(float maxBossHp, float bossHp)
    {
        _bossHpGaugeSlider.value = bossHp / maxBossHp;
        if (bossHp <= 0)
        {
            _bossHpGaugeSlider.gameObject.SetActive(false);
        }
    }

    private async UniTask StageClear()
    {
        if (CurrentGameState == GameState.Clear)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: _token);
            await _showImageManager.ShowImage(ImageType.ClearImage, 1.5f);

            RpcMoveResultScene(ClearTime);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private async void RpcMoveResultScene(float clearTime)
    {
        ClearTime = clearTime;
        RemainHpPercentage = AllPlayerHP / MaxPlayersHP;
        await _transitionProgressController.FadeIn();
        
        if (HasStateAuthority)
        {
            Runner.LoadScene("Result");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private async void RpcMoveGameOverScene()
    {
        await _transitionProgressController.FadeIn();

        if (HasStateAuthority)
        {
            Runner.LoadScene("StageSelect");
        }
    }
}

