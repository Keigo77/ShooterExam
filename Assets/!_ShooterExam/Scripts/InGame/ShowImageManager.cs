using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

public enum ImageType
{
    StartImage,
    WaringImage,
    ClearImage
}

public class ShowImageManager : NetworkBehaviour
{
    [SerializeField] private GameObject _startImageObj;
    [SerializeField] private Animator _startImageAnimator;
    private int _startImageIsDelete;
    
    [SerializeField] private GameObject _warningImageObj;
    [SerializeField] private Animator _warningImageAnimator;
    private int _warningImageIsDelete;
    
    [SerializeField] private GameObject _clearImageObj;
    [SerializeField] private Animator _clearImageAnimator;
    private int _clearImageIsDelete;
    
    private CancellationToken _token;

    private void Awake()
    {
        _token = this.GetCancellationTokenOnDestroy();
        _startImageIsDelete = Animator.StringToHash("IsDelete");
        _warningImageIsDelete = Animator.StringToHash("IsDelete");
        _clearImageIsDelete = Animator.StringToHash("IsDelete");
    }

    public async UniTask ShowImage(ImageType imageType, float duration)
    {
        RpcShowImage(imageType);
        switch (imageType)
        {
            case ImageType.StartImage:
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: _token);
                _startImageAnimator.SetBool(_startImageIsDelete, true);
                break;
            case ImageType.WaringImage:
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: _token);
                _warningImageAnimator.SetBool(_warningImageIsDelete, true);
                break;
            case ImageType.ClearImage:
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: _token);
                _clearImageAnimator.SetBool(_clearImageIsDelete, true);
                break;
        }
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
        }
    }
}
