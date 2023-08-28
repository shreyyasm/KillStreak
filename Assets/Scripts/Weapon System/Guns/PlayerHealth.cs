using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    public GameObject PlayerCanvas;    
    public GameObject AnimatedCanvas;

    [SerializeField]
    private int _MaxHealth = 100;
    [SerializeField]
    private int _Health;
    public bool playerDead;
    public RigBuilder RigController;
    public int CurrentHealth { get => _Health; private set => _Health = value;}

    public int Maxhealth { get => _MaxHealth; private set => _MaxHealth = value; }

    public event IDamageable.TakeDamageEvent OnTakeDamage;
    public event IDamageable.DeathEvent OnDeath;
    Animator anim;
    NetworkObject player;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        player = GetComponent<NetworkObject>();
        if (PlayerCanvas != null)
            PlayerCanvas.SetActive(true);
        
        playerDead = false;
    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (PlayerCanvas != null)
            PlayerCanvas.SetActive(true);
        playerDead = false;
    }
    private void OnEnable()
    {
        CurrentHealth = Maxhealth;
        SetMaxHealth(CurrentHealth);
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
        if (base.IsServer)
            TakeDamageObserver(damage);

        else
            TakeDamageServer(damage);
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
    }
    private void Update()
    {

        if (CurrentHealth > 0)
            RigController.enabled = true;

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
            PlayerRespawn.Instance.Respawn(player.gameObject, PlayerCanvas, AnimatedCanvas);
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
    IEnumerator DespawnPlayer()
    {
        yield return new WaitForSeconds(2f);

        //HealthAmmoSpawner.Instance.GetObject(transform.position, Quaternion.identity);
        //gameObject.SetActive(false);
        DespwanPlayer();
        //InstanceFinder.ServerManager.Despawn(player.gameObject, DespawnType.Pool);   
    }
    public HealthAmmoSpawner healthAmmoSpawner;
    public void DespwanPlayer()
    {
        if (base.IsServer)
            DespawnPlayerObserver();
        else
            DespawnPlayerServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void DespawnPlayerServer()
    {
        healthAmmoSpawner.GetObject(transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void DespawnPlayerObserver()
    {
        healthAmmoSpawner.GetObject(transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
    public void RestoreHealth()
    {
        CurrentHealth = Maxhealth;
    }
   
}
