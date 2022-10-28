using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTimer : Level
{
    public int timeInSeconds;
    public int targetScore;

    private float timer;
    private bool timeOut = false;

    private void Start()
    {
        type = LevelType.TIMER;
        _levelHUD.SetlevelType(type);
        _levelHUD.SetScore(currentScore);
        _levelHUD.SetTarget(targetScore); 
        _levelHUD.SetRemaining(string.Format("{0}: {1:00}", timeInSeconds/60, timeInSeconds % 60));
    }
    private void Update()
    {
        if (!timeOut)
        {
            timer += Time.deltaTime;
            // if the time remaining is negative, it'll return 0. 
            _levelHUD.SetRemaining(string.Format("{0}: {1:00}", (int)Mathf.Max(((timeInSeconds - timer) / 60), 0), (int)Mathf.Max(((timeInSeconds - timer) % 60), 0)));
            if (timeInSeconds - timer <= 0)
            {
                if (currentScore >= targetScore)
                {
                    GameWin();
                }
                else
                {
                    GameLose();
                }
                timeOut = true;
            }
        }
    }
    public void AddTime()
    {
        timeInSeconds = 15;
        // reset timer. 
        timer = 0; 
        timeOut = false;
    }
    /// <summary>
    /// Add level time. 
    /// </summary>
    /// <param name="timeInSeconds"> Time in seconds to be added </param>
    public void AddTime(int timeInSecs)
    {
        timeInSeconds = (int)(timeInSeconds - timer) + timeInSecs;
        // reset timer. 
        timer = 0;
        timeOut = false;
    }
}
