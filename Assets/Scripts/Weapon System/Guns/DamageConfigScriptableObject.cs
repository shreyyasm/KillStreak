using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[CreateAssetMenu(fileName = "Damage Config", menuName = "Guns/Damage Config", order = 1)]
public class DamageConfigScriptableObject : ScriptableObject
{
    public MinMaxCurve DamageCurve;
    public int HeadDamage;
    public int BodyDamage;
    public int HandDamage;
    public int LegDamage;
    public int otherDamage;
    private void Reset()
    {
        DamageCurve.mode = ParticleSystemCurveMode.Curve;
    }

    public int GetDamage(GameObject playerPart)
    {
        if(playerPart.CompareTag("Head"))
        {
            return HeadDamage;
        }
        else if(playerPart.CompareTag("Body"))
        {
            return BodyDamage;
        }
        else if (playerPart.CompareTag("Hand"))
        {
            return HandDamage;
        }
        else if (playerPart.CompareTag("Leg"))
        {
            return LegDamage;
        }
        else if (playerPart.CompareTag("Joint"))
        {
            return otherDamage;
        }
        return 0;
       // return Mathf.CeilToInt(DamageCurve.Evaluate(Distance, Random.value)); 
    }
}
