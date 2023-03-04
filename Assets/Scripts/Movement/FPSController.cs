using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.Netcode;
public class FPSController : MonoBehaviour
{
    public static FPSController instance;
    private float blendSpeedFPS;
    Animator fpsAnimator;

    public CinemachineVirtualCamera fpsCamera;
    public GameObject Crosshair;
    bool isFPSAiming;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        fpsAnimator = GetComponent<Animator>();
    }
    
    void Update()
    {
       //fpsAnimator.SetLayerWeight(1, Mathf.Lerp(fpsAnimator.GetLayerWeight(1), 0f, Time.deltaTime * 10f)); ;
        fpsAnimator.SetFloat("Speed", blendSpeedFPS);        
    }
    public void SetMovementSpeed(float blendSpeed) 
    {
        blendSpeedFPS = blendSpeed;
    }
    public void AimFPS(bool state)
    {
        if(state)
        {
            fpsAnimator.SetLayerWeight(1, Mathf.Lerp(fpsAnimator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
            Mathf.Lerp(fpsCamera.m_Lens.FieldOfView = 40, fpsCamera.m_Lens.FieldOfView = 36, Time.deltaTime * 100f);       
            Crosshair.SetActive(false);
        }
        else
        {
            fpsAnimator.SetLayerWeight(1, Mathf.Lerp(fpsAnimator.GetLayerWeight(1), 0f, Time.deltaTime * 10f)); 
            Mathf.Lerp(fpsCamera.m_Lens.FieldOfView = 36, fpsCamera.m_Lens.FieldOfView = 40, Time.deltaTime * 20f);
            Crosshair.SetActive(true);
        }
        isFPSAiming = state;
    }
    
}
