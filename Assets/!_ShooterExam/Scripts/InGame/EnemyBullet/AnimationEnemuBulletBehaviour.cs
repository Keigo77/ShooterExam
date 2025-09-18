using Fusion;
using UnityEngine;

public class AnimationEnemuBulletBehaviour : EnemyBulletBase
{
    [SerializeField] private Rigidbody2D _rigidbody;
    private Animator _animator;
    private int _animatorIsHit;
    
    public override void Spawned()
    {
        _animator = this.GetComponent<Animator>();
        _animatorIsHit = Animator.StringToHash("IsHit");
        _networkObject = this.GetComponent<NetworkObject>();
        Invoke(nameof(DespawnBullet), _existTime);
    }
    
    /// <summary>
    /// ホストしかでスポーンさせられないため，ホストにデスポーンを要求する．
    /// </summary>
    public void DespawnBullet()
    {
        this.gameObject.SetActive(false);
        if (HasStateAuthority)
        {
            // InvokeとOnTriggerで，Despawnした後に実行されるのを防ぐ
            CancelInvoke(nameof(DespawnBullet));
            Runner.Despawn(_networkObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("hit");
            _rigidbody.linearVelocity = Vector2.zero;
            _animator.SetBool(_animatorIsHit, true);
            
            if (collision.GetComponent<NetworkObject>().HasStateAuthority)
            {
                collision.GetComponent<ICharacter>().Damage(BulletPower);
            }
        }
    }
    
    
}
