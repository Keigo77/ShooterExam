using Fusion;
using TMPro;
using UnityEngine;

public class ShowPlayerPreview : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _myNameText;
    [Networked] public NetworkString<_16> MyName { get;  set; }

    public override void Spawned()
    {
        UpdateMyName(MyName.Value);
    }
    
    private void UpdateMyName(string playerName)
    {
        _myNameText.text = playerName;
    }
}

