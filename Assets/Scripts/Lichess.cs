using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit.Input;
using System.Runtime.CompilerServices;
using System;
using Unity.Collections;
using UnityEngine.UI;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using System.ComponentModel;

using System.Threading.Tasks;
using Unity.Mathematics;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
public class Lichess : MonoBehaviour
{
    private string gameId;

    public GameObject indicator;

    [HideInInspector]
    public bool eventThreadStarted = false;
    [HideInInspector]
    public bool seekCreated = false;

    public NativeList<Char> moves = new NativeList<Char>(Allocator.Persistent);

    private string playerColor = "";

    [HideInInspector]
    public string colorChoice = "random";
    [HideInInspector]
    public int moveCount;
    [HideInInspector]
    public int turnCount = 0;
    [HideInInspector]
    public bool spawnChessBoard = false;

    bool gameOver = false;

    private string token = "lip_sXCCP6GO5OLqTFLDBvjr";

    private bool canStartGameStream = false;

    private Chessboard cb;

    bool moveMade = false;

    int wTime = 0;
    int bTime = 0;

    public GameObject loader;
    public GameObject loaderText;

    public GameObject whiteTime;
    public GameObject blackTime;


    public GameObject hud;

    private bool isPVP = false;
    public GameObject pvpHand;
    public GameObject aiHand;

    [HideInInspector]
    public int level = 1;

    private int turn_ = -2;

    private int pvpTime = 15;
    private int pvpInc = 13;

    public TextMeshPro pvpText;
    public TextMeshPro pvpIncText;

    private void Start()
    {
        cb = GetComponent<Chessboard>();
    }

    // Update is called once per frame

    //The code in the update function checks if there are any available moves.
    //It gives access to the latest move and updates the turnCount which allows you to tell whose turn it is.
    void Update()
    {

        if (spawnChessBoard && playerColor!="")
        {

            cb.setPlayerColor(playerColor);
            cb.GenerateAllTiles();
            cb.SpawnAllPieces();
            cb.PositionAllPieces();
            cb.SpawnBorder();
            cb.PlaceChessboardRelativeToCamera();
            indicator.SetActive(true);
            spawnChessBoard = false;
            hud.SetActive(true);

            //if pvp
            if (isPVP)
            {

                whiteTime.SetActive(true);
                blackTime.SetActive(true);

                whiteTime.GetComponent<PlayerTimer>().showTimer();
                blackTime.GetComponent<PlayerTimer>().showTimer();

                whiteTime.GetComponent<PlayerTimer>().startPlaying();
                pvpHand.SetActive(true);
            }
            else
            {
                whiteTime.SetActive(false);
                blackTime.SetActive(false);

                aiHand.SetActive(true);
            }

            cb.auxObj.GetComponent<AudioHandler>().PlayStartGame();
            
        }


        //Initiate pvp after starting event stream. No need to touch.
        if (eventThreadStarted && !seekCreated)
        {
            seekCreated = true;
            createSeek();
        }

        if (canStartGameStream)
        {
            canStartGameStream = false;
            startGameStream();
        }

        if (isPVP && cb.currentTeam!=-1 && turn_ != cb.currentTeam)
        {
            if (cb.currentTeam == 0)
            {
                whiteTime.GetComponent<PlayerTimer>().startPlaying();
                blackTime.GetComponent<PlayerTimer>().stopPlaying();
            }
            else
            {
                blackTime.GetComponent<PlayerTimer>().startPlaying();
                whiteTime.GetComponent<PlayerTimer>().stopPlaying();
            }
            turn_ = cb.currentTeam;
        }


    }

    //Resets the pvp flags so that the game can handle a second pvp game. 
    public void resetState()
    {
        eventThreadStarted = false;
        seekCreated = false;
        wTime = 0;
        bTime = 0;

        /*        Transform[] objs = GetComponentsInChildren<Transform>();


                for (int i = 0; i < objs.Length; i++)
                {
                    Destroy(objs[i].gameObject);
                }

                spawnChessBoard = false;
                cb.chessPieceCreated = false;
        */
    }

    //Function to abort an AI game. Attach to a unity button. 
    public async void abortGame()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpContent xyz = new StringContent("test");
        await client.PostAsync($"https://lichess.org/api/board/game/{gameId}/abort", xyz);
        gameOver = true;

