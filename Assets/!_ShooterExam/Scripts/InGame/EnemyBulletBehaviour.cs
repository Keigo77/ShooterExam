using Fusion;
using UnityEngine;

// 弾はホストが生成させてるから，ホストじゃないとでスポーンさせれない



public class EnemyBulletBehaviour : NetworkBehaviour
{
    [Networked] public float BulletPower { get; set; }
    [SerializeField] private float _existTime = 6.0f;
    [SerializeField] private GameObject _bulletViewObj;
    private NetworkObject _networkObject;
    
    public override void Spawned()
    {
        _networkObject = this.GetComponent<NetworkObject>();
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
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.GetComponent<NetworkObject>().HasStateAuthority)
            {
                Debug.Log("デスポーン");
                collision.GetComponent<ICharacter>().Damage(BulletPower);
                RpcDespawnBullet();
                _bulletViewObj.SetActive(false);
            }
            else
            {
                _bulletViewObj.SetActive(false);
            }
        }
    }
}
