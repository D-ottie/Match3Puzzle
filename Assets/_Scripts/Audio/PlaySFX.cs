using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFX : MonoBehaviour
{
    private PlaySFX instance;
    public static PlaySFX Singleton;

    public AudioClip _clear;
    public AudioClip _select;

    private AudioSource _audio;
    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        else { Singleton = this; }
        DontDestroyOnLoad(Singleton);
        //PlayerPrefs.DeleteAll(); // for debugging purposes. 
    }
    public void AUDIO_PlayClearFX()
    {
        _audio.PlayOneShot(_clear);
    }
    public void AUDIO_PlaySelectFX()
    {
        _audio.PlayOneShot(_select);
    }
}
