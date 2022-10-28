using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterPiece : MonoBehaviour
{
    public enum BoosterType
    {
        CHERRY, BANANA, STAR
    }
    public enum BoosterColor
    {
        YELLOW, PURPLE, RED
    }

    // Cherry gives more moves or time depending on level. 
    // Banana doubles score obtained. 
    // Star gives an extra life. 
    public BoosterType _boosterType = BoosterType.CHERRY;
    public BoosterColor _boosterColor = BoosterColor.RED;

    internal void OnBoosterClear(BoosterType type)
    {
        switch (type)
        {
            case BoosterType.CHERRY:
                OnCherryClear(FindObjectOfType<Level>().Type);
                break;
            case BoosterType.BANANA:
                OnBananaClear();
                break;
            case BoosterType.STAR:
                OnStarClear();
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Applies Cherry boost item Effect 
    /// </summary>
    /// <param name="levelType"> Specify the level type to receive level specific boosts. </param>
    internal void OnCherryClear(Level.LevelType levelType)
    {
        switch (levelType)
        {
            case Level.LevelType.TIMER:
                FindObjectOfType<LevelTimer>().AddTime(5);
                break;
            case Level.LevelType.OBSTACLE:
                FindObjectOfType<LevelObstacles>().AddObstacleMoves(2);
                break;
            case Level.LevelType.MOVES:
                FindObjectOfType<LevelMoves>().AddMoves(2);
                break;
            default:
                break;
        }

    }
    internal void OnBananaClear()
    {
        // Change Score Text color to Yellow for the duration of the Effect. 
        // Just add 1000 points to the total score for now. 
    }
    internal void OnStarClear()
    {
        PlayerPrefs.SetInt("Tries Left", PlayerPrefs.GetInt("Tries Left", 3) + 1);
        // update HUD
        FindObjectOfType<HUD>().UpdateLives();
    }
}
