using System.Threading;
using UnityEngine;

public abstract class AttackerSmallEnemy : Enemy
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _attackSpan;
    private CancellationToken _token;
    
    protected abstract void Attack();
}
