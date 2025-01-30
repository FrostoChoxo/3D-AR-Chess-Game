using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPawn : MonoBehaviour
{
    public void pieceref()
    {
        FindAnyObjectByType<Chessboard>().setHitCp(GetComponent<ChessPiece>());
        Debug.Log(GetComponent<ChessPiece>().currentX);
        Debug.Log(GetComponent<ChessPiece>().currentY);
    }
}
