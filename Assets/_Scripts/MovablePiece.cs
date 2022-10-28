using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GamePiece))]
public class MovablePiece : MonoBehaviour
{
    private GamePiece piece;
    private IEnumerator moveCoroutine; // storing this, means we can stop it when 
    // another move call occurs or something if we desire. 
    private void Awake()
    {
        piece = GetComponent<GamePiece>();
    }
    internal void MovePiece(int newX, int newY, float time )
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = MoveCoroutine(newX, newY, time);
        StartCoroutine(moveCoroutine); 
    }

    private IEnumerator MoveCoroutine(int newX, int newY, float time)
    {
        piece.X = newX;
        piece.Y = newY; 

        Vector3 startPos = transform.position;
        Vector3 endPos = piece.GridRef.GetWorldPosition(newX, newY);
        for (float t = 0; t <= 1 * time; t += Time.deltaTime)
        {
            piece.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            yield return 0; // waits for one frame. 
        }

        piece.transform.position = endPos; 
    }
}
