using Fusion;
using TMPro;
using UnityEngine;

public class WaitInRoom : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _sessionNameText;
    [SerializeField] private TextMeshProUGUI _playerCountText;
    [SerializeField] private GameObject[] _playerPreviewPrefabs;
    
    public override void Spawned()
    {
        _sessionNameText.text = $"SessionName: {Runner.SessionInfo.Name}";
        // 各色のプレイヤーをスポーン
        var previewObj = _playerPreviewPrefabs[Runner.SessionInfo.PlayerCount - 1];
        Runner.Spawn(previewObj, previewObj.transform.position, onBeforeSpawned: (_, obj) =>
        {
            obj.GetComponent<ShowPlayerPreview>().MyName = PlayerInfo.PlayerName;
        });
    }
    
    
}
