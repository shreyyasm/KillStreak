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


 

    [SerializeField]
    private List<SurfaceType> Surfaces = new List<SurfaceType>();
    [SerializeField]
    private int DefaultPoolSizes = 10;

    [SerializeField]
    private Surface DefaultSurface;
    [SerializeField]
    private SurfaceEffect DefaultSurfaceEffect;

    [SerializeField]
    private Surface ConreteSurface;
    [SerializeField]
    private SurfaceEffect ConreteSurfaceEffect;
    [SerializeField]
    private Surface BloodSurface;
    [SerializeField]
    private SurfaceEffect BloodSurfaceEffect;

    public void HandleImpactConrete(GameObject HitObject, Vector3 HitPoint, Vector3 HitNormal, ImpactType Impact, int TriangleIndex)
    {
        if (HitObject.TryGetComponent<Terrain>(out Terrain terrain))
        {
            
            GameObject prefab = GetObjectImpact(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal).gameObject;
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
                            DefaultSurface = ConreteSurface;
                            DefaultSurfaceEffect = ConreteSurfaceEffect;
                            PlayEffects(prefab,HitPoint, HitNormal, activeTexture.Alpha);

                        }
                    }
                }
                else
                {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
                    {
                        if (typeEffect.ImpactType == Impact)
                        {
                            DefaultSurface = ConreteSurface;
                            DefaultSurfaceEffect = ConreteSurfaceEffect;
                            PlayEffects(prefab, HitPoint, HitNormal, 1);
                        }
                    }
                }
            }
        }
        else
        {
                   
            GameObject prefab = GetObjectImpact(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal).gameObject;
            HitObject.TryGetComponent<Renderer>(out Renderer renderer);
            //Debug.Log("work");
            SurfaceType surfaceType = Surfaces.Find(surface => renderer);
            if (surfaceType != null)
            {
                foreach (Surface.SurfaceImpactTypeEffect typeEffect in surfaceType.Surface.ImpactTypeEffects)
                {
                    if (typeEffect.ImpactType == Impact)
                    {
                        DefaultSurface = ConreteSurface;
                        DefaultSurfaceEffect = ConreteSurfaceEffect;
                        PlayEffects(prefab, HitPoint, HitNormal, 1);

                    }
                }
            }
            else
            {
                foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
                {
                    if (typeEffect.ImpactType == Impact)
                    {
                        DefaultSurface = ConreteSurface;
                        DefaultSurfaceEffect = ConreteSurfaceEffect;
                        PlayEffects(prefab, HitPoint, HitNormal, 1);
                    }
                }
            }
        }
    }
    public void HandleImpactBlood(GameObject HitObject, Vector3 HitPoint, Vector3 HitNormal, ImpactType Impact, int TriangleIndex)
    {
        if (HitObject.TryGetComponent<Terrain>(out Terrain terrain))
        {
            
           
            GameObject prefab =  GetObjectBlood(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal).gameObject;
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
                            DefaultSurface = BloodSurface;
                            DefaultSurfaceEffect = BloodSurfaceEffect;
                            PlayEffects(prefab, HitPoint, HitNormal, activeTexture.Alpha);

                        }
                    }
                }
                else
                {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
                    {
                        if (typeEffect.ImpactType == Impact)
                        {
                            DefaultSurface = BloodSurface;
                            DefaultSurfaceEffect = BloodSurfaceEffect;
                            PlayEffects(prefab, HitPoint, HitNormal, 1);
                        }
                    }
                }
            }
        }
        else
        {
           
           
            GameObject prefab = GetObjectBlood(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal).gameObject;
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
                        DefaultSurface = BloodSurface;
                        DefaultSurfaceEffect = BloodSurfaceEffect;
                        PlayEffects(prefab, HitPoint, HitNormal, 1);

                    }
                }
            }
            else
            {
                foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
                {
                    if (typeEffect.ImpactType == Impact)
                    {
                        DefaultSurface = BloodSurface;
                        DefaultSurfaceEffect = BloodSurfaceEffect;
                        PlayEffects(prefab, HitPoint, HitNormal, 1);
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
    
    public void PlayEffects(GameObject PoolObject, Vector3 HitPoint, Vector3 HitNormal, float SoundOffset)
    {
        if (base.IsServer)
            PlayEffectsObserver(PoolObject,HitPoint, HitNormal, SoundOffset);

        else
            PlayEffectsServer(PoolObject, HitPoint, HitNormal, SoundOffset);
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    private void PlayEffectsServer(GameObject PoolObject, Vector3 HitPoint, Vector3 HitNormal, float SoundOffset)
    {
        foreach (SpawnObjectEffect spawnObjectEffect in DefaultSurfaceEffect.SpawnObjectEffects)
        {
            if (spawnObjectEffect.Probability > Random.value)
            {

                //GameObject getobject =  GetPooledObject(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal);
                GameObject getobject = PoolObject;
                    

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

        foreach (PlayAudioEffect playAudioEffect in DefaultSurfaceEffect.PlayAudioEffects)
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
    private void PlayEffectsObserver(GameObject PoolObject, Vector3 HitPoint, Vector3 HitNormal, float SoundOffset)
    {
        foreach (SpawnObjectEffect spawnObjectEffect in DefaultSurfaceEffect.SpawnObjectEffects)
        {
            if (spawnObjectEffect.Probability > Random.value)
            {

                //GameObject getobject =  GetPooledObject(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal);
                GameObject getobject = PoolObject;

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

        foreach (PlayAudioEffect playAudioEffect in DefaultSurfaceEffect.PlayAudioEffects)
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

    IEnumerator DisableImpact(GameObject pooledObject)
    {
        yield return new WaitForSeconds(3f);
        InstanceFinder.ServerManager.Despawn(pooledObject, DespawnType.Pool);
    }
    

    [ObserversRpc(BufferLast = true)]
    public void SetSpawnImpact(GameObject spawned, SurfaceManager script)
    {

        //script.spawnedObject = spawned;
        spawnedObject.Add(spawned);
    }

    public GameObject impactPrefab;
    public GameObject impactAudioPrefab;

    public GameObject impactBloodPrefab;
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

        DefaultObjectPool impactBloodPool = InstanceFinder.NetworkManager.GetComponent<DefaultObjectPool>();
        impactBloodPool.CacheObjects(impactBloodPrefab.GetComponent<NetworkObject>(), 40, IsServer);

        DefaultObjectPool audioPool = InstanceFinder.NetworkManager.GetComponent<DefaultObjectPool>();
        audioPool.CacheObjects(impactAudioPrefab.GetComponent<NetworkObject>(), 40, IsServer);
    }

    public NetworkObject GetObjectImpact(Vector3 Position, Quaternion Rotation, Vector3 HitNormal)
    {

        NetworkObject getobject = NetworkManager.GetPooledInstantiated(impactPrefab.GetComponent<NetworkObject>(), true);
        getobject.transform.position = Position;
        getobject.transform.rotation = Rotation;
        getobject.gameObject.SetActive(true);
        InstanceFinder.ServerManager.Spawn(getobject);
        spawned.Add(getobject);

        return getobject;
    }
    public NetworkObject GetObjectBlood(Vector3 Position, Quaternion Rotation, Vector3 HitNormal)
    {

        NetworkObject getobject = NetworkManager.GetPooledInstantiated(impactBloodPrefab.GetComponent<NetworkObject>(), true);
        getobject.transform.position = Position;
        getobject.transform.rotation = Rotation;
        getobject.gameObject.SetActive(true);
        InstanceFinder.ServerManager.Spawn(getobject);
        spawned.Add(getobject);

        return getobject;
    }
}