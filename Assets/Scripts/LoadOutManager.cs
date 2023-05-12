using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadOutManager : NetworkBehaviour
{
    [SerializeField] GameObject LoadOutMenu;

    [SerializeField] List<GameObject> loadOutsUI;
    [SerializeField] List<GameObject> selectedHighlightUI;

    [SerializeField] List<ScriptableObject> Loadout1;
    [SerializeField] List<ScriptableObject> Loadout2;
    [SerializeField] List<ScriptableObject> Loadout3;
    [SerializeField] List<ScriptableObject> Loadout4;
    [SerializeField] List<ScriptableObject> Loadout5;

    [SerializeField]
    private Transform GunParent;
    public GunScriptableObject gun1;
    public GunScriptableObject gun2;
    private void Awake()
    {
        loadOutsUI[0].SetActive(true);
        selectedHighlightUI[0].SetActive(true);
        foreach (GameObject number in loadOutsUI) //   <--- go back to here --------+
        {                               //                                |
            if (number == loadOutsUI[0])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            number.SetActive(false);
        }
        foreach (GameObject selectedLoadOut in selectedHighlightUI) //   <--- go back to here --------+
        {                               //                                |
            if (selectedLoadOut == selectedHighlightUI[0])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            selectedLoadOut.SetActive(false);
        }
    }
    public void GetLoadOutInput(int loadOutNumber)
    {
        loadOutsUI[loadOutNumber].SetActive(true);
        selectedHighlightUI[loadOutNumber].SetActive(true);
        foreach (GameObject number in loadOutsUI) //   <--- go back to here --------+
        {                               //                                |
            if (number == loadOutsUI[loadOutNumber])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            number.SetActive(false);
        }
        foreach (GameObject selectedLoadOut in selectedHighlightUI) //   <--- go back to here --------+
        {                               //                                |
            if (selectedLoadOut == selectedHighlightUI[loadOutNumber])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            selectedLoadOut.SetActive(false);
        }

    }
    public void GetGunLoadOut(GunScriptableObject PrimaryGun, GunScriptableObject SecondayGun)
    {
        gun1.Spawn(GunParent, this);
        gun2.Spawn(GunParent, this);
    }
    public void OpenLoadOut()
    {
        LoadOutMenu.SetActive(true);
    }
    public void CloseLoadOut()
    {
        LoadOutMenu.SetActive(false);
    }
}
