using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
//using Unity.Netcode;
using FishNet.Object;
using UnityEngine.Animations.Rigging;
public class CameraSwitch : NetworkBehaviour
{

    [SerializeField] CinemachineVirtualCamera fpsCamera;
    [SerializeField] GameObject FPSplayer;
    [SerializeField] Transform[] playerGears;

    private CinemachineVirtualCamera m_FollowCamera;
    private CinemachineVirtualCamera m_AimCamera;

    public bool cameraSwitched;
    bool inFPSMode = false;

    Camera MainCamera;
    public ThirdPersonController thirdPersonController;
    public ShooterController shooterController;


    private void Awake()
    {
        m_FollowCamera = GameObject.FindWithTag("Follow Camera").GetComponent<CinemachineVirtualCamera>();
        m_AimCamera = GameObject.FindWithTag("Aim Camera").GetComponent<CinemachineVirtualCamera>();
        MainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();       
        cameraSwitched = true;
        
    }
    void Update()
    {  
        if (!base.IsOwner)
            return;

        //if (!inFPSMode)
        //    fpsCamera.transform.rotation = MainCamera.transform.rotation;

        //foreach (Transform gears in playerGears)
        //{
        //    var childGameObjects = gears.GetComponentsInChildren<Transform>();
        //    foreach (Transform allObjects in childGameObjects)
        //    {
        //        allObjects.gameObject.layer = LayerMask.NameToLayer("HideItself");
        //    }          
        //}
              
    }
    public void ChangeMode()
    {
        if (cameraSwitched)
        {
            //SwitchMode
            inFPSMode = true;
            shooterController.FPSModeCheck(cameraSwitched);
            thirdPersonController.FPSMode(inFPSMode);
            //MainCamera.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "Ignore Raycast", "PostProcessing", "Water", "UI", "Player", "Ground and Walls", "PhysicalAmmo", "FirstPersonWeapon", "Projectile", "OtherPlayers", "Buildings");
            //fpsCamera.enabled = true;
            FPSplayer.SetActive(true);
            //m_FollowCamera.enabled = false;
            //m_AimCamera.enabled = false;

            cameraSwitched = false;
        }
        else
        {
            //SwitchMode
            inFPSMode = false;
            thirdPersonController.FPSMode(inFPSMode);
            shooterController.FPSModeCheck(cameraSwitched);

           // MainCamera.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "Ignore Raycast", "HideItself", "PostProcessing", "Water", "UI", "Player", "Ground and Walls", "PhysicalAmmo", "FirstPersonWeapon", "Projectile", "OtherPlayers", "Buildings");

            //fpsCamera.enabled = false;
            FPSplayer.SetActive(false);
            //m_FollowCamera.enabled = true;
            //m_AimCamera.enabled = true;

            cameraSwitched = true;
        }
    }

}
