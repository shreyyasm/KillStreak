using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomCanvasManager : MonoBehaviour
{
    [SerializeField] GameObject MainScreenCanvas;
    [SerializeField] List<GameObject> SubCanvases;
    [SerializeField] List<GameObject> ViewCameras;

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
    }
    public void BackToMainScreen()
    {
        MainScreenCanvas.SetActive(true);
        foreach (GameObject canvas in SubCanvases) //   <--- go back to here --------+
        {          
            // do work
            canvas.SetActive(false);
        }
    }
    public void OpenCustomizationTab()
    {
        SubCanvases[0].SetActive(true);
        MainScreenCanvas.SetActive(false);
    }
}
