using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

// NetworkTransformで弾の位置を同期しようとすると，激重になる．そのためのNetworkRigidbody2D(networkRB自体ないと，他プレイヤーの球が飛ばない)．
// 弾をspawnで同期させると，他プレイヤーの球がずれているように見えるため，各プレイヤーが全員の弾を生成するようにする．
// (敵などへの当たり判定は，そのプレイヤーが判定し，他プレイヤーに通知する)

public class BulletBehaviour : MonoBehaviour
{
    public BulletObjectPool BulletObjectPoolScript { private get; set; }
    [SerializeField] private float _existTime;

    /// <summary>
    /// 弾のSetActiveがtrueになるたびに呼ばれる
    /// </summary>
    private void OnEnable()
    {
        Invoke(nameof(ReleaseBullet), _existTime);
    }

    private void ReleaseBullet()
    {
        BulletObjectPoolScript.DeleteBullet(this.gameObject);
    }
}
