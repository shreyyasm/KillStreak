using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class WeaponManager : NetworkBehaviour
    {
        enum GunType {  Rifle = 0, Pistol = 1 }
        [SerializeField] GunType gunType;

        [Tooltip("Projectile Prefab. This is the prefab spawned when the weapon shoots.")]
        [SerializeField]

        private GameObject prefabProjectile;
        [Tooltip("Bullet Spawns from this position")]
        [SerializeField] Transform bulletSpawnPosition;

        //Camera's Reference
        public GameObject fpsVirtualCamera;
        GameObject aimVirtualCamera;
        GameObject followVirtualCamera;

        //References
        private ParticleSystem particles;
        private Light flashLight;
        private AudioSource audioSource;


        [SerializeField] ShooterController shooterController;
        [SerializeField] GameObject TPPspawnPosition;
        [SerializeField] AudioClip audioClipFire;
        [SerializeField] GameObject flashPrefab;
        [SerializeField] GameObject prefabFlashLight;
        GameObject spawnedObject;

        //Gun stats
        public int damage;
        public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
        public int magazineSize, bulletsPerTap;
        public bool allowButtonHold;
        int bulletsLeft, bulletsShot;

        //bools 
        bool shooting, readyToShoot, reloading;

        //Reference
        public Vector3 offset;
        public Vector3 vfxSpawnOffset;
        public Camera mainCamera;

        public RaycastHit rayHit;
        public LayerMask whatIsEnemy;

        //Graphics
        public GameObject muzzleFlash, bulletHoleGraphic;
        //public CamShake camShake;
        public float camShakeMagnitude, camShakeDuration;
        public TextMeshProUGUI text;
        Vector3 mouseWorldPosition;
        Vector3 aimDir;
        Quaternion rotation;
        GameObject spawnedFlashLightPrefab;
        bool inFPSMode;
        private void Awake()
        {

            bulletsLeft = magazineSize;
            readyToShoot = true;
            aimVirtualCamera = GameObject.FindWithTag("Aim Camera");
            followVirtualCamera = GameObject.FindWithTag("Follow Camera");

            //Instansiate Flash 
            audioSource = GetComponent<AudioSource>();
            GameObject flash = Instantiate(flashPrefab, bulletSpawnPosition);
            flash.transform.localPosition = default;
            flash.transform.localEulerAngles = default;
            flash.SetActive(true);

            //Instansiate FlashLight 
            spawnedFlashLightPrefab = Instantiate(prefabFlashLight, bulletSpawnPosition);
            spawnedFlashLightPrefab.transform.localPosition = offset;
            spawnedFlashLightPrefab.transform.localEulerAngles = default;
            flashLight = spawnedFlashLightPrefab.GetComponent<Light>();
            flashLight.enabled = false;

            particles = flash.GetComponent<ParticleSystem>();
            particles = flash.GetComponent<ParticleSystem>();
            
        }
   
    private void Update()
        {
            spawnedFlashLightPrefab.transform.localPosition = offset;
            //MyInput();
            mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            bulletSpawnPosition.transform.position = TPPspawnPosition.transform.position;
            if (inFPSMode)
            {
                bulletSpawnPosition.transform.localPosition = vfxSpawnOffset;
            }
            shooterController.GunType((int)gunType);
        //SetText
        //text.SetText("Bullets "+bulletsLeft + " / " + magazineSize);
        //if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();
    }
        public void FireBullet(bool FPSMODE, float input)
        {
            inFPSMode = FPSMODE;
            //if (allowButtonHold) shooting = Input.GetMouseButton(0);
            //else shooting = Input.GetMouseButtonDown(0);

            

            //Shoot
            if (allowButtonHold)
            {
                if (input == 1)
                    shooting = true;

                if (input == 0)
                {
                    shooting = false;
                    CancelInvoke("Fire");

                }

            }
            if (input == 1)
                shooting = true;
            else
                shooting = false;

            if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
            {
                bulletsShot = bulletsPerTap;
                if (allowButtonHold)
                    InvokeRepeating("Fire", 0f, timeBetweenShooting);
                else
                {
                    Fire();
                }

            }

        }
        private void ResetShot()
        {
            readyToShoot = true;
        }
        private void Reload()
        {
            reloading = true;
            Invoke("ReloadFinished", reloadTime);
        }
        private void ReloadFinished()
        {
            bulletsLeft = magazineSize;
            reloading = false;
        }
        [ServerRpc]
        public void Fire()
        {

            //Show Flash
            particles.Emit(5);
            flashLight.enabled = true;
            StartCoroutine(nameof(DisableFlashLight));
            audioSource.PlayOneShot(audioClipFire, 0.5f);

            //Camera Shake
            followVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
            aimVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
            fpsVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
            flashPrefab.SetActive(false);
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
            {
                //debugTransform.position = raycastHit.point;
                mouseWorldPosition = raycastHit.point;
            }
            //mouseWorldPosition = Vector3.zero;
            Vector3 worldAimTarget = mouseWorldPosition;
            aimDir = (mouseWorldPosition - bulletSpawnPosition.position).normalized;
            rotation = Quaternion.LookRotation(aimDir, Vector3.up);

            bulletsShot = bulletsPerTap;
            
            //Spawn projectile from the projectile spawn point.
            GameObject projectile = Instantiate(prefabProjectile, bulletSpawnPosition.position, rotation);
            ServerManager.Spawn(projectile, base.Owner);
            SetSpawnBullet(projectile, this);

            readyToShoot = false;

            //Spread
            float x = Random.Range(-spread, spread);
            float y = Random.Range(-spread, spread);

            //Calculate Direction with Spread
            Vector3 direction = mainCamera.transform.forward + new Vector3(x, y, 0);

            //RayCast
            if (Physics.Raycast(mainCamera.transform.position, direction, out rayHit, range, whatIsEnemy))
            {
                Debug.Log(rayHit.collider.name);

                //if (rayHit.collider.CompareTag("Enemy"))
                //rayHit.collider.GetComponent<ShootingAi>().TakeDamage(damage);
            }

            //ShakeCamera
            //camShake.Shake(camShakeDuration, camShakeMagnitude);

            //Graphics
            // Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.Euler(0, 180, 0));
            //Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

            bulletsLeft--;
            bulletsShot--;

            Invoke("ResetShot", timeBetweenShooting);

            if (bulletsShot > 0 && bulletsLeft > 0)
                Invoke("Fire", timeBetweenShots);


        }
        [ObserversRpc]
        public void SetSpawnBullet(GameObject spawned, WeaponManager script)
        {
            script.spawnedObject = spawned;
        }

        private IEnumerator DisableFlashLight()
        {
            //Wait.
            yield return new WaitForSeconds(0.1f);
            //Disable.
            flashLight.enabled = false;
        }
    }
