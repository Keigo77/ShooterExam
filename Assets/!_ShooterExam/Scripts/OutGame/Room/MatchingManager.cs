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
    [SerializeField] private int _testSceneIndex;
    [SerializeField] private string _matchingSceneName;
    
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private TransitionProgressController _transitionProgressController;
    [SerializeField] private AudioClip _bgmClip;
    private CancellationToken _token;

    private void Awake()
    {
        _transitionProgressController.Progress = 1f;
    }
    
    private void Start()
    {
        AudioSingleton.Instance.PlayBgm(_bgmClip);
        _transitionProgressController.FadeOut().Forget();
        _token = this.GetCancellationTokenOnDestroy();
    }

    /// <summary>
    /// ランダムマッチの開始
    /// </summary>
    public async void StartRandomMatching() {
        var networkRunner = Instantiate(_networkRunnerPrefab);
        _loadingPanel.SetActive(true);
        
        var result = await networkRunner.StartGame(new StartGameArgs {
            GameMode = GameMode.Shared,
            PlayerCount = 4,
        });
        Debug.Log(result);
        CheckResult(networkRunner, result, "MatchingRoom").Forget();
    }
    
    /// <summary>
    /// プライベートマッチの開始
    /// </summary>
    public async void StartPrivateMatching() {
        var networkRunner = Instantiate(_networkRunnerPrefab);
        _loadingPanel.SetActive(true);

        var result = await networkRunner.StartGame(new StartGameArgs {
            GameMode = GameMode.Shared,
            SessionName = _inputRoomNameField.text,
            IsVisible = false,
            PlayerCount = 4,
        });
        Debug.Log(result);
        CheckResult(networkRunner, result, "MatchingRoom").Forget();
    }
    
    /// <summary>
    /// 敵の挙動などを確認するためのテスト用ボタン
    /// </summary>
    public async void TestMatching() {
        var networkRunner = Instantiate(_networkRunnerPrefab);
        _loadingPanel.SetActive(true);
        PlayerInfo.PlayerColor = PlayerColorEnum.Purple;
        
        var result = await networkRunner.StartGame(new StartGameArgs {
            GameMode = GameMode.Shared,
            SessionName = _inputRoomNameField.text,
            IsVisible = false,
            PlayerCount = 4,
        });
        Debug.Log(result);
        CheckResult(networkRunner, result, "PreEnemy").Forget();
    }

    /// <summary>
    /// 部屋の作成を実行し，成功したらマッチングルームへ，失敗したら，エラーパネルを表示する．
    /// </summary>
    private async UniTask CheckResult(NetworkRunner runner, StartGameResult result, string sceneName)
    {
        _loadingPanel.SetActive(false);
        if (result.Ok)
        {
            // StartGameでもシーン遷移できるが，遷移アニメーションを再生しきれないため，runner.LoadSceneで遷移する
            await _transitionProgressController.FadeIn();
            runner.LoadScene(sceneName);
        } 
        else 
        {
            ErrorSingleton.Instance.ShowErrorPanel(ErrorType.NetworkConnectFailed);
        }
    }
    
}
