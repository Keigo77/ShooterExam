using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;




public class RobotMan : BossBase, ICharacter
{
    private enum RobotAttack
    {
        NormalAttack,
        ParalysisAttack,
    }
    
    [Serializable] private struct RobotAction
    {
        public RobotAttack attackType;
        public float beforeWaitTime;
    }
    
    [Header("攻撃順序の設定")]
    [SerializeField] private List<RobotAction> _actions;
    
    // 攻撃
    [SerializeField] private GameObject _normalBulletPrefab;
    [SerializeField] private GameObject _paralysisBulletPrefab;
    [SerializeField] private float _bulletSpeed;
    private GameObject _nowBullet;
    public List<NetworkObject> _playerObjects = new List<NetworkObject>();
    
    // アニメーション
    private Animator _animator;
    private int _animatorIsAttack;

    public override async void Spawned()
    {
        _animator = this.GetComponent<Animator>();
        _animatorIsAttack = Animator.StringToHash("IsAttack");
        
        if (!HasStateAuthority)
        {
            return;
        }
        
        // 場にいるプレイヤーPrefab(ジェット機)のオブジェクトを取得する(全プレイヤー分取得するまで実行)
        while (_playerObjects.Count != Runner.SessionInfo.PlayerCount)
        {
            foreach (var player in Runner.ActivePlayers)
            {
                if (Runner.TryGetPlayerObject(player, out var playerObject) && !_playerObjects.Contains(playerObject))
                {
                    _playerObjects.Add(playerObject);
                }
            }
            
            await UniTask.Yield(cancellationToken: _token);
        }
        
        GameManager.Instance.RpcInitializeBossHpGauge(Hp);
        GetToken();
        AttackLoop().Forget();
    }

    private async UniTask AttackLoop()
    {
        while (!_token.IsCancellationRequested)
        {
            foreach (var action in _actions)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(action.beforeWaitTime), cancellationToken: _token);
                switch (action.attackType)
                {
                    case RobotAttack.NormalAttack:
                        _nowBullet = _normalBulletPrefab;
                        await NormalAttack();
                        break;
                    case RobotAttack.ParalysisAttack:
                        _nowBullet = _paralysisBulletPrefab;
                        await ParalysisAttack();
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 通常攻撃．全プレイヤーの高さに移動し，攻撃する．
    /// </summary>
    private async UniTask NormalAttack()
    {
        foreach (var playerObject in _playerObjects)
        {
            if (playerObject != null)
            {
                var playerPos = playerObject.transform.position;
                await MoveAndAttack(playerPos);
            }
        }
    }
    
    /// <summary>
    /// 1人のプレイヤーをランダムで選び，麻痺攻撃をする．
    /// </summary>
    private async UniTask ParalysisAttack()
    {
        int randPlayer = Random.Range(0, _playerObjects.Count);
        if (_playerObjects[randPlayer] != null)
        {
            var playerPos = _playerObjects[randPlayer].transform.position;
            await MoveAndAttack(playerPos);   
        }
    }

    /// <summary>
    /// プレイヤーの高さまで瞬時に移動し，指定された弾を発射する．通常攻撃でも，麻痺攻撃でも使用する．
    /// </summary>
    private async UniTask MoveAndAttack(Vector3 playerPos)
    {
        NetworkDOTween.MyDOMove(this.transform, new Vector2(this.transform.position.x, playerPos.y), 0.3f, _token).Forget();
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: _token);
        
        _animator.SetBool(_animatorIsAttack, true);
        await UniTask.WaitUntil(() => !_animator.GetBool(_animatorIsAttack));
    }

    /// <summary>
    /// 攻撃アニメーションから実行する．弾を発射．
    /// </summary>
    private void Attack()
    {
        if (!HasStateAuthority) { return; }
        Debug.Log("Attack");
        var bulletSpawnPos = new Vector2(this.transform.position.x - 2.0f, this.transform.position.y);
        if (Runner.IsRunning)
        {
            Runner.Spawn(_nowBullet, bulletSpawnPos, onBeforeSpawned: (_, bullet) =>
            {
                bullet.GetComponent<Rigidbody2D>().AddForce(new Vector2(-1, 0).normalized * _bulletSpeed, ForceMode2D.Impulse);
            });
        }
        
        _animator.SetBool(_animatorIsAttack, false);
    }

    public void Damage(float damage)
    {
        GameManager.Instance.RpcDecreaseBossHpGauge(damage);
    }
}
