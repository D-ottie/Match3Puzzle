using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GamePiece : MonoBehaviour
{
    // private int x
    //public int X
    //{
    //    get { return x; }
    //    set
    //    {
    //        if (IsMovable())
    //            x = value;
    //    }
    //}
    public int X { get; set; }
    public int Y { get; set; }
    // private int y; 
    //public int Y {
    //    get { return y; }
    //    set
    //    {
    //        if (IsMovable())
    //            y = value;
    //    }
    //}


    // cast a Ray on all 4s and get their pieces for now. 
    RaycastHit2D[] hitRight;
    RaycastHit2D[] hitLeft;
    RaycastHit2D[] hitUp;
    RaycastHit2D[] hitDown;
    public GameGrid.PieceType Type { get; set; }

    public MovablePiece MovableComponent { get; set; }
    public ColorPiece ColorComponent { get; set; }
    public ClearablePiece ClearableComponent { get; set; }
    public GameGrid GridRef { get; set; }

    public int score;

    private void Awake()
    {
        ColorComponent = GetComponent<ColorPiece>();
        MovableComponent = GetComponent<MovablePiece>();
        ClearableComponent = GetComponent<ClearablePiece>();
    }

    public void Initialize(int _x, int _y, GameGrid _Grid, GameGrid.PieceType _type)
    {
        X = _x;
        Y = _y;
        GridRef = _Grid;
        Type = _type;
    }

    public bool IsMovable()
    {
        return !(MovableComponent == null);
    }

    public bool IsColored()
    {
        return ColorComponent != null;
    }
    public bool IsClearable()
    {
        return ClearableComponent != null;
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        GridRef.EnterPiece(this);
    }
    private void OnMouseDown()
    {
        // if we are pressing through a UI (we will create a transparent panel to block out touches, then just return). 
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        PieceRayMatch();
        GridRef.PressPiece(this);

    }
    private void OnMouseUp()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        GridRef.ReleasePiece();
    }
    internal bool PieceRayMatch()
    {
        hitRight = Physics2D.RaycastAll(transform.position, Vector2.right);
        //print(hit[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite); 
        hitLeft = Physics2D.RaycastAll(transform.position, Vector2.left);
        hitUp = Physics2D.RaycastAll(transform.position, Vector2.up);
        hitDown = Physics2D.RaycastAll(transform.position, Vector2.down);

        int _collisionLeftHits = hitLeft.Length;
        int _collisionRightHits = hitRight.Length;
        int _collisionUpHits = hitUp.Length;
        int _collisionDownHits = hitDown.Length;

        // one thing we need to get is the length of all hit collisions, or else 
        // we will get IndexOUtOfBounds errors. 

        #region FULL DIAMOND CONDITIONS COVERED
        /* Before checking for Diamond possibility, make sure only one of the direction at most is null */
        // Diamond is a long one. 
        if (_collisionDownHits <= 1 || _collisionUpHits <= 1 || _collisionLeftHits <= 1 || _collisionRightHits <= 1)
        {
            //print("Skipping Diamond");
        }
        else
        {
            //print("Checking Diamond Chance");
            if (CheckDiamondPossibility())
                return true;
        }

        #endregion

        #region Handling Vertical Chances. 
        // to check Swap Down Possibility
        _collisionUpHits = hitUp.Length;
        _collisionDownHits = hitDown.Length;
        if (_collisionUpHits > 1 && _collisionDownHits > 2)
        {
            if (hitUp[1].collider != null && hitDown[2].collider != null &&
              hitDown[1].collider != null
              )
            {
                if (CheckSwapDownPossibility())
                    return true;
            }
        }

        // to check Swap Up Possibility
        if (_collisionDownHits > 1 && _collisionUpHits > 2)
        {
            if (hitDown[1].collider != null && hitUp[2].collider != null &&
            hitUp[1].collider != null
            )
            {
                if (CheckSwapUpPossibility())
                    return true;
            }
        }
        #endregion

        #region Handling Horizontal Chances
        // to check swap Right Possibility
        _collisionLeftHits = hitLeft.Length;
        _collisionRightHits = hitRight.Length;

        if (_collisionLeftHits > 1 && _collisionRightHits > 2)
        {
            if (hitLeft[1].collider != null && hitRight[1].collider != null && hitRight[2].collider != null)
            {
                if (CheckSwapRightPossibility())
                    return true;
            }
        }

        if (_collisionRightHits > 1 && _collisionLeftHits > 2)
        {
            // to check swap Left Possibility. 
            if (hitRight[1].collider != null && hitLeft[1].collider != null && hitLeft[2].collider != null)
            {
                if (CheckSwapLeftPossibility())
                    return true;
            }
        }
        #endregion

        #region Handling L Shaped Chances

        // HORIZONTAL L SWAP CHANCES
        //  L check vertical Right Chance
        if (_collisionLeftHits > 1 && _collisionDownHits > 2)
        {
            // check for L Right Chance. 
            if (Check_LSwapRight())
                return true;
        }
        // L Left Chance
        if (_collisionRightHits > 1 && _collisionUpHits > 2)
        {
            if (Check_LSwapLeft())
                return true;
        }
        // VERTICAL L SWAP CHANCES
        if (_collisionUpHits > 1 && _collisionRightHits > 2)
        {
            // L chance swap down. 
            if (Check_LSwapDown())
                return true;
        }
        if (_collisionDownHits > 1 && _collisionLeftHits > 2)
        {
            // L chance swap up. 
            if (Check_LSwapUp())
                return true;
        }
        #endregion

        #region Handling C Shaped Chances
        // HORIZONTAL C SWAP CHANCES
        // C down swipe chance
        if (_collisionUpHits > 1 && _collisionRightHits > 1 && _collisionLeftHits > 1)
        {
            if (Check_CSwapDown())
                return true;
            // C down swipe chance. 
        }
        // C up swipe chance
        if (_collisionDownHits > 1 && _collisionLeftHits > 1 && _collisionRightHits > 1)
        {
            // C up swipe chance. 
            if (Check_CSwapUp())
                return true;
        }
        if (_collisionRightHits > 1 && _collisionUpHits > 1 && _collisionDownHits > 1)
        {
            // C Left Swipe chance.
            if (Check_CSwapLeft())
                return true;
        }
        if (_collisionLeftHits > 1 && _collisionUpHits > 1 && _collisionDownHits > 1)
        {
            // C Right Swipe chance. 
            if (Check_CSwapRight())
                return true;
        }
        #endregion

        return false;
    }

    // For all Check Swap Methods, basically, if the center piece (Piece casting the rays
    // on all axis) is not movable, then the swap can't be done. 
    private bool Check_CSwapDown()
    {
        //print("Checking C Down Possibility");
        if (hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
            hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite &&
            hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
            hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print("A C swap Down chance");
            return true;
        }
        return false;
    }
    private bool Check_CSwapUp()
    {
        //print("Checking C Up Possibility");
      
        if (hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
           hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite &&
           hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
           hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print("A C swap Up chance");
            return true;
        }
        return false;
    }
    private bool Check_CSwapLeft()
    {
        //print("Checking C Left Possibility");

        if (hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
           hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite &&
           hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
           hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print("A C swap Left chance");
            return true;
        }
        return false;
    }
    private bool Check_CSwapRight()
    {
        //print("Checking C RIght Possibility");

        if (hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
             hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite &&
             hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
             hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print("A C swap Right chance");
            return true;
        }
        return false;
    }
    private bool Check_LSwapRight()
    {
        //print("Checking L Right");
        if (hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
            hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            && hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
            hitDown[2].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print("L swap Right chance");
            return true;
        }
        return false;
    }
    private bool Check_LSwapLeft()
    {
        //print("Checking L Left");

        if (hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
           hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
           && hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
           hitUp[2].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print("L swap Left chance");
            return true;
        }
        return false;
    }
    private bool Check_LSwapDown()
    {
        //print("Checking L Down");

        if (hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
          hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
          && hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
          hitRight[2].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print("L swap Left chance");
            return true;
        }
        return false;
    }
    private bool Check_LSwapUp()
    {
        //print("Checking L Up");

        if (hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
          hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
          && hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
          hitLeft[2].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print("L swap Left chance");
            return true;
        }
        return false;
    }
    private bool CheckSwapDownPossibility()
    {
        //print("Checking Down Possibility");
        if (hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            == hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            && hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            == hitDown[2].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print(" A Vertical Down Possibility");
            return true;
        }
        return false;
    }
    private bool CheckSwapUpPossibility()
    {
        //print("Checking Up Possibility");

        if (hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
           == hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
           && hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
           == hitUp[2].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print("A vertical Up Possibility");
            return true;
        }
        return false;
    }
    private bool CheckSwapRightPossibility()
    {
        //print("Checking Right Possibility");

        if (hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            == hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            && hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            == hitRight[2].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print("A horizontal Swap Right Possibility");
            return true;
        }
        return false;
    }
    private bool CheckSwapLeftPossibility()
    {
        //print("Checking Left Possibility");

        if (hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            == hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            && hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            == hitLeft[2].collider.gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            //print("A horizontal Swap Left Possibility");
            return true;
        }
        return false;
    }
    private bool CheckDiamondPossibility()
    {
        // diamond is long again. 
        if (

            (hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            == hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite &&
            hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
            hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite)

            ||

            (hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            == hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite &&
            hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
            hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite)

            ||

            (hitRight[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            == hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite &&
            hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
            hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite)

            ||

            (hitLeft[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite
            == hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite &&
            hitUp[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite ==
            hitDown[1].collider.gameObject.GetComponent<SpriteRenderer>().sprite)

            )
        {
            //print("A Diamond Possibility");
            return true;
        }
        return false;
    }
}
