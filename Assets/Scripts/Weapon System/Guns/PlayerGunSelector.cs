using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FishNet.Object;
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
    public PlayerAction playerAction;
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
            if (Time.time > ActiveGun.ShootConfig.FireRate + ActiveGun.LastShootTime)
            {
                if (ActiveGun.ShootConfig.IsHitscan)
                    FireCondition();
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
            obj.SetActive(false);
            pooledObjects.Add(obj);
            //PlayerAction.Instance.SpawnBulletServerRPC(pooledObjects[i]);

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
        
        if (Physics.Raycast(
                ActiveGun.ray,
                out RaycastHit hit,
                float.MaxValue,
                ActiveGun.ShootConfig.HitMask
            ))
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
        SurfaceManager.Instance.HandleImpact(
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
}
