using Fusion;
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
    
    [SerializeField] private Image _playerHealthBar;
    [SerializeField] private Image _bossHealthBar;

    public override void Spawned()
    {
        
    }
    
}
