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
    public Surface Concrete;
    public Surface Blood;

    public SurfaceEffect ActiveSurfaceEffect;
    public SurfaceEffect surfaceEffectConcrete;
    public SurfaceEffect surfaceEffectBlood;
    public void HandleImpact(GameObject HitObject, Vector3 HitPoint, Vector3 HitNormal, ImpactType Impact, int TriangleIndex)
    {
        if (HitObject.CompareTag("Player"))
            DefaultSurface = Blood;
        else
            DefaultSurface = Concrete;
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
                            if(HitObject.CompareTag("Player"))
                                ActiveSurfaceEffect = surfaceEffectBlood;
                                                      
                            else
                                ActiveSurfaceEffect = surfaceEffectConcrete;
      
                            PlayEffects(HitObject, HitPoint, HitNormal, activeTexture.Alpha);
                            
                        }
                    }
                }
                else
                {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
                    {
                        if (typeEffect.ImpactType == Impact)
                        {
                            if (HitObject.CompareTag("Player"))
                                ActiveSurfaceEffect = surfaceEffectBlood;

                            else
                                ActiveSurfaceEffect = surfaceEffectConcrete;

                            PlayEffects(HitObject, HitPoint, HitNormal, 1);
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
                        if (HitObject.CompareTag("Player"))
                            ActiveSurfaceEffect = surfaceEffectBlood;

                        else
                            ActiveSurfaceEffect = surfaceEffectConcrete;

                        PlayEffects(HitObject, HitPoint, HitNormal, 1);
                       
                    }
                }
            }
            else
            {
                foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
                {
                    if (typeEffect.ImpactType == Impact)
                    {
                        if (HitObject.CompareTag("Player"))
                            ActiveSurfaceEffect = surfaceEffectBlood;

                        else
                            ActiveSurfaceEffect = surfaceEffectConcrete;

                        PlayEffects(HitObject,HitPoint, HitNormal, 1);
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

    
    
    public void PlayEffects(GameObject HitObject, Vector3 HitPoint, Vector3 HitNormal, float SoundOffset)
    {
        if (base.IsServer)
            PlayEffectsObserver(HitObject,HitPoint, HitNormal, SoundOffset);

        else
            PlayEffectsServer(HitObject,HitPoint, HitNormal, SoundOffset);
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    private void PlayEffectsServer(GameObject HitObject,Vector3 HitPoint, Vector3 HitNormal, float SoundOffset)
    {
        foreach (SpawnObjectEffect spawnObjectEffect in ActiveSurfaceEffect.SpawnObjectEffects)
        {
            if (spawnObjectEffect.Probability > Random.value)
            {
                GameObject getobject;

                if (HitObject.CompareTag("Player"))
                    getobject = GetObjectBlood(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal).gameObject;
                else
                    getobject = GetObjectConrete(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal).gameObject;
               


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

        foreach (PlayAudioEffect playAudioEffect in ActiveSurfaceEffect.PlayAudioEffects)
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
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    private void PlayEffectsObserver(GameObject HitObject, Vector3 HitPoint, Vector3 HitNormal, float SoundOffset)
    {
        foreach (SpawnObjectEffect spawnObjectEffect in ActiveSurfaceEffect.SpawnObjectEffects)
        {
            if (spawnObjectEffect.Probability > Random.value)
            {

                GameObject getobject;

                if (HitObject.CompareTag("Player"))
                    getobject = GetObjectBlood(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal).gameObject;
                else
                    getobject = GetObjectConrete(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal), HitNormal).gameObject;

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

        foreach (PlayAudioEffect playAudioEffect in ActiveSurfaceEffect.PlayAudioEffects)
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
    
    public GameObject concreteImpactPrefab;
    public GameObject bloodImpactPrefab;
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
        impactPool.CacheObjects(concreteImpactPrefab.GetComponent<NetworkObject>(), 40, IsServer);

        DefaultObjectPool audioPool = InstanceFinder.NetworkManager.GetComponent<DefaultObjectPool>();
        audioPool.CacheObjects(impactAudioPrefab.GetComponent<NetworkObject>(), 40, IsServer);

            DefaultObjectPool bloodPool = InstanceFinder.NetworkManager.GetComponent<DefaultObjectPool>();
        audioPool.CacheObjects(bloodImpactPrefab.GetComponent<NetworkObject>(), 20, IsServer);
    }

    public NetworkObject GetObjectConrete(Vector3 Position, Quaternion Rotation, Vector3 HitNormal)
    {
        
        NetworkObject getobject = NetworkManager.GetPooledInstantiated(concreteImpactPrefab.GetComponent<NetworkObject>(), true);
        getobject.transform.position = Position;
        getobject.transform.rotation = Rotation;
        getobject.gameObject.SetActive(true);
        InstanceFinder.ServerManager.Spawn(getobject);
        spawned.Add(getobject);

        return getobject;
    }
    public NetworkObject GetObjectBlood(Vector3 Position, Quaternion Rotation, Vector3 HitNormal)
    {

        NetworkObject getobject = NetworkManager.GetPooledInstantiated(bloodImpactPrefab.GetComponent<NetworkObject>(), true);
        getobject.transform.position = Position;
        getobject.transform.rotation = Rotation;
        getobject.gameObject.SetActive(true);
        InstanceFinder.ServerManager.Spawn(getobject);
        spawned.Add(getobject);

        return getobject;
    }
}