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

    //Offsets
    Vector3 aimDir;
    Quaternion rotation;
    Vector3 mouseWorldPosition;

    //Conditions
    bool FPSMode;
    bool Aiming = false;

    //Camera's Reference
    public GameObject fpsVirtualCamera;
    CinemachineVirtualCamera aimVirtualCamera;
    CinemachineVirtualCamera followVirtualCamera;

    //References
    public StarterAssetsInputs starterAssetsInputs;
    public ThirdPersonController thirdPersonController;
    public Animator animator;
    
    public GameObject spawnedObject;

    float lastShotTime;
    private WeaponManager equippedWeapon;

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
        if (thirdPersonController.changingGun)
        {          
            Aiming = false;
            animator.SetLayerWeight(1, 0);
            animator.SetLayerWeight(3, 0);
            thirdPersonController.Aiming(false);
            screenTouch.SetSensitivity(8);
            if (!FPSMode)
            {
                aimVirtualCamera.enabled = false;
                thirdPersonController.SetSensitivity(normalSensitivity);
            }
        }
    }
    //RaycastHit raycastHit;
    public void AimMovenment()
    {
        //aimVirtualCamera.transform.position = followVirtualCamera.transform.position;
        mouseWorldPosition = Vector3.zero;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimcolliderLayerMask))
        {
            //debugTransform.position = raycastHit.point;
            // debugTransform.position = Vector3.Lerp(debugTransform.position, raycastHit.point, Time.deltaTime * 20f);
            debugTransform.position = Vector3.Lerp(debugTransform.position, raycastHit.point, Time.deltaTime * 20f);
            //if (base.IsServer)
            //    AimMovementObserver();

            //if (base.IsOwner)
            //    AimMovementServer();
            mouseWorldPosition = raycastHit.point;
        }
        if (!FPSMode)
        {
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 10f);
        }

        

    }
    [ServerRpc(RequireOwnership = false, RunLocally = false)]
    public void AimMovementServer()
    {
        
    }
    [ObserversRpc(BufferLast = true, IncludeOwner = false)]
    public void AimMovementObserver()
    {
        
    }
    public void Aim()
    {
        if(!thirdPersonController.changingGun)
        {
            if (!Aiming)
            {
                Aiming = true;
                thirdPersonController.Aiming(true);
                if (!FPSMode)
                {
                    aimVirtualCamera.GetComponent<CinemachineVirtualCamera>().enabled = true;
                    thirdPersonController.SetSensitivity(aimSensitivity);
                    screenTouch.SetSensitivity(4);
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
                thirdPersonController.Aiming(false);
                screenTouch.SetSensitivity(8);
                if (!FPSMode)
                {
                    aimVirtualCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;
                    thirdPersonController.SetSensitivity(normalSensitivity);
                }
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
