using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadOutManager : NetworkBehaviour
{
    public static LoadOutManager Instance;
    [SerializeField] PlayerGunSelector playerGunSelector;
    [SerializeField] WeaponSwitching weaponSwitching;
    [SerializeField] GameObject LoadOutMenu;

    public TextMeshProUGUI TimerLoadout;
    public float timeRemaining;
    public bool countdownState;

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

    [field: SyncVar]
    public int loadNumber{ get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }
    public AudioSource audioSource;
    public AudioClip loadoutUISFX;
    public AudioClip loadoutSFX;

    public Animator anim;
    private void Awake()
    {
        if (Instance != null)
            Instance = this;
        loadNumber = 0;
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
    private void Update()
    {
        if (!base.IsOwner)
            return;
        Countdown();
       
        if(anim!= null)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("LoadOutChange") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
                anim.SetBool("LoadOutDone", false);
        }
        

    }
    
    public void SetGunUI(int loadOutNumber)
    {
        
        if(PrimaryGunsUI[0] != null)
        {
            foreach (GameObject Gun in PrimaryGunsUI) //   <--- go back to here --------+
            {                               //                                |
                if (Gun == PrimaryGunsUI[loadOutNumber])             //                                |
                {
                    //                                |
                    continue;   // Skip the remainder of this iteration. -----+
                }
                if (PrimaryGunsUI[0] != null)
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
                if (PrimaryGunsUI[0] != null)
                    SecondaryGunsUI[loadOutNumber].SetActive(true);// do work
                Gun.SetActive(false);
            }
        }
            
        
        
    }
    public void GetLoadOutInput(int loadOutNumber)
    {
        
        if (base.IsServer)
            GetLoadOutInputObserver(loadOutNumber);

        else
            GetLoadOutInputServer(loadOutNumber);
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void GetLoadOutInputServer(int loadOutNumber)
    {
        
        loadNumber = loadOutNumber;
        
        weaponSwitching.SetActiveGun(loadOutNumber);
        if (weaponSwitching.selectedWeapon == 1)
        {
           
            weaponSwitching.ChangeLoadoutIndex();
        }
        
        //loadNumber = loadOutNumber;
        //GetGunLoadOut(loadOutNumber);
        //UI Part
        if (loadOutsUI[0] != null)
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
        }

        if (selectedHighlightUI[0] != null)
        {
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
        audioSource.PlayOneShot(loadoutUISFX);
        playerGunSelector.ChangeGunLoadOut(loadOutNumber);
        SetGunUI(loadOutNumber);


    }
    [ObserversRpc(BufferLast = true)]
    public void GetLoadOutInputObserver(int loadOutNumber)
    {
        
        loadNumber = loadOutNumber;
        
        weaponSwitching.SetActiveGun(loadOutNumber);
        if (weaponSwitching.selectedWeapon == 1)
        {          
            weaponSwitching.ChangeLoadoutIndex();
        }

        //loadNumber = loadOutNumber;
        //GetGunLoadOut(loadOutNumber);
        //UI Part
        if (loadOutsUI[0] != null)
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
        }
        if(selectedHighlightUI[0] != null)
        {
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
        
        audioSource.PlayOneShot(loadoutUISFX);
        playerGunSelector.ChangeGunLoadOut(loadOutNumber);
        SetGunUI(loadOutNumber);


    }
   
    public void OpenLoadOut()
    {
        LoadOutMenu.SetActive(true);
        audioSource.PlayOneShot(loadoutUISFX);
        countdownState = true;
        timeRemaining = 6;
        
        StartCoroutine(playLoadoutSound());

    }
    public void CloseLoadOut()
    {
        if (anim != null)
            anim.SetBool("LoadOutDone", true);
        audioSource.PlayOneShot(loadoutSFX);
        timeRemaining = 0;
        StopAllCoroutines();
        LoadOutMenu.SetActive(false);
    }
    public void Countdown()
    { 
        if(countdownState)
        {
            //timeRemaining = 5;
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                TimerLoadout.text = "Select your Loadout(" + (int)timeRemaining + "s)";
                
                    
            }
            else
                countdownState = false;
            

        }       
        else
        {
            
            //TimerLoadout.text = "TIME'S UP!";
            LoadOutMenu.SetActive(false);

        }
        
    }
    public IEnumerator playLoadoutSound()
    {
        yield return new WaitForSeconds(6f);
        if (anim != null)
            anim.SetBool("LoadOutDone", true);
        audioSource.PlayOneShot(loadoutSFX);
    }
}
