using System;
using Cysharp.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;

public class ResultManager : NetworkBehaviour
{
    private int _stageNumber;
    
    [SerializeField] private TransitionProgressController _transitionProgressController;
    [SerializeField] private AudioClip _resultBgm;
    [SerializeField] private TextMeshProUGUI _stageNameText;
    [SerializeField] private TextMeshProUGUI _clearTimeText;
    [SerializeField] private TextMeshProUGUI _remainHpText;
    [SerializeField] private GameObject _retryButton;
    [SerializeField] private GameObject _returnStageSelectButton;
    
    // 星
    [SerializeField] private StageRankBasisSO _stageRankDatas;
    [SerializeField] private GameObject[] _starImageObjects;
    private int score;

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
        
        AudioSingleton.Instance.PlayBgm(_resultBgm);
        _transitionProgressController.FadeOut().Forget();
        _stageNumber = StageSelectManager.StageNumber;
        ShowResultDetail();
    }

    private void ShowResultDetail()
    {
        _stageNameText.text = $"Stage{_stageNumber}";
        int clearTime = (int)Math.Floor(GameManager.ClearTime);
        _clearTimeText.text = $"{clearTime / 60:D2} : {clearTime % 60:D2}";
        _remainHpText.text = $"{(int)GameManager.RemainHpPercentage * 100}%";
        
        ShowRank(CalucScore(clearTime, (int)GameManager.RemainHpPercentage * 100));
    }

    private void ShowRank(int score)
    {
        int starCount = 0;
        
        switch (score)
        {
            case >= 70:
                starCount = 3;
                break;
            case >= 50:
                starCount = 2;
                break;
            case >= 30:
                starCount = 1;
                break;
        }

        for (int i = 0; i < starCount; i++)
        {
            _starImageObjects[i].SetActive(true);
        }
    }

    private int CalucScore(int clearTime, int remainHp)
    {
        int score = 0;
        int clearTimeScore = 0;
        int remainHpScore = 0;

        int maxClearTimeScore = _stageRankDatas.StageRankDatas[_stageNumber - 1].MaxPointClearTime;
        int minClearTimeScore = _stageRankDatas.StageRankDatas[_stageNumber - 1].MinPointClearTime;

        if (clearTime <= maxClearTimeScore)
        {
            clearTimeScore = 80;
        }
        else if (clearTime <= minClearTimeScore)
        {
            float rate = (float)(minClearTimeScore - clearTime) / (minClearTimeScore - maxClearTimeScore);
            clearTimeScore = (int)Math.Ceiling(80 * rate);
        }
        else
        {
            clearTimeScore = 0;
        }

        if (remainHp >= 60)
        {
            remainHpScore = 20;
        }
        else
        {
            float rate = remainHp / 60f;
            remainHpScore = (int)Math.Ceiling(remainHpScore * rate);
        }
        
        Debug.Log($"クリアスコア{clearTimeScore}，HPスコア{remainHpScore}，計{clearTimeScore + remainHpScore}");
        return score = clearTimeScore + remainHpScore;
    }

    public async void RetryButtonClicked()
    {
        await _transitionProgressController.FadeIn();
        Runner.LoadScene($"Stage{_stageNumber}");
    }
    
    public async void StageSelectButtonClicked()
    {
        await _transitionProgressController.FadeIn();
        Runner.LoadScene("StageSelect");
    }
}
