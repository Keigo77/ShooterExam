using Fusion;
using UnityEngine;

// 弾はホストが生成させてるから，ホストじゃないとでスポーンさせれない



public class EnemyBulletBehaviour : NetworkBehaviour
{
    [Networked] public float BulletPower { private get; set; }
    [SerializeField] private float _existTime = 6.0f;
    private NetworkObject _networkObject;
    
    public override void Spawned()
    {
        _networkObject = this.GetComponent<NetworkObject>();
        Invoke(nameof(RpcDespawnBullet), _existTime);
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcDespawnBullet()
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
        if (collision.CompareTag("Player") && collision.gameObject.GetComponent<NetworkObject>().HasStateAuthority)
        {
            collision.GetComponent<ICharacter>().Damage(BulletPower);
            RpcDespawnBullet();
        }
    }
}
