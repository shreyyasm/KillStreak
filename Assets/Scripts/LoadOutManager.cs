using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadOutManager : MonoBehaviour
{
    [SerializeField] List<GameObject> loadOuts;
    [SerializeField] List<GameObject> selectedHighlight;
    private void Awake()
    {
        loadOuts[0].SetActive(true);
        selectedHighlight[0].SetActive(true);
        foreach (GameObject number in loadOuts) //   <--- go back to here --------+
        {                               //                                |
            if (number == loadOuts[0])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            number.SetActive(false);
        }
        foreach (GameObject selectedLoadOut in selectedHighlight) //   <--- go back to here --------+
        {                               //                                |
            if (selectedLoadOut == selectedHighlight[0])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            selectedLoadOut.SetActive(false);
        }
    }
    public void GetLoadOutInput(int loadOutNumber)
    {
        loadOuts[loadOutNumber].SetActive(true);
        selectedHighlight[loadOutNumber].SetActive(true);
        foreach (GameObject number in loadOuts) //   <--- go back to here --------+
        {                               //                                |
            if (number == loadOuts[loadOutNumber])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            number.SetActive(false);
        }
        foreach (GameObject selectedLoadOut in selectedHighlight) //   <--- go back to here --------+
        {                               //                                |
            if (selectedLoadOut == selectedHighlight[loadOutNumber])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            selectedLoadOut.SetActive(false);
        }

    }
}
