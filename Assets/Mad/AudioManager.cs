using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource sfxSourceClick;

    [Header("BGM Clips")]
    public AudioClip mainMenuBGM;
    public AudioClip inGameBGM;

    [Header("Typing SFX")]
    public AudioClip[] typingSFX;
    public AudioClip spacebarSFX;

    [Header("Result SFX")]
    public AudioClip successSFX;
    public AudioClip failSFX;

    [Header("Event SFX")]
    public AudioClip customerArrivesSFX;
    public AudioClip dayAlmostEndedSFX;



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
    public void PlayRandomKeyTypeSFX()
    {
        var clip = typingSFX[Random.Range(0, typingSFX.Length)];
        sfxSourceClick.PlayOneShot(clip);
    }
    public void PlaySpacebarSFX()
    {
        if (spacebarSFX != null)
            sfxSourceClick.PlayOneShot(spacebarSFX);
    }
    public void PlayCustomerArrivesSFX()
    {
        if (customerArrivesSFX != null)
            sfxSource.PlayOneShot(customerArrivesSFX);
    }

    public void PlayDayAlmostEndedSFX()
    {
        if (dayAlmostEndedSFX != null)
            sfxSource.PlayOneShot(dayAlmostEndedSFX);
    }
}
