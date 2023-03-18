using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Observing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceManager : NetworkBehaviour
{
    public GameObject spawnedObject;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        StartPoolServer();
    }



    [SerializeField]
    private List<SurfaceType> Surfaces = new List<SurfaceType>();
    [SerializeField]
    private int DefaultPoolSizes = 10;
    [SerializeField]
    private Surface DefaultSurface;

    public void HandleImpact(GameObject HitObject, Vector3 HitPoint, Vector3 HitNormal, ImpactType Impact, int TriangleIndex)
    {
        if (HitObject.TryGetComponent<Terrain>(out Terrain terrain))
        {
            List<TextureAlpha> activeTextures = GetActiveTexturesFromTerrain(terrain, HitPoint);
            foreach (TextureAlpha activeTexture in activeTextures)
            {
                SurfaceType surfaceType = Surfaces.Find(surface => activeTexture.Texture);
                if (surfaceType != null)
                {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in surfaceType.Surface.ImpactTypeEffects)
                    {
                        if (typeEffect.ImpactType == Impact)
                        {
                            PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, activeTexture.Alpha);
                        }
                    }
                }
                else
                {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
                    {
                        if (typeEffect.ImpactType == Impact)
                        {
                            PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, 1);
                        }
                    }
                }
            }
        }
        else if (HitObject.TryGetComponent<Renderer>(out Renderer renderer))
        {
            //Texture activeTexture = GetActiveTextureFromRenderer(renderer, TriangleIndex);

            SurfaceType surfaceType = Surfaces.Find(surface => renderer);
            if (surfaceType != null)
            {
                foreach (Surface.SurfaceImpactTypeEffect typeEffect in surfaceType.Surface.ImpactTypeEffects)
                {
                    if (typeEffect.ImpactType == Impact)
                    {
                        PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, 1);
                    }
                }
            }
            else
            {
                foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
                {
                    if (typeEffect.ImpactType == Impact)
                    {
                        PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, 1);
                    }
                }
            }
        }
    }

    private List<TextureAlpha> GetActiveTexturesFromTerrain(Terrain Terrain, Vector3 HitPoint)
    {
        Vector3 terrainPosition = HitPoint - Terrain.transform.position;
        Vector3 splatMapPosition = new Vector3(
            terrainPosition.x / Terrain.terrainData.size.x,
            0,
            terrainPosition.z / Terrain.terrainData.size.z
        );

        int x = Mathf.FloorToInt(splatMapPosition.x * Terrain.terrainData.alphamapWidth);
        int z = Mathf.FloorToInt(splatMapPosition.z * Terrain.terrainData.alphamapHeight);

        float[,,] alphaMap = Terrain.terrainData.GetAlphamaps(x, z, 1, 1);

        List<TextureAlpha> activeTextures = new List<TextureAlpha>();
        for (int i = 0; i < alphaMap.Length; i++)
        {
            if (alphaMap[0, 0, i] > 0)
            {
                activeTextures.Add(new TextureAlpha()
                {
                    Texture = Terrain.terrainData.terrainLayers[i].diffuseTexture,
                    Alpha = alphaMap[0, 0, i]
                });
            }
        }

        return activeTextures;
    }

    private Texture GetActiveTextureFromRenderer(Renderer Renderer, int TriangleIndex)
    {
        if (Renderer.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
        {
            Mesh mesh = meshFilter.mesh;

            if (mesh.subMeshCount > 1)
            {
                int[] hitTriangleIndices = new int[]
                {
                    mesh.triangles[TriangleIndex * 3],
                    mesh.triangles[TriangleIndex * 3 + 1],
                    mesh.triangles[TriangleIndex * 3 + 2]
                };

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    int[] submeshTriangles = mesh.GetTriangles(i);
                    for (int j = 0; j < submeshTriangles.Length; j += 3)
                    {
                        if (submeshTriangles[j] == hitTriangleIndices[0]
                            && submeshTriangles[j + 1] == hitTriangleIndices[1]
                            && submeshTriangles[j + 2] == hitTriangleIndices[2])
                        {
                            return Renderer.sharedMaterials[i].mainTexture;
                        }
                    }
                }
            }
            else
            {
                return Renderer.sharedMaterial.mainTexture;
            }
        }

        Debug.LogError($"{Renderer.name} has no MeshFilter! Using default impact effect instead of texture-specific one because we'll be unable to find the correct texture!");
        return null;
    }

    private void PlayEffects(Vector3 HitPoint, Vector3 HitNormal, SurfaceEffect SurfaceEffect, float SoundOffset)
    {
        foreach (SpawnObjectEffect spawnObjectEffect in SurfaceEffect.SpawnObjectEffects)
        {
            if (spawnObjectEffect.Probability > Random.value)
            {
               
                GameObject getobject =  GetPooledObject(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal);
                getobject.transform.forward = HitNormal;

                //getobject.GetComponent<ImpactDespawn>().StartDespawn();
                if (spawnObjectEffect.RandomizeRotation)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(0, 180 * spawnObjectEffect.RandomizedRotationMultiplier.x),
                        Random.Range(0, 180 * spawnObjectEffect.RandomizedRotationMultiplier.y),
                        Random.Range(0, 180 * spawnObjectEffect.RandomizedRotationMultiplier.z)
                    );

                    getobject.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + offset);
                }
                StartCoroutine(DisableImpact(getobject));
            }
        }

        //foreach (PlayAudioEffect playAudioEffect in SurfaceEffect.PlayAudioEffects)
        //{
        //    AudioClip clip = playAudioEffect.AudioClips[Random.Range(0, playAudioEffect.AudioClips.Count)];
        //    ObjectPool pool = ObjectPool.CreateInstance(playAudioEffect.AudioSourcePrefab.GetComponent<PoolableObject>(), DefaultPoolSizes);
        //    AudioSource audioSource = pool.GetObject().GetComponent<AudioSource>();

        //    audioSource.transform.position = HitPoint;
        //    audioSource.PlayOneShot(clip, SoundOffset * Random.Range(playAudioEffect.VolumeRange.x, playAudioEffect.VolumeRange.y));
        //    StartCoroutine(DisableAudioSource(audioSource, clip.length));
        //}
    }

    private IEnumerator DisableAudioSource(AudioSource AudioSource, float Time)
    {
        yield return new WaitForSeconds(Time);

        AudioSource.gameObject.SetActive(false);
    }

    private class TextureAlpha
    {
        public float Alpha;
        public Texture Texture;
    }
    
    public static ObjectPooler SharedInstance;
    private GameObject impactTrailPool;
    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int amountToPool;

    public void StartPoolServer()
    {
        Debug.Log("Pool");
        impactTrailPool = new GameObject("Imapct Pool");
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {

            GameObject obj = (GameObject)Instantiate(objectToPool);
            
            obj.transform.parent = impactTrailPool.transform;
            //obj.AddComponent<NetworkObject>();
            //obj.AddComponent<NetworkObserver>();
            
            //obj.SetActive(false);
            pooledObjects.Add(obj);
            
        }
        foreach (GameObject o in pooledObjects)
        {
            SpawnImpactServerRPC(o);
            base.Despawn(o, DespawnType.Pool);
        }
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void GetPoolObjectServerRPC(int i, Vector3 Position, Quaternion Rotation, Vector3 HitNormal)
    {
        pooledObjects[i].transform.position = Position;
        pooledObjects[i].transform.rotation = Rotation;
        pooledObjects[i].gameObject.SetActive(true);
       // pooledObjects[i].transform.forward = HitNormal;
    }
    [ObserversRpc(BufferLast = false, IncludeOwner = true)]
    public void GetPoolObjectObserverRPC(int i,Vector3 Position, Quaternion Rotation, Vector3 HitNormal)
    {
        pooledObjects[i].transform.position = Position;
        pooledObjects[i].transform.rotation = Rotation;
        pooledObjects[i].gameObject.SetActive(true);
       // pooledObjects[i].transform.forward = HitNormal;
    }

    public GameObject GetPooledObject(Vector3 Position, Quaternion Rotation, Vector3 HitNormal)
    {
        //1
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            //2
            if (!pooledObjects[i].activeInHierarchy)
            {
                if (base.IsServer)
                    GetPoolObjectObserverRPC(i,Position, Rotation, HitNormal);

                else
                    GetPoolObjectServerRPC(i, Position, Rotation, HitNormal);

                return pooledObjects[i];
            }
        }
        //3   
        return null;
    }
    IEnumerator DisableImpact(GameObject pooledObject)
    {
        yield return new WaitForSeconds(3f);
        //pooledObject.SetActive(false);
        if (base.IsServer)
            DespawnImpactServer(pooledObject);
        if(base.IsOwner)
            DespawnImpactObserver(pooledObject);
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void SpawnImpactServerRPC(GameObject prefab)
    {
        ////Instansiate Bullet
        ServerManager.Spawn(prefab, base.Owner);
        SetSpawnImpact(prefab, this);
    }

    [ObserversRpc(BufferLast = false, IncludeOwner = true)]
    public void SetSpawnImpact(GameObject spawned, SurfaceManager script)
    {
        script.spawnedObject = spawned;
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