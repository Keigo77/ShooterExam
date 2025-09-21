using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSingleton : MonoBehaviour
{
    public static AudioSingleton Instance;
    [SerializeField] private AudioClip[] _bgmClips;
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
        SceneManager.sceneLoaded += ChangeBGM;
    }

    /// <summary>
    /// シーン遷移する度に実行し，同じBGMを使うシーンの遷移のみ，BGMを連続させる．
    /// </summary>
    private void ChangeBGM(Scene scene, LoadSceneMode loadMode)
    {
        if (_bgmClips[scene.buildIndex] == null)
        {
            _audioSources[0].Stop();
        }
        else if (_audioSources[0].clip != _bgmClips[scene.buildIndex])
        {
            _audioSources[0].clip = _bgmClips[scene.buildIndex];
            _audioSources[0].Play();
        }
    }

    public void PlayBgm(AudioClip bgmClip)
    {
        _audioSources[0].Stop();
        _audioSources[0].clip = bgmClip;
        _audioSources[0].Play();
    }
    
    public void PlayButtonSe(AudioClip clip)
    {
        _audioSources[1].PlayOneShot(clip);
    }
    
    private void OnDestroy()
    {
        // オブジェクトが破壊された際にイベントの購読を解除
        SceneManager.sceneLoaded -= ChangeBGM;
    }
}
