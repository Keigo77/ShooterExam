using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MatchingManager : MonoBehaviour
{
    [SerializeField] private NetworkRunner _networkRunnerPrefab;
    [SerializeField] private TMP_InputField _inputRoomNameField;
    [SerializeField] private int _matchingSceneIndex;
    [SerializeField] private string _matchingSceneName;
    
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private TransitionProgressController _transitionProgressController;
    private CancellationToken _token;

    private void Awake()
    {
        _transitionProgressController.Progress = 1f;
    }
    
    private void Start()
    {
        _transitionProgressController.FadeOut().Forget();
        _token = this.GetCancellationTokenOnDestroy();
    }

    /// <summary>
    /// ランダムマッチの開始
    /// </summary>
    public async void StartRandomMatching() {
        var networkRunner = Instantiate(_networkRunnerPrefab);
        ShowLoadingPanel(networkRunner).Forget();
        
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
        ShowLoadingPanel(networkRunner).Forget();
        
        var result = await networkRunner.StartGame(new StartGameArgs {
            GameMode = GameMode.Shared,
            Scene = SceneRef.FromIndex(_matchingSceneIndex),
            SessionName = _inputRoomNameField.text,
            IsVisible = false,
            PlayerCount = 4,
        });
        Debug.Log(result);
    }

    /// <summary>
    /// 部屋に入ったらLoadingパネルを削除し，シーン遷移アニメーションを再生後にシーン遷移
    /// </summary>
    private async UniTask ShowLoadingPanel(NetworkRunner runner)
    {
        _loadingPanel.SetActive(true);
        await UniTask.WaitUntil(() => runner.IsRunning, cancellationToken: _token);
        _loadingPanel.SetActive(false);
        await _transitionProgressController.FadeIn();
        //SceneManager.LoadScene(_matchingSceneName);
    }
}
