using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Addons.Physics;
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
    [SerializeField] private AudioClip _attackSe;
    private GameObject _nowBullet;
    private List<NetworkObject> _playerObjects = new List<NetworkObject>();
    
    public override async void Spawned()
    {
        _maxBossHp = Hp;
        
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
        
        await MoveInScreen();

        IsSpawned = true;
        GameManager.Instance.RpcInitializeBossHpGauge(Hp);
        AttackLoop().Forget();
    }

    private async UniTask AttackLoop()
    {
        while (!_token.IsCancellationRequested && Hp > 0)
        {
            foreach (var action in _actions)
            {
                if (Hp <= 0)
                {
                    return;
                }
                
                // Hpが最大値の半分以下なら，攻撃速度が倍になる
                if (Hp <= _maxBossHp / 2)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(action.beforeWaitTime / 2), cancellationToken: _token);
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(action.beforeWaitTime), cancellationToken: _token);
                }
                
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
        await UniTask.WaitUntil(() => !_animator.GetBool(_animatorIsAttack), cancellationToken: _token);
    }

    /// <summary>
    /// 攻撃アニメーションから実行する．弾を発射．
    /// </summary>
    private void Attack()
    {
        AudioSingleton.Instance.PlaySe(_attackSe);
        if (!HasStateAuthority) { return; }
        
        var bulletSpawnPos = new Vector2(this.transform.position.x - 2.0f, this.transform.position.y);
        if (Runner.IsRunning)
        {
            Runner.Spawn(_nowBullet, bulletSpawnPos, onBeforeSpawned: (_, bullet) =>
            {
                bullet.GetComponent<NetworkRigidbody2D>().enabled = true;
                bullet.GetComponent<Rigidbody2D>().AddForce(new Vector2(-1, 0).normalized * _bulletSpeed, ForceMode2D.Impulse);
            });
        }
        
        _animator.SetBool(_animatorIsAttack, false);
    }
    
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    protected override void RpcDamage(float damage)
    {
        GameManager.Instance.RpcUpdateBossHpGauge(_maxBossHp, Hp - damage);
        base.RpcDamage(damage);
    }
    
}
