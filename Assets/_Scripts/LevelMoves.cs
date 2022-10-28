using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMoves : Level
{
    public int numMoves;
    public int targetScore;
    private bool _outOfMoves; 

    private int movesUsed = 0;

    private void Start()
    {
        type = LevelType.MOVES;
        _levelHUD.SetlevelType(Type);
        _levelHUD.SetScore(currentScore);
        _levelHUD.SetTarget(targetScore);
        _levelHUD.SetRemaining(numMoves); 

        //Debug.Log($"Number of moves: {numMoves} Target score: {targetScore}"); 
    }

    public override void OnMove()
    {
        if (_outOfMoves)
        {
            return;
        }
        movesUsed++;

        _levelHUD.SetRemaining(numMoves - movesUsed); 
        //Debug.Log("Moves remaining: " + (numMoves - movesUsed)); 

        if (numMoves - movesUsed == 0)
        {
            _outOfMoves = true; 
            if (currentScore >= targetScore)
            {
                GameWin(); 
            }else
            {
                GameLose(); 
            }
        }
    }
    public void AddMoves()
    {
        numMoves = 3;
        movesUsed = 0;
        _outOfMoves = false;
        _levelHUD.SetRemaining(numMoves); 
    }
    /// <summary>
    /// Add moves to level
    /// </summary>
    /// <param name="movesToAdd"> The number of moves to be added </param>
    public void AddMoves(int movesToAdd)
    {
        numMoves = (numMoves - movesUsed) + movesToAdd;
        movesUsed = 0;
        _outOfMoves = false;
        _levelHUD.SetRemaining(numMoves);
    }
}
