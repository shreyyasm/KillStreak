using FishNet;
using FishNet.Object;
using FishNet.Utility.Performance;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthAmmoSpawner : NetworkBehaviour
{
    public static HealthAmmoSpawner Instance;
    public GameObject AddOnPrefab;
    

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        // Prewarm pool
        PrewarmPools();
        
        
        
    }
   
    void PrewarmPools()
    {
        DefaultObjectPool impactPool = InstanceFinder.NetworkManager.GetComponent<DefaultObjectPool>();
        impactPool.CacheObjects(AddOnPrefab.GetComponent<NetworkObject>(), 5, IsServer);

        
    }
    public NetworkObject ReferenceObject;
    public NetworkObject GetObject(Vector3 Position, Quaternion Rotation)
    {

        NetworkObject getobject = NetworkManager.GetPooledInstantiated(AddOnPrefab.GetComponent<NetworkObject>(), true);
        getobject.transform.position = Position;
        getobject.transform.rotation = Rotation;
        getobject.gameObject.SetActive(true);
        InstanceFinder.ServerManager.Spawn(getobject);
        //if (base.IsServer)
        //    GetHealthObjectObserver(Position,Rotation);
        //else
        //    GetHealthObjectServer(Position, Rotation);
        Debug.Log("Spawn");

        return getobject;
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void GetHealthObjectServer(Vector3 Position, Quaternion Rotation)
    {
        NetworkObject getobject = NetworkManager.GetPooledInstantiated(AddOnPrefab.GetComponent<NetworkObject>(), true);
        getobject.transform.position = Position;
        getobject.transform.rotation = Rotation;
        getobject.gameObject.SetActive(true);
        InstanceFinder.ServerManager.Spawn(getobject);
        ReferenceObject = getobject;
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void GetHealthObjectObserver(Vector3 Position, Quaternion Rotation)
    {
        NetworkObject getobject = NetworkManager.GetPooledInstantiated(AddOnPrefab.GetComponent<NetworkObject>(), true);
        getobject.transform.position = Position;
        getobject.transform.rotation = Rotation;
        getobject.gameObject.SetActive(true);
        InstanceFinder.ServerManager.Spawn(getobject);
        ReferenceObject = getobject;
    }
}
