using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    AudioSource audioSource;
    public AudioClip backgroundMusic;

    public AudioClip loseSoundEffect;
    public AudioClip winSoundEffect;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void StartAudioBackground()
    {
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void PlayWinSound()
    {
        if (audioSource != null && winSoundEffect != null)
        {
            //audioSource.clip = winSoundEffect;
            //audioSource.loop = true;
            //audioSource.Play();
            audioSource.PlayOneShot(winSoundEffect);
        }
    }

    public void PlayLoseSound()
    {
        if (audioSource != null && loseSoundEffect != null)
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(loseSoundEffect);
        }
    }
    
    public void StopAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }
}
