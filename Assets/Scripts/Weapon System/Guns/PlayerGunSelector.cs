using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FishNet.Object;
[DisallowMultipleComponent]
public class PlayerGunSelector : NetworkBehaviour
{
    [SerializeField]
    private GunType PrimaryGun;

    [SerializeField]
    private GunType SecondaryGun;

    [SerializeField]
    private Transform GunParent;

    public List<GunScriptableObject> Guns;

    [SerializeField]
    private PlayerIK InverseKinematics;

    [Space]
    [Header("Runtime Filled")]
    public GunScriptableObject ActiveGun;

    [SerializeField]
    private WeaponSwitching weaponSwitching;

    int gunSelected;
    GunScriptableObject gun1;
    GunScriptableObject gun2;

    private void Start()
    {
        //gun1 = Guns.Find(gun => gun.Type == PrimaryGun);
        //gun2 = Guns.Find(gun => gun.Type == SecondaryGun);
        gun1 = Guns[0];
        gun2 = Guns[1];
        
        if (gun1 == null)
        {
            Debug.Log($"No GunscriptableObject found for GunType: {gun1}");
            return;
        }

       
        gun1.Spawn(GunParent, this);
        gun2.Spawn(GunParent, this);
       
    }
    private void Update()
    {
        //if (!base.IsOwner)
        //    return;
        gunSelected = weaponSwitching.selectedWeapon;
        if (gunSelected == 0)
        {
            if(!weaponSwitching.gunChanging)
                ActiveGun = gun1;
        }
        else
        {
            if (!weaponSwitching.gunChanging)
                ActiveGun = gun2;
        }
    }
}
