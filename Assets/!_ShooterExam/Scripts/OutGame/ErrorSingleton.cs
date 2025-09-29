using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

public enum ErrorType
{
    HostDisconnected,
    NetworkConnectFailed,
    DisconnectedFromServer,
}

public class ErrorSingleton : MonoBehaviour
{
    public static ErrorSingleton Instance;
    [SerializeField] private GameObject _errorPanel;
    [SerializeField] private TextMeshProUGUI _errorMessageText;
    
    // 退出したプレイヤーの名前を表示する
    [SerializeField] private AudioClip _playerLeftSe;
    [SerializeField] private TextMeshProUGUI _leftPlayerNameText;
    public Dictionary<int, string> PlayerNames = new Dictionary<int, string>();
    public ReactiveCollection<string> _leftPlayerNames = new ReactiveCollection<string>();
    private CancellationToken _token;
    

    private void Awake()
    {
        _token = this.GetCancellationTokenOnDestroy();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        _leftPlayerNames.ObserveAdd().Subscribe(
            _ => ShowLeftPlayer());
        
        _leftPlayerNames.ObserveRemove().Subscribe(
            _ => ShowLeftPlayer());
    }

    public void ShowErrorPanel(ErrorType errorType)
    {
        switch (errorType)
        {
            case ErrorType.HostDisconnected:    // ホストが退出した
                _errorMessageText.text = "The host was disconnected";
                break;
            case ErrorType.NetworkConnectFailed:    // 通信を試みたが，失敗
                _errorMessageText.text = "The network connect failed";
                break;
            case ErrorType.DisconnectedFromServer:      // 正常に通信していたのに，サーバーとの通信が途絶えた
                _errorMessageText.text = "Disconnected from the server";
                break;
        }
        
        _errorPanel.SetActive(true);
    }

    public void OkButtonOnClicked()
    {
        _errorPanel.SetActive(false);
    }
    
    public async UniTaskVoid UpdateLeftPlayerName(int playerId)
    {
        AudioSingleton.Instance.PlaySe(_playerLeftSe);
        _leftPlayerNames.Add(PlayerNames[playerId]);
        await UniTask.Delay(TimeSpan.FromSeconds(3.0f), cancellationToken: _token);
        _leftPlayerNames.Remove(PlayerNames[playerId]);
    }

    private void ShowLeftPlayer()
    {
        _leftPlayerNameText.text = "";
        foreach (var leftPlayerName in _leftPlayerNames)
        {
            _leftPlayerNameText.text += $"{leftPlayerName} has left\n";
        }
    }
}
