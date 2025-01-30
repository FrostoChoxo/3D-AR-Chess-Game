using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6,
}

public class ChessPiece : MonoBehaviour
{
    public GameObject FracturedPiece;
    
    public int team;
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

    public Material fracturedWhite;
    public Material fracturedBlack;

    private Quaternion r;

    float disappear = 0;

    public bool isMoving = true;

    private Outline ol;

    private void Start()
    {
        if (team == 1)
        {
            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 180));
        }

        ol = GetComponent<Outline>();
       
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPosition, Time.deltaTime * 10);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);

        disappear -= Time.deltaTime;

        if (disappear <= 0)
        {
            GetComponent<MeshRenderer>().enabled = true;
        }

    }

    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, Vector2Int BOARD_SIZE)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        r.Add(new Vector2Int(3,3));
        r.Add(new Vector2Int(3, 4));
        r.Add(new Vector2Int(4, 3));
        r.Add(new Vector2Int(4, 4));

        return r;
    }

    public virtual SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        return SpecialMove.None;
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
        {
            transform.localPosition = position;
        }

    }

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = scale;
        }
    }

    public GameObject Fracture(bool force = false)
    {

        GameObject instance = Instantiate(FracturedPiece, transform.position, transform.rotation);

        MeshRenderer[] mrs = instance.GetComponentsInChildren<MeshRenderer>();
        Material mat = GetComponent<MeshRenderer>().material;
        for (int i = 0; i < mrs.Length; i++)
        {
            if (team == 0)
            {
                mrs[i].material = fracturedWhite;
            }
            else
            {
                mrs[i].material = fracturedBlack;
            }
            
        }
        if (force)
        {
            instance.GetComponent<DetectPiece>().ForceBreak();
        }
        Destroy(gameObject);
        return instance;
    }

    public virtual GameObject Promote(string pName_) { return null; }

    public void DisappearFor1Second()
    {
        disappear = 1.0f;
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void PieceRef()
    {
        r = this.gameObject.transform.rotation;
        FindAnyObjectByType<Chessboard>().setHitCp(this);
    }

    public void SetRef()
    {
        this.gameObject.transform.rotation = r;
        FindAnyObjectByType<Chessboard>().ReleaseCP(this);
    }

    public void HoverEnter()
    {
        if(ol != null)
        {
            ol.enabled = true;
        }
    }

    public void HoverExit()
    {
        if (ol != null)
        {
            ol.enabled = false;
        }
    }
}
