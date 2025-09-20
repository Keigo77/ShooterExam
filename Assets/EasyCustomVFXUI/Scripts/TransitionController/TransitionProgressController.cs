using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Controlling parameters of material for transition image
/// </summary>


[RequireComponent(typeof(RawImage))]
[ExecuteAlways]
public class TransitionProgressController : MonoBehaviour
{
    [Range(0f, 1f)] public float Progress = 0f;
    [SerializeField] private float _transitionDuration = 1.5f;

    private RawImage rawImage;
    private Material material;

    void OnEnable()
    {
        rawImage = GetComponent<RawImage>();
        material = rawImage.material;
        SetProgressProperty();
    }

    void Update()
    {
        SetProgressProperty();
    }

    private void SetProgressProperty()
    {
        if (material.HasProperty("_Progress"))
        {
            material.SetFloat("_Progress", Progress);
        }
        else
        {
            Debug.LogWarning("The material does not have a 'Progress' property.");
        }
    }

    public async UniTask FadeOut()
    {
        await DOTween.To(() => Progress, x => Progress = x,
                0f, _transitionDuration)
            .OnUpdate(SetProgressProperty);
    }
    
    public async UniTask FadeIn()
    {
        await DOTween.To(() => Progress, x => Progress = x,
                1f, _transitionDuration)
            .OnUpdate(SetProgressProperty);
    }
    
    /// <summary> For Preventing memory leak </summary>  
    private void OnDestroy()
    {
        if (!material)
        {
            Destroy(material);
            material = null;
        }
    }
}
