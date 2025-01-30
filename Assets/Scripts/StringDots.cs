using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StringDots : MonoBehaviour
{
    private string txt = "";
    private float time;
    // Start is called before the first frame update
    void Start()
    {
        txt = GetComponent<TextMeshProUGUI>().text;
    }

    // Update is called once per frame
    void Update()
    {
        if (time > 4)
        {
            time = 0;
        }else if(time > 3)
        {
            GetComponent<TextMeshProUGUI>().text = txt + "...";
        }else if (time > 2)
        {
            GetComponent<TextMeshProUGUI>().text = txt + "..";
        }else if (time > 1)
        {
            GetComponent<TextMeshProUGUI>().text = txt + ".";
        }
        else if (time > 0)
        {
            GetComponent<TextMeshProUGUI>().text = txt;
        }



        time += Time.deltaTime;
    }
}
