using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScreenTouch : MonoBehaviour
{
    public float rightFingerID, leftFingerId;
    public float screenWidth;

    public RectTransform rectTransform;
    public float screenShowcaseWidth;
    public Vector2 lookInput;

    public Vector2 moveTouchStartPosition;
    public Vector2 moveInput;

    public Vector2 moveShowInput;
    public float sensitivity;
    public float sensitivityShowcase;

    public bool MainMenu;

    // Start is called before the first frame update
    Touch t;
    void Start()
    {
        
        leftFingerId = -1;
        rightFingerID = -1;
        screenWidth = Screen.width;

      

    }

    void Update()
    {

        //GetTouch();
        if(MainMenu)
            GetTouchForShowcase();
    }
 
    private void GetTouch()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).position.x > (screenWidth / 2))
            {
                t = Input.GetTouch(i);
                //Debug.Log(t.fingerId);
                continue;
            }
        }
        switch (t.phase)
        {
            case TouchPhase.Began:
                if (rightFingerID == -1)
                    rightFingerID = t.fingerId;
                break;

            case TouchPhase.Canceled:
            case TouchPhase.Ended:
                if (t.fingerId == rightFingerID)
                    rightFingerID = -1;
                lookInput = Vector2.zero;
                break;


            case TouchPhase.Moved:
                if (t.position.x > (screenWidth / 2))
                {
                    float valueX = t.deltaTime > 0 ? t.deltaPosition.x / Screen.width / t.deltaTime : 0;
                    float valuY = t.deltaTime > 0 ? t.deltaPosition.y / Screen.width / t.deltaTime : 0;
                    Vector2 net = new Vector2(valueX, valuY);
                    //Debug.Log(net);
                    //if (Mathf.Abs(t.deltaPosition.x) > Mathf.Abs(t.deltaPosition.y))
                    //{
                    //    //Debug.Log("Delta Pos " + t.deltaPosition);
                        
                    //    lookInput = net * Time.deltaTime * sensitivity;
                    //}
                    //if (Mathf.Abs(t.deltaPosition.x) < Mathf.Abs(t.deltaPosition.y))
                    //{
                        

                    //    lookInput = net * Time.deltaTime * sensitivity;
                    //}
                    lookInput = net * Time.deltaTime * sensitivity;
                }
                else
                    lookInput = Vector2.zero;

                break;

            case TouchPhase.Stationary:
                lookInput = Vector2.zero;           
                break;     
        }
    }
    private void GetTouchForShowcase()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            switch (t.phase)
            {
                case TouchPhase.Began:

                    if (t.position.x > 520 && t.position.x < 1400)
                    {                      
                        rightFingerID = t.fingerId;
                    }



                    break;

                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    if (t.fingerId == rightFingerID)
                        rightFingerID = -1;

                    moveShowInput = Vector2.zero;
                    break;

                case TouchPhase.Moved:
                    if (rightFingerID == t.fingerId)
                        moveShowInput = t.deltaPosition * Time.deltaTime * sensitivityShowcase;
                    break;

                case TouchPhase.Stationary:
                    moveShowInput = Vector2.zero;

                    break;


            }
        }
    }
    public void SetSensitivity(float Newsensitivity)
    {
        sensitivity = Newsensitivity;
    }
}
