using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IDamageable))]
public class SpawnParticleSystemOnDeath : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem DeathSystem;
    public IDamageable Damageable;
    private void OnEnable()
    {
        Damageable.OnDeath += Damageable_OnDeath;
    }

    private void Damageable_OnDeath(Vector3 Position)
    {
        Instantiate(DeathSystem, Position, Quaternion.identity);
    }
}
