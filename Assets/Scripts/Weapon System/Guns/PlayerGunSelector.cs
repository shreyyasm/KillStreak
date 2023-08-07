using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FishNet.Object;
using FishNet.Observing;
using FishNet.Connection;
using FishNet;
using FishNet.Utility.Performance;
using System;
using TMPro;
using StarterAssets;
using UnityEngine.Animations.Rigging;
[DisallowMultipleComponent]
public class PlayerGunSelector : NetworkBehaviour
{
    public LayerMask IdentifyEnemy;
    public static PlayerGunSelector instance;
    
    //[SerializeField]
    //private GunType PrimaryGun;

    //[SerializeField]
    //private GunType SecondaryGun;

    [SerializeField]
    private Transform GunParent;

    public List<GunScriptableObject> Guns;

    [SerializeField]
    private PlayerIK InverseKinematics;

    [Space]
    [Header("Runtime Filled")]
    public GunScriptableObject ActiveGun;

    [SerializeField]
    private ShooterController shooterController;
    [SerializeField]
    private WeaponSwitching weaponSwitching;

    [SerializeField]
    PlayerSoundManager playerSoundManager;

    [SerializeField]
    private SurfaceManager surfaceManager;
    [SerializeField]
    private FloatingDamage floatingDamage;
    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private ThirdPersonController thirdPersonController;

    public int gunSelected;
    public GunScriptableObject gun1;
    public GunScriptableObject gun2;
   

    public static ObjectPooler SharedInstance;
    private GameObject bulletTrailPool;
    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int amountToPool;
    private Ray ray;
    Camera ActiveCamera;
    public PlayerAction playerAction;
    public GameObject spawnedObject;
    private NetworkConnection ownerConnection;
    public bool blocked;
    public float mouseX,mouseY;
    private  float moveX, moveZ;
    [SerializeField] GameObject BlockUI;
    [SerializeField] GameObject CrosshairUI;
    GameObject GunModel;
   

    public LoadOutManager loadOutManager;
    public Transform PrimaryParent;
    public Transform SecondaryParent;
    public List<GunScriptableObject> PrimaryGuns;
    public List<GunScriptableObject> SecondaryGuns;

    public List<GameObject> PrimaryGunsPrefabs;
    public List<GameObject> SecondaryGunsPrefabs;

    public List<GameObject> HideUI;

    public GameObject CrosshairPrimary;
    public GameObject CrosshairSecondary;
    Animator anim;

    public GameObject ActiveGunPrefab;

    public bool redTeamPlayer;
    public bool blueTeamPlayer;
    private void Awake()
    {
        instance = this;
        ActiveCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        anim = GetComponent<Animator>();

    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        PrewarmPools();

        if (redTeamPlayer)
            gameObject.AddComponent<RedTeamPlayer>();

        if (blueTeamPlayer)
            gameObject.AddComponent<BlueTeamPlayer>();

    }

    [SerializeField] bool aimAssist;
    [SerializeField] float aimAssistSize = 1f;
    public TwoBoneIKConstraint SniperRig;

    private void Start()
    {  
        SpawnAllGuns();

        Vector3 screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);
        ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        gun1 = PrimaryGuns[loadOutManager.loadNumber];
        gun2 = SecondaryGuns[loadOutManager.loadNumber];

        PrimaryGunsPrefabs[loadOutManager.loadNumber].SetActive(true);
      
        gunSelected = weaponSwitching.selectedWeapon;
        if (gunSelected == 0)
        {
            if (!weaponSwitching.gunChanging)
                ActiveGun = PrimaryGuns[loadOutManager.loadNumber];
            ActiveGunPrefab = PrimaryGunsPrefabs[loadOutManager.loadNumber];
        }
        else
        {
            if (!weaponSwitching.gunChanging)
                ActiveGun = SecondaryGuns[loadOutManager.loadNumber];
            ActiveGunPrefab = SecondaryGunsPrefabs[loadOutManager.loadNumber];
        }

