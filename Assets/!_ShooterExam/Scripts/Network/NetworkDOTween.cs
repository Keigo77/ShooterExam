using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class NetworkDOTween : MonoBehaviour
{
    public static async UniTask MyDOMove(Transform target, Vector3 endValue, float duration, CancellationToken token)
    {
        // 開始位置と開始時間
        Vector3 startPos = target.position;
        float startTime = Time.time;
        float elapsedTime = 0f;

        // 指定された時間が経過するまでループ
        while (elapsedTime < duration || !token.IsCancellationRequested)
        {
            elapsedTime = Time.time - startTime;
            
            // 経過時間に基づいて0から1までの補間値を計算
            float t = Mathf.Clamp01(elapsedTime / duration);

            // 現在のフレームにおける新しい位置を計算
            if (target != null)
            {
                target.position = Vector3.Lerp(startPos, endValue, t);
            }
            
            
            await UniTask.Yield();
        }
    }


    public static async UniTask MyDORotate(Transform target, Vector3 endValue, float duration, CancellationToken token)
    {
        Quaternion startRot = target.rotation;
        Quaternion endRot = Quaternion.Euler(endValue);
        float startTime = Time.time;
        float elapsedTime = 0f;

        // 指定された時間が経過するまでループ
        while (elapsedTime < duration || !token.IsCancellationRequested)
        {
            elapsedTime = Time.time - startTime;
            
            // 経過時間に基づいて0から1までの補間値を計算
            float t = Mathf.Clamp01(elapsedTime / duration);

            // 現在のフレームにおける新しい回転を計算
            if (target != null)
            {
                target.rotation = Quaternion.Slerp(startRot, endRot, t);
            }
            
            await UniTask.Yield();
        }
    }
}
