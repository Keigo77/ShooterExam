using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class AnimationEnemyBulletBehaviour : EnemyBulletBase
{
    [SerializeField] private NetworkRigidbody2D _networkRigidbody;
    [SerializeField] private Rigidbody2D _rigidbody;
    private Animator _animator;
    private int _animatorIsHit;
    
    public override void Spawned()
    {
        _animator = this.GetComponent<Animator>();
        _animatorIsHit = Animator.StringToHash("IsHit");
        // trueのまま速度を0にすると，他プレイヤーのも0になるため，enableをflaseにする
        _networkRigidbody.enabled = false;
        Invoke(nameof(RpcDespawnBullet), _existTime);
    }
    
    /// <summary>
    /// ホストしかでスポーンさせられないため，ホストにデスポーンを要求する．
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcDespawnBullet()
    {
        if (HasStateAuthority)
        {
            // InvokeとOnTriggerで，Despawnした後に実行されるのを防ぐ
            CancelInvoke(nameof(RpcDespawnBullet));
            Runner.Despawn(_networkObject);
        }
    }
    
    /// <summary>
    /// 他プレオヤーがたまに当たっていたら，アニメーションを再生し，非表示にする．
    /// 本人が当たっていたら，ダメージを反映させる．
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _animator.SetBool(_animatorIsHit, true);
            if (collision.GetComponent<NetworkObject>().HasStateAuthority)
            {
                collision.GetComponent<ICharacter>().Damage(BulletPower);
            }
        }
    }
    
    
}
