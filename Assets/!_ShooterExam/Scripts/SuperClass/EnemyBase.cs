using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;

public class EnemyBase : NetworkBehaviour
{
    [Networked] public float Hp { get; set; }
    [Networked] protected NetworkBool IsSpawned { get; set; } = false;
    public Vector2 SpawnPos { get; set; }
    public IObservable<Unit> OnDeath => _onDeath;
    [SerializeField] private AudioClip _deathSeClip;
    [SerializeField] private float _moveInScreenTime;
    protected CancellationToken _token;
    private readonly Subject<Unit> _onDeath = new Subject<Unit>();
    private NetworkObject _networkObject;
    
    protected Animator _animator;
    protected int _animatorIsAttack;
    protected int _animatorIsDead;

    private void Awake()
    {
        _token = this.GetCancellationTokenOnDestroy();
        _networkObject = this.GetComponent<NetworkObject>();
        _animator = this.GetComponent<Animator>();
        _animatorIsAttack = Animator.StringToHash("IsAttack");
        _animatorIsDead = Animator.StringToHash("IsDead");
    }
    

    /// <summary>
    /// 画面外にスポーンしてから，画面内に横移動で登場させる関数
    /// </summary>
    protected async UniTask MoveInScreen()
    {
        NetworkDOTween.MyDOMove(this.transform, SpawnPos, _moveInScreenTime, _token).Forget();
        await UniTask.Delay(TimeSpan.FromSeconds(_moveInScreenTime), cancellationToken: _token);
    }
    
    /// <summary>
    /// HPが0になったら実行．WaveManagerに死亡を通知し，デスポーンする
    /// </summary>
    private void DespawnEnemy()
    {
        if (HasStateAuthority)
        {
            // 死亡イベントを流す
            _onDeath.OnNext(Unit.Default);
            _onDeath.OnCompleted();
            Runner.Despawn(_networkObject);
        }
    }

    private void PlayDeathSe()
    {
        AudioSingleton.Instance.PlaySe(_deathSeClip);
    }
}
