using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UniRx;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    protected readonly Subject<Unit> _onDeath = new Subject<Unit>();
    public IObservable<Unit> OnDeath => _onDeath;
    
    [SerializeField] private int _enemyId;
    protected NetworkObject _networkObject;
    [Networked] public float Hp { get; set; }
    protected float _bulletPower = 0;
    // 死亡を通知するためのTaskCompletionSource
    private UniTaskCompletionSource<bool> _deathCompletionSource;
    
    
    /// <summary>
    /// エクセルのマスタデータから，自身のデータを取得
    /// </summary>
    protected void GetEnemyData()
    {
        Hp = 20;
        _bulletPower = 10;
    }
}
