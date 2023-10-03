using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using EOSLobbyTest;
using FishNet.Object;

public class PointSystem : NetworkBehaviour
{
    public static PointSystem Instance;
    [SerializeField] GameManagerEOS gameManagerEOS;

    [SerializeField] public int RedTeamScore = 0;
    [SerializeField] public int BlueTeamScore = 0;

    public TextMeshProUGUI TimerGameStart;
    public float timeRemaining;
    public bool countdownStateStart;

    public TextMeshProUGUI RespawnTimer;
    public float timeRemainingRespawn;
    public bool countdownStateRespawn;

    public PlayerRespawn playerRespawn;
    public bool GameStarted;
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
       // GameStartCountdown();
    }
    
    // Update is called once per frame
    void Update()
    {
        GameStartTimer();
        RespawnStartTimer();
    }
 
    public void GameStartCountdown()
    {
        countdownStateStart = true;
        TimerGameStart.enabled = true;
    }
    public void GameStartTimer()
    {
        if (countdownStateStart)
        {
            //timeRemaining = 5;
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                TimerGameStart.text = "Game Starts In(" + (int)timeRemaining + "s)";
            }
            else
            {

                TimerGameStart.enabled = false;
                PlayerRespawn.Instance.ResetPosition();
                PlayerRespawn.Instance.HideLoadOutButton();
                GameStarted = true;
                countdownStateStart = false;
            }

        }
        else
        {
            timeRemaining = 10;
            //TimerLoadout.text = "TIME'S UP!";

        }
    }
    public void RespawnStartCountdown()
    {
        countdownStateRespawn = true;
        RespawnTimer.enabled = true;
    }
    public void RespawnStartTimer()
    {
        if (countdownStateRespawn)
        {
            //timeRemaining = 5;
            if (timeRemainingRespawn > 0)
            {
                timeRemainingRespawn -= Time.deltaTime;
                RespawnTimer.text = "Respawns In(" + (int)timeRemainingRespawn + "s)";
            }
            else
            {

                RespawnTimer.enabled = false;
                countdownStateRespawn = false;
            }

        }
        else
        {
            timeRemainingRespawn = 10;
            //TimerLoadout.text = "TIME'S UP!";

        }
    }
}
