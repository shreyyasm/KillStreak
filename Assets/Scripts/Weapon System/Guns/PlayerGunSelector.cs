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

[DisallowMultipleComponent]
public class PlayerGunSelector : NetworkBehaviour
{
    public LayerMask IdentifyEnemy;
    public static PlayerGunSelector instance;
    [SerializeField]
    private GunType PrimaryGun;

    [SerializeField]
    private GunType SecondaryGun;

    [SerializeField]
    private Transform GunParent;

    public List<GunScriptableObject> Guns;

    [SerializeField]
    private PlayerIK InverseKinematics;

    [Space]
    [Header("Runtime Filled")]
    public GunScriptableObject ActiveGun;

    [SerializeField]
    private WeaponSwitching weaponSwitching;
    [SerializeField]
    private SurfaceManager surfaceManager;
    int gunSelected;
    public GunScriptableObject gun1;
    public GunScriptableObject gun2;
   
    public GameObject bulletTrail;

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
    private float mouseX,mouseY;
    private  float moveX, moveZ;
    [SerializeField] GameObject BlockUI;
    [SerializeField] GameObject CrosshairUI;
    private void Awake()
    {
        instance = this;
        ActiveCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    [SerializeField] bool aimAssist;
    [SerializeField] float aimAssistSize = 1f;

    private void Start()
    {
       
        //gun1 = Guns.Find(gun => gun.Type == PrimaryGun);
        //gun2 = Guns.Find(gun => gun.Type == SecondaryGun);
        gun1 = Guns[0];
        gun2 = Guns[1];
        
        if (gun1 == null)
        {
            Debug.Log($"No GunscriptableObject found for GunType: {gun1}");
            return;
        }

        gunSelected = weaponSwitching.selectedWeapon;
        if (gunSelected == 0)
        {
            if (!weaponSwitching.gunChanging)
                ActiveGun = gun1;
        }
        else
        {
            if (!weaponSwitching.gunChanging)
                ActiveGun = gun2;
        }
        gun1.Spawn(GunParent, this);
        gun2.Spawn(GunParent, this);
        //StartPool();
        //SurfaceManager.Instance.StartPool();
        //gun1.StartPool();
        //gun2.StartPool();
        //ActiveGun.StartPool();
    }
    [Range(0.1f, 1f)] public float sphereCastRadius;
    [Range(1f, 100f)] public float range;
    
    private void Update()
    {
        if (!base.IsOwner)
            return;
        Vector3 screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);
        ray = Camera.main.ScreenPointToRay(screenCenterPoint);
       
        gunSelected = weaponSwitching.selectedWeapon;
       
        if (gunSelected == 0)
        {
            if(!weaponSwitching.gunChanging)
                ActiveGun = gun1;
        }
        else
        {
            if (!weaponSwitching.gunChanging)
                ActiveGun = gun2;
        }
        bulletTrail = ActiveGun.ReturnBullet();
        if (playerAction.IsShooting && !playerAction.IsReloading)
        {
            if(!playerAction.IsChangingGun)
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
        CheckBlocked();
    }
    RaycastHit hitCheck;
    public float distanceObject;
    public float distancePlayer;
    public float timeLeft = 0f;
    public float timeLeftBlock = 0f;
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
    public void StartPool()
    {
        bulletTrailPool = new GameObject("Bullet Pool");
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {

            GameObject obj = (GameObject)Instantiate(objectToPool);
            //PlayerAction.Instance.SpawnBulletServerRPC(obj);
            //PlayerAction.Instance.SpawnBulletServerRPC(obj);
            obj.transform.parent = bulletTrailPool.transform;
            //obj.AddComponent<NetworkObject>();
            //obj.AddComponent<NetworkObserver>();
            obj.SetActive(false);
            
            pooledObjects.Add(obj);
           // SpawnBulletServerRPC(pooledObjects[i]);
           //base.Despawn(obj, DespawnType.Pool);
        }


    }
    public GameObject GetPooledObject()
    {
        //1
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            //2
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        //3   
        return null;
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

    
    [SerializeField] GameObject sphere;
    public GameObject CinemachineCameraTarget;
    //private void OnDrawGizmos()
    //{

    //    //RaycastHit hit;
    //    if (aimAssist)
    //    {
    //        //Debug.DrawRay(transform.position, transform.forward * 100f, Color.green);
    //        //Physics.SphereCast(ActiveCamera.transform.position, sphereCastRadius, sphere.transform.position, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask)
    //        if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
    //        {
    //            //Debug.DrawLine(ActiveGun.Model.transform.position, hitnew.point, Color.green); 
    //            //Debug.Log("Aim Assist: Hit");
    //            Gizmos.color = Color.green; 
    //            Vector3 sphereCastMidpoint = hit.point;
    //            //Debug.Log(hit.transform);
    //            Gizmos.DrawWireSphere(sphereCastMidpoint, sphereCastRadius);
    //            Gizmos.DrawSphere(hit.point, 0.1f);
    //            Debug.DrawLine(ActiveCamera.transform.position, sphereCastMidpoint, Color.green);
    //            Debug.DrawLine(ActiveGun.Model.transform.position, hitCheck.point, Color.green);
    //            //float yVelocity = 0f;
    //            //float oldPos;

    //            // oldPos = Mathf.SmoothDamp, 1f, ref yVelocity, Time.deltaTime * 30f);



    //            //CinemachineCameraTarget.transform.localPosition = new Vector3(0, oldPos, 0);
    //        }
    //        //Debug.DrawLine(ActiveGun.Model.transform.position,r.direction, Color.blue);
    //    }
        //else
        //{
        //    //Debug.DrawRay(transform.position, transform.forward * 100f, Color.red);

        //    if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
        //    {
        //        Debug.Log("No Aim Assist: Hit");
        //        Gizmos.color = Color.red;
        //        Vector3 sphereCastMidpoint = transform.position + (transform.forward * (range - sphereCastRadius));
        //        Gizmos.DrawWireSphere(sphereCastMidpoint, sphereCastRadius);
        //        //Debug.DrawLine(screenCenterPoint, sphereCastMidpoint, Color.red);
        //    }
        //}
   // }
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
            HandleBulletImpact(distance, EndPoint, Hit.normal, Hit.collider);
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
       Collider HitCollider)
    {
        
        surfaceManager.HandleImpact(
                HitCollider.gameObject,
                HitLocation,
                HitNormal,
                ActiveGun.ImpactType,
                0
            );

        if (HitCollider.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(ActiveGun.DamageConfig.GetDamage(DistanceTraveled));
        }
    }
    
    public GameObject spawnObject;
    public uint SpawnInterval;

    public List<NetworkObject> spawned = new List<NetworkObject>();

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        // Prewarm pool
        PrewarmPools();
    }

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
}
