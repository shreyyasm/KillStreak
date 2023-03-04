using FishNet.Object;
using TMPro;
using UnityEngine;

public class AmmoDisplayer : NetworkBehaviour
{
    [SerializeField]
    private PlayerGunSelector GunSelector;
    [SerializeField] TextMeshProUGUI AmmoText;

    private void Update()
    {
        if (!base.IsOwner)
            return;
        AmmoText.SetText
           ($"{ GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo} / "
           + $"{GunSelector.ActiveGun.AmmoConfig.CurrentAmmo}");
       
    }
}