using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class MachineMonster : AttackerSmallEnemy, ICharacter
{
    [SerializeField] private float _rotateDuration;
    
    private Animator _animator;
    private int _animatorIsAttack;
    private int _animatorIsDead;
    
    public override void Spawned()
    {
        GetEnemyData();
        _networkObject = this.GetComponent<NetworkObject>();
        _token = this.GetCancellationTokenOnDestroy();
        _animator = this.GetComponent<Animator>();
        _animatorIsAttack = Animator.StringToHash("IsAttack");
        _animatorIsDead = Animator.StringToHash("IsDead");
        
        if (Runner.IsSharedModeMasterClient)
        {
            AttackLoop().Forget();
        }
    }

    protected override async UniTask AttackLoop()
    {
        while (!_token.IsCancellationRequested)
        {
            _animator.SetBool(_animatorIsAttack, true);
            await UniTask.Delay(TimeSpan.FromSeconds(_attackSpan), cancellationToken: _token);
        }
    }

    protected override void Attack()
    {
        if (Runner != null && HasStateAuthority)
        {
            GenerateBullet(new Vector2(0, 1).normalized);
            GenerateBullet(new Vector2(1, -1).normalized);
            GenerateBullet(new Vector2(-1, -1).normalized);
        }
    }

    private void RandomRotate()
    {
        this.transform.DOLocalRotate(new Vector3(0, 0, Random.Range(45, 315)), _rotateDuration).SetEase(Ease.Linear);
    }

    private void GenerateBullet(Vector2 direction)
    {
        Runner.Spawn(_bulletPrefab, this.transform.position, onBeforeSpawned: (_, bullet) =>
        {
            bullet.GetComponent<Rigidbody2D>().AddForce(direction * _bulletSpeed, ForceMode2D.Impulse);
            bullet.GetComponent<EnemyBulletBehaviour>().BulletPower = _bulletPower;
        });
    }
    
    private void FinishAttack()
    {
        _animator.SetBool(_animatorIsAttack, false);
    }

    public void Heal()
    {
        
    }
    
    public void Damage(float damage)
    {
        if (HasStateAuthority && Hp > 0)
        {
            Hp -= damage;
            if (Hp  <= 0)
            { 
                RpcDeath();
            }
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcDeath()
    {
        _animator.SetBool(_animatorIsDead, true);
    }

    private void DespawnEnemy()
    {
        Runner.Despawn(_networkObject);
    }
    
}
