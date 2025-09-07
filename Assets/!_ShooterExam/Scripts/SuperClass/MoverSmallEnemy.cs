using DG.Tweening;
using Fusion;
using UnityEngine;

public class MoverSmallEnemy : Enemy
{
    [SerializeField] private float _speed;
    [SerializeField] private float _moveSpan;
    private Vector2 _minCameraPos;  // カメラの左下のワールド座標
    private Vector2 _maxCameraPos;  // カメラの右上のワールド座標

    /// <summary>
    /// カメラの描写範囲のワールド座標を取得する．敵をランダムに移動させるのに使用する．
    /// // ToDo: スマホ対応させる場合，1番小さいスマホの描写範囲を代入する
    /// </summary>
    public void GetCameraMaxPos()
    {
        if (Camera.main != null)
        {
            _minCameraPos = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
            _maxCameraPos = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        }
        else
        {
            Debug.LogError("Can't find camera");
        }
    }

    /// <summary>
    /// カメラ描写の範囲内かつ，端すぎない・UIに被らない範囲をランダムで移動．(0.5以上，上下するように移動させる)
    /// </summary>
    public void MoveRandomPos()
    {
        Vector2 randMovePos = this.transform.position;

        while (Mathf.Abs(randMovePos.y - this.transform.position.y) >= 0.5f)
        {
            randMovePos = new Vector2(Random.Range(_maxCameraPos.x * 0.66f, _maxCameraPos.x - 1.0f), Random.Range(_minCameraPos.y + 1.0f, _maxCameraPos.y - 2.5f));
        }
        this.transform.DOMove(randMovePos, _speed).SetEase(Ease.Linear).SetSpeedBased();
    }
}
