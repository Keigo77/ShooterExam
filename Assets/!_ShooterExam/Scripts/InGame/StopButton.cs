using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StopButton : NetworkBehaviour
{
    [SerializeField] private GameObject _stopPanel;
    [SerializeField] private Button _stageSelectButton;
    [SerializeField] private TextMeshProUGUI _stageSelectButtonText;
    [SerializeField] private TransitionProgressController _transitionProgressController;
    
    public override void Spawned()
    {
        // ホスト以外は，ステージセレクトボタンが使えないようにする(一時停止ボタン自体消すと配置が不自然になるため，クライアントも押せるようにはする)
        if (!HasStateAuthority)
        {
            _stageSelectButton.interactable = false;
            _stageSelectButtonText.text = "Only the host\ncan use it";
        }
    }

    public void StopButtonClicked()
    {
        _stopPanel.SetActive(true);
    }
    
    public void ContinueButtonClicked()
    {
        _stopPanel.SetActive(false);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void RpcStageSelectButtonClicked()
    {
        await _transitionProgressController.FadeIn();
        await Runner.LoadScene("StageSelect");
    }
    
}
