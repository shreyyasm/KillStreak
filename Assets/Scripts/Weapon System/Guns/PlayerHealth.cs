using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    public GameObject PlayerCanvas;
    [SerializeField]
    private int _MaxHealth = 100;
    [SerializeField]
    private int _Health;
    public bool playerDead;
    public int CurrentHealth { get => _Health; private set => _Health = value;}

    public int Maxhealth { get => _MaxHealth; private set => _MaxHealth = value; }

    public event IDamageable.TakeDamageEvent OnTakeDamage;
    public event IDamageable.DeathEvent OnDeath;
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        CurrentHealth = Maxhealth;
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
        Debug.Log("server");
        int damageTaken = Mathf.Clamp(Damage, 0, CurrentHealth);
        CurrentHealth -= damageTaken;
        if(damageTaken != 0)
        {
            OnTakeDamage?.Invoke(damageTaken);
        }
        if(CurrentHealth == 0 && damageTaken != 0)
        {
            OnDeath?.Invoke(transform.position);
        }
    }
    [ObserversRpc(BufferLast = true)]
    public void TakeDamageObserver(int Damage)
    {
        Debug.Log("observer");
        int damageTaken = Mathf.Clamp(Damage, 0, CurrentHealth);
        CurrentHealth -= damageTaken;
        if (damageTaken != 0)
        {
            OnTakeDamage?.Invoke(damageTaken);
        }
        if (CurrentHealth == 0 && damageTaken != 0)
        {
            OnDeath?.Invoke(transform.position);
        }
    }
    private void Update()
    {
        PlayerDeath();
    }
    public void PlayerDeath()
    {
        if(CurrentHealth <= 0)
        {
            if(PlayerCanvas != null)
                PlayerCanvas.SetActive(false);

            StartCoroutine(DespawnPlayer());
            playerDead = true;
            anim.SetLayerWeight(7, 1);
            anim.SetInteger("DeadIndex", Random.Range(0, 4));
            anim.SetBool("Dead", true);

        }
    }
    public bool PlayerDeathState()
    {
        return playerDead;
    }
    IEnumerator DespawnPlayer()
    {
        yield return new WaitForSeconds(5f);
        InstanceFinder.ServerManager.Despawn(gameObject);
    }   
}
