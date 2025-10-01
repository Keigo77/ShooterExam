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
    [SerializeField] private GameObject _clientTextObj;
    
    // 星
    [SerializeField] private StageRankBasisSO _stageRankDatas;
    [SerializeField] private GameObject[] _starImageObjects;
    private int score;

    private void Awake()
    {
        _transitionProgressController.Progress = 1.0f;
    }

    /// <summary>
    /// ホストはリトライボタンとステージセレクトボタンを表示．クライアントはテキストを表示．
    /// </summary>
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            _retryButton.SetActive(true);
            _returnStageSelectButton.SetActive(true);
        }
        else
        {
            _clientTextObj.SetActive(true);
        }
        
        AudioSingleton.Instance.PlayBgm(_resultBgm);
        _transitionProgressController.FadeOut().Forget();
        _stageNumber = StageSelectManager.StageNumber;
        ShowResultDetail();
    }

    /// <summary>
    /// ステージ名・クリアタイム・残ったHPを表示し，最終スコアを元にスターを表示する．
    /// </summary>
    private void ShowResultDetail()
    {
        _stageNameText.text = $"Stage";
        int clearTime = (int)Math.Floor(GameManager.ClearTime);
        _clearTimeText.text = $"{clearTime / 60:D2} : {clearTime % 60:D2}";
        _remainHpText.text = $"{(int)GameManager.RemainHpPercentage}%";
        
        ShowRank(CalcScore(clearTime, (int)GameManager.RemainHpPercentage));
    }
    
    /// <summary>
    /// スコアに応じて，スターの数を決め，表示させる．
    /// </summary>
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

    /// <summary>
    /// クリアタイム80%，残量HPを20%，計100点で評価する．
    /// クリアタイムは，80点満点のクリアタイムと，0点のクリアタイムをScriptableObjectから取得し，線形的に減点されるように計算．
    /// HPスコアは，残量HPが60%以上なら20点満点．60%未満は，0%を0点として線形的に減点．
    /// </summary>
    private int CalcScore(int clearTime, int remainHp)
    {
        int score = 0;
        int clearTimeScore = 0;
        int remainHpScore = 0;

        int maxClearTimeScore = _stageRankDatas.StageRankDatas[_stageNumber - 1].MaxPointClearTime;
        int minClearTimeScore = _stageRankDatas.StageRankDatas[_stageNumber - 1].MinPointClearTime;

        if (clearTime <= maxClearTimeScore)
        {
            clearTimeScore = 80;    // 満点
        }
        else if (clearTime <= minClearTimeScore)
        {
            float rate = (float)(minClearTimeScore - clearTime) / (minClearTimeScore - maxClearTimeScore);
            clearTimeScore = (int)Math.Ceiling(80 * rate);
        }
        else
        {
            clearTimeScore = 0;     // 最低クリア時間より遅ければ，0点．
        }

        if (remainHp >= 60)
        {
            remainHpScore = 20;
        }
        else
        {
            float rate = remainHp / 60f;
            remainHpScore = (int)Math.Ceiling(20 * rate);
        }
        
        Debug.Log($"クリアスコア{clearTimeScore}，HPスコア{remainHpScore}，計{clearTimeScore + remainHpScore}");
        return score = clearTimeScore + remainHpScore;
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void RpcRetryButtonClicked()
    {
        await _transitionProgressController.FadeIn();
        if (HasStateAuthority)
        {
            Runner.LoadScene($"Stage");
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void RpcStageSelectButtonClicked()
    {
        await _transitionProgressController.FadeIn();
        if (HasStateAuthority)
        {
            Runner.LoadScene("StageSelect");
        }
    }
}
