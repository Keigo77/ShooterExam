using Fusion;
using UnityEngine;

// 弾はホストが生成させてるから，ホストじゃないとでスポーンさせれない

public class EnemyBulletBehaviour : EnemyBulletBase
{
    public override void Spawned()
    {
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
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.GetComponent<NetworkObject>().HasStateAuthority)
            {
                collision.GetComponent<ICharacter>().Damage(BulletPower);
                RpcDespawnBullet();
                this.gameObject.SetActive(false);
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}
