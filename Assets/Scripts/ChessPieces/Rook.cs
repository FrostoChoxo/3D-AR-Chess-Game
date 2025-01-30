using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, Vector2Int BOARD_SIZE)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Down
        for(int i = currentY-1; i >= 0; i--)
        {
            if (board[currentX, i] == null)
            {
                r.Add(new Vector2Int(currentX, i));
            }
            else
            {
                if (board[currentX, i].team != team)
                {
                    r.Add(new Vector2Int(currentX, i));
                }
                break;
            }
        }

        //Up
        for (int i = currentY + 1; i <BOARD_SIZE.y; i++)
        {
            if (board[currentX, i] == null)
            {
                r.Add(new Vector2Int(currentX, i));
            }
            else
            {
                if (board[currentX, i].team != team)
                {
                    r.Add(new Vector2Int(currentX, i));
                }
                break;
            }
        }

        //Left
        for (int i = currentX - 1; i >= 0; i--)
        {
            if (board[i, currentY] == null)
            {
                r.Add(new Vector2Int(i, currentY));
            }
            else
            {
                if (board[i, currentY].team != team)
                {
                    r.Add(new Vector2Int(i, currentY));
                }
                break;
            }
        }

        //Right
        for (int i = currentX + 1; i < BOARD_SIZE.x; i++)
        {
            if (board[i, currentY] == null)
            {
                r.Add(new Vector2Int(i, currentY));
            }
            else
            {
                if (board[i, currentY].team != team)
                {
                    r.Add(new Vector2Int(i, currentY));
                }
                break;
            }
        }

        return r;
    }
}
