using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using StarterAssets;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    public GameObject PlayerCanvas;    
    public GameObject AnimatedCanvas;
    public LoadOutManager loadOutManager;
    public AmmoDisplayer ammoDisplayer;

    [SerializeField]
    public int _MaxHealth = 100;
    [SerializeField]
    public int _Health;
    public bool playerDead;
    public bool playerDeadConfirmed;
    public RigBuilder RigController;
    public int CurrentHealth { get => _Health; private set => _Health = value;}

    public int Maxhealth { get => _MaxHealth; private set => _MaxHealth = value; }

    public event IDamageable.TakeDamageEvent OnTakeDamage;
    public event IDamageable.DeathEvent OnDeath;
    Animator anim;
    NetworkObject player;
    public PointSystem pointSystem;
    public ThirdPersonController thirdPersonController;

    //Invincibilty
    public bool invincible;
    public float invincibiltyTimer;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        player = GetComponent<NetworkObject>();
        

        if (PlayerCanvas != null)
            PlayerCanvas.SetActive(true);
        
        playerDead = false;
        playerDeadConfirmed = false;
    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (PlayerCanvas != null)
            PlayerCanvas.SetActive(true);
        playerDead = false;
        playerDeadConfirmed = false;

        //pointSystem = GameObject.FindGameObjectWithTag("PointSystem").GetComponent<PointSystem>(); 
    }
    private void OnEnable()
    {
        CurrentHealth = Maxhealth;
        SetMaxHealth(CurrentHealth);

        invincible = true;
        thirdPersonController.SeeInvincibilty();
        
        if(base.Owner.IsLocalClient)
        {
            RespawnAmmoLoadout();
            ammoDisplayer.UpdateGunAmmo();
        }
        
    }
   
    public Slider slider;
    public Gradient gradient;
    public Image fill;
    
   
    public void SetMaxHealth(int health)
    {
        if(slider != null)
        {
            slider.maxValue = health;
            slider.value = health;

            fill.color = gradient.Evaluate(1f);
        }
        
    }

    public void SetHealth(int health)
    {
        if (slider != null)
        {
            slider.value = health;

            fill.color = gradient.Evaluate(slider.normalizedValue);
        }         
    }
    public void SetPlayerHealth(int damage)
    {
       if(!invincible)
       {
            if (base.IsServer)
                TakeDamageObserver(damage);

            else
                TakeDamageServer(damage);
       }
 
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void TakeDamageServer(int Damage)
    {
        
        int damageTaken = Mathf.Clamp(Damage, 0, CurrentHealth);
        CurrentHealth -= damageTaken;
        SetHealth(CurrentHealth);
        if(CurrentHealth <= 0)
        {
            PlayerDeath();
        }
        if (damageTaken != 0)
        {
            OnTakeDamage?.Invoke(damageTaken);
        }
        if(CurrentHealth == 0 && damageTaken != 0)
        {
            OnDeath?.Invoke(transform.position);
        }
        SetHealth(CurrentHealth);
        if (base.IsOwner)
        {
            if (CurrentHealth <= 0)
                PointSystem.Instance.RespawnStartCountdown();
        }
    }
    [ObserversRpc(BufferLast = true)]
    public void TakeDamageObserver(int Damage)
    {
        
        int damageTaken = Mathf.Clamp(Damage, 0, CurrentHealth);
        CurrentHealth -= damageTaken;
        if (CurrentHealth <= 0)
        {
            PlayerDeath();
        }
        if (damageTaken != 0)
        {
            OnTakeDamage?.Invoke(damageTaken);
        }
        if (CurrentHealth == 0 && damageTaken != 0)
        {
            OnDeath?.Invoke(transform.position);
        }
        SetHealth(CurrentHealth);
        if (base.IsOwner)
        {
            if (CurrentHealth <= 0)
                PointSystem.Instance.RespawnStartCountdown();
        }
    }
    private void Update()
    {

        if (CurrentHealth > 0)
            RigController.enabled = true;

        StartInvincibilty();
    }
   
    public void PlayerDeath()
    {
        if (CurrentHealth <= 0)
        {
            
            if (PlayerCanvas != null)
                PlayerCanvas.SetActive(false);
            if (AnimatedCanvas != null)
                AnimatedCanvas.SetActive(false);
            RigController.enabled = false;
            StartCoroutine(DespawnPlayer());

            playerDead = true;
            anim.SetLayerWeight(7, 1);
            anim.SetInteger("DeadIndex", Random.Range(0, 4));
            anim.SetBool("Dead", true);
            PlayerRespawn.Instance.Respawn(gameObject, PlayerCanvas, AnimatedCanvas);
        }
        //if (base.IsServer)
        //    PlayerDeathObserver();
        //else
        //    PlayerDeathServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void PlayerDeathServer()
    {
        
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void PlayerDeathObserver()
    {
        if (CurrentHealth <= 0)
        {
            if (PlayerCanvas != null)
                PlayerCanvas.SetActive(false);
            if (AnimatedCanvas != null)
                AnimatedCanvas.SetActive(false);
            RigController.enabled = false;
            StartCoroutine(DespawnPlayer());

            playerDead = true;
            anim.SetLayerWeight(7, 1);
            anim.SetInteger("DeadIndex", Random.Range(0, 4));
            anim.SetBool("Dead", true);
            PlayerRespawn.Instance.Respawn(player.gameObject, PlayerCanvas, AnimatedCanvas);
        }
    }
    public bool PlayerDeathState()
    {
        return playerDead;
    }
    GameObject spawnedCarte;
    IEnumerator DespawnPlayer()
    {
        yield return new WaitForSeconds(2f);

    
        gameObject.SetActive(false);
        //yield return new WaitForSeconds(0.5f);
        if (base.Owner.IsLocalClient)
        {
            NetworkObject spawnCrate = healthAmmoSpawner.GetObject(transform.position, Quaternion.identity);
            spawnedCarte = spawnCrate.gameObject;
        }
        //HealthAmmoSpawner.Instance.GetObject(transform.position, Quaternion.identity);
        //gameObject.SetActive(false);
        //DespwanPlayer();
        //InstanceFinder.ServerManager.Despawn(player.gameObject, DespawnType.Pool);   
    }
    public HealthAmmoSpawner healthAmmoSpawner;

    public void RestoreHealth()
    {
        CurrentHealth = Maxhealth;
    }
    public PlayerGunSelector playerGunSelector;
    public void RespawnAmmoLoadout()
    {
        //loadOutManager.GetLoadOutInput(loadOutManager.loadNumber);
        playerGunSelector.gun1.AmmoConfig.RefillAmmo();
        playerGunSelector.gun2.AmmoConfig.RefillAmmo();
    }
    public void DespawnHealthNAmmoCrate()
    {
        InstanceFinder.ServerManager.Despawn(spawnedCarte, DespawnType.Pool);
        //spawnedCarte.SetActive(false);
    }
    public void StartRespawnTimer()
    {
        if (CurrentHealth <= 0)
            pointSystem.RespawnStartCountdown();
    }
    public void StartInvincibilty()
    {
        if(invincible)
        {
            invincibiltyTimer -= Time.deltaTime;
        }
        if(invincibiltyTimer <= 0)
        {
            invincible = false;
            invincibiltyTimer = 5;
        }
    }
}
