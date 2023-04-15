using FishNet.Object;
using TMPro;
using UnityEngine;

public class AmmoDisplayer : NetworkBehaviour
{
    [SerializeField]
    private PlayerGunSelector GunSelector;
    [SerializeField] GameObject PrimaryGunHighlight;
    [SerializeField] GameObject SecondaryGunHighlight;
    [SerializeField] TextMeshProUGUI AmmoTextGunPrimary;
    [SerializeField] TextMeshProUGUI AmmoTextGunSecondary;

    private void Start()
    {
        AmmoTextGunPrimary.SetText
                     ($"{ GunSelector.gun1.AmmoConfig.CurrentClipAmmo} / "
                        + $"{GunSelector.gun1.AmmoConfig.CurrentAmmo}");


        AmmoTextGunSecondary.SetText
                    ($"{ GunSelector.gun2.AmmoConfig.CurrentClipAmmo} / "
                        + $"{GunSelector.gun2.AmmoConfig.CurrentAmmo}");
    }
    private void Update()
    {
        if (!base.IsOwner)
            return;
        
        if(GunSelector.ActiveGun.Automatic)
        {
            AmmoTextGunPrimary.SetText
           ($"{ GunSelector.gun1.AmmoConfig.CurrentClipAmmo} / "
           + $"{GunSelector.gun1.AmmoConfig.CurrentAmmo}");

            PrimaryGunHighlight.SetActive(true);
            SecondaryGunHighlight.SetActive(false);
        }
        if (!GunSelector.ActiveGun.Automatic)
        {
            AmmoTextGunSecondary.SetText
           ($"{ GunSelector.gun2.AmmoConfig.CurrentClipAmmo} / "
           + $"{GunSelector.gun2.AmmoConfig.CurrentAmmo}");
            PrimaryGunHighlight.SetActive(false);
            SecondaryGunHighlight.SetActive(true);
        }
    }
}