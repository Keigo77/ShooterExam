using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;

public class BulletGenerator : MonoBehaviour
{
    [SerializeField] private BulletObjectPool _bulletObjectPool;
    [SerializeField] private PlayerStatusEffectManager _playerStatusEffectManager;
    [SerializeField] private float _generateSpan;
    [SerializeField] private float _shootPower;
    private CancellationToken _token;

    private void Start()
    {
        _token = this.GetCancellationTokenOnDestroy();
        StartGenerateLoop().Forget();
    }

    private async UniTask StartGenerateLoop()
    {
        while (!_token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_generateSpan), cancellationToken: _token);
            
            if (_playerStatusEffectManager.PlayerStatusEffects == StatusEffect.Paralysis)
            {
                continue;
            }
            
            var bullet = _bulletObjectPool.GetBullet();
            bullet.GetComponent<Rigidbody2D>().AddForce(new Vector2(1, 0) * _shootPower, ForceMode2D.Impulse);
            bullet.GetComponent<BulletBehaviour>().BulletPower = _shootPower;
            bullet.transform.position = this.transform.position;
        }
    }
}
