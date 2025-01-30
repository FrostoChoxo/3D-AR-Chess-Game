using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTimer : MonoBehaviour
{
    public int team;
    public float time_;
    public float increment;
    public bool isPlaying = false;
    bool neverPlayed = true;

    public Lichess lc;
    // Start is called before the first frame update
    void Start()
    {
        Image img = GetComponent<Image>();
        img.color = new Color(0, 0.25f, 0, 1);
        if(team == 0)
        {
            img.enabled = true;
        }
        else
        {
            img.enabled = false;

        }

    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            time_-=Time.deltaTime;
        }
        string minute = ((int)time_ / 60).ToString();
        int s = (int)time_ % 60;
        string second;
        if (s < 10)
        {
            second = "0" + s;
        }
        else
        {
            second = s + "";
        }
        GetComponentInChildren<TextMeshProUGUI>().text = minute + ":" + second;
    }


    public void setTime(float time__)
    {
        time_ = time__;
    }

    public void setIncrement(float i)
    {
        increment = i;
    }

    public void startPlaying()
    {
        neverPlayed = false;
        isPlaying = true;
        GetComponent<Image>().enabled=true;
    }

    public void stopPlaying()
    {
        if (!neverPlayed)
        {
            time_ += increment;
        }
            

        isPlaying = false;
        GetComponent<Image>().enabled = false;
    }

    public void showTimer()
    {
        GetComponent<Image>().enabled = true;
        GetComponentInChildren<TextMeshProUGUI>().enabled = true;
    }

    public void hideTimer()
    {
        GetComponent<Image>().enabled = false;
        GetComponentInChildren<TextMeshProUGUI>().enabled = false;
    }
}
