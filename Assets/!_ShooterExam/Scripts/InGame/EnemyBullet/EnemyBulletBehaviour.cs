using Fusion;
using UnityEngine;

// 弾はホストが生成させてるから，ホストじゃないとでスポーンさせれない

public class EnemyBulletBehaviour : EnemyBulletBase
{
    private SpriteRenderer _renderer;
    
    public override void Spawned()
    {
        _networkObject = this.GetComponent<NetworkObject>();
        _rigidbody = this.GetComponent<Rigidbody2D>();
        _renderer = this.GetComponent<SpriteRenderer>();
        if (HasStateAuthority)
        {
            Invoke(nameof(RpcDespawnBullet), _existTime);
        }
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
    /// 
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && GameManager.Instance.CurrentGameState == GameState.Playing)
        {
            collision.GetComponent<PlayerController>().ChangeDamageColor().Forget();
            _renderer.enabled = false;
            
            if (collision.GetComponent<NetworkObject>().HasStateAuthority)
            {
                collision.GetComponent<ICharacter>().Damage(BulletPower);
            }
            RpcDespawnBullet();
        }
    }
}
