using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLookAt : MonoBehaviour
{
    [SerializeField] LayerMask aimcolliderLayerMask;
    public GameObject sphere;
    private void Update()
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimcolliderLayerMask))
        {
            sphere = GameObject.FindGameObjectWithTag("Aim");
            sphere.transform.position = raycastHit.point;
            //sphere.transform.position = Vector3.Lerp(sphere.transform.position, raycastHit.point, Time.deltaTime * 20f);
           // gameObject.transform.LookAt(sphere.transform.position);

        }
    }
   
}
