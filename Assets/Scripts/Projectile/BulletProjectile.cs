using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Rigidbody bulletRigidbody;
    [SerializeField] Transform vfxHitTarget;
    [SerializeField] Transform vfxHitNull;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }
    private void Start()
    {
       float speed = 5f;
       bulletRigidbody.velocity = transform.forward* speed;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Wall"))
        {
            //Hit target
            Instantiate(vfxHitTarget, transform.position, Quaternion.LookRotation(Vector3.forward));
        }
        else
        {
            //Hit Something else
            Instantiate(vfxHitNull, transform.position, Quaternion.LookRotation(Vector3.forward));

        }
        Destroy(gameObject);
    }
}
