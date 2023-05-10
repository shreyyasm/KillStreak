using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthAmmoPickup : NetworkBehaviour
{
    public float rotSpeed = 60;
    public float maxSize;
    public float growFactor;
    public float waitTime;


    void Start()
    {
        StartCoroutine(Scale());
    }

    void Update()
    {
        transform.Rotate(0, rotSpeed * Time.deltaTime, 0, Space.World);
    }
    private void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("work");
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("work");
            InstanceFinder.ServerManager.Despawn(gameObject, DespawnType.Pool);
            collision.gameObject.GetComponent<PlayerHealth>().RestoreHealth();
            collision.gameObject.GetComponent<PlayerGunSelector>().gun1.AmmoConfig.RefillAmmo();
            collision.gameObject.GetComponent<PlayerGunSelector>().gun2.AmmoConfig.RefillAmmo();
            //CarGun.instance.AddAmmo(50);
            //gameObject.SetActive(false);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        
    }
    
    IEnumerator Scale()
    {
        float timer = 0;

        while (true) // this could also be a condition indicating "alive or dead"
        {
            // we scale all axis, so they will have the same value, 
            // so we can work with a float instead of comparing vectors
            //while (maxSize > transform.localScale.x)
            //{
            //    timer += Time.deltaTime;
            //    transform.localScale += new Vector3(1, 1, 1) * Time.deltaTime * growFactor;
            //    yield return null;
            //}
            //// reset the timer

            yield return new WaitForSeconds(waitTime);

            timer = 0;
            while (0.1f < transform.localScale.x)
            {
                timer += Time.deltaTime;
                transform.localScale -= new Vector3(1, 1, 1) * Time.deltaTime * growFactor;
                yield return null;
            }

            timer = 0;
            //yield return new WaitForSeconds(waitTime);
        }
    }

}
