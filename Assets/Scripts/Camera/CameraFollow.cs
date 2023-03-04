using UnityEngine;
//using Unity.Netcode;
using Cinemachine;
using UnityEngine.Assertions;

public class CameraFollow : MonoBehaviour
{
    private CinemachineVirtualCamera m_MainCamera;
    private CinemachineVirtualCamera m_AimCamera;
    private CinemachineVirtualCamera m_FPSCamera;

    
    //GameObject sphere;

    private void Awake()
    {
        m_MainCamera = GameObject.FindWithTag("Follow Camera").GetComponent<CinemachineVirtualCamera>();
        m_AimCamera = GameObject.FindWithTag("Aim Camera").GetComponent<CinemachineVirtualCamera>();
    }
    void Start()
    {
        AttachCamera();
        
    }
 
    private void AttachCamera()
    {     
        m_MainCamera.enabled = true;    
        //m_FPSCamera = GameObject.FindWithTag("FPS Camera").GetComponent<CinemachineVirtualCamera>();
        //sphere = GameObject.FindGameObjectWithTag("Aim");
        //Assert.IsNotNull(m_MainCamera, "CameraController.AttachCamera: Couldn't find gameplay freelook camera");

        if (m_MainCamera)
        {
            // camera body / aim
            m_MainCamera.Follow = transform;
            m_MainCamera.LookAt = transform;
            m_AimCamera.Follow = transform;
            m_AimCamera.LookAt = transform;
            //m_FPSCamera.LookAt = sphere.transform;
        }

    }


}