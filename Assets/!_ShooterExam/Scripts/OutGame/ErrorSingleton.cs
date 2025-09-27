using TMPro;
using UnityEngine;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
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

}
