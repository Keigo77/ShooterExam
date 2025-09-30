using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class Machine : AttackerSmallEnemyBase, ICharacter
{
    [SerializeField] private float _rotateDuration;
    
    public override async void Spawned()
    {
        if (HasStateAuthority)
        {
            await MoveInScreen();

            IsSpawned = true;
            AttackLoop().Forget();
        }
    }

    protected override async UniTaskVoid AttackLoop()
    {
        while (!_token.IsCancellationRequested && Hp > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_attackSpan + Random.Range(0.0f, 2.0f)), cancellationToken: _token);
            _animator.SetBool(_animatorIsAttack, true);
            await UniTask.WaitUntil(() => !_animator.GetBool(_animatorIsDead), cancellationToken: _token);
        }
    }

    protected override void Attack()
    {
        if (Runner.IsRunning && HasStateAuthority)
        {
            GenerateBullet(new Vector2(-1, 0.7f).normalized);
            GenerateBullet(new Vector2(-1, -0.7f).normalized);
        }
    }

    /// <summary>
    /// 攻撃前に，少し回転してから攻撃する．(弾の飛ぶ位置が一定でなくなるようにする)
    /// </summary>
    private void RandomRotate()
    {
        var resultAngle = this.transform.localRotation.eulerAngles + new Vector3(0, 0, Random.Range(-20, 20));
        NetworkDOTween.MyDORotate(this.transform, resultAngle, _rotateDuration, _token).Forget();
    }

    private void GenerateBullet(Vector2 direction)
    {
        Vector2 shotDirection = transform.TransformDirection(direction);
        Runner.Spawn(_bulletPrefab, this.transform.position, onBeforeSpawned: (_, bullet) =>
        {
            bullet.GetComponent<Rigidbody2D>().AddForce(shotDirection * _bulletSpeed, ForceMode2D.Impulse);
        });
    }
    
    private void FinishAttack()
    {
        _animator.SetBool(_animatorIsAttack, false);
    }
    
    /// <summary>
    /// プレイヤーの弾が当たったら，HPを減らす
    /// </summary>
    public void Damage(float damage)
    {
        if (IsSpawned && Hp > 0)
        {
            RpcDamage(damage);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcDamage(float damage)
    {
        Hp -= damage;
        if (Hp <= 0)
        {
            _animator.SetBool(_animatorIsDead, true);
        }
    }
}
