using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundManager : NetworkBehaviour
{
    [SerializeField] PlayerGunSelector playerGunSelector;



    public void PlayShootingClip(Vector3 ImpactPos ,bool IsLastBullet = false)
    {
        if (base.IsServer)
            PlayShootingClipObserver(ImpactPos,IsLastBullet);

        if(base.IsClientOnly)
            PlayShootingClipServer(ImpactPos,IsLastBullet);
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void PlayShootingClipServer(Vector3 ImapctPos,bool IsLastBullet = false)
    {
        
        if (IsLastBullet && playerGunSelector.ActiveGun.AudioConfig.LastBulletClip != null)
        {
            //AudioSource.PlayClipAtPoint(playerGunSelector.ActiveGun.AudioConfig.LastBulletClip ,new Vector3(0,0,0),0.5f);
            
           playerGunSelector.ActiveGunPrefab.GetComponent<AudioSource>().PlayOneShot(playerGunSelector.ActiveGun.AudioConfig.LastBulletClip, playerGunSelector.ActiveGun.AudioConfig.Volume);
        }
        else
        {
            playerGunSelector.ActiveGunPrefab.GetComponent<AudioSource>().PlayOneShot(playerGunSelector.ActiveGun.AudioConfig.FireClips[Random.Range(0, playerGunSelector.ActiveGun.AudioConfig.FireClips.Length)], playerGunSelector.ActiveGun.AudioConfig.Volume);
        }
    }
    [ObserversRpc(BufferLast = true,RunLocally = true)]
    public void PlayShootingClipObserver(Vector3 ImpactPos, bool IsLastBullet = false)
    {
        
        if (IsLastBullet && playerGunSelector.ActiveGun.AudioConfig.LastBulletClip != null)
        {

            //AudioSource.PlayClipAtPoint(playerGunSelector.ActiveGun.AudioConfig.LastBulletClip, new Vector3(0, 0, 0), 0.5f);
            playerGunSelector.ActiveGunPrefab.GetComponent<AudioSource>().PlayOneShot(playerGunSelector.ActiveGun.AudioConfig.LastBulletClip, playerGunSelector.ActiveGun.AudioConfig.Volume);
        }
        else
        {
            playerGunSelector.ActiveGunPrefab.GetComponent<AudioSource>().PlayOneShot(playerGunSelector.ActiveGun.AudioConfig.FireClips[Random.Range(0, playerGunSelector.ActiveGun.AudioConfig.FireClips.Length)], playerGunSelector.ActiveGun.AudioConfig.Volume);
        }
    }

    public void  PlayReloadClip()
    {
        if (base.IsServer)
            PlayReloadClipObserver();

        if (base.IsOwner)
            PlayReloadClipServer();
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void PlayReloadClipServer()
    {
        if (playerGunSelector.ActiveGun.AudioConfig.ReloadClip != null)
        {
            playerGunSelector.ActiveGun.ShootingAudioSource.PlayOneShot(playerGunSelector.ActiveGun.AudioConfig.ReloadClip, playerGunSelector.ActiveGun.AudioConfig.Volume);
        }
    }

    [ObserversRpc]
    public void PlayReloadClipObserver()
    {
        if (playerGunSelector.ActiveGun.AudioConfig.ReloadClip != null)
        {
            playerGunSelector.ActiveGun.ShootingAudioSource.PlayOneShot(playerGunSelector.ActiveGun.AudioConfig.ReloadClip, playerGunSelector.ActiveGun.AudioConfig.Volume);
        }
    }
}
