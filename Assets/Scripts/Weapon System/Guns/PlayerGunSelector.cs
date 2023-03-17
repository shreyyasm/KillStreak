using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FishNet.Object;
using FishNet.Observing;
using FishNet.Connection;

[DisallowMultipleComponent]
public class PlayerGunSelector : NetworkBehaviour
{
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
        StartPool();
        //SurfaceManager.Instance.StartPool();
        //gun1.StartPool();
        //gun2.StartPool();
        //ActiveGun.StartPool();
    }
    private void Update()
    {
        //if (!base.IsOwner)
        //    return;
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
        //ActiveGun.RayCast();
        if (playerAction.IsShooting && !playerAction.IsReloading)
        {
            //ActiveGun.CheckRay();
            //ActiveGun.RayCast();

                if (ActiveGun.ShootConfig.IsHitscan)
                    FireCondition();

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
            SpawnBulletServerRPC(pooledObjects[i]);
            base.Despawn(obj, DespawnType.Pool);
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
    public void FireCondition()
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

        TrailRenderer tail = GetPooledObject().GetComponent<TrailRenderer>();
        //TrailRenderer instance = TrailPool.Get();
        
        //bulletTrail = instance.gameObject;
        //bulletTrail.AddComponent<NetworkObject>();
        //bulletTrail.AddComponent<NetworkObserver>();
        //Debug.Log(bulletTrail);
        tail.gameObject.SetActive(true);
        tail.transform.position = StartPoint;
        yield return null; // avoid position carry-over from last frame if reused

        tail.emitting = true;

        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            tail.transform.position = Vector3.Lerp(
                StartPoint,
                EndPoint,
                Mathf.Clamp01(1 - (remainingDistance / distance))
            );
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
        tail.emitting = false;
        tail.gameObject.SetActive(false);
        //TrailPool.Release(instance);
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
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void SpawnBulletServerRPC(GameObject prefab)
    {

        ////Instansiate Bullet
        ServerManager.Spawn(prefab, base.Owner);
        SetSpawnBullet(prefab, this);
    }

    [ObserversRpc(BufferLast = false, IncludeOwner = true)]
    public void SetSpawnBullet(GameObject spawned, PlayerGunSelector script)
    {
        script.spawnedObject = spawned;

    }
}
