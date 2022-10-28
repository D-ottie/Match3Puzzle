using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGrid : MonoBehaviour
{
    public enum PieceType
    {
        EMPTY,
        NORMAL,
        BOX,
        ICEBLOCK,
        ROW_CLEAR,
        COLUMN_CLEAR,
        RAINBOW,
        CHERRY,
        BANANA,
        STAR,
        COUNT,
    };

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    }

    [System.Serializable]
    public struct PiecePosition
    {
        public PieceType type;
        public int x;
        public int y;
    }
    public PiecePosition[] initialPieces;

    public int xDim;
    public int yDim;
    public float fillTime;

    public GameObject backgroundTilePrefab;
    public PiecePrefab[] piecePrefabs;
    public GameObject[] specialPiecePrefabs; // 0 - Banana, 1 - Cherry, 2 - Star. 


    // dictionaries can't be loaded on the inspector, so we load a structure that will get the KvP we need. 
    private Dictionary<PieceType, GameObject> piecePrefabDictionary;

    private GamePiece[,] pieces;

    private bool inverse = false;

    // handling reshuffling. 
    private bool _isMatchAvailable;
    //private bool _isChecking; 

    // Handling Matching at Start issues with these variables. 
    Sprite LeftSprite = null;
    List<Sprite> oldList = new List<Sprite>();
    private List<Sprite> newList = new List<Sprite>();
    private bool firstFill = false;

    public Sprite toInitializeNewList;

    private GamePiece pressedPiece;
    private GamePiece enteredPiece;

    private bool gameOver = false;

    public Level _level;

    public bool IsFilling { get; set; }
    void Awake()
    {
        firstFill = true;
        // then we use this start menu to populate our dictionary with what was passed through the inspector to the struture array. 
        piecePrefabDictionary = new Dictionary<PieceType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!piecePrefabDictionary.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDictionary.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject background = Instantiate(backgroundTilePrefab,
                    GetWorldPosition(x, y), Quaternion.identity, transform);
            }
        }

        pieces = new GamePiece[xDim, yDim];

        // this is where initial pieces was formally at. 

        // the rest is filled with empty pieces, before calling the FIll method to now fill actual tiles. 
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                #region Previous Method for future Ref
                /* GameObject newPiece = Instantiate(piecePrefabDictionary[PieceType.NORMAL],
                    Vector3.zero, Quaternion.identity, transform);
                newPiece.name = "Piece(" + x + "," + y + ")";
                //pieces[x, y].transform.parent = transform; 
                pieces[x, y] = newPiece.GetComponent<GamePiece>();
                pieces[x, y].Initialize(x, y, this, PieceType.NORMAL); 
                if (pieces[x, y].IsMovable())
                {
                    //x, y is where the 2-d array is at, which makes coordinates auto. 
                    pieces[x, y].MovableComponent.MovePiece(x, y); 
                }
                if (pieces[x, y].IsColored())
                {
                    pieces[x, y].ColorComponent.SetColor( 
                        // here we are casting an integer as an enum, because ofcos enums can 
                        // be converted as integers. 
                        (ColorPiece.ColorType)Random.Range(0, pieces[x, y].ColorComponent.NumColors)
                        );
                    pieces[x, y].GetComponent<SpriteRenderer>().sprite = pieces[x, y].ColorComponent.GetReferencedColorSprite(); 
                    // we could also pass a sprite variable by reference, and then change the value in ColorPiece function. 
                    // after, we use the sprite which have been changed by reference and use here. 
                    // or we could use the multiple function calls we did above. 
                } */
                #endregion
                if (pieces[x, y] == null)
                {
                    var piece = SpawnNewPiece(x, y, PieceType.EMPTY);
                }
            }
            // initialize an Empty list. 
            newList.Add(toInitializeNewList);
        }
        StartCoroutine(Fill());
    }
    public IEnumerator Fill()
    {
        bool needsRefill = true;
        IsFilling = true;
        while (needsRefill)
        {
            while (FillStep())
            {
                inverse = !inverse;
                yield return new WaitForSeconds(fillTime);
            }
            needsRefill = ClearAllValidMatches();
        }
        // initial pieces are for pieces like boxes and ice blocks that will first appear on the board before the actual tiles. 
        // we should do this after our pieces have filled the board so we can respect our firstFill mechanic. 
        if (firstFill)
        {
            List<GamePiece> tempList = new List<GamePiece>();

            for (int i = 0; i < initialPieces.Length; i++)
            {
                // Make sure to destroy the existing piece on this position. 
                // clear existing piece on that area before replacing. 
                if (initialPieces[i].x >= 0 && initialPieces[i].x < xDim &&
                    initialPieces[i].y >= 0 && initialPieces[i].y < yDim)
                {
                    //print(pieces[initialPieces[i].x, initialPieces[i].y]); // this is used to get the pieces
                    //before they are replaced
                    tempList.Add(pieces[initialPieces[i].x, initialPieces[i].y]);
                    SpawnNewPiece(initialPieces[i].x, initialPieces[i].y, initialPieces[i].type);
                }
            }
            foreach (var item in tempList)
            {
                item.gameObject.SetActive(false);
            }
            // now if Level is obstacles, call the method to update obstacles left. 
            if (_level.Type == Level.LevelType.OBSTACLE)
            {
                FindObjectOfType<LevelObstacles>().InitializeObstaclesLeft();
            }

        }
        yield return new WaitForSeconds(fillTime);
        yield return new WaitUntil(() => !needsRefill);
        firstFill = false;
        IsFilling = false;
    }
    public bool FillStep()
    {
        bool movedPiece = false;
        for (int y = yDim - 2; y >= 0; y--)
        {
            for (int loopX = 0; loopX < xDim; loopX++)
            {
                int x = loopX;
                if (inverse)
                {
                    x = xDim - 1 - loopX;
                }
                GamePiece piece = pieces[x, y];
                if (piece.IsMovable())
                {
                    GamePiece pieceBelow = pieces[x, y + 1];
                    if (pieceBelow.Type == PieceType.EMPTY)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.MovableComponent.MovePiece(x, y + 1, fillTime);
                        pieces[x, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                    }
                    // this is for the cause the piece below is an obstacle. 
                    else
                    {
                        for (int diag = -1; diag <= 1; diag++)
                        {
                            if (diag != 0)
                            {
                                int diagX = x + diag;
                                if (inverse)
                                {
                                    diagX = x - diag;
                                }
                                if (diagX >= 0 && diagX < xDim)
                                {
                                    GamePiece diagonalPiece = pieces[diagX, y + 1];
                                    if (diagonalPiece.Type == PieceType.EMPTY)
                                    {
                                        bool hasPieceAbove = true;
                                        for (int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            GamePiece pieceAbove = pieces[diagX, aboveY];
                                            if (pieceAbove.IsMovable())
                                            {
                                                break;
                                            }
                                            else if (!pieceAbove.IsMovable() && pieceAbove.Type != PieceType.EMPTY)
                                            {
                                                hasPieceAbove = false;
                                                break;
                                            }
                                        }

                                        if (!hasPieceAbove)
                                        {
                                            Destroy(diagonalPiece.gameObject);
                                            piece.MovableComponent.MovePiece(diagX, y + 1, fillTime);
                                            pieces[diagX, y + 1] = piece;
                                            SpawnNewPiece(x, y, PieceType.EMPTY);
                                            movedPiece = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    } // diagonal movement n stuff. 
                }
            }
        }

        if (firstFill)
        {
            // for the top row. This is where our primary interest is at if we intend to fix the matching as game begins bug. 
            for (int x = 0; x < xDim; x++)
            {
                GamePiece pieceBelow = pieces[x, 0];
                if (pieceBelow.Type == PieceType.EMPTY)
                {
                    Destroy(pieceBelow.gameObject);
                    GameObject newPiece = Instantiate(piecePrefabDictionary[PieceType.NORMAL],
                        GetWorldPosition(x, -1), Quaternion.identity, transform);

                    pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                    pieces[x, 0].Initialize(x, -1, this, PieceType.NORMAL);
                    pieces[x, 0].MovableComponent.MovePiece(x, 0, fillTime);

                    // all of this checking should occur only at the start. 
                    #region Making Sure Left tiles Don't repeat themselves 
                    do
                    {
                        // also we have to store an old list of sprites, to compare when generating the ones above, making sure they
                        // don't repeat as well. 
                        pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.NumColors));
                        pieces[x, 0].GetComponent<SpriteRenderer>().sprite = pieces[x, 0].ColorComponent.GetReferencedColorSprite();
                    } while (LeftSprite == pieces[x, 0].GetComponent<SpriteRenderer>().sprite
                    || pieces[x, 0].GetComponent<SpriteRenderer>().sprite == newList[x]);
                    LeftSprite = pieces[x, 0].GetComponent<SpriteRenderer>().sprite;
                    oldList.Add(LeftSprite);  // inserting at the index of x, so basically this list will never go beyond xDim. 
                    #endregion
                    movedPiece = true;
                }
            }
            newList.Clear();
            newList.AddRange(oldList);
            //print(newList.Count);
            oldList.Clear();
        }
        else
        {
            for (int x = 0; x < xDim; x++)
            {
                GamePiece pieceBelow = pieces[x, 0];
                if (pieceBelow.Type == PieceType.EMPTY)
                {
                    Destroy(pieceBelow.gameObject);
                    GameObject newPiece = Instantiate(piecePrefabDictionary[PieceType.NORMAL],
                        GetWorldPosition(x, -1), Quaternion.identity, transform);
                    pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                    pieces[x, 0].Initialize(x, -1, this, PieceType.NORMAL);
                    pieces[x, 0].MovableComponent.MovePiece(x, 0, fillTime);
                    pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.NumColors));
                    pieces[x, 0].GetComponent<SpriteRenderer>().sprite = pieces[x, 0].ColorComponent.GetReferencedColorSprite();
                    movedPiece = true;
                }
            }
        }
        // after calling everypiece, store the Left and Up sprites. These will be removed from the next list from which to choose 
        // from, for the next random Color Piece selection. And our auto match issue will be fixed. 
        return movedPiece;
    }
    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - xDim / 2.0f + x, transform.position.y + yDim / 2.0f - y);
    }
    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = Instantiate(piecePrefabDictionary[type], GetWorldPosition(x, y), Quaternion.identity, transform);
        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Initialize(x, y, this, type);

        return pieces[x, y];
    }
    public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
        return (piece1.X == piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1
            || (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1));
    }
    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if (gameOver)
        {
            return;
        }
        if (piece1.IsMovable() && piece2.IsMovable())
        {
            IsFilling = true; // the moment a match starts,the board is filling, otherwise some actions will occur before it stops filling. 
            pieces[piece1.X, piece1.Y] = piece2;
            pieces[piece2.X, piece2.Y] = piece1;

            //we only swap when there's a match. 
            if (GetMatch(piece1, piece2.X, piece2.Y) != null || GetMatch(piece2, piece1.X, piece1.Y) != null
                 || piece1.Type == PieceType.RAINBOW || piece2.Type == PieceType.RAINBOW)
            {
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;

                Reshuffle.Singleton.InCombo = true;
                Reshuffle.Singleton.ComboCounter += 1;
                if (Reshuffle.Singleton.ComboCounter > 5)
                {
                    SpawnSpecialPiece();
                }
                // you can only move if you get a match. 
                // so this is a good place to set our Combo status. 

                piece1.MovableComponent.MovePiece(piece2.X, piece2.Y, fillTime);
                piece2.MovableComponent.MovePiece(piece1X, piece1Y, fillTime);

                if (piece1.Type == PieceType.RAINBOW && piece1.IsClearable() && piece2.IsColored())
                {
                    ClearColorPiece clearColor = piece1.GetComponent<ClearColorPiece>();
                    if (clearColor)
                    {
                        clearColor.Color = piece2.ColorComponent.Color;
                    }
                    ClearPiece(piece1.X, piece1.Y);
                }

                if (piece2.Type == PieceType.RAINBOW && piece2.IsClearable() && piece2.IsColored())
                {
                    ClearColorPiece clearColor = piece1.GetComponent<ClearColorPiece>();
                    if (clearColor)
                    {
                        clearColor.Color = piece2.ColorComponent.Color;
                    }
                    ClearPiece(piece1.X, piece1.Y);
                }

                ClearAllValidMatches();
                if (piece1.Type == PieceType.ROW_CLEAR || piece1.Type == PieceType.COLUMN_CLEAR)
                {
                    ClearPiece(piece1.X, piece1.Y);
                }

                if (piece2.Type == PieceType.ROW_CLEAR || piece2.Type == PieceType.COLUMN_CLEAR)
                {
                    ClearPiece(piece2.X, piece2.Y);
                }
                pressedPiece = null;
                enteredPiece = null;

                StartCoroutine(Fill());
                _level.OnMove();

            }
            else
            {
                pieces[piece1.X, piece1.Y] = piece1;
                pieces[piece2.X, piece2.Y] = piece2;
            }
        }
    }
    private void SpawnSpecialPiece()
    {
        var odds = Random.Range(0, 10);
        int randomX = Random.Range(0, xDim);
        int randomY = Random.Range(0, yDim);

        // keep doing this until you pick up a non obstacle piece. 
        while (pieces[randomX, randomY].gameObject.CompareTag("Obstacle"))
        {
            randomX = Random.Range(0, xDim);
            randomY = Random.Range(0, yDim);
        }
        //pieces[randomX, randomY].gameObject.SetActive(false);
        //SpawnNewPiece(randomX, randomY, PieceType.CHERRY);
        if (odds == 1) // so there's only a .1 chance of spawning star
        {
            pieces[randomX, randomY].gameObject.SetActive(false);
            SpawnNewPiece(randomX, randomY, PieceType.STAR);
        }
        else if (odds < 3) // there's a .3 chance of spawning cherry
        {
            pieces[randomX, randomY].gameObject.SetActive(false);
            SpawnNewPiece(randomX, randomY, PieceType.CHERRY);
        }
        else
        {
            pieces[randomX, randomY].gameObject.SetActive(false);
            SpawnNewPiece(randomX, randomY, PieceType.BANANA);
        }
        Reshuffle.Singleton.ComboCounter = 0;
    }

    internal void KickOfCheck()
    {
        Reshuffle.Singleton.currentTime = 0;
        StartCoroutine(CheckForAvailableMatch());
    }
    // The best instance to check this, is if after some time the user 
    // haven't caused the clear method to be called. Maybe after 10 seconds. 
    IEnumerator CheckForAvailableMatch()
    {
        // Take care of the case it's still saying there's a match because it thinks obstacles can be moved too. 
        // our grid is filling like a table, top down, not from the ground up unlike 
        // we expect. 
        //print(pieces[1, 1].ColorComponent.GetReferencedColorSprite());

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                _isMatchAvailable = pieces[x, y].PieceRayMatch();
                if (_isMatchAvailable)
                {
                    //print("Match Found, Exiting Routine with nothing to show for it");
                    Reshuffle.Singleton._alreadyChecked = true;// we have checked the existing grid
                                                               //_isChecking = false;
                    yield break; // end the routine. 
                }
            }
        }

        ClearAllNonObstaclePieces();
        Reshuffle.Singleton._alreadyChecked = false;
        yield return null;
    }
    private void ClearAllNonObstaclePieces()
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                // clear all Normal pieces and maybe special pieces too. 
                if (pieces[x, y].Type == PieceType.NORMAL)
                {
                    pieces[x, y].ClearableComponent.ClearPiece();
                    // spawn an empty piece in it's place. 
                    SpawnNewPiece(x, y, PieceType.EMPTY); // apparently we need to spawn new empty pieces or something along the entire board. 
                }
            }
        }
        StartCoroutine(Fill()); // fill the board once more. 
    }
    // one line function. 
    public void PressPiece(GamePiece piece) => pressedPiece = piece;
    public void EnterPiece(GamePiece piece) => enteredPiece = piece;
    public void ReleasePiece()
    {
        if (IsAdjacent(pressedPiece, enteredPiece))
        {
            SwapPieces(pressedPiece, enteredPiece);
        }
    }
    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (piece.IsColored())
        {
            ColorPiece.ColorType color = piece.ColorComponent.Color;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // If the future we face matching and swapping problems with regards to that 
            // check the codes here. 
            #region Checking Horizontal
            // first check horizontal. 
            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;
                    if (dir == 0)
                    {
                        // Going left
                        x = newX - xOffset;
                    }
                    else
                    {
                        // Right
                        x = newX + xOffset;
                    }
                    if (x < 0 || x >= xDim)
                    {
                        // if x goes outside of bounds, we break out of the loop. 
                        break;
                    }
                    if (pieces[x, newY].IsColored() && pieces[x, newY].ColorComponent.Color == color)
                    {
                        horizontalPieces.Add(pieces[x, newY]);
                    }
                    else
                    // if you encounter a booster with matching colors, add too. 
                    if (pieces[x, newY].GetComponent<BoosterPiece>() != null &&
                        pieces[x, newY].GetComponent<BoosterPiece>()._boosterColor.ToString() == color.ToString())
                    {
                        horizontalPieces.Add(pieces[x, newY]);
                    }
                    else
                    {
                        break; // stop traversing in this direction. 
                    }
                }
            }

            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }
            // if we find a horizontal match for 3, we need to check for vertical in the same vein. 
            // that is traversing vertically to look for an L or T match.
            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int yOffset = 1; yOffset < yDim; yOffset++)
                        {
                            int y;
                            if (dir == 0)
                            {
                                // traverse up. 
                                y = newY - yOffset;
                            }
                            else
                            {
                                y = newY + yOffset;
                            }

                            if (y < 0 || y >= yDim)
                            {
                                break; // outside dimensions. 
                            }
                            if (pieces[horizontalPieces[i].X, y].IsColored() && pieces[horizontalPieces[i].X, y].ColorComponent.Color == color)
                            {
                                verticalPieces.Add(pieces[horizontalPieces[i].X, y]);
                            }
                            else
                            // add any boosters with matching colors. 
                            if (pieces[horizontalPieces[i].X, y].GetComponent<BoosterPiece>() != null &&
                       pieces[horizontalPieces[i].X, y].GetComponent<BoosterPiece>()._boosterColor.ToString() == color.ToString())
                            {
                                verticalPieces.Add(pieces[horizontalPieces[i].X, y]);
                            }
                            else { break; }
                        }
                    }
                    if (verticalPieces.Count < 2)
                    {
                        verticalPieces.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < verticalPieces.Count; j++)
                        {
                            matchingPieces.Add(verticalPieces[j]);
                        }
                        break;
                    }
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
            #endregion
            #region Vertical
            // Didn't find anything Horizontal, check vertical. 
            // first clear the vertical and horizontal arrays to get ready to start again. 
            horizontalPieces.Clear();
            verticalPieces.Clear();
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;
                    if (dir == 0)
                    {
                        // Traversing Up
                        y = newY - yOffset;
                    }
                    else
                    {
                        // Traversing down
                        y = newY + yOffset;
                    }
                    if (y < 0 || y >= yDim)
                    {
                        break;
                    }
                    if (pieces[newX, y].IsColored() && pieces[newX, y].ColorComponent.Color == color)
                    {
                        verticalPieces.Add(pieces[newX, y]);
                    }
                    else
                    // add any boosters with matching colors. 
                    if (pieces[newX, y].GetComponent<BoosterPiece>() != null &&
               pieces[newX, y].GetComponent<BoosterPiece>()._boosterColor.ToString() == color.ToString())
                    {
                        verticalPieces.Add(pieces[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }
            // if we find a verticle match for 3, we need to check for vertical in the same vein. 
            // that is traversing horizontally to look for an L or T match.
            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int xOffset = 1; xOffset < xDim; xOffset++)
                        {
                            int x;
                            if (dir == 0)
                            {
                                // traverse left. 
                                x = newX - xOffset;
                            }
                            else
                            {// traverse right
                                x = newX + xOffset;
                            }

                            if (x < 0 || x >= xDim)
                            {
                                break; // outside dimensions. 
                            }
                            if (pieces[x, verticalPieces[i].Y].IsColored() && pieces[x, verticalPieces[i].Y].ColorComponent.Color == color)
                            {
                                //verticalPieces.Add(pieces[x, verticalPieces[i].Y]);
                                horizontalPieces.Add(pieces[x, verticalPieces[i].Y]);
                            }
                            else
                            if (pieces[x, verticalPieces[i].Y].GetComponent<BoosterPiece>() != null &&
                             pieces[x, verticalPieces[i].Y].GetComponent<BoosterPiece>()._boosterColor.ToString() == color.ToString())
                            {
                                horizontalPieces.Add(pieces[x, verticalPieces[i].Y]);
                            }
                            else { break; }
                        }
                    }
                    if (horizontalPieces.Count < 2)
                    {
                        horizontalPieces.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < horizontalPieces.Count; j++)
                        {
                            matchingPieces.Add(horizontalPieces[j]);
                        }
                        break;
                    }
                }
            }
            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
        }
        #endregion
        return null;
    }
    public bool ClearAllValidMatches()
    {
        // checking full board after a Clear for any resulting matches that can were made. 
        // we can use this similar idea to make sure our board is randomly shuffled if they're not matches left. 
        bool needsRefill = false;
        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                if (pieces[x, y].IsClearable())
                {
                    List<GamePiece> match = GetMatch(pieces[x, y], x, y);
                    if (match != null)
                    {
                        PieceType specialPieceType = PieceType.COUNT;
                        GamePiece randomPiece = match[Random.Range(0, match.Count)];
                        int specialPieceX = randomPiece.X;
                        int specialPieceY = randomPiece.Y;

                        if (match.Count == 4)
                        {
                            if (pressedPiece == null || enteredPiece == null)
                            {
                                specialPieceType = (PieceType)Random.Range((int)PieceType.ROW_CLEAR, (int)PieceType.COLUMN_CLEAR);
                            }
                            else if (pressedPiece.Y == enteredPiece.Y)
                            {
                                specialPieceType = PieceType.ROW_CLEAR;
                            }
                            else
                            {
                                specialPieceType = PieceType.COLUMN_CLEAR;
                            }
                        }
                        // we use greater than or equal because clearing 6 or more tiles didn't give anything. 
                        else if (match.Count >= 5)
                        {
                            specialPieceType = PieceType.RAINBOW;
                        }

                        for (int i = 0; i < match.Count; i++)
                        {
                            if (ClearPiece(match[i].X, match[i].Y))
                            {
                                needsRefill = true;

                                if (match[i] == pressedPiece || match[i] == enteredPiece)
                                {
                                    specialPieceX = match[i].X;
                                    specialPieceY = match[i].Y;
                                }
                            }
                        }
                        if (specialPieceType != PieceType.COUNT)
                        {
                            Destroy(pieces[specialPieceX, specialPieceY]);
                            GamePiece newPiece = SpawnNewPiece(specialPieceX, specialPieceY, specialPieceType);

                            if ((specialPieceType == PieceType.ROW_CLEAR || specialPieceType == PieceType.COLUMN_CLEAR)
                                && newPiece.IsColored() && match[0].IsColored())
                            {
                                newPiece.ColorComponent.SetColor(match[0].ColorComponent.Color);
                                newPiece.GetComponent<SpriteRenderer>().sprite = newPiece.ColorComponent.GetReferencedColorSprite();
                            }
                            else if (specialPieceType == PieceType.RAINBOW && newPiece.IsColored())
                            {
                                newPiece.ColorComponent.SetColor(ColorPiece.ColorType.ANY);
                            }
                        }
                    }
                }
            }
        }
        return needsRefill;
    }
    public bool ClearPiece(int x, int y) // takes position on the grid to clear. 
    {
        if (pieces[x, y].IsClearable() && !pieces[x, y].ClearableComponent.IsBeingCleared)
        {
            if (pieces[x, y].GetComponent<BoosterPiece>() != null)
            {
                pieces[x, y].GetComponent<BoosterPiece>().OnBoosterClear(pieces[x, y].
                    GetComponent<BoosterPiece>().
                    _boosterType);
            }
            pieces[x, y].ClearableComponent.ClearPiece(); // clear the piece
            SpawnNewPiece(x, y, PieceType.EMPTY);

            ClearObstacles(x, y);
            PlaySFX.Singleton.AUDIO_PlayClearFX(); // play clear fx. 
            // if a clear is made, reset reshuffle Time and also we haven't checked the new grid;
            Reshuffle.Singleton.currentTime = 0;
            Reshuffle.Singleton._alreadyChecked = false;
            //print(Reshuffle.Singleton.currentTime);
            return true;
        }
        return false;
    }

    // we have to pass the type of obstacle that's being destroyed, 
    // which this function will use to conjure it's operations. 
    // Events is the proper way to handle this situations, but for the purpose of this lesson
    public void ClearObstacles(int x, int y)
    {
        for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++)
        {
            if (adjacentX != x && adjacentX >= 0 && adjacentX < xDim)
            {
                if (pieces[adjacentX, y].Type == PieceType.BOX && pieces[adjacentX, y].IsClearable())
                {
                    pieces[adjacentX, y].ClearableComponent.ClearPiece();
                    SpawnNewPiece(adjacentX, y, PieceType.EMPTY);
                }
                else if (pieces[adjacentX, y].Type == PieceType.ICEBLOCK && pieces[adjacentX, y].IsClearable())
                {
                    bool spawnNew = pieces[adjacentX, y].ClearableComponent.ClearStep();
                    if (spawnNew)
                    {
                        SpawnNewPiece(adjacentX, y, PieceType.EMPTY);
                    }
                }
            }
        }
        for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
        {
            if (adjacentY != y && adjacentY >= 0 && adjacentY < yDim)
            {
                if (pieces[x, adjacentY].Type == PieceType.BOX && pieces[x, adjacentY].IsClearable())
                {
                    pieces[x, adjacentY].ClearableComponent.ClearPiece();
                    SpawnNewPiece(x, adjacentY, PieceType.EMPTY);
                }
                else if (pieces[x, adjacentY].Type == PieceType.ICEBLOCK && pieces[x, adjacentY].IsClearable())
                {
                    bool spawnNew = pieces[x, adjacentY].ClearableComponent.ClearStep();
                    if (spawnNew)
                    {
                        SpawnNewPiece(x, adjacentY, PieceType.EMPTY);
                    }
                }
            }
        }
    }

    #region SPECIAL TILES CLEAR 
    public void ClearRow(int row)
    {
        for (int x = 0; x < xDim; x++)
        {
            ClearPiece(x, row);
        }
    }

    public void ClearColumn(int column)
    {
        for (int y = 0; y < yDim; y++)
        {
            ClearPiece(column, y);
        }
    }
    public void ClearColor(ColorPiece.ColorType color)
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y].IsColored() && (pieces[x, y]).ColorComponent.Color == color
                    || color == ColorPiece.ColorType.ANY)
                {
                    ClearPiece(x, y);
                }
            }
        }
    }
    #endregion
    public void GameOver()
    {
        gameOver = true;
    }
    public void ResumeGame()
    {
        gameOver = false;
    }
    public List<GamePiece> GetPiecesOfType(PieceType type)
    {
        List<GamePiece> piecesOfType = new List<GamePiece>();
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y].Type == type)
                {
                    piecesOfType.Add(pieces[x, y]);
                }
            }
        }
        return piecesOfType;
    }

}
