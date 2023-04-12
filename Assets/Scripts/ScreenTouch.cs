using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenTouch : MonoBehaviour
{
    public float rightFingerID, leftFingerId;
    public float screenWidth;
    public Vector2 lookInput;

    public Vector2 moveTouchStartPosition;
    public Vector2 moveInput;
    public float sensitivity;
    // Start is called before the first frame update
    void Start()
    {
        leftFingerId = -1;
        rightFingerID = -1;
        screenWidth = Screen.width;
    }

    // Update is called once per frame
    void Update()
    {
        GetTouch();
    }

    private void GetTouch()
    {
        for(int i = 0; i < Input.touchCount; i ++)
        {
            Touch t = Input.GetTouch(i);

            switch (t.phase)
            {
                case TouchPhase.Began:
                    if(t.position.x > (screenWidth/2) && rightFingerID == -1)
                    {
                        rightFingerID = t.fingerId;
                    }
                    else if(t.position.x < (screenWidth / 2) && leftFingerId == -1)
                    {
                        leftFingerId = t.fingerId;
                        moveTouchStartPosition = t.position;
                    }

                    break;

                case TouchPhase.Canceled:   
                case TouchPhase.Ended:
                    if (t.fingerId == rightFingerID)
                        rightFingerID = -1;
                    else if(t.fingerId == leftFingerId)
                    {
                        leftFingerId = -1;
                        moveTouchStartPosition = moveInput = Vector2.zero;
                    }
                        //lookInput = Vector2.zero;
                    break;

                case TouchPhase.Moved:
                    if (rightFingerID == t.fingerId)
                        lookInput = t.deltaPosition * Time.deltaTime * sensitivity;
                    else if (leftFingerId == t.fingerId)
                        moveInput = t.position - moveTouchStartPosition;
                    break;

                case TouchPhase.Stationary:
                    lookInput = Vector2.zero;

                    break;


            }
        }
    }
    public void SetSensitivity(float Newsensitivity)
    {
        sensitivity = Newsensitivity;
    }
}
