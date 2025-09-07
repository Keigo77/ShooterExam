using Fusion;
using UnityEngine;

public class EnemyBulletBehaviour : NetworkBehaviour
{
    public float BulletPower { private get; set; }
    [SerializeField] private float _existTime = 6.0f;
    private NetworkObject _networkObject;
    
    public override void Spawned()
    {
        _networkObject = this.GetComponent<NetworkObject>();
        Invoke(nameof(DespawnBullet), _existTime);
    }
    
    private void DespawnBullet()
    {
        // InvokeとOnTriggerで，Despawnした後に実行されるのを防ぐ
        if (_networkObject != null)
        {
            Runner.Despawn(_networkObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (HasStateAuthority && collision.CompareTag("Player"))
        {
            collision.GetComponent<ICharacter>().Damage(BulletPower);
            DespawnBullet();
        }
    }
}
