using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using EOSLobbyTest;
public class PointSystem : MonoBehaviour
{
    public static PointSystem Instance;
    [SerializeField] GameManagerEOS gameManagerEOS;

    [SerializeField] public int RedTeamScore = 0;
    [SerializeField] public int BlueTeamScore = 0;

    public bool GameStarted;
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        Countdown();
    }
    IEnumerator StartGameTimer()
    {
        yield return new WaitForSeconds(5f);

    }
    public TextMeshProUGUI TimerGameStart;
    public float timeRemaining;
    public bool countdownState;
    public void GameStartCountdown()
    {
        countdownState = true;
        TimerGameStart.enabled = true;
    }
    public void Countdown()
    {
        if (countdownState)
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
                gameManagerEOS.ResetPosition();
                GameStarted = true;
                countdownState = false;
            }

        }
        else
        {
            timeRemaining = 10;
            //TimerLoadout.text = "TIME'S UP!";

        }
    }
}
