using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    protected readonly Subject<Unit> _onDeath = new Subject<Unit>();
    public IObservable<Unit> OnDeath => _onDeath;
    protected CancellationToken _token;
    
    public Vector2 spawnPos { get; set; }
    [SerializeField] private float _moveInScreenTime;
    
    
    [SerializeField] private int _enemyId;
    [Networked] public float Hp { get; set; }
    protected float _bulletPower = 0;
    
    protected NetworkObject _networkObject;
    
    
    
    /// <summary>
    /// エクセルのマスタデータから，自身のデータを取得
    /// </summary>
    protected void GetEnemyData()
    {
        Hp = 20;
        _bulletPower = 10;
    }

    protected void GetToken()
    {
        _token = this.GetCancellationTokenOnDestroy();
    }

    /// <summary>
    /// 画面外にスポーンしてから，画面内に横移動で登場させる関数
    /// </summary>
    protected async UniTask MoveInScreen()
    {
        NetworkDOTween.MyDOMove(this.transform, spawnPos, _moveInScreenTime, _token).Forget();
        await UniTask.Delay(TimeSpan.FromSeconds(_moveInScreenTime), cancellationToken: _token);
    }
}
