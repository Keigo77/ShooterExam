using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

public class GiveStatusEffectObj : MonoBehaviour
{
    [SerializeField] private StatusEffect _statusEffect;
    [SerializeField] private float _effectTime;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.GetComponent<NetworkObject>().HasStateAuthority)
        {
            collision.GetComponent<PlayerStatusEffectManager>().AddStatusEffect(_statusEffect, _effectTime).Forget();
        }
    }
}
