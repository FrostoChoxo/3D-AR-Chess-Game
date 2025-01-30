using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{

    public AudioClip startGame;
    public AudioClip move;
    public AudioClip castle;
    public AudioClip check;
    public AudioClip checkmate;
    public AudioClip wrongMove;
    public AudioClip resign;
    public AudioClip capture;
    public AudioClip[] fracture;
    public AudioClip promotion;
    AudioSource wav;

    // Start is called before the first frame update
    void Start()
    {
        wav = GetComponent<AudioSource>();
    }

    public void PlayStartGame()
    {
        wav.PlayOneShot(startGame);
    }

    public void PlayMove()
    {
        wav.PlayOneShot(move);
    }

    public void PlayCastle()
    {
        wav.PlayOneShot(castle);
    }
    public void PlayCheck()
    {
        wav.PlayOneShot(check);
    }
    public void PlayCheckmate() {
        wav.PlayOneShot(checkmate);

    }
    public void PlayWrongMove() {
        wav.PlayOneShot(wrongMove,0.3f); 

    }

    public void PlayResign()
    {
        wav.PlayOneShot(resign);

    }

    public void PlayCapture()
    {
        wav.PlayOneShot(capture);
    }

    public void PlayFracture() {
        wav.PlayOneShot(fracture[(int)(Random.value*fracture.Length)]);
    }

    public void PlayPromotion()
    {
        wav.PlayOneShot(promotion);
    }
}
