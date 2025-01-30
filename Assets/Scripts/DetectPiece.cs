using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectPiece : MonoBehaviour
{
    Rigidbody[] rbs;
    public bool force = false;

    float time = 2.0f;
    float timeLeft = 0;

    private MeshRenderer[] mrs;

    private void OnTriggerEnter(Collider other)
    {
        if (!force && other.gameObject.CompareTag("enemy"))
        {
            Break();
        }
        
    }

    public void ForceBreak()
    {
        force = true;
        Break();
    }

    private void Break()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rbs.Length; i++)
        {
            rbs[i].isKinematic = false;
        }
        timeLeft = time;
        Destroy(gameObject, time);
    }

    private void Start()
    {
        mrs = GetComponentsInChildren<MeshRenderer>();
    }

    private void Update()
    {
        if(timeLeft >= 0 && timeLeft<= time)
        {
            for(int i = 0;i < mrs.Length; i++)
            {
                mrs[i].material.color = new Color(mrs[i].material.color.r, mrs[i].material.color.g, mrs[i].material.color.b, timeLeft / time);
            }
        }

        timeLeft -= Time.deltaTime;
    }

}
