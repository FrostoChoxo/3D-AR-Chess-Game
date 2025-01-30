using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MovesHUD : MonoBehaviour
{
    public Chessboard cb;

    public TextMeshProUGUI number;
    public TextMeshProUGUI whiteMove;
    public TextMeshProUGUI blackMove;
    public GameObject blackBox;

    int movelistSize = -1;
    float offset = 50;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(cb.moveList.Count == 0)
        {
            return;
        }

        if(cb.moveList.Count == movelistSize)
        {
            return;
        }

        Vector2Int cm = cb.moveList[cb.moveList.Count - 1][1];
        int ct = cb.GetTeam(cm);

        if (ct == 0)
        {
            TextMeshProUGUI n = Instantiate(number, transform);
            n.text = ((cb.moveList.Count / 2) + 1).ToString();
            n.rectTransform.anchoredPosition = n.rectTransform.anchoredPosition - new Vector2(0, Mathf.Floor((cb.moveList.Count-1) / 2)*offset);
            movelistSize = cb.moveList.Count;
            TextMeshProUGUI w = Instantiate(whiteMove, transform);
            w.text = cb.IndexToUCI(cb.moveList[cb.moveList.Count - 1][1]);
            w.rectTransform.anchoredPosition = w.rectTransform.anchoredPosition - new Vector2(0, (cb.moveList.Count / 2) * offset);
            GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition + new Vector2(0, offset);
            if (!blackBox.GetComponent<UnityEngine.UI.Image>().enabled)
            {
                blackBox.GetComponent<UnityEngine.UI.Image>().enabled = true;
            }
        }
        else
        {
            TextMeshProUGUI b = Instantiate(blackMove, transform);
            b.text = cb.IndexToUCI(cb.moveList[cb.moveList.Count - 1][1]);
            b.rectTransform.anchoredPosition = b.rectTransform.anchoredPosition - new Vector2(0, Mathf.Floor((cb.moveList.Count-1) / 2) * offset);
        }

        movelistSize = cb.moveList.Count;



    }
}
