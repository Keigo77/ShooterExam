using Fusion;
using UnityEngine;

public class EnemyBulletBase : NetworkBehaviour
{
    [Networked] public float BulletPower { get; set; }
    [SerializeField] protected float _existTime = 6.0f;
    [SerializeField] protected NetworkObject _networkObject;
}
