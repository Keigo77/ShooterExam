using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class AttackerSmallEnemyBase : EnemyBase
{
    [SerializeField] protected GameObject _bulletPrefab;
    [SerializeField] protected float _bulletSpeed;
    [SerializeField] protected float _attackSpan;
    
    protected abstract UniTask AttackLoop();
    protected abstract void Attack();
}
