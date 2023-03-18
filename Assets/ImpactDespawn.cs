using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactDespawn : NetworkBehaviour
{
    public void StartDespawn()
    {
        StartCoroutine(Despawn());
    }
    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(3f);
        if (base.IsServer)
            DespawnImpactServer(gameObject);
        else
            DespawnImpactObserver(gameObject);
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void DespawnImpactServer(GameObject pooledObject)
    {
        //base.Despawn(pooledObject, DespawnType.Pool);
        pooledObject.SetActive(false);
    }
    [ObserversRpc(BufferLast = false, IncludeOwner = true)]
    public void DespawnImpactObserver(GameObject pooledObject)
    {
        //base.Despawn(pooledObject, DespawnType.Pool);
        pooledObject.SetActive(false);
    }
}
