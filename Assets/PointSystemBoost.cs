using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSystemBoost : MonoBehaviour
{
    public PointSystem pointSystem;
    public bool RedTeamButton;
    public bool BlueTeamButton;

    public GameObject buttonCanvas;
    public bool ButtonPressed;
    public float ButtonResetTimer;
    public Outline outline;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ButtonTimer();
    }
    public void ButtonTimer()
    {
        if(ButtonPressed)
        {
            ButtonResetTimer -= Time.deltaTime;
        }
        if(ButtonResetTimer <= 0)
        {
            ButtonPressed = false;
            ButtonResetTimer = 30f;

        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(!ButtonPressed)
        {
            if (BlueTeamButton)
            {
                if (other.gameObject.GetComponent<PlayerGunSelector>().redTeamPlayer)
                {
                    buttonCanvas.SetActive(true);
                    outline.enabled = true;
                    //pointSystem.AddScoreToBoostRed();
                }

            }
            else
            {
                if (other.gameObject.GetComponent<PlayerGunSelector>().blueTeamPlayer)
                {
                    buttonCanvas.SetActive(true);
                    outline.enabled = true;
                    //pointSystem.AddScoreToBoostRed();
                }

            }
            
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        buttonCanvas.SetActive(false);
        outline.enabled = false;
    }
    public void BoostScore()
    {
        if(BlueTeamButton)
        {
            ButtonPressed = true;
            pointSystem.AddScoreToBoostRed();
            buttonCanvas.SetActive(false);
        }
            
        else
        {
            ButtonPressed = true;
            pointSystem.AddScoreToBoostBlue();
            buttonCanvas.SetActive(false);
        }
            

    }
}
