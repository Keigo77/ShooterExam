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
    public List<StatusEffect> PlayerStatusEffects { get; private set; } = new List<StatusEffect>(){StatusEffect.None};
    [SerializeField] private Sprite[] _statusEffectSprites;
    [SerializeField] private SpriteRenderer[] _showStatusEffectsPos;
    private CancellationToken _token;

    public override void Spawned()
    {
        PlayerStatusEffects.Add(StatusEffect.None);
        _token = this.GetCancellationTokenOnDestroy();
    }

    /// <summary>
    /// 敵の弾やアイテムから実行され，プレイヤーに状態異常 or アイテムの効果を付与する．
    /// </summary>
    public async UniTask AddStatusEffect(StatusEffect addStatusEffect, float effectTime)
    {
        PlayerStatusEffects.Add(addStatusEffect);
        int statusEffectIndex = PlayerStatusEffects.Count - 1;
        
        RpcShowStatusEffectIcon(addStatusEffect, statusEffectIndex);
        await UniTask.Delay(TimeSpan.FromSeconds(effectTime), cancellationToken: _token);
        PlayerStatusEffects.Remove(addStatusEffect);
        RpcDeleteStatusEffectIcon(statusEffectIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcShowStatusEffectIcon(StatusEffect addStatusEffect, int index)
    {
        _showStatusEffectsPos[index].sprite = _statusEffectSprites[(int)addStatusEffect];
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcDeleteStatusEffectIcon(int index)
    {
        _showStatusEffectsPos[index].sprite = null;
    }
}
