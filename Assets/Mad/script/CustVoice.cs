using UnityEngine;

public class CustVoice : MonoBehaviour
{
    public AudioClip voiceOnArrival;
    public AudioClip voiceOnAngry;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayArrivalVoice()
    {
        if (voiceOnArrival != null)
            audioSource.PlayOneShot(voiceOnArrival);
    }

    public void PlayAngryVoice()
    {
        if (voiceOnAngry != null)
            audioSource.volume = 0.3f;
            audioSource.PlayOneShot(voiceOnAngry);
    }
}
