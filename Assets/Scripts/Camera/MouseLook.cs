using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
public class MouseLook : NetworkBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    float xRotation = 0f;
    float mouseX, mouseY;
    [SerializeField] ScreenTouch screenTouch;

    // Start is called before the first frame update
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!base.IsOwner)
            return;

        mouseX = screenTouch.lookInput.x;
        mouseY = screenTouch.lookInput.y;
        

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -30f, 60f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);

    }

}