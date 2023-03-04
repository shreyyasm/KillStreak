using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyHealth Health;
    //enemyMovement
    //somekind of damageHurt

    private void Start()
    {
        //take damage
        Health.OnTakeDamage += HandlePain;
        Health.OnDeath += Die;
    }

    private void Die(Vector3 Position)
    {
       //stop movement if died
       //damage hurt
    }
    public void HandlePain(int Damage)
    {
        if (Health.CurrentHealth != 0)
        {
            // you can do some cool stuff based on the
            // amount of damage taken relative to max health
            // here we're simply setting the additive layer
            // weight based on damage vs max pain threshhold         
        }
    }
}
