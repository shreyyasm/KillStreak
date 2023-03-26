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

[DisallowMultipleComponent]
public class PlayerGunSelector : NetworkBehaviour
{

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
    GunScriptableObject gun1;
    GunScriptableObject gun2;

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

    private void Awake()
    {
        instance = this;
        ActiveCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
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
    private void Update()
    {
        if (!base.IsOwner)
            return;

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
            if (ActiveGun.ShootConfig.IsHitscan)
            {                         
                if (ActiveGun.Automatic)
                    FireConditionAutomatic();
                else
                    FireConditionManual();
            }

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
            ray = Camera.main.ScreenPointToRay(screenCenterPoint);

            Vector3 shootDirection = Vector3.zero;
            shootDirection = ActiveCamera.transform.forward + ActiveCamera.transform.TransformDirection(ActiveGun.ShootConfig.GetSpread(ActiveGun.shootHoldTime - ActiveGun.InitialClickTime));
            Vector3 origin = ActiveCamera.transform.position
                        + ActiveCamera.transform.forward * Vector3.Distance(
                                ActiveCamera.transform.position,
                                ActiveGun.ShootSystem.transform.position);
            ActiveGun.Fired = false;
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, ActiveGun.ShootConfig.HitMask))
            {
                Debug.Log(hit.collider);
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
            ray = Camera.main.ScreenPointToRay(screenCenterPoint);

            Vector3 shootDirection = Vector3.zero;
            shootDirection = ActiveCamera.transform.forward + ActiveCamera.transform.TransformDirection(ActiveGun.ShootConfig.GetSpread(ActiveGun.shootHoldTime - ActiveGun.InitialClickTime));
            Vector3 origin = ActiveCamera.transform.position
                        + ActiveCamera.transform.forward * Vector3.Distance(
                                ActiveCamera.transform.position,
                                ActiveGun.ShootSystem.transform.position);
            ActiveGun.Fired = false;
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
    private NetworkConnection newOwnerConnection;

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
}
