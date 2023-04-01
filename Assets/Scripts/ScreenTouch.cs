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
                    break;

                case TouchPhase.Canceled:
                    lookInput = Vector2.zero;
                    break;

                case TouchPhase.Ended:
                    if (t.fingerId == rightFingerID)
                        rightFingerID = -1;
                        lookInput = Vector2.zero;
                    break;

                case TouchPhase.Moved:
                    if (rightFingerID == t.fingerId)
                        lookInput = t.deltaPosition * Time.deltaTime * sensitivity;
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
