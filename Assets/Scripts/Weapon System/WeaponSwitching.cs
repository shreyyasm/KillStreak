using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Animations.Rigging;

public class WeaponSwitching : NetworkBehaviour
{
    //Index of selected weapon
    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    public int selectedWeapon { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }
    [SerializeField]
    private PlayerHealth playerHealth;
    [SerializeField] ThirdPersonController thirdPersonController;
    [SerializeField] ShooterController shooterController;
    [SerializeField] PlayerGunSelector playerGunSelector;
    [SerializeField] LoadOutManager loadOutManager;
    [SerializeField] WeaponInventory weaponInventory;
    public bool gunChanged = false;

    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    public bool gunChanging { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

    public GameObject animator;
    public Transform realRifle;
    public Transform realPistol;
    NetworkObject networkObject;
    [SerializeField] GameObject fakePistol; 
    [SerializeField] GameObject fakeRifle;

    public Animator anim;
    bool gunInHand = true;
    bool checkAnimationState;
    private readonly NetworkConnection newOwnerConnection;

    public List<Transform> PrimaryGunsFakePrefabs;
    public List<Transform> SecondaryGunsFakePrefabs;
    //public override void OnStartServer()
    //{
    //    base.OnStartServer();
    //    networkObject = GetComponent<NetworkObject>();
    //    networkObject.GiveOwnership(newOwnerConnection);
    //    //This is run when the server initializes the object.
    //}
    // Start is called before the first frame update
    
    void Start()
    {
        //gunChanging = false;
        //realRifle = this.gameObject.transform.GetChild(0);
        //realPistol = this.gameObject.transform.GetChild(1);
        realRifle = playerGunSelector.PrimaryGunsPrefabs[loadOutManager.loadNumber].transform;
        realPistol = playerGunSelector.SecondaryGunsPrefabs[loadOutManager.loadNumber].transform;
       
        fakeRifle = PrimaryGunsFakePrefabs[loadOutManager.loadNumber].transform.gameObject;
        fakePistol = SecondaryGunsFakePrefabs[loadOutManager.loadNumber].transform.gameObject;

        SetActiveGun(loadOutManager.loadNumber);
        //SelectedWeapon();

        //newOwnerConnection = GetComponent<NetworkConnection>();

        //ManagerLayerWeights();
    }
    public void SetGuns()
    {
        realRifle = playerGunSelector.PrimaryGunsPrefabs[loadOutManager.loadNumber].transform;
        realPistol = playerGunSelector.SecondaryGunsPrefabs[loadOutManager.loadNumber].transform;

        fakeRifle = PrimaryGunsFakePrefabs[loadOutManager.loadNumber].transform.gameObject;
        fakePistol = SecondaryGunsFakePrefabs[loadOutManager.loadNumber].transform.gameObject;
    }
    // Update is called once per frame
    void Update()
    {
        

        if (!base.IsOwner)
            return;
       
        if (checkAnimationState)
        {
            if (anim.GetCurrentAnimatorStateInfo(4).IsName("Rifle To Pistol Locomotions") && anim.GetCurrentAnimatorStateInfo(4).normalizedTime > 1f)
            {
                gunChanging = false;
                playerGunSelector.SetActiveGun(selectedWeapon);
                playerGunSelector.ChangeCrosshair();
                thirdPersonController.changingGun = false;
                thirdPersonController.SetRigWeight();
                checkAnimationState = false;
                ManagerLayerWeights();

            }
            if (anim.GetCurrentAnimatorStateInfo(4).IsName("Rifle To Pistol Locomotions Crouch") && anim.GetCurrentAnimatorStateInfo(4).normalizedTime > 1f)
            {
                gunChanging = false;
                playerGunSelector.SetActiveGun(selectedWeapon);
                playerGunSelector.ChangeCrosshair();
                thirdPersonController.changingGun = false;
                thirdPersonController.SetRigWeight();
                checkAnimationState = false;
                ManagerLayerWeights();
            }
            if (anim.GetCurrentAnimatorStateInfo(4).IsName("Slide change gun") && anim.GetCurrentAnimatorStateInfo(4).normalizedTime > 1f)
            {
                gunChanging = false;
                playerGunSelector.SetActiveGun(selectedWeapon);
                playerGunSelector.ChangeCrosshair();
                thirdPersonController.changingGun = false;
                thirdPersonController.SetRigWeight();
                checkAnimationState = false;
                ManagerLayerWeights();

            }
        }
        //if (Input.GetMouseButtonDown(2))
        //    ChangeGunIndex();

        //ManagerLayerWeights();
    }
    public void SetActiveGun(int loadNumber)
    {
        
        if (base.IsServer)
            SetActiveGunObserver(loadNumber);

        else
            SetActiveGunServer(loadNumber);
    }
    
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void SetActiveGunServer(int loadNumber)
    {
        foreach (Transform fakeGun in SecondaryGunsFakePrefabs) //   <--- go back to here --------+
        {                               //                                |
            if (fakeGun == SecondaryGunsFakePrefabs[loadNumber])             //                                |
            {
                SecondaryGunsFakePrefabs[loadNumber].gameObject.SetActive(true);//                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            fakeGun.gameObject.SetActive(false);
        }
    }
    [ObserversRpc(BufferLast = true)]
    public void SetActiveGunObserver(int loadNumber)
    {
        foreach (Transform fakeGun in SecondaryGunsFakePrefabs) //   <--- go back to here --------+
        {                               //                                |
            if (fakeGun == SecondaryGunsFakePrefabs[loadNumber])             //                                |
            {
                SecondaryGunsFakePrefabs[loadNumber].gameObject.SetActive(true);//                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            fakeGun.gameObject.SetActive(false);
        }
    }
    public void ManagerLayerWeights()
    {
        if (gunChanging)
        {
            if (selectedWeapon == 1)
            {
                anim.SetLayerWeight(4, 1);
                anim.SetLayerWeight(5, 0);
                anim.SetBool("Gun Changing", true);
            }
            else
            {
                anim.SetLayerWeight(5, 1);
                anim.SetLayerWeight(4, 0);
                anim.SetBool("Gun Changing", true);
            }
        }
        else
        {
            if (selectedWeapon == 1)
            {
                anim.SetLayerWeight(4, 0);
                anim.SetBool("Gun Changing", false);
            }
            else
            {
                anim.SetLayerWeight(5, 0);
                anim.SetBool("Gun Changing", false);
            }
        }
    }
    void SelectedWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }

            i++;
        }
    }
    public void ChangeLoadoutIndex()
    {
        if (base.IsServer)
            ChangeLoadoutIndexObserver();

        else
            ChangeLoadoutIndexServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void ChangeLoadoutIndexServer()
    {
       
        if (!gunChanging)
        {
            //int previousSelectedWeapon = selectedWeapon;
            if (!gunChanged)
            {
                gunChanged = true;
                selectedWeapon = 0;
            }
            else
            {
                gunChanged = false;
                selectedWeapon = 0;
            }
            //if (previousSelectedWeapon != selectedWeapon)
            //{
            //    //SelectedWeapon();
            //}
            shooterController.GunChanged();
            
            //playerGunSelector.ActiveGunPrefab = playerGunSelector.PrimaryGunsPrefabs[loadOutManager.loadNumber];
            
        }
        foreach (Transform fakeGun in PrimaryGunsFakePrefabs) //   <--- go back to here --------+
        {                               //                                |         
            // do work
            fakeGun.gameObject.SetActive(false);
        }
    }
    [ObserversRpc(BufferLast = true)]
    public void ChangeLoadoutIndexObserver()
    {
       
        if (!gunChanging)
        {
            //int previousSelectedWeapon = selectedWeapon;
            if (!gunChanged)
            {

                gunChanged = true;
                selectedWeapon = 0;
            }
            else
            {
                gunChanged = false;
                selectedWeapon = 0;
            }
            //if (previousSelectedWeapon != selectedWeapon)
            //{
            //    //SelectedWeapon();
            //}
            shooterController.GunChanged();
            
            //playerGunSelector.ActiveGunPrefab = playerGunSelector.PrimaryGunsPrefabs[loadOutManager.loadNumber];
            
           
        }
        foreach (Transform fakeGun in PrimaryGunsFakePrefabs) //   <--- go back to here --------+
        {                               //                                |         
            // do work
            fakeGun.gameObject.SetActive(false);
        }
    }
    public void ChangeGunIndex(int index)
    {
        if(selectedWeapon != index)
        {
            if (base.IsClient)
                ChangeGunServer();

            if (base.IsServer)
                ChangeGunObserver();
        }
        

    }
    [ServerRpc(RequireOwnership = false , RunLocally = true)]
    public void ChangeGunServer()
    {
  
        if (!gunChanging)
        {
            
            anim.SetBool("SniperReload", false);
            anim.SetLayerWeight(8, 0);
            checkAnimationState = true;
            gunChanging = true;
            
            thirdPersonController.changingGun = true;
            thirdPersonController.SetRigWeight();
            int previousSelectedWeapon = selectedWeapon;
            
            if (!gunChanged)
            {
                gunChanged = true;
                if (selectedWeapon == 1)
                {
                    selectedWeapon--;
                }
                else
                {
                    selectedWeapon++;
                }
            }
            else
            {
                gunChanged = false;
                if (selectedWeapon == 1)
                {
                    selectedWeapon--;
                }
                else
                {
                    selectedWeapon++;
                }
            }
            if (previousSelectedWeapon != selectedWeapon)
            {
                //SelectedWeapon();
            }
            shooterController.GunChanged();
            if (selectedWeapon == 0)
            {
                playerGunSelector.ActiveGunPrefab = playerGunSelector.PrimaryGunsPrefabs[loadOutManager.loadNumber];
            }
            else
            {
                playerGunSelector.ActiveGunPrefab = playerGunSelector.SecondaryGunsPrefabs[loadOutManager.loadNumber];
            }
            
            SetGuns();
           
            ManagerLayerWeights();
        }
    }
    [ObserversRpc(BufferLast = true)]
    public void ChangeGunObserver()
    {
        if (!gunChanging)
        {
            
            anim.SetBool("SniperReload", false);
            anim.SetLayerWeight(8, 0);
            checkAnimationState = true;
            gunChanging = true;
           
            thirdPersonController.changingGun = true;
            thirdPersonController.SetRigWeight();
            int previousSelectedWeapon = selectedWeapon;
            
            if (!gunChanged)
            {
                gunChanged = true;
                if (selectedWeapon == 1)
                {
                    selectedWeapon--;
                }
                else
                {
                    selectedWeapon++;
                }
            }
            else
            {
                gunChanged = false;
                if (selectedWeapon == 1)
                {
                    selectedWeapon--;
                }
                else
                {
                    selectedWeapon++;
                }
            }
            if (previousSelectedWeapon != selectedWeapon)
            {
                //SelectedWeapon();
            }
            shooterController.GunChanged();
            if (selectedWeapon == 0)
            {
                playerGunSelector.ActiveGunPrefab = playerGunSelector.PrimaryGunsPrefabs[loadOutManager.loadNumber];
            }
            else
            {
                playerGunSelector.ActiveGunPrefab = playerGunSelector.SecondaryGunsPrefabs[loadOutManager.loadNumber];
            }
            
            SetGuns();
            
            ManagerLayerWeights();
        }
    }
    public bool GunSwaping()
    {
        return gunChanging;
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void GunSwapVisualTakeInServer()
    {
        if (animator.GetComponent<Animator>().GetLayerWeight(4) == 1)
        {
            gunInHand = false;
            if (selectedWeapon == 1)
            {
                realRifle.gameObject.SetActive(false);
                //fakeRifle.SetActive(true);
                foreach (Transform fakeGun in PrimaryGunsFakePrefabs) //   <--- go back to here --------+
                {                               //                                |
                    if (fakeGun == PrimaryGunsFakePrefabs[loadOutManager.loadNumber])             //                                |
                    {
                        PrimaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(true);//                                |
                        continue;   // Skip the remainder of this iteration. -----+
                    }

                    // do work
                    fakeGun.gameObject.SetActive(false);
                }
                //PrimaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(true);
            }
            else
            {
               
                realPistol.gameObject.SetActive(false);
                //fakePistol.SetActive(true);
                foreach (Transform fakeGun in SecondaryGunsFakePrefabs) //   <--- go back to here --------+
                {                               //                                |
                    if (fakeGun == SecondaryGunsFakePrefabs[loadOutManager.loadNumber])             //                                |
                    {
                        SecondaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(true);//                                |
                        continue;   // Skip the remainder of this iteration. -----+
                    }

                    // do work
                    fakeGun.gameObject.SetActive(false);
                }
                //SecondaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(true);
            }
        }
        if (animator.GetComponent<Animator>().GetLayerWeight(5) == 1)
        {
            gunInHand = false;
            //fakeRifle.SetActive(false);
            PrimaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(false);
            realRifle.gameObject.SetActive(true);
        }
    }

    [ObserversRpc(BufferLast = true)]
    public void GunSwapVisualTakeInObserver()
    {

        if (animator.GetComponent<Animator>().GetLayerWeight(4) == 1)
        {
            gunInHand = false;
            if (selectedWeapon == 1)
            {
                realRifle.gameObject.SetActive(false);
                //fakeRifle.SetActive(true);
                foreach (Transform fakeGun in PrimaryGunsFakePrefabs) //   <--- go back to here --------+
                {                               //                                |
                    if (fakeGun == PrimaryGunsFakePrefabs[loadOutManager.loadNumber])             //                                |
                    {
                        PrimaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(true);//                                |
                        continue;   // Skip the remainder of this iteration. -----+
                    }

                    // do work
                    fakeGun.gameObject.SetActive(false);
                }
                //PrimaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(true);
            }
            else
            {

                realPistol.gameObject.SetActive(false);
                //fakePistol.SetActive(true);
                foreach (Transform fakeGun in SecondaryGunsFakePrefabs) //   <--- go back to here --------+
                {                               //                                |
                    if (fakeGun == SecondaryGunsFakePrefabs[loadOutManager.loadNumber])             //                                |
                    {
                        SecondaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(true);//                                |
                        continue;   // Skip the remainder of this iteration. -----+
                    }

                    // do work
                    fakeGun.gameObject.SetActive(false);
                }
                //SecondaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(true);
            }
        }
        if (animator.GetComponent<Animator>().GetLayerWeight(5) == 1)
        {
            gunInHand = false;
            //fakeRifle.SetActive(false);
            PrimaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(false);
            realRifle.gameObject.SetActive(true);
        }
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void GunSwapVisualTakeOutServer()
    {
        if (animator.GetComponent<Animator>().GetLayerWeight(4) == 1)
        {
            gunInHand = true;
            if (selectedWeapon == 1)
            {
                //fakePistol.SetActive(false);
                foreach (Transform fakeGun in SecondaryGunsFakePrefabs) //   <--- go back to here --------+
                {                               //                                |                    
                    // do work
                    fakeGun.gameObject.SetActive(false);
                }
                //SecondaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(false);
                realPistol.gameObject.SetActive(true);
            }
            else
            {
                //fakeRifle.SetActive(false);
                foreach (Transform fakeGun in PrimaryGunsFakePrefabs) //   <--- go back to here --------+
                {                               //                                |                  
                    // do work
                    fakeGun.gameObject.SetActive(false);
                }
               // PrimaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(false);
                realRifle.gameObject.SetActive(true);
            }
        }
        if (animator.GetComponent<Animator>().GetLayerWeight(5) == 1)
        {

            gunInHand = false;
            realPistol.gameObject.SetActive(false);
            //fakePistol.SetActive(true);
            SecondaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(true);
        }
    }
    [ObserversRpc(BufferLast = true)]
    public void GunSwapVisualTakeOutObserver()
    {
        if (animator.GetComponent<Animator>().GetLayerWeight(4) == 1)
        {
            gunInHand = true;
            if (selectedWeapon == 1)
            {
                //fakePistol.SetActive(false);
                foreach (Transform fakeGun in SecondaryGunsFakePrefabs) //   <--- go back to here --------+
                {                               //                                |                    
                    // do work
                    fakeGun.gameObject.SetActive(false);
                }
                //SecondaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(false);
                realPistol.gameObject.SetActive(true);
            }
            else
            {
                //fakeRifle.SetActive(false);
                foreach (Transform fakeGun in PrimaryGunsFakePrefabs) //   <--- go back to here --------+
                {                               //                                |                  
                    // do work
                    fakeGun.gameObject.SetActive(false);
                }
                // PrimaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(false);
                realRifle.gameObject.SetActive(true);
            }
        }
        if (animator.GetComponent<Animator>().GetLayerWeight(5) == 1)
        {

            gunInHand = false;
            realPistol.gameObject.SetActive(false);
            //fakePistol.SetActive(true);
            SecondaryGunsFakePrefabs[loadOutManager.loadNumber].gameObject.SetActive(true);
        }
    }

}