        GunModel = ActiveGunPrefab;
    }
    
   
    [Range(0.1f, 1f)] public float sphereCastRadius;
    [Range(0.1f, 10f)] public float sphereCastRadiusAimAssist;
    [Range(1f, 100f)] public float range;

   
    private void Update()
    {
        if (!base.IsOwner)
            return;
        GunModel = ActiveGunPrefab;
        GunModelRecoil();
        AimAssis();
        if (playerAction.IsShooting )
        {
            Vector3 screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);
            ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (!playerAction.IsReloading)
            {
                if (!thirdPersonController.changingGun)
                {
                    if (ActiveGun.ShootConfig.IsHitscan)
                    {

                        
                        if (ActiveGun.Automatic)
                            FireConditionAutomatic();
                        else
                            FireConditionManual();
                    }
                }
            }
        }
        CheckBlocked();
        
    }
    public void SniperReloadAimatioCheck()
    {
        anim.SetBool("SniperReload", false);
        anim.SetLayerWeight(8, 0);
        SniperRig.weight = 1;
    }
    public void SetActiveGun(int selectedWeapon)
    {
        
        gun1 = PrimaryGuns[loadOutManager.loadNumber];
        gun2 = SecondaryGuns[loadOutManager.loadNumber];

        
        if (selectedWeapon == 0)
        {
            if (!weaponSwitching.gunChanging)
                ActiveGun = PrimaryGuns[loadOutManager.loadNumber];

        }
        else
        {
            if (!weaponSwitching.gunChanging)
                ActiveGun = SecondaryGuns[loadOutManager.loadNumber];

        }
    }
    public void SpawnAllGuns()
    {
        foreach (GunScriptableObject Gun in PrimaryGuns) 
        {
            // do work
            
            GameObject GunPrefab = Gun.Spawn(PrimaryParent, this);
            PrimaryGunsPrefabs.Add(GunPrefab);
        } 
        foreach (GunScriptableObject Gun in SecondaryGuns)
        {
            // do work
            GameObject GunPrefab = Gun.Spawn(SecondaryParent, this);
            SecondaryGunsPrefabs.Add(GunPrefab);
        }

    }
    public void ChangeCrosshair()
    {
        if(CrosshairPrimary != null)
        {
            if (!ActiveGun.shotgun)
            {
                CrosshairPrimary.SetActive(true);
                CrosshairSecondary.SetActive(false);
            }
            else
            {
                CrosshairPrimary.SetActive(false);
                CrosshairSecondary.SetActive(true);
            }
        }
      
    }
    public void ChangeGunLoadOut(int loadNumber)
    {
        
        if (base.IsServer)
            ChangeGunLoadOutObserver(loadNumber);

        if (base.IsOwner)
            ChangeGunLoadOutServer(loadNumber);
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void ChangeGunLoadOutServer(int loadout)
    {
        
        PrimaryGunsPrefabs[loadout].SetActive(true);
        foreach (GameObject Gun in PrimaryGunsPrefabs) //   <--- go back to here --------+
        {                               //                                |
            if (Gun == PrimaryGunsPrefabs[loadout])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work            
            Gun.SetActive(false);
        }
        foreach (GameObject Gun in SecondaryGunsPrefabs) //   <--- go back to here --------+
        {                               //                                |
            if (Gun == SecondaryGunsPrefabs[loadout])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            Gun.SetActive(false);
        }
        
        if (weaponSwitching.selectedWeapon == 0)
        {        
            ActiveGunPrefab = PrimaryGunsPrefabs[loadOutManager.loadNumber];
        }
        else
        {          
            ActiveGunPrefab = SecondaryGunsPrefabs[loadOutManager.loadNumber];
        }
        GunModel = ActiveGunPrefab;
        weaponSwitching.SetGuns();
        gun1.AmmoConfig.RefillAmmo();
        gun2.AmmoConfig.RefillAmmo();
    }
    [ObserversRpc(BufferLast = true)]
    public void ChangeGunLoadOutObserver(int loadout)
    {
        
        PrimaryGunsPrefabs[loadout].SetActive(true);
        foreach (GameObject Gun in PrimaryGunsPrefabs) //   <--- go back to here --------+
        {                               //                                |
            if (Gun == PrimaryGunsPrefabs[loadout])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work            
            Gun.SetActive(false);
        }
        foreach (GameObject Gun in SecondaryGunsPrefabs) //   <--- go back to here --------+
        {                               //                                |
            if (Gun == SecondaryGunsPrefabs[loadout])             //                                |
            {                           //                                |
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            Gun.SetActive(false);
        }
        
        if (weaponSwitching.selectedWeapon == 0)
        {
            ActiveGunPrefab = PrimaryGunsPrefabs[loadOutManager.loadNumber];
        }
        else
        {
            ActiveGunPrefab = SecondaryGunsPrefabs[loadOutManager.loadNumber];
        }
        GunModel = ActiveGunPrefab;
        weaponSwitching.SetGuns();
        gun1.AmmoConfig.RefillAmmo();
       gun2.AmmoConfig.RefillAmmo();
    }

    RaycastHit hitCheck;
    public float distanceObject;
    public float distancePlayer;
    public float timeLeft = 0f;
    public float timeLeftBlock = 0f;
    public void GunModelRecoil()
    {
        if(!playerAction.IsShooting)
        {
            GunModel.transform.localRotation = Quaternion.Euler(ActiveGun.SpawnRotation);
        }
        GunModel.transform.localRotation = Quaternion.Lerp(
           GunModel.transform.localRotation,
           Quaternion.Euler(ActiveGun.SpawnRotation),
           Time.deltaTime * ActiveGun.ShootConfig.RecoilRecoverySpeed);

        //GunModel.transform.forward += GunModel.transform.TransformDirection(spreadAmount);


    }
 
    public void CheckBlocked()
    {
        if (Physics.Raycast(ray, out hitCheck, float.MaxValue, ActiveGun.ShootConfig.HitMask))
        {
            rayHitPoint = hitCheck.point;
        }
       
        distanceObject = Vector3.Distance(ActiveCamera.transform.position, rayHitPoint);
        distancePlayer = Vector3.Distance(ActiveCamera.transform.position, transform.position);

        
        if (distanceObject < distancePlayer)
        {
            //Physics.IgnoreCollision(hitnew.transform.GetComponent<Collider>(), hit.collider);
            //objectRef = hitnew.transform.gameObject;
            //if (hitnew.transform.gameObject.layer != 2)
            //{
            //    objectRef.layer = LayerMask.NameToLayer("Ignore Raycast");
            //    objectRef = hitnew.transform.gameObject;
            //    layer = objectRef.ToString();
            //}
            blocked = true;
           
            timeLeft = 0.1f;
            timeLeftBlock = 2f;
            
        }
        else
        {
            //blocked = false;
           
            CheckBlock();
        }
        if(blocked)
        {
            BlockUI.SetActive(true);         
             CrosshairUI.SetActive(false);
        }
        else
        {
            BlockUI.SetActive(false);
            CrosshairUI.SetActive(true);
        }
    }
    public void CheckBlock()
    {
        if(timeLeft >= 0)
            timeLeft -= Time.deltaTime;

        if (timeLeftBlock >= 0.1)
            timeLeftBlock -= Time.deltaTime;

        if (timeLeft < 0)
        {
            blocked = false;
        }
    }
   
  
    Vector3 hitpoint;
    Vector3 rayHitPoint;
    RaycastHit aimAssistHit;
    RaycastHit hitnew;

    
    public void FireConditionAutomatic()
    { 
        
        if (Time.time - ActiveGun.LastShootTime - ActiveGun.ShootConfig.FireRate > Time.deltaTime)
        {
            float lastDuration = Mathf.Clamp(
                0,
                (ActiveGun.StopShootingTime - ActiveGun.InitialClickTime),
                ActiveGun.ShootConfig.MaxSpreadTime
            );
            float lerpTime = (ActiveGun.ShootConfig.RecoilRecoverySpeed - (Time.time - ActiveGun.StopShootingTime))
                / ActiveGun.ShootConfig.RecoilRecoverySpeed;

            ActiveGun.InitialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));
        }
        if (Time.time > ActiveGun.ShootConfig.FireRate + ActiveGun.LastShootTime)
        {
           
            ActiveCamera.transform.forward += ActiveCamera.transform.TransformDirection(ActiveGun.ShootConfig.GetSpread(ActiveGun.shootHoldTime - ActiveGun.InitialClickTime));
            Vector3 screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);

            var heading = sphere.transform.position - transform.position;
            //heading += new Vector3(1.07f, -0.2f, 0);
            var distance = heading.magnitude;
            var direction = heading / distance;

            
            //Vector3 dirNew = (ActiveGun.ShootSystem.transform.position - ActiveCamera.transform.position);
            Vector3 shootDirection = direction + sphere.transform.TransformDirection(ActiveGun.ShootConfig.GetSpread(ActiveGun.shootHoldTime - ActiveGun.InitialClickTime));
            Vector3 origin = ActiveGun.ShootSystem.transform.position
                        + ActiveGun.ShootSystem.transform.forward * Vector3.Distance(
                               ActiveGun.ShootSystem.transform.position,
                                ActiveGun.ShootSystem.transform.position);
            
            GunModel.transform.forward += GunModel.transform.TransformDirection(ActiveGun.spreadAmount);
            //GunModelRecoil(ActiveGun.spreadAmount);
            playerSoundManager.PlayShootingClip(transform.position, ActiveGun.AmmoConfig.CurrentClipAmmo == 1);
            
            if (ActiveGun.shotgun)
            {
                for(int i = 0; i < ActiveGun.bulletPerShot; i++)
                {
                    if (!blocked)
                        ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                    else
                    {
                        //Ray r = ;
                        ray = new Ray(ActiveGun.ShootSystem.transform.position, ActiveCamera.transform.forward);

                        //Debug.DrawLine(ActiveGun.Model.transform.position, hitnew.point, Color.green); 
                    }

                    ActiveGun.Fired = false;
                    if (!aimAssist)
                    {
                        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                        {
                            
                            StartCoroutine(
                                PlayTrail(
                                    ActiveGun.ShootSystem.transform.position,
                                    hit.point,
                                    hit
                                )
                            );
                        }

                        else
                        {
                            StartCoroutine(
                                 PlayTrail(
                                     ActiveGun.ShootSystem.transform.position,
                                     ActiveGun.ShootSystem.transform.position + (ActiveGun.ShootSystem.transform.forward * ActiveGun.TrailConfig.MissDistance),
                                     new RaycastHit()
                                 )
                             );
                        }
                    }
                    else
                    {
                        if (Physics.SphereCast(ray, sphereCastRadius, out hitnew, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                        {
                            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                            {
                                rayHitPoint = hit.point;
                            }
                            //Debug.DrawLine(ActiveGun.Model.transform.position, hitnew.point, Color.green); 
                            //Debug.Log("Aim Assist: Hit");
                            //if (hitnew.collider.gameObject.TryGetComponent<CapsuleCollider>(out CapsuleCollider collider))
                            //{
                                
                            //   // thirdPersonController._cinemachineTargetYaw += hitnew.collider.ClosestPointOnBounds(hitnew.point).x;
                            //}
                            
                            Vector3 sphereCastMidpoint = hitnew.point;

                            

                            if (hitnew.collider.gameObject.layer != 18)
                            {

                                hitpoint = rayHitPoint;
                                aimAssistHit = hit;
                            }
                            else
                            {
                                if (mouseX == 0 && mouseY == 0)
                                {
                                    hitpoint = rayHitPoint;
                                    aimAssistHit = hit;
                                }
                                else
                                {

                                    if (timeLeftBlock < 0.1)
                                    {
                                        //Debug.Log("laand");
                                        hitpoint = hitnew.collider.ClosestPointOnBounds(hitnew.point);
                                        aimAssistHit = hitnew;
                                    }

                                    else
                                    {
                                        hitpoint = rayHitPoint;
                                        aimAssistHit = hit;
                                    }

                                }

                            }
                            //Debug.DrawLine(ActiveCamera.transform.position, sphereCastMidpoint, Color.green);


                            StartCoroutine(
                                PlayTrail(
                                    ActiveGun.ShootSystem.transform.position,
                                    hitpoint,
                                    aimAssistHit
                                )
                            );

                        }
                        else
                        {
                            StartCoroutine(
                                 PlayTrail(
                                     ActiveGun.ShootSystem.transform.position,
                                     ActiveGun.ShootSystem.transform.position + (ActiveGun.ShootSystem.transform.forward * ActiveGun.TrailConfig.MissDistance),
                                     new RaycastHit()
                                 )
                             );
                        }
                    }
                }
            }
            else
            {
                if (!blocked)
                    ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                else
                {
                    //Ray r = ;
                    ray = new Ray(ActiveGun.ShootSystem.transform.position, ActiveCamera.transform.forward);

                    //Debug.DrawLine(ActiveGun.Model.transform.position, hitnew.point, Color.green); 
                }

                ActiveGun.Fired = false;
                if (!aimAssist)
                {
                    if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                    {

                        StartCoroutine(
                            PlayTrail(
                                ActiveGun.ShootSystem.transform.position,
                                hit.point,
                                hit
                            )
                        );
                    }

                    else
                    {
                        StartCoroutine(
                             PlayTrail(
                                 ActiveGun.ShootSystem.transform.position,
                                 ActiveGun.ShootSystem.transform.position + (ActiveGun.ShootSystem.transform.forward * ActiveGun.TrailConfig.MissDistance),
                                 new RaycastHit()
                             )
                         );
                    }
                }
                else
                {
                    if (Physics.SphereCast(ray, sphereCastRadius, out hitnew, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                    {
                        //Debug.DrawLine(ActiveGun.Model.transform.position, hitnew.point, Color.green); 
                        //Debug.Log("Aim Assist: Hit");

                        Vector3 sphereCastMidpoint = hitnew.point;

                        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                        {
                            rayHitPoint = hit.point;
                        }

                        if (hitnew.collider.gameObject.layer != 18)
                        {

                            hitpoint = rayHitPoint;
                            aimAssistHit = hit;
                        }
                        else
                        {
                            if (mouseX == 0 && mouseY == 0)
                            {
                                hitpoint = rayHitPoint;
                                aimAssistHit = hit;
                            }
                            else
                            {

                                if (timeLeftBlock < 0.1)
                                {
                                    //Debug.Log("laand");
                                    hitpoint = hitnew.collider.ClosestPointOnBounds(hitnew.point);
                                    aimAssistHit = hitnew;
                                }

                                else
                                {
                                    hitpoint = rayHitPoint;
                                    aimAssistHit = hit;
                                }

                            }

                        }
                        //Debug.DrawLine(ActiveCamera.transform.position, sphereCastMidpoint, Color.green);


                        StartCoroutine(
                            PlayTrail(
                                ActiveGun.ShootSystem.transform.position,
                                hitpoint,
                                aimAssistHit
                            )
                        );

                    }
                    else
                    {
                        StartCoroutine(
                             PlayTrail(
                                 ActiveGun.ShootSystem.transform.position,
                                 ActiveGun.ShootSystem.transform.position + (ActiveGun.ShootSystem.transform.forward * ActiveGun.TrailConfig.MissDistance),
                                 new RaycastHit()
                             )
                         );
                    }
                }
            }


        }    
    }
    
    public void FireConditionManual()
    {
        if (Time.time - ActiveGun.LastShootTime - ActiveGun.ShootConfig.FireRate > Time.deltaTime)
        {
            float lastDuration = Mathf.Clamp(
                0,
                (ActiveGun.StopShootingTime - ActiveGun.InitialClickTime),
                ActiveGun.ShootConfig.MaxSpreadTime
            );
            float lerpTime = (ActiveGun.ShootConfig.RecoilRecoverySpeed - (Time.time - ActiveGun.StopShootingTime))
                / ActiveGun.ShootConfig.RecoilRecoverySpeed;

            ActiveGun.InitialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));
            
        }
        
        if (Time.time > ActiveGun.ShootConfig.FireRate + ActiveGun.LastShootTime)
        {
            
            ActiveCamera.transform.forward += ActiveCamera.transform.TransformDirection(ActiveGun.ShootConfig.GetSpread(ActiveGun.shootHoldTime - ActiveGun.InitialClickTime));
            Vector3 screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);

            var heading = sphere.transform.position - transform.position;
            //heading += new Vector3(1.07f, -0.2f, 0);
            var distance = heading.magnitude;
            var direction = heading / distance;
            

            //Vector3 dirNew = (ActiveGun.ShootSystem.transform.position - ActiveCamera.transform.position);
            Vector3 shootDirection = direction + sphere.transform.TransformDirection(ActiveGun.ShootConfig.GetSpread(ActiveGun.shootHoldTime - ActiveGun.InitialClickTime));
            Vector3 origin = ActiveGun.ShootSystem.transform.position
                        + ActiveGun.ShootSystem.transform.forward * Vector3.Distance(
                               ActiveGun.ShootSystem.transform.position,
                                ActiveGun.ShootSystem.transform.position);

            
            GunModel.transform.forward += GunModel.transform.TransformDirection(ActiveGun.spreadAmount);
            playerSoundManager.PlayShootingClip(transform.position,ActiveGun.AmmoConfig.CurrentClipAmmo == 1);
            if (ActiveGun.shotgun)
            {
                for (int i = 0; i < ActiveGun.bulletPerShot; i++)
                {
                    if (!blocked)
                        ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                    else
                    {
                        //Ray r = ;
                        ray = new Ray(ActiveGun.ShootSystem.transform.position, ActiveCamera.transform.forward);

                        //Debug.DrawLine(ActiveGun.Model.transform.position, hitnew.point, Color.green); 
                    }

                    ActiveGun.Fired = false;
                    if (!aimAssist)
                    {
                        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                        {

                            StartCoroutine(
                                PlayTrail(
                                    ActiveGun.ShootSystem.transform.position,
                                    hit.point,
                                    hit
                                )
                            );
                        }

                        else
                        {
                            StartCoroutine(
                                 PlayTrail(
                                     ActiveGun.ShootSystem.transform.position,
                                     ActiveGun.ShootSystem.transform.position + (ActiveGun.ShootSystem.transform.forward * ActiveGun.TrailConfig.MissDistance),
                                     new RaycastHit()
                                 )
                             );
                        }
                    }
                    else
                    {
                        if (Physics.SphereCast(ray, sphereCastRadius, out hitnew, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                        {
                            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                            {
                                rayHitPoint = hit.point;
                            }
                            //Debug.DrawLine(ActiveGun.Model.transform.position, hitnew.point, Color.green); 
                            //Debug.Log("Aim Assist: Hit");
                            //if (hitnew.collider.gameObject.TryGetComponent<CapsuleCollider>(out CapsuleCollider collider))
                            //{

                            //   // thirdPersonController._cinemachineTargetYaw += hitnew.collider.ClosestPointOnBounds(hitnew.point).x;
                            //}

                            Vector3 sphereCastMidpoint = hitnew.point;



                            if (hitnew.collider.gameObject.layer != 18)
                            {

                                hitpoint = rayHitPoint;
                                aimAssistHit = hit;
                            }
                            else
                            {
                                if (mouseX == 0 && mouseY == 0)
                                {
                                    hitpoint = rayHitPoint;
                                    aimAssistHit = hit;
                                }
                                else
                                {

                                    if (timeLeftBlock < 0.1)
                                    {
                                        //Debug.Log("laand");
                                        hitpoint = hitnew.collider.ClosestPointOnBounds(hitnew.point);
                                        aimAssistHit = hitnew;
                                    }

                                    else
                                    {
                                        hitpoint = rayHitPoint;
                                        aimAssistHit = hit;
                                    }

                                }

                            }
                            //Debug.DrawLine(ActiveCamera.transform.position, sphereCastMidpoint, Color.green);


                            StartCoroutine(
                                PlayTrail(
                                    ActiveGun.ShootSystem.transform.position,
                                    hitpoint,
                                    aimAssistHit
                                )
                            );

                        }
                        else
                        {
                            StartCoroutine(
                                 PlayTrail(
                                     ActiveGun.ShootSystem.transform.position,
                                     ActiveGun.ShootSystem.transform.position + (ActiveGun.ShootSystem.transform.forward * ActiveGun.TrailConfig.MissDistance),
                                     new RaycastHit()
                                 )
                             );
                        }
                    }
                }
            }
            else
            {
                if (!blocked)
                    ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                else
                {
                    //Ray r = ;
                    ray = new Ray(ActiveGun.ShootSystem.transform.position, ActiveCamera.transform.forward);

                    //Debug.DrawLine(ActiveGun.Model.transform.position, hitnew.point, Color.green); 
                }

                ActiveGun.Fired = false;
                if (!aimAssist)
                {
                    if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                    {

                        StartCoroutine(
                            PlayTrail(
                                ActiveGun.ShootSystem.transform.position,
                                hit.point,
                                hit
                            )
                        );
                    }

                    else
                    {
                        StartCoroutine(
                             PlayTrail(
                                 ActiveGun.ShootSystem.transform.position,
                                 ActiveGun.ShootSystem.transform.position + (ActiveGun.ShootSystem.transform.forward * ActiveGun.TrailConfig.MissDistance),
                                 new RaycastHit()
                             )
                         );
                    }
                }
                else
                {
                    if (Physics.SphereCast(ray, sphereCastRadius, out hitnew, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                    {
                        //Debug.DrawLine(ActiveGun.Model.transform.position, hitnew.point, Color.green); 
                        //Debug.Log("Aim Assist: Hit");

                        Vector3 sphereCastMidpoint = hitnew.point;

                        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                        {
                            rayHitPoint = hit.point;
                        }

                        if (hitnew.collider.gameObject.layer != 18)
                        {

                            hitpoint = rayHitPoint;
                            aimAssistHit = hit;
                        }
                        else
                        {
                            if (mouseX == 0 && mouseY == 0)
                            {
                                hitpoint = rayHitPoint;
                                aimAssistHit = hit;
                            }
                            else
                            {

                                if (timeLeftBlock < 0.1)
                                {
                                    //Debug.Log("laand");
                                    hitpoint = hitnew.collider.ClosestPointOnBounds(hitnew.point);
                                    aimAssistHit = hitnew;
                                }

                                else
                                {
                                    hitpoint = rayHitPoint;
                                    aimAssistHit = hit;
                                }

                            }

                        }
                        //Debug.DrawLine(ActiveCamera.transform.position, sphereCastMidpoint, Color.green);


                        StartCoroutine(
                            PlayTrail(
                                ActiveGun.ShootSystem.transform.position,
                                hitpoint,
                                aimAssistHit
                            )
                        );

                    }
                    else
                    {
                        StartCoroutine(
                             PlayTrail(
                                 ActiveGun.ShootSystem.transform.position,
                                 ActiveGun.ShootSystem.transform.position + (ActiveGun.ShootSystem.transform.forward * ActiveGun.TrailConfig.MissDistance),
                                 new RaycastHit()
                             )
                         );
                    }
                }
            }
            if (loadOutManager.loadNumber == 3 || loadOutManager.loadNumber == 4)
            {
                if (weaponSwitching.selectedWeapon == 0)
                {
                    if (ActiveGun.AmmoConfig.CurrentClipAmmo != 1)
                    {
                        if (ActiveGun.shotgun)
                            anim.SetFloat("AnimSpeedChange", 1.3f);
                        else
                            anim.SetFloat("AnimSpeedChange", 1f);
                        SniperRig.weight = 0;
                        anim.SetLayerWeight(8, 1);
                        anim.SetBool("SniperReload", true);

                    }
                    if (shooterController.Aiming)
                    {
                        if(loadOutManager.loadNumber == 3)
                            StartCoroutine(CallAim());
                    }
                        
                }

            }
        }
    }
    public void HideUIScope()
    {
        foreach (GameObject i in HideUI)
        {
            i.SetActive(false);
        }
    }
    public void ShowUIScope()
    {
        foreach (GameObject i in HideUI)
        {
            i.SetActive(true);
        }
    }
    public IEnumerator CallAim()
    {
        
        yield return new WaitForSeconds(0.1f);
        shooterController.Aim();
    }
    [SerializeField] GameObject sphere;
    public GameObject CinemachineCameraTarget;


    private void OnDrawGizmos()
    {

        //RaycastHit hit;
        if (aimAssist)
        {
            //Debug.DrawRay(transform.position, transform.forward * 100f, Color.green);
            //Physics.SphereCast(ActiveCamera.transform.position, sphereCastRadius, sphere.transform.position, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask)
            if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, float.MaxValue, AimAssistHitMask))
            {
                //Debug.DrawLine(ActiveGun.Model.transform.position, hitnew.point, Color.green); 
                //Debug.Log("Aim Assist: Hit");
                Gizmos.color = Color.green;
                Vector3 sphereCastMidpoint = hit.point;
                //Debug.Log(hit.transform);
                Gizmos.DrawWireSphere(sphereCastMidpoint, sphereCastRadiusAimAssist);
                Gizmos.DrawSphere(hit.point, 0.1f);
                Debug.DrawLine(ActiveCamera.transform.position, sphereCastMidpoint, Color.green);
                if (Physics.Raycast(ray, out RaycastHit hitnew, float.MaxValue, AimAssistHitMask))
                {
                    Debug.DrawLine(ActiveCamera.transform.position, hitnew.point, Color.green);
                }

                //Debug.DrawLine(ActiveGun.Model.transform.position, hitCheck.point, Color.green);
                //float yVelocity = 0f;
                //float oldPos;

                // oldPos = Mathf.SmoothDamp, 1f, ref yVelocity, Time.deltaTime * 30f);



                //CinemachineCameraTarget.transform.localPosition = new Vector3(0, oldPos, 0);
            }
            //Debug.DrawLine(ActiveGun.Model.transform.position,r.direction, Color.blue);
        }
        else
        {
            //Debug.DrawRay(transform.position, transform.forward * 100f, Color.red);

            if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
            {
                Debug.Log("No Aim Assist: Hit");
                Gizmos.color = Color.red;
                Vector3 sphereCastMidpoint = transform.position + (transform.forward * (range - sphereCastRadius));
                Gizmos.DrawWireSphere(sphereCastMidpoint, sphereCastRadius);
                //Debug.DrawLine(screenCenterPoint, sphereCastMidpoint, Color.red);
            }
        }
    }
    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {
        
        TrailRenderer tail = GetObject().GetComponent<TrailRenderer>();
        //tail.GetComponent<Rigidbody>().AddForce(EndPoint*ActiveGun.TrailConfig.SimulationSpeed);
        
        
        tail.gameObject.SetActive(true);
        tail.transform.position = StartPoint;

        //yield return null; // avoid position carry-over from last frame if reused

        //tail.emitting = true;

        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            //tail.transform.position = Vector3.Lerp(
            //    StartPoint,
            //    EndPoint,
            //    Mathf.Clamp01(1 - (remainingDistance / distance))
            //);
            //remainingDistance -= ActiveGun.TrailConfig.SimulationSpeed * Time.deltaTime;
            if (base.IsServer)
                HitDirectionObserver(tail.gameObject, StartPoint, EndPoint, distance, remainingDistance);
            else
                HitDirectionServer(tail.gameObject, StartPoint, EndPoint, distance, remainingDistance);

            remainingDistance -= ActiveGun.TrailConfig.SimulationSpeed * Time.deltaTime;
            yield return null;
        }

        tail.transform.position = EndPoint;
        //Debug.Log(Hit.collider);
        if (Hit.collider != null)
        {
            HandleBulletImpact(distance, EndPoint, Hit.normal, Hit.collider,Hit.transform);

        }

        yield return new WaitForSeconds(ActiveGun.TrailConfig.Duration);
        yield return null;
        //tail.emitting = false;
        if (base.IsServer)
            DisableTrailObserver(tail.gameObject);
        else
            DisableTrailServer(tail.gameObject);
        tail.gameObject.SetActive(false);
        //DisableTrailServer(tail.gameObject);
        InstanceFinder.ServerManager.Despawn(tail.gameObject, DespawnType.Pool);
       
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void HitDirectionServer(GameObject tail, Vector3 StartPoint, Vector3 EndPoint, float distance, float remainingDistance)
    {
        tail.GetComponent<TrailRenderer>().emitting = true;
       
        tail.transform.position = Vector3.Lerp(
                StartPoint,
                EndPoint,
                Mathf.Clamp01(1 - (remainingDistance / distance))
            );
        
    }
    [ObserversRpc(BufferLast = false)]
    public void HitDirectionObserver(GameObject tail, Vector3 StartPoint, Vector3 EndPoint, float distance, float remainingDistance)
    {
        
        tail.GetComponent<TrailRenderer>().emitting = true;
        
        tail.transform.position = Vector3.Lerp(
                StartPoint,
                EndPoint,
                Mathf.Clamp01(1 - (remainingDistance / distance))
            );
        
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void DisableTrailServer(GameObject tail)
    {
        tail.GetComponent<TrailRenderer>().emitting = false;
    }
    [ObserversRpc(BufferLast = false)]
    public void DisableTrailObserver(GameObject tail)
    {
        tail.GetComponent<TrailRenderer>().emitting = false;
    }
    private void HandleBulletImpact(
       float DistanceTraveled,
       Vector3 HitLocation,
       Vector3 HitNormal,
       Collider HitCollider,Transform enemyPos)
    {
        
        if (HitCollider.gameObject.TryGetComponent<CapsuleCollider>(out CapsuleCollider collider))
            surfaceManager.HandleImpactBlood(HitCollider.gameObject,HitLocation,HitNormal,ActiveGun.ImpactType,0);
        
        else
            surfaceManager.HandleImpactConrete(HitCollider.gameObject, HitLocation, HitNormal, ActiveGun.ImpactType, 0);
        //if (HitCollider.TryGetComponent(out IDamageable damageable))
        //{
        //    damageable.TakeDamage(ActiveGun.DamageConfig.GetDamage(DistanceTraveled));
        //}
        IDamageable Damage = HitCollider.GetComponentInParent<IDamageable>();      
        if (Damage != null)
        {
            if(loadOutManager.loadNumber == 4)
            {
                if (weaponSwitching.selectedWeapon == 0)
                {
                    float distance = Vector3.Distance(transform.position, HitCollider.gameObject.transform.position);
                    if (distance > 10)
                        ActiveGun.DamageConfig.DamageReduction = 2;
                    else
                        ActiveGun.DamageConfig.DamageReduction = 1;
                }

            }
            else
                ActiveGun.DamageConfig.DamageReduction = 1;

            
            Damage.SetPlayerHealth(ActiveGun.DamageConfig.GetDamage(HitCollider.gameObject));
            floatingDamage.GetPosition(enemyPos, ActiveGun.DamageConfig.GetDamage(HitCollider.gameObject));
            //floatingDamage.CallStartAnimation();
        }
    }
    
    public GameObject spawnObject;
    public uint SpawnInterval;

    public List<NetworkObject> spawned = new List<NetworkObject>();  

    void PrewarmPools()
    {
        DefaultObjectPool pool = InstanceFinder.NetworkManager.GetComponent<DefaultObjectPool>();
        pool.CacheObjects(spawnObject.GetComponent<NetworkObject>(), 10, IsServer);
    }

    public NetworkObject GetObject()
    {

        NetworkObject getobject = NetworkManager.GetPooledInstantiated(spawnObject.GetComponent<NetworkObject>(), true);      
        getobject.gameObject.SetActive(true);
        InstanceFinder.ServerManager.Spawn(getobject);
        spawned.Add(getobject);

        return getobject;
    }
    public void SetLookInput(float lookX, float lookY,float movex, float movez)
    {
        mouseX = lookX;
        mouseY = lookY;
        moveX = movex;
        moveZ = movez;
    }
    public TextMeshProUGUI aimAssistText;
    public void AimAssistOption()
    {
        if (aimAssist)
        {
            aimAssistText.text = "Off";
            aimAssist = false;
        }
        else
        {
            aimAssistText.text = "On";
            aimAssist = true;
        }

    }
    public LayerMask AimAssistHitMask;
    public void AimAssis()
    { 
        if(aimAssist)
        {
            Vector3 screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);
            ray = Camera.main.ScreenPointToRay(screenCenterPoint);

            if (Physics.SphereCast(ray, sphereCastRadiusAimAssist, out RaycastHit hitnew, float.MaxValue, AimAssistHitMask))
            {
                Vector3 newpOS;
                if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
                {
                    //rayHitPoint = hit.point;
                }
                //Debug.DrawLine(ActiveGun.Model.transform.position, hitnew.point, Color.green); 
                //Debug.Log("Aim Assist: Hit");
                if (hitnew.collider.gameObject.TryGetComponent<CapsuleCollider>(out CapsuleCollider collider))
                {
                    if (!hit.collider.gameObject.TryGetComponent<CapsuleCollider>(out CapsuleCollider colliderNew))
                    {
                        if (thirdPersonController._animationBlend > 2)
                        {
                            if (!thirdPersonController.isSliding)
                            {
                                if (!ActiveGun.sniper)
                                {
                                    newpOS = (hit.collider.transform.position - hitnew.collider.transform.position);
                                   
                                    //Debug.Log(hitnew.collider.transform.position.x + "Sphere collider");
                                    //Debug.Log(hit.point.x + "Ray collider");
                                    //Debug.Log(hit.collider);
                                    //Debug.Log(newpOS.x);

                                    if (hitnew.collider.transform.position.x > hit.point.x)
                                    {
                                        thirdPersonController._cinemachineTargetYaw = Mathf.Lerp(thirdPersonController._cinemachineTargetYaw, thirdPersonController._cinemachineTargetYaw += 0.2f, Time.deltaTime * 200f);
                                    }


                                    if (hitnew.collider.transform.position.x < hit.point.x)
                                    {
                                        thirdPersonController._cinemachineTargetYaw = Mathf.Lerp(thirdPersonController._cinemachineTargetYaw, thirdPersonController._cinemachineTargetYaw -= 0.2f, Time.deltaTime * 200f);

                                    }
                                    
                                }

                            }


                        }

                    }
                    //
                }

            }
        }
    }

}
