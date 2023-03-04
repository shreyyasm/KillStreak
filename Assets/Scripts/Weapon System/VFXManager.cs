using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(destroyVFX());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator destroyVFX()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

}
