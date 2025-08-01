using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
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
            SceneManager.sceneLoaded += OnSceneLoaded;

            Debug.Log("✅ AudioManager Instance Created");

            // Try auto-assign if missing
            if (sfxSourceClick == null)
            {
                Transform clickObj = transform.Find("SFXClick");
                if (clickObj != null)
                {
                    sfxSourceClick = clickObj.GetComponent<AudioSource>();
                    Debug.Log("✅ sfxSourceClick assigned in Awake");
                }
                else
                {
                    Debug.LogWarning("⚠️ sfxSourceClick not found in children");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Auto BGM switch
        switch (scene.name)
        {
            case "MainMenu":
                PlayBGM(mainMenuBGM);
                break;
            case "GameLevel":
                PlayBGM(inGameBGM);
                break;
        }

        // Re-assign sfxSourceClick if missing or inactive
        if (sfxSourceClick == null || !sfxSourceClick.gameObject.activeInHierarchy)
        {
            Debug.Log("🔁 Reassigning sfxSourceClick...");
            var clickObj = transform.Find("SFXClick");
            if (clickObj != null)
            {
                sfxSourceClick = clickObj.GetComponent<AudioSource>();
                if (sfxSourceClick != null)
                    Debug.Log("✅ sfxSourceClick successfully reassigned.");
                else
                    Debug.LogWarning("❌ sfxSourceClick AudioSource not found.");
            }
            else
            {
                Debug.LogWarning("❌ GameObject 'SFXClick' not found under AudioManager.");
            }
        }
    }

    // ------------------- BGM -------------------
    public void PlayBGM(AudioClip clip)
    {
        if (clip != null && bgmSource != null && bgmSource.clip != clip)
        {
            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // ------------------- SFX General -------------------
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("⚠️ SFX clip is null!");
            return;
        }

        if (sfxSource == null || !sfxSource.enabled || !sfxSource.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("❌ SFX Source is not available!");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    // ------------------- SFX Typing -------------------
    public void PlayRandomKeyTypeSFX()
    {
        if (typingSFX.Length > 0 && sfxSourceClick != null)
        {
            var clip = typingSFX[Random.Range(0, typingSFX.Length)];
            sfxSourceClick.PlayOneShot(clip);
        }
    }

    public void PlaySpacebarSFX()
    {
        if (spacebarSFX != null && sfxSourceClick != null)
        {
            sfxSourceClick.PlayOneShot(spacebarSFX);
        }
    }

    // ------------------- SFX Events -------------------
    public void PlayCustomerArrivesSFX()
    {
        if (customerArrivesSFX != null) PlaySFX(customerArrivesSFX);
    }

    public void PlayDayAlmostEndedSFX()
    {
        if (dayAlmostEndedSFX != null) PlaySFX(dayAlmostEndedSFX);
    }

    public void PlaySuccessSFX()
    {
        if (successSFX != null) PlaySFX(successSFX);
    }

    public void PlayFailSFX()
    {
        if (failSFX != null) PlaySFX(failSFX);
    }

}
