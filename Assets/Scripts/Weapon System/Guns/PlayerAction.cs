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
    private WeaponSwitching weaponSwitch;
    public GameObject spawnedObject;

    [field: SyncVar]
    public bool IsReloading { get; [ServerRpc(RunLocally = true)] set; }

    private bool IsShooting;

    private void Update()
    {
        if (!base.IsOwner)
            return;
        if(Instance == null)
            Instance = this;
        if (GunSelector.ActiveGun != null)
        {
             if (!IsReloading)
                GunSelector.ActiveGun.Tick(IsShooting);
        }
        //ManualReloadMouse();
        //GunSelector.ActiveGun.Tick(
        //    !IsReloading
        //    && Application.isFocused && Mouse.current.leftButton.isPressed
        //    && GunSelector.ActiveGun != null
        //);
        if (!IsReloading)
            PlayerAnimator.SetLayerWeight(6, 0);

        if (weaponSwitch.gunChanging)
            IsReloading = false;

        if (ShouldAutoReload())
        {
            if (weaponSwitch.gunChanging)
                return;

            PlayerAnimator.SetLayerWeight(6, 1);
            GunSelector.ActiveGun.StartReloading();
            IsReloading = true;
            networkAnimator.SetTrigger("Reload");
            //InverseKinematics.HandIKAmount = 0.25f;
            //InverseKinematics.ElbowIKAmount = 0.25f;
        }
        thirdPersonController.ReloadCheck(IsReloading);
        //if (Input.GetMouseButtonDown(1))
        //    ManualReloadServerTest();
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

    private void EndReload()
    {
        if(base.IsOwner)
        {
            GunSelector.ActiveGun.EndReload();
            //InverseKinematics.HandIKAmount = 1f;
            //InverseKinematics.ElbowIKAmount = 1f;
            IsReloading = false;
        }
       
    }
    public void Shoot(float input)
    {
        if (GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo > 0)
            GunSelector.ActiveGun.FireCheck();
        if (!thirdPersonController.changingGun)
        {
            if (input == 1)
            {
                IsShooting = true;
                thirdPersonController.ShotFired(true);
                thirdPersonController.FiringContinous(true);
            }
            else
            {
                IsShooting = false;
                thirdPersonController.FiringContinous(false);
            }
        }
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
            networkAnimator.SetTrigger("Reload");
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
            networkAnimator.SetTrigger("Reload");
        }
    }
    [ObserversRpc(BufferLast = true, IncludeOwner = true)]
    public void ManualReloadObserver()
    {
        if (ShouldManualReload())
        {

            if (weaponSwitch.gunChanging)
                return;
            PlayerAnimator.SetLayerWeight(6, 1);
            GunSelector.ActiveGun.StartReloading();
            IsReloading = true;
            networkAnimator.SetTrigger("Reload");
        }
    }
    [ServerRpc]
    public void SpawnBulletServerRPC(GameObject prefab)
    {

        ////Instansiate Bullet
        ServerManager.Spawn(prefab, base.Owner);
        SetSpawnBullet(prefab, this);
    }

    [ObserversRpc]
    public void SetSpawnBullet(GameObject spawned, PlayerAction script)
    {
        script.spawnedObject = spawned;

    }
}