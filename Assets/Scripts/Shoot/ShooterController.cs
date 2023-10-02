using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using FishNet.Object;
using UnityEngine.Animations.Rigging;
using FishNet;

public class ShooterController : NetworkBehaviour
{
    //Aim Settings  
    [SerializeField] float normalSensitivity;
    [SerializeField] float aimSensitivity;
    [SerializeField] LayerMask aimcolliderLayerMask;
    [SerializeField] Transform debugTransform;

    //outer References
    [SerializeField] private float aimRigWeight;
    [SerializeField] GameObject fPSController;
    [SerializeField] ScreenTouch screenTouch;
    [SerializeField] WeaponSwitching weaponSwitching;
    [SerializeField] PlayerGunSelector playerGunSelector;

    //Offsets
    Vector3 aimDir;
    Quaternion rotation;
    Vector3 mouseWorldPosition;

    //Conditions
    bool FPSMode;
    public bool Aiming = false;
    public AudioSource audioSource;
    public AudioClip aimSFX;

    //Camera's Reference
    public GameObject fpsVirtualCamera;
    public CinemachineVirtualCamera aimVirtualCamera;
    public CinemachineVirtualCamera followVirtualCamera;

    //References
    public StarterAssetsInputs starterAssetsInputs;
    public ThirdPersonController thirdPersonController;
    public Animator animator;
    
    public GameObject spawnedObject;

    float lastShotTime;
    private WeaponManager equippedWeapon;

    [SerializeField] LoadOutManager loadOutManager;
    [SerializeField] GameObject sniperScopeUI;
    [SerializeField] GameObject crosshairUI;
    private void Awake()
    {
        //References       
        aimVirtualCamera = GameObject.FindWithTag("Aim Camera").GetComponent<CinemachineVirtualCamera>();
        followVirtualCamera = GameObject.FindWithTag("Follow Camera").GetComponent<CinemachineVirtualCamera>();
    }
  
    public void Update()
    {
        
        if (!base.IsOwner)
            return;

        AimMovenment();
    }
    public void GunChangeCheck()
    {
        Aiming = false;
        animator.SetLayerWeight(1, 0);
        animator.SetLayerWeight(3, 0);
        thirdPersonController.Aiming(false);
        screenTouch.SetSensitivity(8);
        if (!FPSMode)
        {
            aimVirtualCamera.enabled = false;
            //thirdPersonController.SetSensitivity(normalSensitivity);
        }
    }
    public void ExitAim()
    {
        Aiming = false;
        if(sniperScopeUI != null)
            sniperScopeUI.SetActive(false);
        var componentBase = aimVirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (componentBase is Cinemachine3rdPersonFollow)
        {
            (componentBase as Cinemachine3rdPersonFollow).CameraDistance = 3.06f;
        }
        aimVirtualCamera.m_Lens.FieldOfView = 32;
        animator.SetLayerWeight(1, 0);
        animator.SetLayerWeight(3, 0);
        thirdPersonController.Aiming(false);
        screenTouch.SetSensitivity(8);
        if (!FPSMode)
        {
            aimVirtualCamera.enabled = false;
            //thirdPersonController.SetSensitivity(normalSensitivity);
        }
    }
    //RaycastHit raycastHit;
    public Vector3 aimDirection;
    public void AimMovenment()
    {
      
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimcolliderLayerMask))
        {
            debugTransform.position = Vector3.Lerp(debugTransform.position, raycastHit.point, Time.deltaTime * 20f);
            
            mouseWorldPosition = raycastHit.point;
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            aimDirection = (worldAimTarget - transform.position).normalized;

            //transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 10f);
        }
    }
   
    public void Aim()
    {
        if(!thirdPersonController.changingGun)
        {
            if (!Aiming)
            {
                Aiming = true;
                audioSource.PlayOneShot(aimSFX);
                thirdPersonController.Aiming(true);
                if (!FPSMode)
                {
                    aimVirtualCamera.GetComponent<CinemachineVirtualCamera>().enabled = true;
                    thirdPersonController.SetSensitivity(aimSensitivity);
                    screenTouch.SetSensitivity(4);
                }
                if(loadOutManager.loadNumber == 3)
                {
                    if(weaponSwitching.selectedWeapon == 0)
                    {
                        playerGunSelector.HideUIScope();
                        crosshairUI.SetActive(false);
                        sniperScopeUI.SetActive(true);
                        var componentBase = aimVirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
                        if (componentBase is Cinemachine3rdPersonFollow)
                        {
                            (componentBase as Cinemachine3rdPersonFollow).CameraDistance = 0;
                        }
                        aimVirtualCamera.m_Lens.FieldOfView = 16;
                    }
                    
                }
                if (FPSMode)
                {
                    //socket.transform.localPosition = vfxSpawnOffset;
                }
                if (gunType == 0)
                    animator.SetLayerWeight(1, 1);
                else
                    animator.SetLayerWeight(3, 1);
            }
            else
            {
                
                Aiming = false;
                audioSource.PlayOneShot(aimSFX);
                playerGunSelector.ShowUIScope();
                thirdPersonController.Aiming(false);
                screenTouch.SetSensitivity(6);
                if (!FPSMode)
                {
                    aimVirtualCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;
                    thirdPersonController.SetSensitivity(normalSensitivity);
                }
                crosshairUI.SetActive(true);
                sniperScopeUI.SetActive(false);
                var componentBase = aimVirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
                if (componentBase is Cinemachine3rdPersonFollow)
                {
                    (componentBase as Cinemachine3rdPersonFollow).CameraDistance = 3.06f;
                }
                aimVirtualCamera.m_Lens.FieldOfView = 32;
                if (gunType == 0)
                    animator.SetLayerWeight(1, 0);
                else
                    animator.SetLayerWeight(3, 0);
            }
        }
        
    }

    public void Fire(float input)
    {
        //if(!gunChanging)
        //{
        //    equippedWeapon.FireBullet(FPSMode, input);
        //    if (input == 1)
        //    {
        //        thirdPersonController.ShotFired(true);
        //        thirdPersonController.FiringContinous(true);
        //    }
                
        //    if (input == 0)
        //        thirdPersonController.FiringContinous(false);
        //}
        //animator.SetLayerWeight(1, 1);
        
        ////Show Flash
        //particles.Emit(5);

        //flashLight.enabled = true;
        //StartCoroutine(nameof(DisableFlashLight));
        //audioSource.PlayOneShot(audioClipFire, 0.5f);

        ////Camera Shake
        //followVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
        //aimVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
        //fpsVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
        //flashPrefab.SetActive(false);        
    }
    [ServerRpc]
    public void SpawnBulletServerRPC(Vector3 aimDir, Quaternion rotation, ShooterController script)
    {

        ////Instansiate Bullet
        //GameObject projectile = Instantiate(bulletPrefab, spawnBulletPosition.position, rotation);
        //ServerManager.Spawn(projectile, base.Owner);
        //SetSpawnBullet(projectile, script);
    }
    
    [ObserversRpc]
    public void SetSpawnBullet(GameObject spawned, ShooterController script)
    {
        script.spawnedObject = spawned;
       
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnBullet()
    {
        ServerManager.Despawn(spawnedObject);
    }

    public void FPSModeCheck(bool state)
    {
        FPSMode = state;
    }  
    public int gunType;
    public void GunType(int CurrentGunType)
    {
        gunType = CurrentGunType;       
    }
    public int ReturnGuntype()
    {
        return gunType;
    }
    public void GunChanged()
    {
        animator.SetLayerWeight(3, 0);
        animator.SetLayerWeight(1, 0);
    }
    

    

}
