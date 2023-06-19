using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomCanvasManager : MonoBehaviour
{
    [SerializeField] GameObject MainScreenCanvas;
    [SerializeField] List<GameObject> SubCanvases;
    [SerializeField] List<GameObject> ViewCameras;

    [SerializeField] GameObject Player;
    [SerializeField] ScreenTouch screenTouch;

    public void OpenSubCanvas(int index)
    {
        foreach (GameObject canvas in SubCanvases) //   <--- go back to here --------+
        {                              
            if (canvas == SubCanvases[index])           
            {
                SubCanvases[index].SetActive(true);                           
                continue;   // Skip the remainder of this iteration. -----+
            }
            // do work
            canvas.SetActive(false);
        }
        if (index == 6)
            RotatePlayerToShowBag();
        else
            RotatePlayerToDefault();
    }
    public void ChangeViewCamera(int index)
    {
        foreach (GameObject camera in ViewCameras) //   <--- go back to here --------+
        {
            if (camera == ViewCameras[index])
            {
                ViewCameras[index].SetActive(true);
                continue;   // Skip the remainder of this iteration. -----+
            }
            // do work
            camera.SetActive(false);
        }
    }
    public void BackToMainCanvasButton()
    {
        foreach (GameObject canvas in SubCanvases) //   <--- go back to here --------+
        {
            if (canvas == SubCanvases[0])
            {
                SubCanvases[0].SetActive(true);
                continue;   // Skip the remainder of this iteration. -----+
            }
            // do work
            canvas.SetActive(false);
        }
        RotatePlayerToDefault();
    }
    public void BackToMainScreen()
    {
        MainScreenCanvas.SetActive(true);
        foreach (GameObject canvas in SubCanvases) //   <--- go back to here --------+
        {          
            // do work
            canvas.SetActive(false);
        }
        RotatePlayerToDefault();
    }
    public void OpenCustomizationTab()
    {
        SubCanvases[0].SetActive(true);
        MainScreenCanvas.SetActive(false);
        RotatePlayerToDefault();
    }
    private void Update()
    {
        RotatePlayer();
    }
    public void RotatePlayer()
    {
        float mouseX = screenTouch.moveShowInput.x;
        if (screenTouch.rightFingerID == -1)
        {
            mouseX = 0;           
        }
        Player.transform.Rotate(0, -mouseX * Time.deltaTime, 0, Space.World);

    }
    [SerializeField] float playerRotateTime;
    public void RotatePlayerToDefault()
    {
        Player.transform.rotation = Quaternion.Euler(0, Mathf.Lerp(Player.transform.rotation.y, 180f, playerRotateTime ),0) ;
    }
    public void RotatePlayerToShowBag()
    {
        Player.transform.rotation = Quaternion.Euler(0, Mathf.Lerp(Player.transform.rotation.y, 0f, playerRotateTime), 0);
    }
}
