using System;
using Cysharp.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;

public class ResultManager : NetworkBehaviour
{
    [SerializeField] private TransitionProgressController _transitionProgressController;
    [SerializeField] private AudioClip _resultBgm;
    [SerializeField] private TextMeshProUGUI _stageNameText;
    [SerializeField] private TextMeshProUGUI _clearTimeText;
    [SerializeField] private TextMeshProUGUI _remainHpText;
    [SerializeField] private GameObject _retryButton;
    [SerializeField] private GameObject _returnStageSelectButton;

    private void Awake()
    {
        _transitionProgressController.Progress = 1.0f;
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            _retryButton.SetActive(true);
            _returnStageSelectButton.SetActive(true);
        }
        
        _stageNameText.text = $"Stage{StageSelectManager.StageNumber}";
        AudioSingleton.Instance.PlayBgm(_resultBgm);
        _transitionProgressController.FadeOut().Forget();
        int clearTime = (int)Math.Floor(GameManager.ClearTime);
        _clearTimeText.text = $"{clearTime / 60:D2} : {clearTime % 60:D2}";
        _remainHpText.text = $"{(int)GameManager.RemainHpPercentage * 100}%";
    }
}
