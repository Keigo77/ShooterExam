using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class MachineMonster : AttackerSmallEnemyBase, ICharacter
{
    [SerializeField] private float _rotateDuration;
    
    private Animator _animator;
    private int _animatorIsAttack;
    private int _animatorIsDead;
    
    public override async void Spawned()
    {
        GetEnemyData();
        GetToken();
        _networkObject = this.GetComponent<NetworkObject>();
        _animator = this.GetComponent<Animator>();
        _animatorIsAttack = Animator.StringToHash("IsAttack");
        _animatorIsDead = Animator.StringToHash("IsDead");

        if (Runner.IsSharedModeMasterClient)
        {
            try
            {
                await MoveInScreen();
            }
            catch (Exception e)
            {
                Debug.Log($"{e}\nMoveInScreen()がキャンセルされました");
            }

            AttackLoop().Forget();
        }
    }

    protected override async UniTask AttackLoop()
    {
        while (!_token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_attackSpan + Random.Range(0.0f, 2.0f)), cancellationToken: _token);
            _animator.SetBool(_animatorIsAttack, true);
            await UniTask.WaitUntil(() => !_animator.GetBool(_animatorIsDead), cancellationToken: _token);
        }
    }

    protected override void Attack()
    {
        if (Runner != null && HasStateAuthority)
        {
            GenerateBullet(new Vector2(-1, 1).normalized);
            GenerateBullet(new Vector2(-1, 0).normalized);
            GenerateBullet(new Vector2(-1, -1).normalized);
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
    
    /// <summary>
    /// プレイヤーの弾が当たったら，HPを減らす
    /// </summary>
    public void Damage(float damage)
    {
        if (Hp > 0)
        {
            Hp -= damage;
            if (Hp  <= 0)
            { 
                RpcDeath();
            }
        }
    }
    
    /// <summary>
    /// 敵の死亡を，全プレイヤーに通知する
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RpcDeath()
    {
        _animator.SetBool(_animatorIsDead, true);
    }
}
