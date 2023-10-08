using FishNet.Managing.Object;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Observing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    public ImpactType ImpactType;
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public DamageConfigScriptableObject DamageConfig;
    public ShootConfigScriptableObject ShootConfig;
    public AmmoConfigScriptableObject AmmoConfig;
    public TrailConfigScriptableObject TrailConfig;
    public AudioConfigScriptableObject AudioConfig;
    public DefaultPrefabObjects fishnetSpawnList;
    private NetworkBehaviour ActiveMonoBehaviour;
    public AudioSource ShootingAudioSource;
    public GameObject Model;
    public List<GameObject> GunModels;
    public float LastShootTime;
    public float InitialClickTime;
    public float StopShootingTime;

    public ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;
    private ObjectPool<Bullet> BulletPool;
    private bool LastFrameWantedToShoot;
    public bool Automatic;
    public bool sniper;
    [SerializeField] public bool shotgun = false;
    [SerializeField] public int bulletPerShot = 6;
    [SerializeField] public float inaccuracyDistance = 5f;
    public bool Fired = false;
    GameObject fpsVirtualCamera;
    public GameObject aimVirtualCamera;
    public GameObject followVirtualCamera;
    GameObject bulletTrail;
    GameObject bulletTrailPool;
    TrailRenderer tail;
    public Vector3 shootDirection;
    public Vector3 spreadAmount;
   
    /// <summary>
    /// Spawns the Gun Model into the scene
    /// </summary>
    /// <param name="Parent">Parent for the gun model</param>
    /// <param name="ActiveMonoBehaviour">An Active MonoBehaviour that can have Coroutines attached to them.
    /// The input handling script is a good candidate for this.
    /// </param>
    public GameObject Spawn(Transform Parent, NetworkBehaviour ActiveMonoBehaviour)
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        // in editor these will not be properly reset, in build it's fine
        LastShootTime = 0;
        StopShootingTime = 0;
        InitialClickTime = 0;
        AmmoConfig.CurrentClipAmmo = AmmoConfig.ClipSize;
        AmmoConfig.CurrentAmmo = AmmoConfig.MaxAmmo;
   
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);
        //PlayerAction.Instance.SpawnBulletServerRPC(TrailPo);
        if (!ShootConfig.IsHitscan)
        {
            BulletPool = new ObjectPool<Bullet>(CreateBullet);
        }

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);
        Model.SetActive(false);
        GunModels.Add(Model); 
        //ActiveMonoBehaviour.ServerManager.Spawn(Model, ActiveMonoBehaviour.Owner);
        ////Instansiate Bullet
        //GameObject projectile = Instantiate(bulletPrefab, spawnBulletPosition.position, rotation);
        //ActiveMonoBehaviour.ServerManager.Spawn(Model, ActiveMonoBehaviour.Owner);
        //SetSpawnBullet(projectile, script);
       // PlayerGunSelector.instance.Spawn(Model);
        
        if (ActiveMonoBehaviour.Owner.IsLocalClient)
        {
            ShootingAudioSource = Model.GetComponent<AudioSource>();
            ShootSystem = Model.GetComponentInChildren<ParticleSystem>();

            aimVirtualCamera = GameObject.FindWithTag("Aim Camera");
            followVirtualCamera = GameObject.FindWithTag("Follow Camera");
            //fpsVirtualCamera = GameObject.FindWithTag("FPS Camera");
        }
        return Model;
        //StartPool();
    }

    /// <summary>
    /// Expected to be called every frame
    /// </summary>
    /// <param name="WantsToShoot">Whether or not the player is trying to shoot</param>
    public void Tick(bool WantsToShoot)
    {
        //foreach (Transform i in TrailPool.Get().transform)
        //{

        //}
        
        //Model.transform.localRotation = Quaternion.Lerp(
        //    Model.transform.localRotation,
        //    Quaternion.Euler(SpawnRotation),
        //    Time.deltaTime * ShootConfig.RecoilRecoverySpeed
        //);

        if (WantsToShoot)
        {
            LastFrameWantedToShoot = true;
            if(Automatic)
                TryToShoot();
            else
                TryToShootManual();
        }

        if (!WantsToShoot && LastFrameWantedToShoot)
        {
            StopShootingTime = Time.time;
            LastFrameWantedToShoot = false;
        }

    }

    /// <summary>
    /// Plays the reloading audio clip if assigned.
    /// Expected to be called on the first frame that reloading begins
    /// </summary>
    public void StartReloading()
    {

        AudioConfig.PlayReloadClip(ShootingAudioSource);
    }

    /// <summary>
    /// Handle ammo after a reload animation.
    /// ScriptableObjects can't catch Animation Events, which is how we're determining when the
    /// reload has completed, instead of using a timer
    /// </summary>
    public void EndReload()
    {
        AmmoConfig.Reload();
    }

    /// <summary>
    /// Whether or not this gun can be reloaded
    /// </summary>
    /// <returns>Whether or not this gun can be reloaded</returns>
    public bool CanReload()
    {
        return AmmoConfig.CanReload();
    }

    /// <summary>
    /// Performs the shooting raycast if possible based on gun rate of fire. Also applies bullet spread and plays sound effects based on the AudioConfig.
    /// </summary>
    public Ray ray;
    public float shootHoldTime;

    private void TryToShoot()
    { 
        if (Time.time - LastShootTime - ShootConfig.FireRate > Time.deltaTime)
        {
            float lastDuration = Mathf.Clamp(
                0,
                (StopShootingTime - InitialClickTime),
                ShootConfig.MaxSpreadTime
            );
            float lerpTime = (ShootConfig.RecoilRecoverySpeed - (Time.time - StopShootingTime))
                / ShootConfig.RecoilRecoverySpeed;

            InitialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));
        }

        if (Time.time > ShootConfig.FireRate + LastShootTime)
        {
            LastShootTime = Time.time;
            if (AmmoConfig.CurrentClipAmmo == 0)
            {
                AudioConfig.PlayOutOfAmmoClip(ShootingAudioSource);
                return;
            }

            //ShootSystem.Play();
            
            //AudioConfig.PlayShootingClip(ShootingAudioSource, AmmoConfig.CurrentClipAmmo == 1);

            shootHoldTime = Time.time;
            spreadAmount = ShootConfig.GetSpread(shootHoldTime - InitialClickTime);
            //Model.transform.forward += Model.transform.TransformDirection(spreadAmount);

            shootDirection = ShootSystem.transform.forward;

            Vector3 screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);
            Camera.main.transform.forward += Camera.main.transform.TransformDirection(spreadAmount);
            ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            
            followVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
            aimVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
            //fpsVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);

            AmmoConfig.CurrentClipAmmo--;
            Fired = false;
            if (ShootConfig.IsHitscan)
            {
                DoHitscanShoot(shootDirection);
            }
            else
            {
                DoProjectileShoot(shootDirection);
            }
        }
    }
    
    private void TryToShootManual()
    {
        if (Time.time - LastShootTime - ShootConfig.FireRate > Time.deltaTime)
        {
            float lastDuration = Mathf.Clamp(
                0,
                (StopShootingTime - InitialClickTime),
                ShootConfig.MaxSpreadTime
            );
            float lerpTime = (ShootConfig.RecoilRecoverySpeed - (Time.time - StopShootingTime))
                / ShootConfig.RecoilRecoverySpeed;

            InitialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));
        }

        if (Time.time > ShootConfig.FireRate + LastShootTime)
        {
            LastShootTime = Time.time;
            if (AmmoConfig.CurrentClipAmmo == 0)
            {
                AudioConfig.PlayOutOfAmmoClip(ShootingAudioSource);
                return;
            }

            //ShootSystem.Play();
            
            //AudioConfig.PlayShootingClip(ShootingAudioSource, AmmoConfig.CurrentClipAmmo == 1);

            shootHoldTime = Time.time;
            spreadAmount = ShootConfig.GetSpread(shootHoldTime - InitialClickTime);
            //Model.transform.forward += Model.transform.TransformDirection(spreadAmount);

            shootDirection = ShootSystem.transform.forward;

            Vector3 screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);
            Camera.main.transform.forward += Camera.main.transform.TransformDirection(spreadAmount);
            ray = Camera.main.ScreenPointToRay(screenCenterPoint);


            
            if(sniper)
            {
                aimVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(6f, 0.3f);
                followVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(3f, 0.1f);
            }
            else if(shotgun)
            {
                followVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(3f, 0.1f);
                aimVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(4f, 0.1f);
            }
            else
            {
                followVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
                aimVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
            }
            //fpsVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
            AmmoConfig.CurrentClipAmmo--;
            Fired = false;

            if (ShootConfig.IsHitscan)
            {
                DoHitscanShoot(shootDirection);
            }
            else
            {
                DoProjectileShoot(shootDirection);
            }

        }
    }
  
    /// <summary>
    /// Generates a live Bullet instance that is launched in the <paramref name="ShootDirection"/> direction
    /// with velocity from <see cref="ShootConfigScriptableObject.BulletSpawnForce"/>.
    /// </summary>
    /// <param name="ShootDirection"></param>
    private void DoProjectileShoot(Vector3 ShootDirection)
    {
        Bullet bullet = BulletPool.Get();
        bullet.gameObject.SetActive(true);
        bullet.OnCollsion += HandleBulletCollision;
        bullet.transform.position = ShootSystem.transform.position;
        bullet.Spawn(ShootDirection * ShootConfig.BulletSpawnForce);

        //TrailRenderer trail = TrailPool.Get();
        //if (trail != null)
        //{
        //    trail.transform.SetParent(bullet.transform, false);
        //    trail.transform.localPosition = Vector3.zero;
        //    trail.emitting = true;
        //    trail.gameObject.SetActive(true);
        //}
    }

    /// <summary>
    /// Performs a Raycast to determine if a shot hits something. Spawns a TrailRenderer
    /// and will apply impact effects and damage after the TrailRenderer simulates moving to the
    /// hit point. 
    /// See <see cref="PlayTrail(Vector3, Vector3, RaycastHit)"/> for impact logic.
    /// </summary>
    /// <param name="ShootDirection"></param>
    private void DoHitscanShoot(Vector3 ShootDirection)
    {

        //if (Physics.Raycast(
        //        ray,
        //        out RaycastHit hit,
        //        float.MaxValue,
        //        ShootConfig.HitMask
        //    ))
        //{
        //    ActiveMonoBehaviour.StartCoroutine(
        //        PlayTrail(
        //            ShootSystem.transform.position,
        //            hit.point,
        //            hit
        //        )
        //    );
        //}
       
        //else
        //{
        //    ActiveMonoBehaviour.StartCoroutine(
        //        PlayTrail(
        //            ShootSystem.transform.position,
        //            ShootSystem.transform.position + (ShootDirection * TrailConfig.MissDistance),
        //            new RaycastHit()
        //        )
        //    );
        //}

    }

    /// <summary>
    /// Plays a bullet trail/tracer from start/end point. 
    /// If <paramref name="Hit"/> is not an empty hit, it will also play an impact using the <see cref="SurfaceManager"/>.
    /// </summary>
    /// <param name="StartPoint">Starting point for the trail</param>
    /// <param name="EndPoint">Ending point for the trail</param>
    /// <param name="Hit">The hit object. If nothing is hit, simply pass new RaycastHit()</param>
    /// <returns>Coroutine</returns>
    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {

        //TrailRenderer tail = GetPooledObject().GetComponent<TrailRenderer>();
        //TrailRenderer instance = TrailPool.Get();
        //tail = bulletTrailPool.GetComponentInChildren<TrailRenderer>();
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
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        tail.transform.position = EndPoint;

        if (Hit.collider != null)
        {
            HandleBulletImpact(distance, EndPoint, Hit.normal, Hit.collider);
        }

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        tail.emitting = false;
        tail.gameObject.SetActive(false);
        //TrailPool.Release(instance);
    }

    /// <summary>
    /// Callback handler for <see cref="Bullet.OnCollsion"/>. Disables TrailRenderer, releases the 
    /// Bullet from the BulletPool, and applies impact effects if <paramref name="Collision"/> is not null.
    /// </summary>
    /// <param name="Bullet"></param>
    /// <param name="Collision"></param>
    private void HandleBulletCollision(Bullet Bullet, Collision Collision)
    {
        TrailRenderer trail = Bullet.GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.transform.SetParent(null, true);
            ActiveMonoBehaviour.StartCoroutine(DelayedDisableTrail(trail));
        }

        Bullet.gameObject.SetActive(false);
        BulletPool.Release(Bullet);

        if (Collision != null)
        {
            ContactPoint contactPoint = Collision.GetContact(0);

            HandleBulletImpact(
                Vector3.Distance(contactPoint.point, Bullet.SpawnLocation),
                contactPoint.point,
                contactPoint.normal,
                contactPoint.otherCollider
            );
        }
    }

    /// <summary>
    /// Disables the trail renderer based on the <see cref="TrailConfigScriptableObject.Duration"/> provided
    ///and releases it from the<see cref="TrailPool"/>
    /// </summary>
    /// <param name="Trail"></param>
    /// <returns></returns>
    private IEnumerator DelayedDisableTrail(TrailRenderer Trail)
    {
        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        Trail.emitting = false;
        Trail.gameObject.SetActive(false);
        TrailPool.Release(Trail);
    }

    /// <summary>
    ///// Calls <see cref="SurfaceManager.HandleImpactConrete(GameObject, Vector3, Vector3, ImpactType, int)"/> and applies damage
    /// if a damagable object was hit
    /// </summary>
    /// <param name="DistanceTraveled"></param>
    /// <param name="HitLocation"></param>
    /// <param name="HitNormal"></param>
    /// <param name="HitCollider"></param>
    private void HandleBulletImpact(
        float DistanceTraveled,
        Vector3 HitLocation,
        Vector3 HitNormal,
        Collider HitCollider)
    {
        //SurfaceManager.Instance.HandleImpact(
        //        HitCollider.gameObject,
        //        HitLocation,
        //        HitNormal,
        //        ImpactType,
        //        0
        //    );

        if (HitCollider.TryGetComponent(out IDamageable damageable))
        {
           // damageable.TakeDamage(DamageConfig.GetDamage(DistanceTraveled));
        }
    }

    /// <summary>
    /// Creates a trail Renderer for use in the object pool.
    /// </summary>
    /// <returns>A live TrailRenderer GameObject</returns>
    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        instance.AddComponent<NetworkObject>();
        instance.AddComponent<NetworkObserver>();
        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return trail;
    }

    /// <summary>
    /// Creates a Bullet for use in the object pool.
    /// </summary>
    /// <returns>A live Bullet GameObject</returns>
    private Bullet CreateBullet()
    {
        return Instantiate(ShootConfig.BulletPrefab);
    }

    public GameObject ReturnBullet()
    {
        return bulletTrail;
    }
    public static ObjectPooler SharedInstance;
    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int amountToPool;

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


}