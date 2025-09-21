using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSingleton : MonoBehaviour
{
    public static AudioSingleton Instance;
    [SerializeField] private AudioClip[] _bgmClips;
    private AudioSource _bgmAudioSource;
    private AudioSource _seAudioSource;
    
    private int _beforeSceneIndex;
    // 添え字0がBGM用，1がSE用のAudioSource
    private AudioSource[] _audioSources;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        _audioSources = this.GetComponents<AudioSource>();
        _bgmAudioSource = _audioSources[0];
        _seAudioSource = _audioSources[1];
    }

    public void PlayBgm(AudioClip bgmClip)
    {
        if (_bgmAudioSource.clip == bgmClip)
        {
            return;
        }
        
        _bgmAudioSource.Stop();
        _bgmAudioSource.clip = bgmClip;
        _bgmAudioSource.Play();
    }
    
    public void PlayButtonSe(AudioClip clip)
    {
        _seAudioSource.PlayOneShot(clip);
    }
}
