using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ResultManager : MonoBehaviour
{
    [SerializeField] private TransitionProgressController _transitionProgressController;
    [SerializeField] private AudioClip _resultBgm;
    [SerializeField] private TextMeshProUGUI _clearTimeText;
    [SerializeField] private TextMeshProUGUI _remainHpText;

    private void Awake()
    {
        _transitionProgressController.Progress = 1.0f;
    }

    void Start()
    {
        AudioSingleton.Instance.PlayBgm(_resultBgm);
        _transitionProgressController.FadeOut().Forget();
        int clearTime = (int)Math.Floor(GameManager.ClearTime);
        _clearTimeText.SetText($"{clearTime / 60:D2} : {clearTime % 60:D2}");
        _remainHpText.SetText($"{(int)GameManager.RemainHpPercentage * 100}%");
    }
}
