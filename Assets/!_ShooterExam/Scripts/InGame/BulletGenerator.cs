using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

public class BulletGenerator : NetworkBehaviour
{
    [SerializeField] private NetworkObject _bulletPrefab;
    [SerializeField] private float _generateSpan;
    [SerializeField] private float _shootPower;
    private CancellationToken _token;

    public override void Spawned()
    {
        _token = this.GetCancellationTokenOnDestroy();
        if (HasStateAuthority)
        {
            StartGenerateLoop().Forget();
        }
    }

    private async UniTask StartGenerateLoop()
    {
        while (!_token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_generateSpan), cancellationToken: _token);
            Runner.Spawn(_bulletPrefab, this.transform.position, Quaternion.identity, onBeforeSpawned: (_, bullet) =>
            {
                bullet.GetComponent<Rigidbody2D>().AddForce(new Vector2(1, 0) * _shootPower, ForceMode2D.Impulse);
            });
        }
    }
}
