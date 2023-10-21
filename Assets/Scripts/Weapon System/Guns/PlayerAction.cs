using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using FishNet.Object;
using UnityEngine.Animations.Rigging;
using FishNet;
using FishNet.Object.Synchronizing;
using FishNet.Component.Animating;

[DisallowMultipleComponent]
public class PlayerAction : NetworkBehaviour
{
    public static PlayerAction Instance;
    [SerializeField]
    private PlayerGunSelector GunSelector;
    [SerializeField]
    private bool AutoReload = true;
    [SerializeField]
    private PlayerIK InverseKinematics;
    [SerializeField]
    private Animator PlayerAnimator;
    [SerializeField]
    private NetworkAnimator networkAnimator;
    [SerializeField]
    private ThirdPersonController thirdPersonController;
    [SerializeField]
    private ShooterController shooterController;
    [SerializeField]
    private PlayerHealth playerHealth;
    [SerializeField]
    private WeaponSwitching weaponSwitch;
    [SerializeField]
    private PlayerSoundManager playerSoundManager;

    [SerializeField]
    private AmmoDisplayer ammoDisplayer;


    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    public bool IsReloading { get; [ServerRpc(RunLocally = true)] set; }

    public bool IsShooting;
    public bool resetShooting = false;
    public Animator anim;

    public AudioClip reloadingVoiceClip;
    private void Update()
    {
        if (!base.IsOwner)
            return;

     
        
        if (GunSelector.ActiveGun != null)
        {
            if (!IsReloading)
            {
                if(!thirdPersonController.changingGun)
                    GunSelector.ActiveGun.Tick(IsShooting);
                //GunSelector.FireCondition(IsShooting);
            }
                
        }
        if(IsShooting)
            ammoDisplayer.UpdateGunAmmo();
   
        if (!IsReloading)
            PlayerAnimator.SetLayerWeight(6, 0);

        if (weaponSwitch.gunChanging)
        {
            IsReloading = false;
            thirdPersonController.ReloadCheck(IsReloading);
            
        }

        if (IsReloading)
            thirdPersonController.SetRigWeight();
        if (ShouldAutoReload())
        {
            if (weaponSwitch.gunChanging)
                return;

            //IsShooting = false;
            shooterController.ExitAim();

            anim.SetBool("SniperReload", false);
            anim.SetLayerWeight(8, 0);

            PlayerAnimator.SetLayerWeight(6, 1);
            //GunSelector.ActiveGun.StartReloading();
            playerSoundManager.PlayReloadClip();
            IsReloading = true;
            
            thirdPersonController.ReloadCheck(IsReloading);
            

            anim.SetBool("Reload",true);
            AudioSource.PlayClipAtPoint(reloadingVoiceClip, Camera.main.transform.position, 0.4f);
            StartCoroutine(EndReload());
            
            //InverseKinematics.HandIKAmount = 0.25f;
            //InverseKinematics.ElbowIKAmount = 0.25f;
        }

        //if (Input.GetMouseButton(1))
        //    Shoot(1);
        //else
        //    Shoot(0);

    }
    IEnumerator DelayRigSet()
    {
        yield return new WaitForSeconds(0.1f);
        thirdPersonController.SetRigWeight();
    }
    public bool ShouldManualReload()
    {
        return !IsReloading            
            && GunSelector.ActiveGun.CanReload();
    }

    private bool ShouldAutoReload()
    {
        return !IsReloading
            && AutoReload
            && GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo == 0
            && GunSelector.ActiveGun.CanReload();
    }

    public IEnumerator EndReload()
    {
        
        yield return new WaitForSeconds(2f);
         
            GunSelector.ActiveGun.EndReload();
            //InverseKinematics.HandIKAmount = 1f;
            //InverseKinematics.ElbowIKAmount = 1f;
            IsReloading = false;
            anim.SetBool("Reload", false);
            thirdPersonController.ReloadCheck(IsReloading);
           
            ammoDisplayer.UpdateGunAmmo();
            PlayerAnimator.SetLayerWeight(6, 0);
            CheckReloadState();
            StartCoroutine(DelayRigSet());


    }
    public void Shoot(float input)
    {
        
        if (!thirdPersonController.changingGun)
        {
            if(GunSelector.ActiveGun.Automatic)
            {
                if (input == 1)
                {

                    IsShooting = true;                  
                    thirdPersonController.ShotFired(true);
                    thirdPersonController.FiringContinous(true);
                    //if (!GunSelector.ActiveGun.Automatic)
                    //    IsShooting = false;
                }
                else
                {
                    IsShooting = false;
                    thirdPersonController.FiringContinous(false);
                }
            }
            else
            {
                if (input != 1)
                    return;
               IsShooting = true;             
               thirdPersonController.ShotFired(true);
               thirdPersonController.FiringContinous(true);

                StartCoroutine(StopShooting());
               
            }
            
        }
        //Debug.Log(GunSelector.ActiveGun.FireCheck());
    }
    public IEnumerator StopShooting()
    {
        yield return new WaitForSeconds(0.1f);
        IsShooting = false;
        thirdPersonController.FiringContinous(false);
    }
   
    //Manual Reload - Server and observer Logic
    public void ManualReload()
    {
 
        if (base.IsServer)
            ManualReloadObserver();

       else
            ManualReloadServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void ManualReloadServer()
    {
       
        if (ShouldManualReload())
        {
            if (weaponSwitch.gunChanging)
                return;

            shooterController.ExitAim();

            anim.SetBool("SniperReload", false);
            anim.SetLayerWeight(8, 0);

            PlayerAnimator.SetLayerWeight(6, 1);
            playerSoundManager.PlayReloadClip();
            //GunSelector.ActiveGun.StartReloading();
            IsReloading = true;
            thirdPersonController.ReloadCheck(IsReloading);       
            anim.SetBool("Reload",true);
            AudioSource.PlayClipAtPoint(reloadingVoiceClip, Camera.main.transform.position, 0.4f);
            StartCoroutine(EndReload());
            
        }
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void ManualReloadObserver()
    {
        if (ShouldManualReload())
        {

            if (weaponSwitch.gunChanging)
                return;

            shooterController.ExitAim();

            anim.SetBool("SniperReload", false);
            anim.SetLayerWeight(8, 0);

            PlayerAnimator.SetLayerWeight(6, 1);
            playerSoundManager.PlayReloadClip();
            //GunSelector.ActiveGun.StartReloading();
            IsReloading = true;
            thirdPersonController.ReloadCheck(IsReloading);           
            anim.SetBool("Reload", true);
            AudioSource.PlayClipAtPoint(reloadingVoiceClip, Camera.main.transform.position, 0.4f);
            StartCoroutine(EndReload());
            
        }
    }
    public void CheckReloadState()
    {
        if (base.IsServer)
            CheckReloadStateObserver();
        else
            CheckReloadStateServer();

    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void CheckReloadStateServer()
    {
        thirdPersonController.SetRigWeight();
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void CheckReloadStateObserver()
    {
        thirdPersonController.SetRigWeight();
    }

}