using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public void Respawn(GameObject player, GameObject playerCanvas)
    {
        StartCoroutine(StartSpawning(player, playerCanvas));
    }
    IEnumerator StartSpawning(GameObject player, GameObject playerCanvas)
    {
        NetworkObject playerObj = GetComponent<NetworkObject>();
        yield return new WaitForSeconds(10f);
        //player.SetActive(true);
        //InstanceFinder.ServerManager.Spawn(player,base.Owner);
        //playerObj.GiveOwnership(ownerConnection);      
        player.SetActive(true);
        if(playerCanvas!= null)
            playerCanvas.SetActive(true);
        player.GetComponent<PlayerHealth>().playerDead = false;

        
    }
}
