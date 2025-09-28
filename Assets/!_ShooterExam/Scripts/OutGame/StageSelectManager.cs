using Cysharp.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;

public class StageSelectManager : NetworkBehaviour
{
    public static int StageNumber;
    [SerializeField] private TransitionProgressController _transitionProgressController;
    [SerializeField] private AudioClip _stageSelectBgm;
    [SerializeField] private TextMeshProUGUI _waitText;
    [SerializeField] private GameObject _clientWaitPanel;

    private void Awake()
    {
        _transitionProgressController.Progress = 1.0f;
    }
    
    public override void Spawned()
    {
        AudioSingleton.Instance.PlayBgm(_stageSelectBgm);
        _transitionProgressController.FadeOut().Forget();
        if (HasStateAuthority)
        {
            _waitText.text = "Other players are waiting...";
        }
        else
        {
            _waitText.gameObject.SetActive(false);
            _clientWaitPanel.SetActive(true);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void RpcSelectStage(int stageNumber)
    {
        StageNumber = stageNumber;
        await _transitionProgressController.FadeIn();
        if (HasStateAuthority)
        {
            Runner.LoadScene($"Stage{stageNumber}");
        }
    }
}
