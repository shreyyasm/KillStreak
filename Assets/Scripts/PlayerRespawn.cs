using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
public class PlayerRespawn : NetworkBehaviour
{
    public static PlayerRespawn Instance;
    CameraFollow player;
    private NetworkConnection ownerConnection;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        
    }
    private void Update()
    {
        //player = FindObjectOfType<CameraFollow>();
    }
    public void Respawn(GameObject player, GameObject playerCanvas, GameObject animatedCanvas)
    {
        StartCoroutine(StartSpawning(player, playerCanvas, animatedCanvas));
    }
    IEnumerator StartSpawning(GameObject player, GameObject playerCanvas,GameObject animatedCanvas)
    {
        NetworkObject playerObj = GetComponent<NetworkObject>();
        yield return new WaitForSeconds(10f);
        RespawnPlayer(player, playerCanvas, animatedCanvas);
        
    
        //player.SetActive(true);
        //InstanceFinder.ServerManager.Spawn(player,base.Owner);
        //playerObj.GiveOwnership(ownerConnection);
        //
        //player.SetActive(true);
        //if(playerCanvas!= null)
        //    playerCanvas.SetActive(true);     
        //if (animatedCanvas != null)
        //    animatedCanvas.SetActive(true);
        //player.GetComponent<PlayerHealth>().playerDead = false;
         

    }
    public void RespawnPlayer(GameObject player, GameObject playerCanvas, GameObject animatedCanvas)
    {

        if (base.IsServer)
            StartRespawnObserver(player,playerCanvas,animatedCanvas);
        else
            StartRespawnServer(player, playerCanvas, animatedCanvas);
    
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void StartRespawnServer(GameObject player, GameObject playerCanvas, GameObject animatedCanvas)
    {
        player.SetActive(true);
        if (playerCanvas != null)
            playerCanvas.SetActive(true);
        if (animatedCanvas != null)
            animatedCanvas.SetActive(true);
        player.GetComponent<PlayerHealth>().playerDead = false;
        //player.GetComponent<PlayerHealth>().RespawnAmmoLoadout();
        player.GetComponent<ThirdPersonController>().SetRigWeight();

    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void StartRespawnObserver(GameObject player, GameObject playerCanvas, GameObject animatedCanvas)
    {
        player.SetActive(true);
        if (playerCanvas != null)
            playerCanvas.SetActive(true);
        if (animatedCanvas != null)
            animatedCanvas.SetActive(true);
        player.GetComponent<PlayerHealth>().playerDead = false;
       // player.GetComponent<PlayerHealth>().RespawnAmmoLoadout();
        player.GetComponent<ThirdPersonController>().SetRigWeight();
    }
}
