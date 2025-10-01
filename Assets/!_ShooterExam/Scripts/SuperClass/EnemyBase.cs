using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;

public class EnemyBase : NetworkBehaviour
{
    [Networked, UnitySerializeField] protected float Hp { get; set; }
    
    // スポーン処理が終わってから，ダメージを受けるようにするため．
    [Networked] protected NetworkBool IsSpawned { get; set; } = false;
    public Vector2 SpawnPos { get; set; }
    
    // WaveManagerで，敵がデスポーンしたか(倒したか)を取得する．
    public IObservable<Unit> OnDeath => _onDeath;
    private readonly Subject<Unit> _onDeath = new Subject<Unit>();
    [SerializeField] private float _moveInScreenTime;
    protected CancellationToken _token;
    private NetworkObject _networkObject;
    
    // アニメーション
    protected Animator _animator;
    protected int _animatorIsAttack;
    protected int _animatorIsDead;
    [SerializeField] private AudioClip _deathSeClip;
    
    // プレイ人数に応じて，Hpを増やす．
    protected float[] _increaseHpList = { 1.0f, 1.8f, 2.6f, 3.4f };

    private async UniTaskVoid Awake()
    {
        _token = this.GetCancellationTokenOnDestroy();
        _networkObject = this.GetComponent<NetworkObject>();
        _animator = this.GetComponent<Animator>();
        _animatorIsAttack = Animator.StringToHash("IsAttack");
        _animatorIsDead = Animator.StringToHash("IsDead");
        
        await UniTask.WaitUntil(() => IsSpawned, cancellationToken: _token);
    }

    protected void UpdateMaxHp()
    {
        Hp *= _increaseHpList[Runner.SessionInfo.PlayerCount - 1];
        Debug.Log($"敵のHPが{_increaseHpList[Runner.SessionInfo.PlayerCount - 1]}倍で，{Hp}");
    }

    /// <summary>
    /// 画面外にスポーンしてから，画面内に横移動で登場させる関数
    /// </summary>
    protected async UniTask MoveInScreen()
    {
        NetworkDOTween.MyDOMove(this.transform, SpawnPos, _moveInScreenTime, _token).Forget();
        await UniTask.Delay(TimeSpan.FromSeconds(_moveInScreenTime), cancellationToken: _token);
    }

    public void Damage(float damage)
    {
        if (IsSpawned && Hp > 0)
        {
            RpcDamage(damage);
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    protected virtual void RpcDamage(float damage)
    {
        Hp -= damage;
        if (Hp <= 0)
        {
            _animator.SetBool(_animatorIsDead, true);
        }
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
