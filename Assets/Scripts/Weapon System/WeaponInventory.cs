using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{

    /// Array of all weapons. These are gotten in the order that they are parented to this object.
    private WeaponManager[] weapons;

    /// Currently equipped WeaponBehaviour.
    private WeaponManager equipped;

    /// Currently equipped index.
    private int equippedIndex = -1;

    bool gunInHand = false;
    public void Init(int equippedAtStart = 0)
    {
        //Cache all weapons. Beware that weapons need to be parented to the object this component is on!
        weapons = GetComponentsInChildren<WeaponManager>(true);

        //Disable all weapons. This makes it easier for us to only activate the one we need.
        //foreach (WeaponManager weapon in weapons)
            //weapon.gameObject.SetActive(false);

        //Equip.
        Equip(equippedAtStart);
    }

    public WeaponManager Equip(int index)
    {
        //If we have no weapons, we can't really equip anything.
        if (weapons == null)
            return equipped;

        //The index needs to be within the array's bounds.
        if (index > weapons.Length - 1)
            return equipped;

        //No point in allowing equipping the already-equipped weapon.
        if (equippedIndex == index)
            return equipped;

        //Disable the currently equipped weapon, if we have one.
        //if (equipped != null && !gunInHand)
            //equipped.gameObject.SetActive(false);

        //Update index.
        equippedIndex = index;
        //Update equipped.
        equipped = weapons[equippedIndex];
        //Activate the newly-equipped weapon.
        //if(gunInHand)
        //    equipped.gameObject.SetActive(true);

        //Return.
        return equipped;
    }
    public int GetLastIndex()
    {
        //Get last index with wrap around.
        int newIndex = equippedIndex - 1;
        if (newIndex < 0)
            newIndex = weapons.Length - 1;

        //Return.
        return newIndex;
    }

    public int GetNextIndex()
    {
        //Get next index with wrap around.
        int newIndex = equippedIndex + 1;
        if (newIndex > weapons.Length - 1)
            newIndex = 0;

        //Return.
        return newIndex;
    }
    public void GunCheck(bool state)
    {
        gunInHand = state;
    }
    public WeaponManager GetEquipped() => equipped;
    public int GetEquippedIndex() => equippedIndex;
}
