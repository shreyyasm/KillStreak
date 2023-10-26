using System;
using UnityEngine;
using System.Collections;
using InfimaGames.LowPolyShooterPack;
using Random = UnityEngine.Random;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine.Pool;
//using Unity.Netcode;
public class Projectile : MonoBehaviour {

	[Range(5, 100)]
	[Tooltip("After how long time should the bullet prefab be destroyed?")]
	public float destroyAfter;
	[Tooltip("If enabled the bullet destroys on impact")]
	public bool destroyOnImpact = false;
	[Tooltip("Minimum time after impact that the bullet is destroyed")]
	public float minDestroyTime;
	[Tooltip("Maximum time after impact that the bullet is destroyed")]
	public float maxDestroyTime;

	[Header("Impact Effect Prefabs")]
	public Transform [] bloodImpactPrefabs;
	public Transform [] metalImpactPrefabs;
	public Transform [] dirtImpactPrefabs;
	public Transform []	concreteImpactPrefabs;
	Rigidbody rb;
	[SerializeField] float projectileImpulse = 400.0f;
	BoxCollider boxCollider;
	TrailRenderer trail;
	[SerializeField] GameObject bulletParticle;

	[HideInInspector]
	public GameObject ObjectVFX;
    private void Awake ()
	{
		//vfxPool = new ObjectPool<Transform>(() =>
		//{
		//	return Instantiate(bloodImpactPrefabs[Random.Range
		//		(0, bloodImpactPrefabs.Length)]);
		//}, vfx =>
		//{
		//	vfx.gameObject.SetActive(true);
		//}, vfx =>
		//{
		//	vfx.gameObject.SetActive(false);
		//}
		//, vfx =>
		//{
		//	Destroy(vfx.gameObject);
		//}, false, 10, 20);
		//Grab the game mode service, we need it to access the player character!
		var gameModeService = ServiceLocator.Current.Get<IGameModeService>();
		//Ignore the main player character's collision. A little hacky, but it should work.
		//Physics.IgnoreCollision(gameModeService.GetPlayerCharacter().GetComponent<Collider>(), GetComponent<Collider>());
		boxCollider = GetComponent<BoxCollider>();
		trail = GetComponent<TrailRenderer>();		
		//ownerConnection = GetComponent<NetworkConnection>();
		//Start destroy timer
		StartCoroutine (DestroyAfter ());
		
	}
    public void Start()
    {
		rb = GetComponent<Rigidbody>();
		rb.velocity = gameObject.transform.forward * projectileImpulse;
	}
    private void Update()
    {
        
    }
    //If the bullet collides with anything
    private void OnCollisionEnter (Collision collision)
	{
		
			boxCollider.enabled = false;
			StartCoroutine(DestroyTrailAfter());			
			bulletParticle.SetActive(false);
		
			
		//Ignore collisions with other projectiles.
		//if (collision.gameObject.GetComponent<Projectile>() != null)
		//	return;

		// //Ignore collision if bullet collides with "Player" tag
		// if (collision.gameObject.CompareTag("Player")) 
		// {
		// 	//Physics.IgnoreCollision (collision.collider);
		// 	Debug.LogWarning("Collides with player");
		// 	//Physics.IgnoreCollision(GetComponent<Collider>(), GetComponent<Collider>());
		//
		// 	//Ignore player character collision, otherwise this moves it, which is quite odd, and other weird stuff happens!
		// 	Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
		//
		// 	//Return, otherwise we will destroy with this hit, which we don't want!
		// 	return;
		// }
		//
		//If destroy on impact is false, start 
		//coroutine with random destroy timer
		if (!destroyOnImpact)
		{
			StartCoroutine(DestroyTimer());
		}
		//Otherwise, destroy bullet on impact
		else
		{
			DespawnBullet();
		}

		//If bullet collides with "Blood" tag
		if (collision.transform.tag == "Wall")
		{
			Transform VFX = Instantiate(bloodImpactPrefabs[Random.Range
				(0, bloodImpactPrefabs.Length)], transform.position,
				Quaternion.LookRotation(collision.contacts[0].normal));
			
			DespawnBullet();
		}
		//If bullet collides with "Blood" tag
		if (collision.transform.tag == "Player")
		{
			Transform VFX = Instantiate(bloodImpactPrefabs[Random.Range
				(0, bloodImpactPrefabs.Length)], transform.position,
				Quaternion.LookRotation(collision.contacts[0].normal));
			
			DespawnBullet();

		}

		//If bullet collides with "Metal" tag
		if (collision.transform.tag == "Metal")
		{
			//Instantiate random impact prefab from array
			Instantiate(metalImpactPrefabs[Random.Range
				(0, bloodImpactPrefabs.Length)], transform.position,
				Quaternion.LookRotation(collision.contacts[0].normal));

			DespawnBullet();

		}

		//If bullet collides with "Dirt" tag
		if (collision.transform.tag == "Dirt")
		{
			//Instantiate random impact prefab from array
			Instantiate(dirtImpactPrefabs[Random.Range
				(0, bloodImpactPrefabs.Length)], transform.position,
				Quaternion.LookRotation(collision.contacts[0].normal));

			DespawnBullet();

		}

		//If bullet collides with "Concrete" tag
		if (collision.transform.tag == "Concrete")
		{
			Transform VFX = Instantiate(bloodImpactPrefabs[Random.Range
				(0, bloodImpactPrefabs.Length)], transform.position,
				Quaternion.LookRotation(collision.contacts[0].normal));
			
			DespawnBullet();

		}

		//If bullet collides with "Target" tag
		if (collision.transform.tag == "Target")
		{
			//Toggle "isHit" on target object
			//collision.transform.gameObject.GetComponent
			//	<TargetScript>().isHit = true;
			//Instantiate random impact prefab from array
			Transform VFX = Instantiate(bloodImpactPrefabs[Random.Range
				(0, bloodImpactPrefabs.Length)], transform.position,
				Quaternion.LookRotation(collision.contacts[0].normal));
			
			DespawnBullet();
			
		}

		//If bullet collides with "ExplosiveBarrel" tag
		if (collision.transform.tag == "ExplosiveBarrel")
		{
			//Toggle "explode" on explosive barrel object
			collision.transform.gameObject.GetComponent
				<ExplosiveBarrelScript>().explode = true;

			DespawnBullet();

		}

		//If bullet collides with "GasTank" tag
		if (collision.transform.tag == "GasTank")
		{
			//Toggle "isHit" on gas tank object
			collision.transform.gameObject.GetComponent
				<GasTankScript>().isHit = true;

			DespawnBullet();
		}
		
	}
	
	private IEnumerator DestroyTimer () 
	{
		//Wait random time based on min and max values
		yield return new WaitForSeconds
			(Random.Range(minDestroyTime, maxDestroyTime));
		DespawnBullet();

	}

	private IEnumerator DestroyAfter () 
	{
		//Wait for set amount of time
		yield return new WaitForSeconds (destroyAfter);
		DespawnBullet();

	}
	
	public void DespawnBullet()
	{
		//ServerManager.Despawn(gameObject);
		//gameObject.SetActive(false);
	}
	private IEnumerator DestroyTrailAfter()
	{
		//Wait for set amount of time
		yield return new WaitForSeconds(0.002f);
		
		trail.emitting = false;

	}

	
}