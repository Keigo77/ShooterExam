using UnityEngine;
using UnityEngine.Pool;

public class BulletObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    private ObjectPool<GameObject > _pool;
    
    private void Start()
    {
        _pool = new ObjectPool<GameObject>(
            OnCreatePoolObject,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            false,
            15,
            20);
    }
    
    // プールに弾がない時に，新たに生成
    private GameObject OnCreatePoolObject()
    {
        GameObject bullet = Instantiate(_bulletPrefab);
        bullet.GetComponent<BulletBehaviour>().BulletObjectPoolScript = this;
        return bullet;
    }
    
    // プールに弾があったときの処理
    void OnTakeFromPool(GameObject bullet)
    {
        Debug.Log(bullet.transform.position);
        bullet.SetActive(true);
    }
    
    // プールに返却するときの処理
    void OnReturnedToPool(GameObject bullet)
    {
        bullet.SetActive(false);
    }
    
    // MAXサイズより多くなったときに自動で破棄する
    void OnDestroyPoolObject(GameObject bullet)
    {
        Destroy(bullet);
    }
    
    public GameObject GetBullet()
    {
        GameObject o = _pool.Get();
        return o;
    }
    
    public void DeleteBullet(GameObject bullet)
    {
        _pool.Release(bullet);
    }
}
