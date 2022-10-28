using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLinePiece : ClearablePiece
{
    public bool isRow;

    public override void ClearPiece()
    {
        base.ClearPiece();
        if (isRow)
        {
            // clear row 
            piece.GridRef.ClearRow(piece.Y); 
        }
        else
        {
            // clear column
            piece.GridRef.ClearColumn(piece.X);
        }
    }
}
