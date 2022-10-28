using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinueGame : MonoBehaviour
{
    public GameObject _continuePanel;
    public TMPro.TextMeshProUGUI retryHint;
    private int defaultLives = 3;
    private void Start()
    {
        _continuePanel.SetActive(false);
    }
    public void ShowContinuePanel()
    {
        _continuePanel.SetActive(true);
    }
    public void RetrySelected(string LevelType)
    {
        if (LevelType == "Moves")
        {
            // add moves 
            FindObjectOfType<LevelMoves>().numMoves = 3;
            retryHint.SetText("Spend 1 heart to buy +3 extra moves...");
            // notify observers is a good way here (for now we go the long route). 
            FindObjectOfType<LevelMoves>().AddMoves(); 
        }
        else if (LevelType == "Timer")
        {
            // add Time. 
            retryHint.SetText("Spend 1 heart to buy 15 more seconds...");
            FindObjectOfType<LevelTimer>().AddTime();
            // reset game over. 
            // Will use Observer/Singleton Pattern in the future for these. 
        }
        else if (LevelType == "Obstacles")
        {
            // add moves still. 
            retryHint.SetText("Spend 1 heart to buy +5 extra moves...");
            FindObjectOfType<LevelObstacles>().AddObstacleMoves(); 
            // notify observers is a good way here (for now we go the long route). 
        }
        FindObjectOfType<GameGrid>().ResumeGame();
        PlayerPrefs.SetInt("Tries Left", PlayerPrefs.GetInt("Tries Left", defaultLives) - 1);
        FindObjectOfType<HUD>().LivesLeft -= 1;
        FindObjectOfType<HUD>().UpdateLives();
    }
}
