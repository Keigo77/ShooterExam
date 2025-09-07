using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class AttackerSmallEnemy : Enemy
{
    [SerializeField] protected GameObject _bulletPrefab;
    [SerializeField] protected float _bulletSpeed;
    [SerializeField] protected float _attackSpan;
    protected CancellationToken _token;
    
    protected abstract UniTask AttackLoop();
    protected abstract void Attack();
}
