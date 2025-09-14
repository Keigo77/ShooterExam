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
    public ReactiveProperty<StatusEffect> PlayerStatusEffect { private get; set; } = new ReactiveProperty<StatusEffect>(StatusEffect.None);

    public override void Spawned()
    {
        
    }
}
