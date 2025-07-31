using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM Clips")]
    public AudioClip mainMenuBGM;
    public AudioClip inGameBGM;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe ke event scene change
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject); // cegah duplikat
        }
    }

    void OnDestroy()
    {
        // Unsubscribe jika object dihancurkan
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ganti BGM berdasarkan nama scene
        switch (scene.name)
        {
            case "MainMenu":
                PlayBGM(mainMenuBGM);
                break;

            case "GameLevel":
                PlayBGM(inGameBGM);
                break;
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip != null && bgmSource.clip != clip)
        {
            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
