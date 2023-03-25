using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slide : MonoBehaviour
{
    CharacterController controller;
    public Transform groundCheck;

    public LayerMask groundMask;
    public LayerMask wallMask;

    Vector3 move;
    Vector3 input;
    Vector3 Yvelocity;
    Vector3 forwardDirection;

    [SerializeField] float speed;

    public float runSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float airSpeedMultiplier;
    public float climbSpeed;

    float gravity;
    public float normalGravity;
    public float wallRunGravity;
    public float jumpHeight;

    public float slideSpeedIncrease;
    public float wallRunSpeedIncrease;
    public float slideSpeedDecrease;
    public float wallRunSpeedDecrease;

    int jumpCharges;

    bool isSprinting;
    bool isCrouching;
    bool isSliding;
    bool isWallRunning;
    bool isGrounded;

    float startHeight;
    float crouchHeight = 0.5f;
    float slideTimer;
    public float maxSlideTimer;
    Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    Vector3 crouchingCenterY = new Vector3(0, 0.92f, 0);
    Vector3 standingCenter = new Vector3(0, 0, 0);

    bool onLeftWall;
    bool onRightWall;
    bool hasWallRun = false;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    Vector3 wallNormal;
    Vector3 lastWall;

    bool isClimbing;
    bool hasClimbed;
    bool canClimb;
    private RaycastHit wallHit;

    float climbTimer;
    public float maxClimbTimer;

    bool isWallJumping;
    float wallJumpTimer;
    public float maxWallJumpTimer;

    float normalFov;
    public float specialFov;
    public float cameraChangeTime;
    public float wallRunTilt;
    public float tilt;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        startHeight = transform.localScale.y;
        jumpCharges = 2;
        //normalFov = playerCamera.fieldOfView;
    }

    void IncreaseSpeed(float speedIncrease)
    {
        speed += speedIncrease;
    }

    void DecreaseSpeed(float speedDecrease)
    {
        speed -= speedDecrease * Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();     
        if (isGrounded && !isSliding)
        {
            GroundedMovement();
            controller.center = crouchingCenterY;
        }
    
        if (isSliding)
        {
            SlideMovement();
            DecreaseSpeed(slideSpeedDecrease);
            slideTimer -= 1f * Time.deltaTime;
            if (slideTimer <= 0)
            {
                isSliding = false;
            }
        }
       
        controller.Move(move * Time.deltaTime);
        CameraEffects();
        ApplyGravity();
    }

    void FixedUpdate()
    {
        CheckGround();
    }

    void CameraEffects()
    {
        float fov = isWallRunning ? specialFov : isSliding ? specialFov : normalFov;
       // playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, cameraChangeTime * Time.deltaTime);

        if (isWallRunning)
        {
            if (onRightWall)
            {
                tilt = Mathf.Lerp(tilt, wallRunTilt, cameraChangeTime * Time.deltaTime);
            }
            else if (onLeftWall)
            {
                tilt = Mathf.Lerp(tilt, -wallRunTilt, cameraChangeTime * Time.deltaTime);
            }
        }
        else
        {
            tilt = Mathf.Lerp(tilt, 0f, cameraChangeTime * Time.deltaTime);
        }
    }

    void HandleInput()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        if (!isWallRunning)
        {
            input = transform.TransformDirection(input);
            input = Vector3.ClampMagnitude(input, 1f);
        }


        if (Input.GetKeyDown(KeyCode.C))
        {
            Crouch();
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            ExitCrouch();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && isGrounded)
        {
            isSprinting = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
        }

        if (Input.GetKeyUp(KeyCode.Space) && jumpCharges > 0)
        {
          
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.25f, groundMask);
        if (isGrounded)
        {
            jumpCharges = 1;
            hasWallRun = false;
            hasClimbed = false;
            climbTimer = maxClimbTimer;
        }
    }
 
    void GroundedMovement()
    {
        speed = isSprinting ? sprintSpeed : isCrouching ? crouchSpeed : runSpeed;
        if (input.x != 0)
        {
            move.x += input.x * speed;
        }
        else
        {
            move.x = 0;
        }
        if (input.z != 0)
        {
            move.z += input.z * speed;
        }
        else
        {
            move.z = 0;
        }

        move = Vector3.ClampMagnitude(move, speed);
    }

    
    void SlideMovement()
    {
        move += forwardDirection;
        move = Vector3.ClampMagnitude(move, speed);
    }

   
    void Crouch()
    {
        //controller.height = crouchHeight;
        //controller.center = crouchingCenter;
        //transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
        if (speed > runSpeed)
        {
            isSliding = true;
            forwardDirection = transform.forward;
            if (isGrounded)
            {
                IncreaseSpeed(slideSpeedIncrease);
            }
            slideTimer = maxSlideTimer;
        }
        isCrouching = true;
    }

    void ExitCrouch()
    {
        controller.height = (startHeight * 2);
        controller.center = standingCenter;
        transform.localScale = new Vector3(transform.localScale.x, startHeight, transform.localScale.z);
        isCrouching = false;
        isSliding = false;
    }


    
    void ApplyGravity()
    {
        gravity = isWallRunning ? wallRunGravity : isClimbing ? 0f : normalGravity;
        Yvelocity.y += gravity * Time.deltaTime;
        controller.Move(Yvelocity * Time.deltaTime);
    }

}