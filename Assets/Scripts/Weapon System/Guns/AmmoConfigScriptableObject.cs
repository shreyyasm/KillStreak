using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ammo Config", menuName = "Guns/Ammo Config", order = 3)]
public class AmmoConfigScriptableObject : ScriptableObject
{
    public int MaxAmmo = 120;
    public int ClipSize = 30;

    public int CurrentAmmo = 120;
    public int CurrentClipAmmo = 30;

    public void Reload()
    {
        int maxReloadAmount = Mathf.Min(ClipSize, CurrentAmmo);
        int availableBulletInCurrentClip = ClipSize - CurrentClipAmmo;
        int reloadAmount = Mathf.Min(maxReloadAmount, availableBulletInCurrentClip);

        CurrentClipAmmo = CurrentClipAmmo + reloadAmount;
        CurrentAmmo -= reloadAmount;

    }
    public bool CanReload()
    {
        return CurrentClipAmmo < ClipSize && CurrentAmmo > 0;
    }
    public void RefillAmmo()
    {
        CurrentAmmo = MaxAmmo;
        CurrentClipAmmo = ClipSize;
    }
}
