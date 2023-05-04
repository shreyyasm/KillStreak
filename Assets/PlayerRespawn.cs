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
    public void Respawn(GameObject player)
    {
        StartCoroutine(StartSpawning(player));
    }
    IEnumerator StartSpawning(GameObject player)
    {
        yield return new WaitForSeconds(10f);
        //player.SetActive(true);
        InstanceFinder.ServerManager.Spawn(player);
    }
}
