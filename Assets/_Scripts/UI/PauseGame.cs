using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public void Pause()
    {
        PlaySFX.Singleton.AUDIO_PlaySelectFX();
        Time.timeScale = 0;
    }
    public void Resume()
    {
        PlaySFX.Singleton.AUDIO_PlaySelectFX();
        Time.timeScale = 1;
    }
}
