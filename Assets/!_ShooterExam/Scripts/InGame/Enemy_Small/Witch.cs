using System;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

public class Witch : AttackerSmallEnemyBase, ICharacter
{
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            this.transform.localPosition = SpawnPos;
            IsSpawned = true;
            AttackLoop().Forget();
        }
    }
    
    protected override async UniTaskVoid AttackLoop()
    {
        while (!_token.IsCancellationRequested)
        {
            _animator.SetBool(_animatorIsAttack, true);
            await UniTask.Delay(TimeSpan.FromSeconds(_attackSpan), cancellationToken: _token);
        }
    }
    
    protected override void Attack()
    {
        var randDirection = new Vector2(-1f, Random.Range(-1f, 1f)).normalized;
        Runner.Spawn(_bulletPrefab, this.transform.position, onBeforeSpawned: (_, bullet) =>
        {
            bullet.GetComponent<Rigidbody2D>().AddForce(randDirection * _bulletSpeed, ForceMode2D.Impulse);
        });
    }

    private void FinishAttack()
    {
        _animator.SetBool(_animatorIsAttack, false);
    }

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
