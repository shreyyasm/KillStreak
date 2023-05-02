using UnityEngine;

public interface IDamageable
{
    public int CurrentHealth { get; }
    public int Maxhealth { get; }
  
    public delegate void TakeDamageEvent(int Damage);
    public event TakeDamageEvent OnTakeDamage;

    public delegate void DeathEvent(Vector3 Position);
    public event DeathEvent OnDeath;
    public void SetPlayerHealth(int damage);
    public void TakeDamageServer(int Damage);
    public void TakeDamageObserver(int Damage);
}
