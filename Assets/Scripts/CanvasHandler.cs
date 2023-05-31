using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasHandler : NetworkBehaviour
{
    
    public GameObject PlayerCheck;
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        /* If you wish to check for ownership inside
        * this method do not use base.IsOwner, use
        * the code below instead. This difference exist
        * to support a clientHost condition. */
        if (!base.Owner.IsLocalClient)
        {
           // Destroy(gameObject);
        }

    }

    private void Start()
    {
        //if (instance != null)
        //    Destroy(gameObject);
        if(!PlayerCheck.GetComponent<CameraFollow>())
        {
            Destroy(gameObject);
        }


    }
    private void Update()
    {
        //if (!PlayerCheck.GetComponent<CameraFollow>())
        //{
        //    Destroy(gameObject);
        //}
        //if(!base.Owner.IsLocalClient)
        //{
        //    Destroy(gameObject);
        //}
    }
}
