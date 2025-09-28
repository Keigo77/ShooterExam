using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

public enum ImageType
{
    StartImage,
    WaringImage,
    ClearImage,
    GameOverImage
}

public class ShowImageManager : NetworkBehaviour
{
    [SerializeField] private GameObject _startImageObj;
    [SerializeField] private Animator _startImageAnimator;
    
    [SerializeField] private GameObject _warningImageObj;
    [SerializeField] private Animator _warningImageAnimator;
    
    [SerializeField] private GameObject _clearImageObj;
    [SerializeField] private Animator _clearImageAnimator;

    [SerializeField] private GameObject _gameOverImageObj;
    [SerializeField] private Animator _gameOverImageAnimator;
    
    private int _animatorIsDelete;
    private CancellationToken _token;

    private void Awake()
    {
        _token = this.GetCancellationTokenOnDestroy();
        _animatorIsDelete = Animator.StringToHash("IsDelete");
    }

    public async UniTask ShowImage(ImageType imageType, float duration)
    {
        RpcShowImage(imageType);
        switch (imageType)
        {
            case ImageType.StartImage:
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: _token);
                _startImageAnimator.SetBool(_animatorIsDelete, true);
                break;
            case ImageType.WaringImage:
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: _token);
                _warningImageAnimator.SetBool(_animatorIsDelete, true);
                break;
            case ImageType.ClearImage:
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: _token);
                _clearImageAnimator.SetBool(_animatorIsDelete, true);
                break;
            case ImageType.GameOverImage:
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: _token);
                _gameOverImageAnimator.SetBool(_animatorIsDelete, true);
                break;
            default:
                Debug.LogError("Unknown image type");
                break;
        }
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: _token);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcShowImage(ImageType imageType)
    {
        switch (imageType)
        {
            case ImageType.StartImage:
                _startImageObj.SetActive(true);
                break;
            case ImageType.WaringImage:
                _warningImageObj.SetActive(true);
                break;
            case ImageType.ClearImage:
                _clearImageObj.SetActive(true);
                break;
            case ImageType.GameOverImage:
                _gameOverImageObj.SetActive(true);
                break;
            default:
                Debug.LogError("Unknown image type");
                break;
        }
    }
}
