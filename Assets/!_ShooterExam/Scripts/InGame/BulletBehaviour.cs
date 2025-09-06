using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

// NetworkTransformで弾の位置を同期しようとすると，激重になる．そのためのNetworkRigidbody2D(networkRB自体ないと，他プレイヤーの球が飛ばない)．

public class BulletBehaviour : NetworkBehaviour
{
    private NetworkObject _networkObject;
    
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            _networkObject = GetComponent<NetworkObject>();
            Invoke(nameof(ReleaseBullet), 3.0f);
        }
    }

    private void ReleaseBullet()
    {
        Runner.Despawn(_networkObject);
    }
}
