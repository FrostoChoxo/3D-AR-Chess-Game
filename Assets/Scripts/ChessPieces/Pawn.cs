using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Pawn : ChessPiece
{
    public GameObject BishopAnimation;
    public GameObject KnightAnimation;
    public GameObject QueenAnimation;
    public GameObject RookAnimation;

    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, Vector2Int BOARD_SIZE)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        Debug.Log("currentx: " + currentX + ", currenty: " + currentY);

        if (board[currentX, currentY + direction] == null)
        {
            r.Add(new Vector2Int(currentX, currentY + direction));
        }

        if (board[currentX, currentY + direction] == null)
        {
            if((team == 0 && currentY==1)||(team == 1 && currentY == 6) && board[currentX, currentY + direction * 2] == null) { 
                r.Add(new Vector2Int(currentX, currentY+direction * 2));
            }
        }

        if(currentX != BOARD_SIZE.x - 1)
        {
            if (board[currentX+1, currentY+direction] != null && board[currentX + 1, currentY + direction].team != team) {
                r.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        }

        if (currentX != 0)
        {
            if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
            {
                r.Add(new Vector2Int(currentX - 1, currentY + direction));
            }
        }

        return r;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = team ==0 ? 1 : -1;

        //promotion
        if((team==0 && currentY == 6) || (team == 1 && currentY == 1))
        {
            return SpecialMove.Promotion;
        }

        //en passant
        if (moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if (board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn)
            {
                if (Mathf.Abs(lastMove[1].y - lastMove[0].y)==2)
                {
                    if(board[lastMove[1].x, lastMove[1].y].team != team)
                    {
                        if (lastMove[1].y == currentY)
                        {
                            if (lastMove[1].x == currentX - 1)
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }

                            if (lastMove[1].x == currentX + 1)
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }

        }

        return SpecialMove.None;
    }
    public override GameObject Promote(string pName)
    {
        GameObject an = null;
        if (pName.ToLower().Equals("b"))
        {
            an = Instantiate(BishopAnimation, transform.position, transform.rotation);
            name = "Bishop";
        }
        else if (pName.ToLower().Equals("n"))
        {
            an = Instantiate(KnightAnimation, transform.position, transform.rotation);
            if(team == 0)
            {
                an.transform.Rotate(0, 0, -90);
            }
            else
            {
                an.transform.Rotate(0, 0, 90);
            }
            
            name = "Knight";
        }
        else if (pName.ToLower().Equals("q"))
        {
            an = Instantiate(QueenAnimation, transform.position, transform.rotation);
            name = "Queen";
        }
        else if (pName.ToLower().Equals("r"))
        {
            an = Instantiate(RookAnimation, transform.position, transform.rotation);
            name = "Rook";
        }
        else
        {
            Debug.LogError("Invalid piece");
        }

        Destroy(an, 1.05f);

        an.transform.localScale = transform.localScale * 100f;

        //change material of grandchildren
        Transform[] trans = an.GetComponentsInChildren<Transform>();

        Material mat = GetComponent<MeshRenderer>().material;

        for (int i = 0; i < trans.Length; i++)
        {
            MeshRenderer[] mrs = trans[i].gameObject.GetComponentsInChildren<MeshRenderer>();
            for (int j = 0; j < mrs.Length; j++)
            {
                mrs[j].material = mat;
            }
        }

        return an;

    }

}
