using Fusion;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    Stopping,
    Playing,
    Finished
}

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    [Networked] public float AllPlayerHP { get; set; }
    [Networked] public float BossHP { get; set; }
    [Networked] public int _numberOfJoinedPlayer { get; set; }
    
    public override void Spawned()
    {
        Debug.Log("すぽーん");
    }
    
    public void AddPlayerHP(float playerHp)
    {
        //AllPlayerHP.Value += playerHp;
        _numberOfJoinedPlayer++;
    }
    
    public void AddBossHP(float bossHp)
    {
        //BossHP.Value += bossHp;
    }

    public void DamagePlayer(float damage)
    {
        //AllPlayerHP.Value -= damage;
    }

    public void DamageBossHP(float damage)
    {
        //BossHP.Value -= damage;
    }
}

