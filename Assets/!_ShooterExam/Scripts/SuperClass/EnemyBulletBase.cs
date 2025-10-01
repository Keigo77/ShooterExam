using Fusion;
using UnityEngine;

public class EnemyBulletBase : NetworkBehaviour
{
    [Networked] public float BulletPower { get; set; }
    [SerializeField] protected float _existTime = 6.0f;
    protected NetworkObject _networkObject;
    protected Rigidbody2D _rigidbody;
}
