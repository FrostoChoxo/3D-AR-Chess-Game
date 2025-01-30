using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit.Input;
using System.Runtime.CompilerServices;
using System;
using Unity.Collections;
using UnityEngine.UI;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Mathematics;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using System.Net.NetworkInformation;



//NOTE: enable team constraints

public enum SpecialMove
{
    None = 0,
    EnPassant = 1,
    Castling = 2,
    Promotion = 3
}

public class Chessboard : MonoBehaviour
{
    Vector2Int hitPosition = -Vector2Int.one;
    bool isManipulation = false;
    bool isNotManipulation = false;
    private ChessPiece grabbed = null;
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private Vector2Int BOARD_SIZE = new Vector2Int(8, 8);
    private float TILE_SIZE = 0.25f;
    private float TILE_HEIGHT = 0.05f;
    private GameObject[,] tiles;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private SpecialMove specialMove;

    [HideInInspector]
    public List<Vector2Int[]> moveList = new List<Vector2Int[]>();

    private Vector2Int[] previousMove = null;
    public string promotionInitial = "";

    public Material[] tileMaterials;
    public Material highlightMaterial;
    public Material moveMaterial;
    public Material ghostMaterialBlue;
    public Material ghostMaterialRed;

    public GameObject[] prefabs;
    public Material[] teamMaterials;
    public GameObject border;

    private Lichess lc;

    private Quaternion originalRotation = Quaternion.identity;

    private bool isPromoting = false;

    public int px = -1;
    public int py = -1;

    Vector2Int previousPosition = -Vector2Int.one;

    //chessboard placement

    private Vector3 offset = new Vector3(0.001f, 1.208236f, -0.547289997f);
    public Camera cam;

    //team settings
    [HideInInspector]
    public int currentTeam = 0;


    private int myTeam = -1;

    //Ghost Piece

    GameObject gp = null;

    [HideInInspector]
    public bool chessPieceCreated = true;

    public GameObject winScreen;
    public GameObject blackPromoUi;
    public GameObject whitePromoUi;

    public GameObject auxObj;
    AudioHandler aux;


    //debug
    private bool debug = false;
    private string[] whiteMoves = new string[5] { "g2g4", "g4g5", "g5h6", "h6g7", "g7g8" };
    private string[] blackMoves = new string[4] { "f7f6", "g8h6", "b7b6", "a7a6"};
    private int wi = 0;
    private int bi = 0;
    private float timePerMove = 5f;
    private float time__ = 0f;




    private void Start()
    {
        lc = GetComponent<Lichess>();
        aux = auxObj.GetComponent<AudioHandler>();
        if (debug)
        {
            
            setPlayerColor("white");
            GenerateAllTiles();
            SpawnAllPieces();
            SpawnBorder();
            PositionAllPieces();
            PlaceChessboardRelativeToCamera();
            aux.PlayStartGame();
        }
        else
        {
            //lc.startGame(lc.colorChoice);
            //lc.startPVPGame();
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (isPromoting)
        {
            return;
        }

        if (myTeam == -1) { return; }

        if (!chessPieceCreated) { return; }


        if (debug)
        {
            if (time__ > timePerMove)
            {
                if (currentTeam == 0)
                {
                    if (wi < whiteMoves.Length)
                    {
                        Vector2Int[] wp = UCItoIndex(whiteMoves[wi]);

                        currentlyDragging = chessPieces[wp[0].x, wp[0].y];

                        availableMoves = chessPieces[wp[0].x, wp[0].y].GetAvailableMoves(ref chessPieces, BOARD_SIZE);

                        specialMove = chessPieces[wp[0].x, wp[0].y].GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);


                        int validMove = MoveTo(chessPieces[wp[0].x, wp[0].y], wp[1].x, wp[1].y);

                        if (validMove == -1)
                        {
                            wi++;
                            return;
                        }

                        RemoveHighlightTiles();

                        if (validMove == 0)
                        {
                            currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y), false);
                            HighlightPreviousTiles();
                        }
                        else if (validMove == 1) { }
                        {
                            RemoveHighlightPreviousTiles();
                        }


                        currentlyDragging = null;
                        grabbed = null;


                        isNotManipulation = false;
                        if (gp != null)
                        {
                            Destroy(gp.gameObject);
                        }

                        wi++;
                    }
                }
                else
                {
                    if (bi < blackMoves.Length)
                    {
                        Vector2Int[] bp = UCItoIndex(blackMoves[bi]);

                        currentlyDragging = chessPieces[bp[0].x, bp[0].y];

                        availableMoves = chessPieces[bp[0].x, bp[0].y].GetAvailableMoves(ref chessPieces, BOARD_SIZE);

                        specialMove = chessPieces[bp[0].x, bp[0].y].GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);


                        int validMove = MoveTo(chessPieces[bp[0].x, bp[0].y], bp[1].x, bp[1].y);

                        if (validMove == -1)
                        {
                            bi++;
                            return;
                        }

                        RemoveHighlightTiles();

                        if (validMove == 0)
                        {
                            currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y), false);
                            HighlightPreviousTiles();
                        }
                        else if (validMove == 1) { }
                        {
                            RemoveHighlightPreviousTiles();
                        }


