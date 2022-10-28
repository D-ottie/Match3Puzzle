using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearablePiece : MonoBehaviour
{
    public AnimationClip clearAnimation;
    public AnimationClip _clearStep;
    private int markStep;
    private bool isBeingCleared = false;
    public bool IsBeingCleared
    {
        get { return isBeingCleared; }
    }

    protected GamePiece piece;

    private void Awake()
    {
        piece = GetComponent<GamePiece>();
        isBeingCleared = false;
    }
    private void Start()
    {
        markStep = 0;
    }
    public bool ClearStep()
    {
        if (markStep == 0)
        {
            markStep++;
            GetComponent<Animator>().Play(_clearStep.name); 
            return false; 
        }
        else
        {
            ClearPiece();
            return true; 
        }
    }
    public virtual void ClearPiece()
    {
        // this will be called when the result is a destruction of a piece. 
        piece.GridRef._level.OnPieceCleared(piece);
        isBeingCleared = true;
        StartCoroutine(ClearCoroutine());
    }
    private IEnumerator ClearCoroutine()
    {
        Animator _animator = GetComponent<Animator>();
        if (_animator)
        {
            _animator.Play(clearAnimation.name);
            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);
        }
    }

}
