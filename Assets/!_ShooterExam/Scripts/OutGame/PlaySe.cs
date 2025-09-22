using UnityEngine;

public class PlaySe : MonoBehaviour
{
    public void PlayButtonSe(AudioClip seClip)
    {
        AudioSingleton.Instance.PlaySe(seClip);
    }
}