                        currentlyDragging = null;
                        grabbed = null;


                        isNotManipulation = false;
                        if (gp != null)
                        {
                            Destroy(gp.gameObject);
                        }

                        bi++;
                    }
                }
                time__ = 0;
            }
            time__ += Time.deltaTime;
            return;
        }



        if (currentTeam == myTeam)
        {
            //enable object manipulation
            for (int x = 0; x < BOARD_SIZE.x; x++)
            {
                for (int y = 0; y < BOARD_SIZE.y; y++)
                {
                    if (chessPieces[x, y] != null)
                    {
                        if (chessPieces[x, y].team == myTeam)
                        {
                            chessPieces[x, y].gameObject.GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>().enabled = true;
                        }
                    }
                }
            }

            if (isManipulation)
            {

                hitPosition = new Vector2Int(grabbed.currentX, grabbed.currentY);
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {

                    currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];



                    originalRotation = currentlyDragging.gameObject.transform.rotation;

                    //if (currentTeam == myTeam && chessPieces[hitPosition.x, hitPosition.y].team == currentTeam)
                    if (chessPieces[hitPosition.x, hitPosition.y].team == currentTeam)
                    {


                        currentlyDragging.isMoving = false;

                        //get list of available moves
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, BOARD_SIZE);

                        //get list of special moves
                        specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);


                        PreventCheck();

                        HighlightPreviousTiles();
                        HighlightTiles();

                    }
                }
                isManipulation = false;
            }

            //if left click up
            /*if (currentlyDragging != null && Input.GetMouseButtonUp(0))*/
            if (currentlyDragging != null && isNotManipulation)
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
                RaycastHit tilehit;
                if (Physics.Raycast(grabbed.gameObject.transform.position + new Vector3(0, 1, 0), Vector3.down, out tilehit, 100f, LayerMask.GetMask("Tile")))
                {
                    hitPosition = LookupTileIndex(tilehit.transform.gameObject);
                }

                currentlyDragging.gameObject.GetComponent<Outline>().enabled = false;


                currentlyDragging.isMoving = true;

                int validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y, false);

                if (validMove == -1)
                {
                    return;
                }

                RemoveHighlightTiles();

                if (validMove == 0)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y), false);
                    HighlightPreviousTiles();
                }
                else if (validMove == 1) { }
                {
                    RemoveHighlightPreviousTiles();
                }


                currentlyDragging.gameObject.transform.rotation = originalRotation;
                originalRotation = Quaternion.identity;


                currentlyDragging = null;
                grabbed = null;


                isNotManipulation = false;
                if (gp != null)
                {
                    Destroy(gp.gameObject);
                }


            }


            //ghost logic

            RaycastHit tilehit_;
            Vector2Int ghostPosition;
            if (currentlyDragging != null && grabbed != null && currentlyDragging.team == currentTeam)
            {
                if (gp == null)
                {
                    gp = Instantiate(prefabs[(int)currentlyDragging.type - 1], currentlyDragging.gameObject.transform);
                    gp.GetComponent<ChessPiece>().isMoving = false;

                    gp.transform.SetParent(transform, true);

                    gp.GetComponent<MeshRenderer>().material = ghostMaterialRed;


                    if (myTeam == 0 && currentlyDragging.team == 1)
                    {
                        gp.transform.rotation = Quaternion.Euler(-90, 0, -90);
                    }
                    else if (myTeam == 1 && currentlyDragging.team == 0)
                    {
                        gp.transform.rotation = Quaternion.Euler(-90, 0, 90);
                    }



                }

                if (Physics.Raycast(grabbed.gameObject.transform.position + new Vector3(0, 1, 0), Vector3.down, out tilehit_, 100f, LayerMask.GetMask("Tile")))
                {
                    ghostPosition = LookupTileIndex(tilehit_.transform.gameObject);
                    /*
                                        if (chessPieces[ghostPosition.x, ghostPosition.y] == null || ghostPosition == new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY))
                                        {*/

                    if (ContainsValidMove(ref availableMoves, ghostPosition))
                    {
                        gp.GetComponent<MeshRenderer>().material = ghostMaterialBlue;
                    }
                    else
                    {
                        gp.GetComponent<MeshRenderer>().material = ghostMaterialRed;
                    }

                    gp.transform.localPosition = GetTileCenter(ghostPosition.x, ghostPosition.y);
                    gp.transform.rotation = originalRotation;
                    /*                    }
                                        else
                                        {
                                            if (gp != null)
                                            {
                                                Destroy(gp);
                                                gp = null;
                                            }
                                        }*/
                }
                else
                {
                    if (gp != null)
                    {
                        Destroy(gp);
                        gp = null;
                    }
                }
            }


        }
        else
        {

            //disable object manipulation
            for (int x = 0; x < BOARD_SIZE.x; x++)
            {
                for (int y = 0; y < BOARD_SIZE.y; y++)
                {
                    if (chessPieces[x, y] != null)
                    {
                        if (chessPieces[x, y].team == myTeam)
                        {
                            chessPieces[x, y].gameObject.GetComponent<Outline>().enabled = false;
                            chessPieces[x, y].gameObject.GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>().enabled = false;
                        }
                    }
                }
            }


            if (lc.moves.Length > lc.moveCount)
            {
                string movesString = "";
                for (int i = 0; i < lc.moves.Length; i++)
                {
                    movesString += lc.moves[i];
                }

                string[] movesList = movesString.Split(' ');
                //You can call the board piece movement function here with movesList[movesList.Length-1] as input. s
                lc.moves.Clear();
                lc.turnCount++;


                //add movement script here


                string m = movesList[movesList.Length - 1];

                Vector2Int[] pos_ = UCItoIndex(m);


                Debug.Log("api output move: " + m);
                if (pos_ != null &&
                    pos_[0].x >= 0 && pos_[0].x < 8 && pos_[0].y >= 0 && pos_[0].y < 8 &&
                    pos_[1].x >= 0 && pos_[1].x < 8 && pos_[1].y >= 0 && pos_[1].y < 8)

                {
                    Debug.Log("api output move: " + m);
                    Debug.Log(pos_[0].x + " " + pos_[0].y);
                    ChessPiece piece_ = chessPieces[pos_[0].x, pos_[0].y];

                    if (piece_ != null)
                    {
                        availableMoves = piece_.GetAvailableMoves(ref chessPieces, BOARD_SIZE);

                        specialMove = piece_.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);

                        if (m.Length == 5)
                        {
                            promotionInitial = m[4] + "";
                        }


                        MoveTo(piece_, pos_[1].x, pos_[1].y, false);

                    }
                }
                else
                {
                    lc.moves.Clear();
                }

            }




        }

    }

    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].GetComponent<MeshRenderer>().material = highlightMaterial;
        }
    }

    private void HighlightPreviousTiles()
    {
        if (previousMove != null)
        {
            tiles[previousMove[0].x, previousMove[0].y].GetComponent<MeshRenderer>().material = moveMaterial;
            tiles[previousMove[1].x, previousMove[1].y].GetComponent<MeshRenderer>().material = moveMaterial;
        }
    }

    private void RemoveHighlightPreviousTiles()
    {
        if (previousMove != null)
        {
            tiles[previousMove[0].x, previousMove[0].y].GetComponent<MeshRenderer>().material = GetCheckerBoardMaterial(previousMove[0].x, previousMove[0].y);
            tiles[previousMove[1].x, previousMove[1].y].GetComponent<MeshRenderer>().material = GetCheckerBoardMaterial(previousMove[1].x, previousMove[1].y);
        }
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {

            tiles[availableMoves[i].x, availableMoves[i].y].GetComponent<MeshRenderer>().material = GetCheckerBoardMaterial(availableMoves[i].x, availableMoves[i].y);
        }

        availableMoves.Clear();
    }

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }
        return false;
    }

    private int MoveTo(ChessPiece piece, int x, int y, bool force = false)
    {
        px = x;
        py = y;
        if (currentTeam == myTeam && !debug)
        {
            if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
            {
                aux.PlayWrongMove();
                return 0;
            }
        }

        previousPosition = new Vector2Int(piece.currentX, piece.currentY);

        //simulate

        ChessPiece targetKing = null;
        for (int i = 0; i < BOARD_SIZE.x; i++)
            for (int j = 0; j < BOARD_SIZE.y; j++)
                if (chessPieces[i, j] != null)
                    if (chessPieces[i, j].type == ChessPieceType.King)
                        if (chessPieces[i, j].team != piece.team)
                            targetKing = chessPieces[i, j];



        bool checkPiece;
        
        if(piece.type==ChessPieceType.Pawn && y == 7 && piece.team == 0)
        {
            checkPiece = false;
        }
        else if(piece.type == ChessPieceType.Pawn && y == 0 && piece.team == 1)
        {
            checkPiece=false;
        }
        else{
            checkPiece = SimulateMoveForCheck(piece, x, y, targetKing);
        }
        


        //is there another piece on the target position

        bool takenPiece = false;
        bool fracturePiece = false;

        if (chessPieces[x, y] != null)
        {
            ChessPiece otherPiece = chessPieces[x, y];

            if (piece.team == otherPiece.team)
            {
                aux.PlayWrongMove();
                return 0;
            }

            //add destruction thing if enemy attacks

            if (piece.team == myTeam)
            {
                takenPiece = true;
                Destroy(otherPiece.gameObject);
            }
            else
            {
                fracturePiece = true;
                GameObject f = otherPiece.Fracture();
                f.transform.localScale = transform.localScale;
            }

        }

        chessPieces[x, y] = piece;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y, force);
        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });





        if (specialMove == SpecialMove.Promotion && currentTeam == myTeam)
        {
            Debug.Log("promotion ui");
            //assume promtion is queen
            //show promotion ui
            Vector3 uipos = GetTileCenter(x, y);
            if(myTeam == 0)
            {
                whitePromoUi.transform.localPosition = uipos + new Vector3(0, 1f, 0);
                whitePromoUi.transform.localScale = new Vector3(7, 7, 4);
                whitePromoUi.transform.SetParent(transform, false);
                whitePromoUi.SetActive(true);
            }
            else
            {
                blackPromoUi.transform.localPosition = uipos + new Vector3(0, 1f, 0);
                blackPromoUi.transform.localScale = new Vector3(7, 7, 4);
                blackPromoUi.transform.SetParent(transform, false);
                blackPromoUi.SetActive(true);
            }

            isPromoting = true;
            return -1;


        }



        ProcessSpecialMove(promotionInitial);

        Debug.Log(IndexToUCI(previousPosition) + IndexToUCI(new Vector2Int(x, y)) + promotionInitial);

        if (currentTeam == myTeam)
        {
            lc.sendMove(IndexToUCI(previousPosition) + IndexToUCI(new Vector2Int(x, y)) + promotionInitial);
        }
        else
        {
            RemoveHighlightPreviousTiles();
            previousMove = new Vector2Int[2] { previousPosition, new Vector2Int(x, y) };
            HighlightPreviousTiles();
        }


        bool checkMate = CheckForCheckmate();

        if (checkMate)
        {
            CheckMate(piece.team);
        }
        else
        {
            if (checkPiece)
            {
                aux.PlayCheck();
            }
            else if (specialMove == SpecialMove.Castling)
            {
                aux.PlayCastle();
            }
            else if (takenPiece)
            {
                aux.PlayCapture();
            }
            else if (fracturePiece)
            {
                aux.PlayFracture();
            }
            else
            {
                aux.PlayMove();
            }
        }


        currentTeam = 1 - currentTeam;

        //return validity
        return 1;
    }

    //Create Coroutines with slight delay to let click audio play for ui
    public void promoteBishop()
    {
        StartCoroutine(PromoteBishop());
        //Reset initial to empty so it isn't passed to enemy move
        promotionInitial = "";
    }

    public void promoteQueen()
    {
        StartCoroutine(PromoteQueen());
        promotionInitial = "";
    }

    public void promoteKnight()
    {
        StartCoroutine(PromoteKnight());
        promotionInitial = "";
    }

    public void promoteRook()
    {
        StartCoroutine(PromoteRook());
        promotionInitial = "";
    }

    private IEnumerator PromoteBishop()
    {
        promotionInitial = "b";
        MoveToPromote();
        isPromoting = false;
        yield return new WaitForSeconds(0.05f);
        whitePromoUi.SetActive(false);
        blackPromoUi.SetActive(false);
    }

    private IEnumerator PromoteQueen()
    {
        promotionInitial = "q";
        MoveToPromote();
        isPromoting = false;
        yield return new WaitForSeconds(0.05f);
        whitePromoUi.SetActive(false);
        blackPromoUi.SetActive(false);
    }

    private IEnumerator PromoteRook()
    {
        promotionInitial = "r";
        MoveToPromote();
        isPromoting = false;
        yield return new WaitForSeconds(0.05f);
        whitePromoUi.SetActive(false);
        blackPromoUi.SetActive(false);
    }

    private IEnumerator PromoteKnight()
    {
        promotionInitial = "n";
        MoveToPromote();
        isPromoting = false;
        yield return new WaitForSeconds(0.05f);
        whitePromoUi.SetActive(false);
        blackPromoUi.SetActive(false);
    }

    public void MoveToPromote()
    {
        ProcessSpecialMove(promotionInitial);


        Debug.Log(IndexToUCI(previousPosition) + IndexToUCI(new Vector2Int(px, py)) + promotionInitial);

        if (currentTeam == myTeam)
        {
            lc.sendMove(IndexToUCI(previousPosition) + IndexToUCI(new Vector2Int(px, py)) + promotionInitial);
        }
        /*        else
                {
                    RemoveHighlightPreviousTiles();
                    previousMove = new Vector2Int[2] { previousPosition, new Vector2Int(px, py) };
                    //HighlightPreviousTiles();
                }*/




        if (CheckForCheckmate())
        {
            CheckMate(myTeam);
        }
        else
        {
            aux.PlayPromotion();
        }


        currentTeam = 1 - currentTeam;


        //valid move
        currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y), false);
        //HighlightPreviousTiles();


        currentlyDragging.gameObject.transform.rotation = originalRotation;
        originalRotation = Quaternion.identity;


        currentlyDragging = null;
        grabbed = null;


        isNotManipulation = false;
        if (gp != null)
        {
            Destroy(gp.gameObject);
        }

    }



    private void ProcessSpecialMove(string pI = "q")
    {
        if (specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];
            if (myPawn.currentX == enemyPawn.currentX)
            {
                if (myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1)
                {
                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                    GameObject f = enemyPawn.Fracture(true);
                    f.transform.localScale = transform.localScale;
                }
            }
        }

        if (specialMove == SpecialMove.Promotion)
        {
            PromotePawn(pI);
        }

        if (specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            // left rook
            if (lastMove[1].x == 2)
            {
                if (lastMove[1].y == 0) //white
                {
                    ChessPiece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7) //black
                {
                    ChessPiece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }
            //right rook
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0) //white
                {
                    ChessPiece rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7) //black
                {
                    ChessPiece rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }

    private void PromotePawn(string toPiece)
    {
        Vector2Int[] lastMove = moveList[moveList.Count - 1];
        ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];


        if (targetPawn.team == 0 && lastMove[1].y == 7)
        {
            GameObject an = targetPawn.Promote(toPiece);
            ChessPiece newPiece = null;

            if (toPiece == "q")
            {
                newPiece = SpawnSinglePiece(ChessPieceType.Queen, 0);
            }
            else if (toPiece == "b")
            {
                newPiece = SpawnSinglePiece(ChessPieceType.Bishop, 0);
            }
            else if (toPiece == "n")
            {
                newPiece = SpawnSinglePiece(ChessPieceType.Knight, 0);
            }
            else if (toPiece == "r")
            {
                newPiece = SpawnSinglePiece(ChessPieceType.Rook, 0);
            }

            newPiece.DisappearFor1Second();

            newPiece.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
            Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
            chessPieces[lastMove[1].x, lastMove[1].y] = newPiece;
            PositionSinglePiece(lastMove[1].x, lastMove[1].y);
            an.transform.SetParent(newPiece.transform, true);
            an.transform.localScale = newPiece.transform.localScale * 100f;

        }
        if (targetPawn.team == 1 && lastMove[1].y == 0)
        {
            //queen
            GameObject an = targetPawn.Promote(toPiece);
            ChessPiece newPiece = null;

            if (toPiece == "q")
            {
                newPiece = SpawnSinglePiece(ChessPieceType.Queen, 1);
            }
            else if (toPiece == "b")
            {
                newPiece = SpawnSinglePiece(ChessPieceType.Bishop, 1);
            }
            else if (toPiece == "k")
            {
                newPiece = SpawnSinglePiece(ChessPieceType.Knight, 1);
            }
            else if (toPiece == "r")
            {
                newPiece = SpawnSinglePiece(ChessPieceType.Rook, 1);
            }

            newPiece.DisappearFor1Second();

            newPiece.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
            Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
            chessPieces[lastMove[1].x, lastMove[1].y] = newPiece;
            PositionSinglePiece(lastMove[1].x, lastMove[1].y);
            an.transform.SetParent(newPiece.transform, true);
            an.transform.localScale = newPiece.transform.localScale * 100f;
        }
    }

    private bool CheckForCheckmate()
    {

        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = 1 - chessPieces[lastMove[1].x, lastMove[1].y].team;

        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        List<ChessPiece> defendingPieces = new List<ChessPiece>();
        ChessPiece targetKing = null;
        for (int x = 0; x < BOARD_SIZE.x; x++)
            for (int y = 0; y < BOARD_SIZE.y; y++)
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team == targetTeam)
                    {
                        defendingPieces.Add(chessPieces[x, y]);
                        if (chessPieces[x, y].type == ChessPieceType.King)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                    else
                    {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                }

        //is the king attacked right now?

        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();

        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref chessPieces, BOARD_SIZE);
            for (int k = 0; k < pieceMoves.Count; k++)
            {
                currentAvailableMoves.Add(pieceMoves[k]);
            }
        }
        //are we in check right now?
        if (ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
            //king is under attack, can we move something to help him?
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieces, BOARD_SIZE);
                SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);
                if (defendingMoves.Count != 0)
                {
                    return false;
                }
            }
            return true; //checkmate exit
        }

        return false;
    }

    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < BOARD_SIZE.x; x++)
            for (int y = 0; y < BOARD_SIZE.y; y++)
                if (chessPieces[x, y] != null)
                    if (chessPieces[x, y].type == ChessPieceType.King)
                        if (chessPieces[x, y].team == currentlyDragging.team)
                            targetKing = chessPieces[x, y];

        SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves, targetKing);
    }

    private void SimulateMoveForSinglePiece(ChessPiece piece, ref List<Vector2Int> moves, ChessPiece targetKing)
    {


        //save current values
        int actualX = piece.currentX;
        int actualY = piece.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        //simulate and detect check

        for (int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPosSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
            if (piece.type == ChessPieceType.King)
            {
                kingPosSim = new Vector2Int(simX, simY);
            }

            //clone chesspiece array
            ChessPiece[,] simulation = new ChessPiece[BOARD_SIZE.x, BOARD_SIZE.y];
            List<ChessPiece> simAttackingPieces = new List<ChessPiece>();

            for (int x = 0; x < BOARD_SIZE.x; x++)
            {
                for (int y = 0; y < BOARD_SIZE.y; y++)
                {
                    if (chessPieces[x, y] != null)
                    {
                        simulation[x, y] = chessPieces[x, y];
                        if (simulation[x, y].team != piece.team)
                        {
                            simAttackingPieces.Add(simulation[x, y]);
                        }
                    }
                }
            }

            // simulate move
            simulation[actualX, actualY] = null;
            piece.currentX = simX;
            piece.currentY = simY;
            simulation[simX, simY] = piece;

            //did a piece get killed during simulation?
            var deadPiece = simAttackingPieces.Find(c => c.currentX == simX && c.currentY == simY);

            if (deadPiece != null)
            {
                simAttackingPieces.Remove(deadPiece);
            }

            // get all the simulated attacking pieces move

            List<Vector2Int> simMoves = new List<Vector2Int>();

            for (int j = 0; j < simAttackingPieces.Count; j++)
            {
                var pieceMoves = simAttackingPieces[j].GetAvailableMoves(ref simulation, BOARD_SIZE);
                for (int k = 0; k < pieceMoves.Count; k++)
                {
                    simMoves.Add(pieceMoves[k]);
                }
            }

            //is the king in trouble? if so, remove move

            if (ContainsValidMove(ref simMoves, kingPosSim))
            {
                movesToRemove.Add(moves[i]);
            }

            //restore actual piece data
            piece.currentX = actualX;
            piece.currentY = actualY;

        }

        //remove from current available list
        for (int i = 0; i < movesToRemove.Count; i++)
        {
            moves.Remove(movesToRemove[i]);
        }
    }


    private bool SimulateMoveForCheck(ChessPiece piece, int x, int y, ChessPiece targetKing)
    {
        //save current values
        int actualX = piece.currentX;
        int actualY = piece.currentY;

        //simulate and detect check
        int simX = x;
        int simY = y;

        Vector2Int kingPosSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
        if (piece.type == ChessPieceType.King)
        {
            return false;
        }

        //clone chesspiece array
        ChessPiece[,] simulation = new ChessPiece[BOARD_SIZE.x, BOARD_SIZE.y];

        for (int xx = 0; xx < BOARD_SIZE.x; xx++)
        {
            for (int yy = 0; yy < BOARD_SIZE.y; yy++)
            {
                if (chessPieces[xx, yy] != null)
                {
                    simulation[xx, yy] = chessPieces[xx, yy];
                }
            }
        }

        // simulate move
        simulation[actualX, actualY] = null;
        piece.currentX = simX;
        piece.currentY = simY;
        simulation[simX, simY] = piece;

        var pieceMoves = simulation[simX, simY].GetAvailableMoves(ref simulation, BOARD_SIZE);

        //is the king in trouble? if so, return false

        if (ContainsValidMove(ref pieceMoves, kingPosSim))
        {
            piece.currentX = actualX;
            piece.currentY = actualY;
            return true;
        }

        //restore actual piece data
        piece.currentX = actualX;
        piece.currentY = actualY;

        return false;
    }


    public void GenerateAllTiles()
    {
        tiles = new GameObject[BOARD_SIZE.x, BOARD_SIZE.y];
        for (int x = 0; x < BOARD_SIZE.y; x++)
        {
            for (int y = 0; y < BOARD_SIZE.x; y++)
            {
                tiles[x, y] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tiles[x, y].transform.position = GetTileCenter(x, y) - new Vector3(0, TILE_HEIGHT * 0.5f, 0);
                tiles[x, y].transform.localScale = new Vector3(TILE_SIZE, TILE_HEIGHT, TILE_SIZE);
                tiles[x, y].layer = LayerMask.NameToLayer("Tile");
                tiles[x, y].GetComponent<MeshRenderer>().material = GetCheckerBoardMaterial(x, y);
                tiles[x, y].transform.SetParent(gameObject.transform, false);
                tiles[x, y].name = x + "," + y;
            }
        }
        //Play Game Start Sound
        //aux.PlayStartGame();
    }

    public Material GetCheckerBoardMaterial(int x, int y)
    {
        return tileMaterials[(x + y) % 2];
    }

    private Vector2Int LookupTileIndex(GameObject hitinfo)
    {
        for (int x = 0; x < BOARD_SIZE.x; x++)
        {
            for (int y = 0; y < BOARD_SIZE.y; y++)
            {
                if (tiles[x, y] == hitinfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one;
    }


    public void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[BOARD_SIZE.x, BOARD_SIZE.y];
        int whiteTeam = 0;
        int blackTeam = 1;

        //white
        ChessPieceType[] pieceLayout = new ChessPieceType[8] { ChessPieceType.Rook, ChessPieceType.Knight, ChessPieceType.Bishop,
            ChessPieceType.Queen, ChessPieceType.King, ChessPieceType.Bishop, ChessPieceType.Knight, ChessPieceType.Rook };

        for (int x = 0; x < BOARD_SIZE.x; x++)
        {
            for (int y = 0; y < BOARD_SIZE.y; y++)
            {
                if (y == 0)
                {
                    chessPieces[x, 0] = SpawnSinglePiece(pieceLayout[x], whiteTeam);
                }
                else if (y == 1)
                {
                    chessPieces[x, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
                }
                else if (y == 6)
                {
                    chessPieces[x, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
                }
                else if (y == 7)
                {
                    chessPieces[x, 7] = SpawnSinglePiece(pieceLayout[x], blackTeam);
                }
            }
        }

        chessPieceCreated = true;
    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece piece = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        piece.type = type;
        piece.team = team;
        if (team != myTeam)
        {
            piece.gameObject.tag = "enemy";
            piece.gameObject.GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>().enabled = false;
        }
        piece.GetComponent<MeshRenderer>().material = teamMaterials[team];
        return piece;
    }

    public void PositionAllPieces()
    {
        for (int x = 0; x < BOARD_SIZE.x; x++)
        {
            for (int y = 0; y < BOARD_SIZE.x; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3((x * TILE_SIZE + TILE_SIZE * 0.5f) - BOARD_SIZE.x * TILE_SIZE * 0.5f, 0, (y * TILE_SIZE + TILE_SIZE * 0.5f) - BOARD_SIZE.y * TILE_SIZE * 0.5f);
    }

    // checkmate

    private void CheckMate(int team)
    {
        aux.PlayCheckmate();
        DisplayVictory(team);
    }


    // need to chang

    private void OnResetButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    public void setHitCp(ChessPiece selected)
    {
        if (grabbed == null)
        {
            grabbed = selected;
            isManipulation = true;
        }
    }

    public void ReleaseCP(ChessPiece selected)
    {
        if (grabbed == selected)
        {
            isNotManipulation = true;
        }
    }


    public string IndexToUCI(Vector2Int pos)
    {
        return (char)(pos[0] + (int)'a') + (pos[1] + 1).ToString();
    }

    private Vector2Int[] UCItoIndex(string uci)
    {
        if (uci.Length < 4)
        {
            return null;
        }

        Vector2Int[] r = new Vector2Int[2];

        r[0] = new Vector2Int((int)uci[0] - 'a', (int)uci[1] - '1');
        r[1] = new Vector2Int((int)uci[2] - 'a', (int)uci[3] - '1');

        return r;
    }

    public void setPlayerColor(string ps)
    {
        if (ps == "white")
        {
            myTeam = 0;
            transform.rotation = transform.rotation * Quaternion.Euler(new Vector3(0, 180, 0));
        }
        else if (ps == "black")
        {
            myTeam = 1;
        }
        else
        {
            Debug.LogError("invalid player color");
        }
    }

    public void PlaceChessboardRelativeToCamera()
    {
        Vector3 offsetXZ = Vector3.Scale(offset, new Vector3(1, 0, 1));
        Vector3 camDir = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1));
        Quaternion rotation = Quaternion.FromToRotation(offsetXZ, camDir);
        Vector3 rotatedFrom = rotation * offset;
        transform.position = transform.position + rotatedFrom;
        transform.rotation = transform.rotation * rotation;
    }

    public void DisplayVictoryResign()
    {
        DisplayVictory(1 - myTeam);
        aux.PlayResign();
    }

    // need to change

    private void DisplayVictory(int winningTeam)
    {
        if (winningTeam == 0)
        {
            winScreen.GetComponentInChildren<TextMeshPro>().text = "White Wins";
        }
        else
        {
            winScreen.GetComponentInChildren<TextMeshPro>().text = "Black Wins";
        }
        winScreen.SetActive(true);

    }

    public int GetTeam(Vector2Int pos__)
    {
        return chessPieces[pos__.x, pos__.y].team;
    }

    //----------------------------------------------------------------------------
    public void ReturnMainMenu()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void SpawnBorder()
    {
        Instantiate(border, transform);
    }


}