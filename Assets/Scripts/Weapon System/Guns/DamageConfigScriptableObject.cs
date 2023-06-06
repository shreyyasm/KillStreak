using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[CreateAssetMenu(fileName = "Damage Config", menuName = "Guns/Damage Config", order = 1)]
public class DamageConfigScriptableObject : ScriptableObject
{
    public MinMaxCurve HeadDamageCurve;
    public MinMaxCurve BodyDamageCurve;
    public MinMaxCurve HandDamageCurve;
    public MinMaxCurve LegDamageCurve;
    public MinMaxCurve OtherDamageCurve;

    public int DamageReduction = 1;
    private void Reset()
    {
        HeadDamageCurve.mode = ParticleSystemCurveMode.Curve;
    }

    public int GetDamage(GameObject playerPart)
    {
        if(playerPart.CompareTag("Head"))
        {
            return  HeadGetDamage() / DamageReduction;
            //return HeadDamage;
        }
        else if(playerPart.CompareTag("Body"))
        {
            return BodyGetDamage() / DamageReduction;
            //return BodyDamage;
        }
        else if (playerPart.CompareTag("Hand"))
        {
            return HandGetDamage() / DamageReduction;
            //return HandDamage;
        }
        else if (playerPart.CompareTag("Leg"))
        {
            return LegGetDamage() / DamageReduction;
            //return LegDamage;
        }
        else if (playerPart.CompareTag("Joint"))
        {
            return OtherGetDamage() / DamageReduction;
            //return otherDamage;
        }
        return 0;
       // return Mathf.CeilToInt(DamageCurve.Evaluate(Distance, Random.value)); 
    }
   


    public int HeadGetDamage(float Distance = 0)
    {
        return Mathf.CeilToInt(HeadDamageCurve.Evaluate(Distance, Random.value));
    }
    public int BodyGetDamage(float Distance = 0)
    {
        return Mathf.CeilToInt(BodyDamageCurve.Evaluate(Distance, Random.value));
    }
    public int HandGetDamage(float Distance = 0)
    {
        return Mathf.CeilToInt(HandDamageCurve.Evaluate(Distance, Random.value));
    }
    public int LegGetDamage(float Distance = 0)
    {
        return Mathf.CeilToInt(LegDamageCurve.Evaluate(Distance, Random.value));
    }
    public int OtherGetDamage(float Distance = 0)
    {
        return Mathf.CeilToInt(OtherDamageCurve.Evaluate(Distance, Random.value));
    }

    public object Clone()
    {
        DamageConfigScriptableObject config = CreateInstance<DamageConfigScriptableObject>();

        config.HeadDamageCurve = HeadDamageCurve;
        return config;
    }
}
