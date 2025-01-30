using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowTurn : MonoBehaviour
{
    // Start is called before the first frame update
    public Chessboard cb;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if(cb.currentTeam == -1)
        {
            return;
        }

        if(cb.currentTeam == 0)
        {
            GetComponent<TextMeshProUGUI>().text = "White's Turn";
        }
        else
        {
            GetComponent<TextMeshProUGUI>().text = "Black's Turn";
        }

    }
}
