using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UniRx;

public enum StatusEffect
{
    None = 0,
    Invincible = 1,     // 無敵 
    Paralysis = 2,      // 麻痺
    Ghost = 3           // 聖水を取ったかどうか
}

public class PlayerStatusEffectManager : NetworkBehaviour
{
    [Networked] public StatusEffect PlayerStatusEffects { get; private set; } = StatusEffect.None;
    [SerializeField] private Sprite[] _statusEffectSprites;
    [SerializeField] private SpriteRenderer _showStatusEffectsPos;
    private CancellationTokenSource cts; 
    private CancellationToken _token;

    public override void Spawned()
    {
        cts  = new CancellationTokenSource();
        _token = cts.Token;
    }

    /// <summary>
    /// 敵の弾やアイテムから実行され，プレイヤーに状態異常 or アイテムの効果を付与する．
    /// </summary>
    public async UniTask AddStatusEffect(StatusEffect addStatusEffect, float effectTime)
    {
        if (PlayerStatusEffects == addStatusEffect)
        {
            cts.Cancel();
        }
        PlayerStatusEffects = addStatusEffect;
        
        RpcShowStatusEffectIcon(addStatusEffect);
        await UniTask.Delay(TimeSpan.FromSeconds(effectTime * 0.7f), cancellationToken: _token);
        // 効果時間の7割が経過したら，アイコンを点滅させてから消去
        RpcDeleteStatusEffectIcon(addStatusEffect, effectTime);
    }

    /// <summary>
    /// 付与された状態のアイコンを表示する
    /// </summary>

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcShowStatusEffectIcon(StatusEffect addStatusEffect)
    {
        _showStatusEffectsPos.sprite = _statusEffectSprites[(int)addStatusEffect];
    }

    /// <summary>
    /// 状態異常の効果が切れる前に，状態異常アイコンを5回点滅させる．
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private async void RpcDeleteStatusEffectIcon(StatusEffect statusEffect, float effectTime)
    {
        int flashingIconCount = 0;
        while (!_token.IsCancellationRequested && flashingIconCount < 5)
        {
            NetworkDOTween.MyDOFade(_showStatusEffectsPos, 0.0f, effectTime * 0.03f, _token).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(effectTime * 0.03f), cancellationToken: _token);
            NetworkDOTween.MyDOFade(_showStatusEffectsPos, 1.0f, effectTime * 0.03f, _token).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(effectTime * 0.03f), cancellationToken: _token);
            flashingIconCount++;
        }

        if (_token.IsCancellationRequested)
        {
            return;
        }
        
        PlayerStatusEffects = StatusEffect.None;
        _showStatusEffectsPos.sprite = null;
    }
    
}
