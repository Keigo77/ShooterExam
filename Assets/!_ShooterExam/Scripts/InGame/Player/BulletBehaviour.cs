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
    public float BulletPower {private get; set; }
    [SerializeField] private float _existTime = 6f;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<ICharacter>().Damage(BulletPower);
            Destroy(gameObject);
        }
    }
}
