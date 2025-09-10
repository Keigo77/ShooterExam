using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerColor
{
    Purple = 0,
    Red = 1,
    Green = 2,
    Yellow = 3
}

public class PlayerInfo : MonoBehaviour
{
    public static string PlayerName { get; private set; }
    public static PlayerColor PlayerColor { get; set; }
    [SerializeField] private TMP_InputField _playerNameInputField;

    public void UpdatePlayerName()
    {
        PlayerName = _playerNameInputField.text;
    }
}
