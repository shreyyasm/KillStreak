using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadOutManager : NetworkBehaviour
{
    [SerializeField] PlayerGunSelector playerGunSelector;
    [SerializeField] WeaponSwitching weaponSwitching;
    [SerializeField] GameObject LoadOutMenu;

    [SerializeField] List<GameObject> loadOutsUI;
    [SerializeField] List<GameObject> selectedHighlightUI;

    [SerializeField] List<GunScriptableObject> PrimaryGuns;
    [SerializeField] List<GunScriptableObject> SecondaryGuns;

    [SerializeField] List<GameObject> PrimaryGunsUI;
    [SerializeField] List<GameObject> SecondaryGunsUI;


    [SerializeField]
    private Transform GunParent;
    public GunScriptableObject gun1;
    public GunScriptableObject gun2;

    public int loadNumber = 0;
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
        SetGunUI(loadNumber);
    }
    public void SetGunUI(int loadOutNumber)
    {
        foreach (GameObject Gun in PrimaryGunsUI) //   <--- go back to here --------+
        {                               //                                |
            if (Gun == PrimaryGunsUI[loadOutNumber])             //                                |
            {      
                //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            PrimaryGunsUI[loadOutNumber].SetActive(true);// do work
            Gun.SetActive(false);
        }
        foreach (GameObject Gun in SecondaryGunsUI) //   <--- go back to here --------+
        {                               //                                |
            if (Gun == SecondaryGunsUI[loadOutNumber])             //                                |
            {
                //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            SecondaryGunsUI[loadOutNumber].SetActive(true);// do work
            Gun.SetActive(false);
        }
    }
    public void GetLoadOutInput(int loadOutNumber)
    {
        weaponSwitching.SetActiveGun(loadOutNumber);
        if (weaponSwitching.selectedWeapon == 1)
        {
            weaponSwitching.ChangeLoadoutIndex();
        }
            
        loadNumber = loadOutNumber;
        //GetGunLoadOut(loadOutNumber);
        //UI Part
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
        playerGunSelector.ChangeGunLoadOut(loadOutNumber);
        SetGunUI(loadOutNumber);
        LoadOutMenu.SetActive(false);
        //GetGunLoadOut(loadOutNumber);
    }
    public void GetGunLoadOut(int LoadOutNumber)
    {
        //DestroyPreviousGuns();
        Debug.Log("work");
        PrimaryGuns[LoadOutNumber].Spawn(GunParent, this);
        SecondaryGuns[LoadOutNumber].Spawn(GunParent, this);

    }
    public void DestroyPreviousGuns()
    {
        Transform[] OldGuns = GunParent.GetComponentsInChildren<Transform>();
        foreach (Transform transform in OldGuns)
        {
            DestroyImmediate(transform, true);
        }

        //DestroyImmediate(playerGunSelector.gun1,true);
        //DestroyImmediate(playerGunSelector.gun2, true);


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
