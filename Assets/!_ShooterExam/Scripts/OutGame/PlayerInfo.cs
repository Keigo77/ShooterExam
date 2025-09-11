using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerColorEnum
{
    Null = 0,
    Purple = 1,
    Red = 2,
    Green = 3,
    Yellow = 4
}

public class PlayerInfo : MonoBehaviour
{
    public static string PlayerName { get; private set; }
    public static PlayerColorEnum PlayerColor { get; set; } = PlayerColorEnum.Null;
    [SerializeField] private TMP_InputField _playerNameInputField;

    private void Start()
    {
        if (ES3.KeyExists("PlayerName"))
        {
            _playerNameInputField.text = ES3.Load<string>("PlayerName");
        }
    }
    
    public void UpdatePlayerName()
    {
        PlayerName = _playerNameInputField.text;
        ES3.Save("PlayerName", PlayerName);
    }
}
