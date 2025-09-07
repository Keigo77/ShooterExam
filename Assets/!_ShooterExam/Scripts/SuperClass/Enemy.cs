using Fusion;
using UniRx;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private int _enemyId;
    protected NetworkObject _networkObject;
    [Networked] public float Hp { get; set; }
    protected float _bulletPower = 0;
    
    /// <summary>
    /// エクセルのマスタデータから，自身のデータを取得
    /// </summary>
    protected void GetEnemyData()
    {
        Hp = 20;
        _bulletPower = 10;
    }
}
