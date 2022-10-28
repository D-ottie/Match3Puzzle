using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public enum LevelType
    {
        TIMER, OBSTACLE, MOVES,
    }

    protected LevelType type;
    protected bool DidWin;
    public int LevelNumber; 
    public LevelType Type
    {
        get { return type; }
    }
    public GameGrid _gameGrid;
    public HUD _levelHUD;

    public int score1Star;
    public int score2Star;
    public int score3Star;

    protected int currentScore;

    private void Start()
    {
        _levelHUD.SetScore(currentScore);
    }
    public virtual void GameWin()
    {
        DidWin = true;
        _gameGrid.GameOver();
        StartCoroutine(WaitForGridFill());
    }
    public virtual void GameLose()
    {
        DidWin = false;
        _gameGrid.GameOver();
        StartCoroutine(WaitForGridFill());
    }
    public virtual void OnMove()
    {
    }
    public virtual void OnPieceCleared(GamePiece piece)
    {
        currentScore += piece.score;
        _levelHUD.SetScore(currentScore);
    }
    protected virtual IEnumerator WaitForGridFill()
    {
        yield return new WaitUntil(() => !_gameGrid.IsFilling);
        if (DidWin)
        {
            _levelHUD.OnGameWin(currentScore);
        }
        else _levelHUD.OnGameLose();
    }
}
