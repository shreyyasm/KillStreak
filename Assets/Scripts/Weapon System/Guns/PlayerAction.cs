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
  

    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    public bool IsReloading { get; [ServerRpc(RunLocally = true)] set; }

    public bool IsShooting;
    public bool resetShooting = false;
    public bool IsChangingGun;
    public Animator anim;
    private void Update()
    {
        if (!base.IsOwner)
            return;
        if(Instance == null)
            Instance = this;


        
        if (GunSelector.ActiveGun != null)
        {
            if (!IsReloading)
            {
                if(!IsChangingGun)
                    GunSelector.ActiveGun.Tick(IsShooting);
                //GunSelector.FireCondition(IsShooting);
            }
                
        }
        IsChangingGun = thirdPersonController.changingGun;
        //ManualReloadMouse();
        //GunSelector.ActiveGun.Tick(
        //    !IsReloading
        //    && Application.isFocused && Mouse.current.leftButton.isPressed
        //    && GunSelector.ActiveGun != null
        //);
        if (!IsReloading)
            PlayerAnimator.SetLayerWeight(6, 0);

        if (weaponSwitch.gunChanging)
        {
            IsReloading = false;
            thirdPersonController.ReloadCheck(IsReloading);
        }
            

        if (ShouldAutoReload())
        {
            if (weaponSwitch.gunChanging)
                return;

            PlayerAnimator.SetLayerWeight(6, 1);
            GunSelector.ActiveGun.StartReloading();
            IsReloading = true;
            
            thirdPersonController.ReloadCheck(IsReloading);
            anim.SetBool("Reload",true);
            StartCoroutine(EndReload());
            //InverseKinematics.HandIKAmount = 0.25f;
            //InverseKinematics.ElbowIKAmount = 0.25f;
        }

        //if (Input.GetMouseButton(1))
        //    Shoot(1);
        //else
        //    Shoot(0);
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
        if(base.IsOwner)
        {
            
            GunSelector.ActiveGun.EndReload();
            //InverseKinematics.HandIKAmount = 1f;
            //InverseKinematics.ElbowIKAmount = 1f;
            IsReloading = false;
            anim.SetBool("Reload", false);
            thirdPersonController.ReloadCheck(IsReloading);
        }
       
    }
    public void Shoot(float input)
    {
        
        if (!IsChangingGun)
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
    public void ManualReloadServerTest()
    {
        if (ShouldManualReload())
        {
            if (weaponSwitch.gunChanging)
                return;

            PlayerAnimator.SetLayerWeight(6, 1);
            GunSelector.ActiveGun.StartReloading();
            IsReloading = true;
            
            thirdPersonController.ReloadCheck(IsReloading);
            anim.SetBool("Reload",true);
            StartCoroutine(EndReload());
        }
    }
    //Manual Reload - Server and observer Logic
    public void ManualReload()
    {
       
        if (base.IsClient)
            ManualReloadServer();

        if (base.IsServer)
            ManualReloadObserver();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void ManualReloadServer()
    {
       
        if (ShouldManualReload())
        {
            if (weaponSwitch.gunChanging)
                return;
            PlayerAnimator.SetLayerWeight(6, 1);
            GunSelector.ActiveGun.StartReloading();
            IsReloading = true;
            thirdPersonController.ReloadCheck(IsReloading);
            anim.SetBool("Reload",true);
            StartCoroutine(EndReload());
        }
    }
    [ObserversRpc(BufferLast = true)]
    public void ManualReloadObserver()
    {
        if (ShouldManualReload())
        {

            if (weaponSwitch.gunChanging)
                return;
            PlayerAnimator.SetLayerWeight(6, 1);
            GunSelector.ActiveGun.StartReloading();
            IsReloading = true;
            thirdPersonController.ReloadCheck(IsReloading);
            anim.SetBool("Reload", true);
            StartCoroutine(EndReload());
        }
    }
    
}