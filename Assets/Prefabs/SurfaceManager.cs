using FishNet;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Observing;
using FishNet.Utility.Performance;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceManager : NetworkBehaviour
{
    List<GameObject> spawnedObject;

    [SerializeField] GameObject startGameCanvas;


    public void pool()
    {
        //startGameCanvas.SetActive(false);
        //if (base.IsServer)
        //    StartPoolObserver();
        //else
        //    StartPoolServer();
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
                            surfaceEffect = typeEffect.SurfaceEffect;
                            PlayEffects(HitPoint, HitNormal, activeTexture.Alpha);

                        }
                    }
                }
                else
                {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
                    {
                        if (typeEffect.ImpactType == Impact)
                        {
                            surfaceEffect = typeEffect.SurfaceEffect;
                            PlayEffects(HitPoint, HitNormal, 1);
                        }
                    }
                }
            }
        }
        else
        {
            //Texture activeTexture = GetActiveTextureFromRenderer(renderer, TriangleIndex);
            HitObject.TryGetComponent<Renderer>(out Renderer renderer);
            //Debug.Log("work");
            SurfaceType surfaceType = Surfaces.Find(surface => renderer);
            if (surfaceType != null)
            {
                foreach (Surface.SurfaceImpactTypeEffect typeEffect in surfaceType.Surface.ImpactTypeEffects)
                {
                    if (typeEffect.ImpactType == Impact)
                    {
                        surfaceEffect = typeEffect.SurfaceEffect;
                        PlayEffects(HitPoint, HitNormal, 1);

                    }
                }
            }
            else
            {
                foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
                {
                    if (typeEffect.ImpactType == Impact)
                    {
                        surfaceEffect = typeEffect.SurfaceEffect;
                        PlayEffects(HitPoint, HitNormal, 1);
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
    public SurfaceEffect surfaceEffect;
    public void PlayEffects(Vector3 HitPoint, Vector3 HitNormal, float SoundOffset)
    {
        if (base.IsServer)
            PlayEffectsObserver(HitPoint, HitNormal, SoundOffset);

        else
            PlayEffectsServer(HitPoint, HitNormal, SoundOffset);
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    private void PlayEffectsServer(Vector3 HitPoint, Vector3 HitNormal, float SoundOffset)
    {
        foreach (SpawnObjectEffect spawnObjectEffect in surfaceEffect.SpawnObjectEffects)
        {
            if (spawnObjectEffect.Probability > Random.value)
            {

                //GameObject getobject =  GetPooledObject(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal);
                GameObject getobject = GetObject(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal).gameObject;

                getobject.transform.forward = HitNormal;
                StartCoroutine(DisableImpact(getobject));
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

            }
        }

        foreach (PlayAudioEffect playAudioEffect in surfaceEffect.PlayAudioEffects)
        {
            AudioClip clip = playAudioEffect.AudioClips[Random.Range(0, playAudioEffect.AudioClips.Count)];
            //ObjectPool pool = ObjectPool.CreateInstance(playAudioEffect.AudioSourcePrefab.GetComponent<PoolableObject>(), DefaultPoolSizes);
            //AudioSource audioSource = pool.GetObject().GetComponent<AudioSource>();
            AudioSource audioSource = NetworkManager.GetPooledInstantiated(impactAudioPrefab.GetComponent<NetworkObject>(), true).GetComponent<AudioSource>();
            InstanceFinder.ServerManager.Spawn(audioSource.gameObject);


            audioSource.transform.position = HitPoint;
            audioSource.PlayOneShot(clip, SoundOffset * Random.Range(playAudioEffect.VolumeRange.x, playAudioEffect.VolumeRange.y));
            StartCoroutine(DisableAudioSource(audioSource, clip.length));
        }
    }
    [ObserversRpc(BufferLast = false, RunLocally = true)]
    private void PlayEffectsObserver(Vector3 HitPoint, Vector3 HitNormal, float SoundOffset)
    {
        foreach (SpawnObjectEffect spawnObjectEffect in surfaceEffect.SpawnObjectEffects)
        {
            if (spawnObjectEffect.Probability > Random.value)
            {

                //GameObject getobject =  GetPooledObject(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal);
                GameObject getobject = GetObject(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal).gameObject;

                getobject.transform.forward = HitNormal;
                StartCoroutine(DisableImpact(getobject));
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

            }
        }

        foreach (PlayAudioEffect playAudioEffect in surfaceEffect.PlayAudioEffects)
        {
            AudioClip clip = playAudioEffect.AudioClips[Random.Range(0, playAudioEffect.AudioClips.Count)];
            //ObjectPool pool = ObjectPool.CreateInstance(playAudioEffect.AudioSourcePrefab.GetComponent<PoolableObject>(), DefaultPoolSizes);
            //AudioSource audioSource = pool.GetObject().GetComponent<AudioSource>();
            AudioSource audioSource = NetworkManager.GetPooledInstantiated(impactAudioPrefab.GetComponent<NetworkObject>(), true).GetComponent<AudioSource>();
            InstanceFinder.ServerManager.Spawn(audioSource.gameObject);


            audioSource.transform.position = HitPoint;
            audioSource.PlayOneShot(clip, SoundOffset * Random.Range(playAudioEffect.VolumeRange.x, playAudioEffect.VolumeRange.y));
            StartCoroutine(DisableAudioSource(audioSource, clip.length));
        }
    }


    private IEnumerator DisableAudioSource(AudioSource AudioSource, float Time)
    {
        yield return new WaitForSeconds(Time);
        InstanceFinder.ServerManager.Despawn(AudioSource.gameObject, DespawnType.Pool);
        //AudioSource.gameObject.SetActive(false);
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
    private NetworkConnection ownerConnection;

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void StartPoolServer()
    {
        Debug.Log("Pool Server");
        //impactTrailPool = new GameObject("Imapct Pool");
        pooledObjects = new List<GameObject>();
        spawnedObject = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {

            GameObject obj = Instantiate(objectToPool);

            //obj.transform.parent = impactTrailPool.transform;
            pooledObjects.Add(obj);
            InstanceFinder.ServerManager.Spawn(obj, base.Owner);
            SetSpawnImpact(obj, this);
            //StartCoroutine(DelayDespawn(obj));
        }

    }
    [ObserversRpc(BufferLast = true)]
    public void StartPoolObserver()
    {
        Debug.Log("Pool Observer");
        //impactTrailPool = new GameObject("Imapct Pool");
        pooledObjects = new List<GameObject>();
        spawnedObject = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {

            GameObject obj = Instantiate(objectToPool);
            InstanceFinder.ServerManager.Spawn(obj, base.Owner);
            //obj.transform.parent = impactTrailPool.transform;
            //obj.SetActive(false);
            pooledObjects.Add(obj);
            //SpawnImpactServerRPC(obj);

            SetSpawnImpact(obj, this);
            //StartCoroutine(DelayDespawn(obj));
            //InstanceFinder.ServerManager.Despawn(obj, DespawnType.Pool);

        }

    }
    public IEnumerator DelayDespawn(NetworkObject obj)
    {
        yield return new WaitForSeconds(0.2f);
        InstanceFinder.ServerManager.Despawn(obj, DespawnType.Pool);

    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void GetPoolObjectServerRPC(int i, Vector3 Position, Quaternion Rotation, Vector3 HitNormal)
    {
        pooledObjects[i].transform.position = Position;
        pooledObjects[i].transform.rotation = Rotation;
        pooledObjects[i].gameObject.SetActive(true);
        // pooledObjects[i].transform.forward = HitNormal;
    }
    [ObserversRpc(BufferLast = true)]
    public void GetPoolObjectObserverRPC(int i, Vector3 Position, Quaternion Rotation, Vector3 HitNormal)
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
                    GetPoolObjectObserverRPC(i, Position, Rotation, HitNormal);

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
        InstanceFinder.ServerManager.Despawn(pooledObject, DespawnType.Pool);
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void SpawnImpactServerRPC(GameObject prefab)
    {
        ////Instansiate Bullet
        InstanceFinder.ServerManager.Spawn(prefab, base.Owner);
        SetSpawnImpact(prefab, this);

    }

    [ObserversRpc(BufferLast = true)]
    public void SetSpawnImpact(GameObject spawned, SurfaceManager script)
    {

        //script.spawnedObject = spawned;
        spawnedObject.Add(spawned);
    }

    public GameObject impactPrefab;
    public GameObject impactAudioPrefab;
    public uint SpawnInterval;

    public List<NetworkObject> spawned = new List<NetworkObject>();

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        // Prewarm pool
        PrewarmPools();
    }

    void PrewarmPools()
    {
        DefaultObjectPool impactPool = InstanceFinder.NetworkManager.GetComponent<DefaultObjectPool>();
        impactPool.CacheObjects(impactPrefab.GetComponent<NetworkObject>(), 40, IsServer);

        DefaultObjectPool audioPool = InstanceFinder.NetworkManager.GetComponent<DefaultObjectPool>();
        audioPool.CacheObjects(impactAudioPrefab.GetComponent<NetworkObject>(), 40, IsServer);
    }

    public NetworkObject GetObject(Vector3 Position, Quaternion Rotation, Vector3 HitNormal)
    {

        NetworkObject getobject = NetworkManager.GetPooledInstantiated(impactPrefab.GetComponent<NetworkObject>(), true);
        getobject.transform.position = Position;
        getobject.transform.rotation = Rotation;
        getobject.gameObject.SetActive(true);
        InstanceFinder.ServerManager.Spawn(getobject);
        spawned.Add(getobject);

        return getobject;
    }
}