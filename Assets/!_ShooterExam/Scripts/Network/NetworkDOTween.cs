using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class NetworkDOTween : MonoBehaviour
{
    public static async UniTask MyDOMove(Transform target, Vector2 endValue, float duration, CancellationToken token)
    {
        // 開始位置と開始時間
        Vector2 startPos = target.position;
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
                target.position = Vector2.Lerp(startPos, endValue, t);
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
    
    public static async UniTask MyDOFade(SpriteRenderer target, float endValue, float duration, CancellationToken token)
    {
        // 開始アルファ値と開始色
        Color startColor = target.color;
        float startAlpha = startColor.a;
        float startTime = Time.time;
        float elapsedTime = 0f;

        // 指定された時間が経過するまでループ
        while (elapsedTime < duration && !token.IsCancellationRequested)
        {
            elapsedTime = Time.time - startTime;
            
            // 経過時間に基づいて0から1までの補間値を計算
            float t = Mathf.Clamp01(elapsedTime / duration);

            // 現在のフレームにおける新しいアルファ値を計算
            if (target != null)
            {
                // Lerp を使用してアルファ値を補間
                float currentAlpha = Mathf.Lerp(startAlpha, endValue, t);
                
                // 新しいアルファ値を設定した色を作成
                Color newColor = startColor;
                newColor.a = currentAlpha;
                target.color = newColor;
            }
            
            await UniTask.Yield();
        }

        // アニメーション終了後に目標値を正確に設定
        if (target != null)
        {
            Color finalColor = target.color;
            finalColor.a = endValue;
            target.color = finalColor;
        }
    }
}
