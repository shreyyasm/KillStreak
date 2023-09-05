using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using EOSLobbyTest;
public class PointSystem : MonoBehaviour
{
    public static PointSystem Instance;
    [SerializeField] GameManagerEOS gameManagerEOS;
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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
                countdownState = false;
            }
                


        }
        else
        {
            timeRemaining = 10;
            //TimerLoadout.text = "TIME'S UP!";
            
        }
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
}