        Debug.Log("Aborted " + gameId);
        resetState();
    }

    //Function to abort a PVP game. Attach to a unity button. 
    public async void resignGame()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpContent xyz = new StringContent("test");
        await client.PostAsync($"https://lichess.org/api/board/game/{gameId}/resign", xyz);
        gameOver = true;

        Debug.Log("Resigned " + gameId);
        resetState();
    }

    public async void drawGame()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpContent xyz = new StringContent("test");
        await client.PostAsync($"https://lichess.org/api/board/game/{gameId}/draw", xyz);
        gameOver = true;

        Debug.Log("Offered Draw: " + gameId);
        resetState();
    }


    //This function initiates a game with the ai and creates a thread which streams the game data. Attach this to the UI button 
    //which will start the game. 
    public async void startGame(string cC = "random")
    {

        if (cC == "random")
        {
            float rand = UnityEngine.Random.value;
            if (rand > 0.5f)
            {
                cC = "white";
            }
            else
            {
                cC = "black";
            }
        }

        gameOver = false;

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Debug.Log("Starting Game");

        var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("level", level+""),
                new KeyValuePair<string, string>("clock.limit", "10800"),
                new KeyValuePair<string, string>("clock.increment", "10"),
                new KeyValuePair<string, string>("variant", "standard"),
                new KeyValuePair<string, string>("color", cC)
            };

        // Encodes the key-value pairs for the ContentType 'application/x-www-form-urlencoded'
        HttpContent gameSettings = new FormUrlEncodedContent(formData);

        var game = await client.PostAsync("https://lichess.org/api/challenge/ai", gameSettings);

        string response = await game.Content.ReadAsStringAsync();

        string[] responseSplit = response.Split('"');

        Debug.Log(response);

        gameId = responseSplit[3];


        Debug.Log(gameId);
        Debug.Log(cC);

        spawnChessBoard = true;

        Debug.Log(cC);

        playerColor = cC;



        Debug.Log("Starting Thread");

        Thread myThread = new Thread(new ThreadStart(streamGame));
        myThread.Start();

        Debug.Log("Finished Starting Thread");

        Debug.Log("Finished Game");
    }

    //This function opens an http stream which will send the game data live. This function doesn't need to be called, 
    //another function calls it inside a thread because an http stream goes in indefinitely until the game ends. 
    private async void streamGame()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Debug.Log("Starting Game Stream");

        using (Stream response = await client.GetStreamAsync($"https://lichess.org/api/board/game/stream/{gameId}"))
        {
            while (true)
            {
                var line = new List<char>();

                while (true)
                {
                    var nextByte = response.ReadByte();

                    if (gameOver)
                    {
                        return;
                    }

                    if (nextByte == -1)
                    {
                        break;
                    }
                    line.Add(Char.ConvertFromUtf32(nextByte)[0]);
                    if (nextByte == '\n')
                    {
                        string gameLine = string.Join("", line).Trim();
                        string[] lineSplit = gameLine.Split('"');

                        if (lineSplit[lineSplit.Length - 1].Trim() == "player")
                        {
                            Debug.Log(gameLine);
                        }

                        if (gameLine.Length == 0)
                        {
                            Debug.Log("KeepAlive Line");
                            line.Clear();
                        }
                        else if (lineSplit[1] == "type")
                        {
                            string incomingMoves = lineSplit[7];
                            for (int i = 0; i < incomingMoves.Length; i++)
                            {
                                moves.Add(incomingMoves[i]);
                            }
                            line.Clear();
                            Debug.Log(gameLine);
                            wTime = Int32.Parse(lineSplit[10].Substring(1, lineSplit[10].Length - 2)) / 1000;
                            Debug.Log(wTime);
                            bTime = Int32.Parse(lineSplit[12].Substring(1, lineSplit[12].Length - 2)) / 1000;
                            Debug.Log(bTime);
                        }
                        else
                        {
                            bTime = Int32.Parse(lineSplit[lineSplit.Length - 9].Substring(1, lineSplit[lineSplit.Length - 9].Length - 2)) / 1000;
                            Debug.Log(bTime);
                            wTime = Int32.Parse(lineSplit[lineSplit.Length - 11].Substring(1, lineSplit[lineSplit.Length - 11].Length - 2)) / 1000;
                            Debug.Log(wTime);
                            Debug.Log("GameStart Line");
                            line.Clear();
                        }
                    }
                }
            }

        }

    }

    //Function that will initiate a PVP game. Similar abstraction as the AI game's function. Just attach to a button. 
    public void startPVPGame()
    {
        //Create event stream in thread
        gameOver = false;
        startEventThread();
    }

    //Starts event stream within a thread. 
    private void startEventThread()
    {
        Debug.Log("Starting Event Stream Thread");

        Thread eventThread = new Thread(new ThreadStart(startEventStream));
        eventThread.Start();

        Debug.Log("Finished Starting Event Stream Thread");
    }

    //Starts an event stream that keeps track of when a pvp game starts. This function runs in a thread. No need to attach to any unity stuff. 
    private async void startEventStream()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Debug.Log("Starting Event Stream");

        using (Stream response = await client.GetStreamAsync($"https://lichess.org/api/stream/event"))
        {
            while (true)
            {
                var line = new List<char>();

                while (true)
                {
                    var nextByte = response.ReadByte();

                    if (!eventThreadStarted)
                    {
                        eventThreadStarted = true;
                        Debug.Log("Set Event Stream Bool to True");
                    }

                    if (nextByte == -1)
                    {
                        break;
                    }
                    line.Add(Char.ConvertFromUtf32(nextByte)[0]);
                    if (nextByte == '\n')
                    {
                        string gameLine = string.Join("", line).Trim();
                        string[] lineSplit = gameLine.Split('"');

                        Debug.Log(gameLine);

                        if (gameLine.Length == 0)
                        {
                            Debug.Log("KeepAlive Line");
                            line.Clear();
                        }
                        else if (lineSplit[3] == "gameStart")
                        {

                            gameId = lineSplit[13].Trim();
                            playerColor = lineSplit[21].Trim();

                            spawnChessBoard = true;

                            Debug.Log(lineSplit[13]);
                            Debug.Log(lineSplit[21]);
                            canStartGameStream = true;
                            break;
                        }
                        else
                        {
                            Debug.Log("GameStart Line");
                            line.Clear();
                        }
                    }
                }

                break;
            }

        }
    }

    //Seeks out a player to play chess with. No need to explicitly call. But you have a loader or something since it can take some time. 
    private async void createSeek()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("rated", "false"),
                new KeyValuePair<string, string>("time", pvpTime+""),
                new KeyValuePair<string, string>("increment", pvpInc+""),
                new KeyValuePair<string, string>("variant", "standard"),
                new KeyValuePair<string, string>("color", "random")
            };

        // Encodes the key-value pairs for the ContentType 'application/x-www-form-urlencoded'
        HttpContent gameSettings = new FormUrlEncodedContent(formData);

        Debug.Log("Starting Seek");

        var game = await client.PostAsync("https://lichess.org/api/board/seek", gameSettings);
        //YOU CAN PUT LOADER HERE IF ANY

        Debug.Log(await game.Content.ReadAsStringAsync());

        //YOU CAN STOP LOADER HERE

        loader.SetActive(false);
        loaderText.SetActive(false);
        isPVP = true;

        Debug.Log("Finished Seek");
    }

    private void startGameStream()
    {
        Debug.Log("Starting Game Stream Thread");

        Thread myThread = new Thread(new ThreadStart(streamGame));
        myThread.Start();

        Debug.Log("Finished Starting Game Stream Thread");
    }




    //This function makes the move in the game. Input is in UCI eg. e2e4. I'm intending this function to be called within another script that
    //that would deal with moving pieces in the board. 
    public async void sendMove(string move)
    {
        moveMade = true;
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Debug.Log("Sending Move");

        var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("gameId123", gameId),
                //CHANGE moveInput.text TO move
                new KeyValuePair<string, string>("move123", move)
        };

        HttpContent moveSettings = new FormUrlEncodedContent(formData);

        var game = await client.PostAsync($"https://lichess.org/api/board/game/{gameId}/move/{move}", moveSettings);

        Debug.Log(await game.Content.ReadAsStringAsync());
        Debug.Log("Sent Move");
    }


    private void OnApplicationQuit()
    {
        gameOver = true;
        //auto resign and abort
        resignGame();
        abortGame();
        moves.Dispose();
    }

    public int getWhiteTime()
    {
        return wTime;
    }

    public int getBlackTime()
    {
        return bTime;
    }

    public void enableLoader()
    {
        loader.SetActive(true);
        loaderText.SetActive(true);
    }

    //__________________________________________________________

    public void SetLevel1()
    {
        level = 1;
    }
    public void SetLevel2()
    {
        level = 2;
    }
    public void SetLevel3()
    {
        level = 3;
    }
    public void SetLevel4()
    {
        level = 4;
    }
    public void SetLevel5()
    {
        level = 5;
    }
    public void SetLevel6()
    {
        level = 6;
    }
    public void SetLevel7()
    {
        level = 7;
    }
    public void SetLevel8()
    {
        level = 8;
    }

    public void SetColorBlack()
    {
        startGame("black");
    }

    public void SetColorWhite()
    {
        startGame("white");
    }

    public void SetColorRandom()
    {
        startGame("random");
    }

    public void SetPvpTimer(SliderEventData data)
    {
        int[] times = new int[11] {1, 2, 3, 5 ,10, 15, 20, 25, 30, 45, 60};
        pvpTime = times[(int)Mathf.Round(data.Slider.SliderValue * 10)];
        pvpText.text = pvpTime+"";
        whiteTime.GetComponent<PlayerTimer>().setTime(pvpTime*60);
        blackTime.GetComponent<PlayerTimer>().setTime(pvpTime*60);

    }
    
    public void SetPvpIncrement(SliderEventData data)
    {
        pvpInc = (int)Mathf.Round(data.Slider.SliderValue * 10)+8;
        pvpIncText.text = pvpInc+"";
        whiteTime.GetComponent<PlayerTimer>().setIncrement(pvpInc);
        blackTime.GetComponent<PlayerTimer>().setIncrement(pvpInc);
    }


}

