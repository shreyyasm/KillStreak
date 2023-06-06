using UnityEngine;
//using Unity.Netcode;
//using Unity.Netcode.Components;
using FishNet.Object;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using System.Collections;
using FishNet.Object.Synchronizing;
using Cinemachine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : NetworkBehaviour
    {

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        public GameObject fPSController;
        //public FixedTouchField fixedTouchField;
        public ScreenTouch screenTouch;
        [SerializeField] private Rig MainRig;
        [SerializeField] private Rig pistolRig;
        [SerializeField] private Rig rifleRig;
        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        public float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDIdleJump;
        private int _animIDWalkJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;


#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        public Animator _animator;
        //NetworkAnimator _networkAnimator;
        public CharacterController _controller;
        public StarterAssetsInputs _input;
        private GameObject _mainCamera;
        public PlayerInput playerInput;
        public InputActionAsset myActionAsset;
        public PlayerGunSelector playerGunSelector;
        public PlayerHealth playerHealth;
        [SerializeField] GameObject cameraRoot;
        public UltimateJoystick ultimateJoystick;
        bool isAiming = false;
        bool isAimWalking = false;
        bool inFPSMode = false;
        public bool firedBullet = false;
        public bool firing = false;
        public bool isCrouching;
        public bool isSliding;
        public bool running;
        public bool playerDead;
        public bool isPressedJump = false;
        public bool isJump = false;
        
        public int gunType;
        
        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public bool changingGun { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public bool isReloading { get; [ServerRpc(RequireOwnership = false,RunLocally = true)] set; }

        float fireBulletTime = 0f;

        public float sensitivity;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;
        public Vector3 direction;
        float mouseX , mouseY;
        [SerializeField] ShooterController shooterController;
        [SerializeField] WeaponSwitching weaponSwitching;
        [SerializeField] float smoothSpeed = 80f;
        [SerializeField] float crouchHeight = 0.5f;
        Vector3 crouchingCenter = new Vector3(0, 0.67f, 0);
        Vector3 crouchingCenterY = new Vector3(0, 0.92f, 0);

        private CinemachineVirtualCamera m_MainCamera;
        private CinemachineVirtualCamera m_AimCamera;
        [SerializeField] Transform[] Root;
        public GameObject UICanvas;
        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            /* If you wish to check for ownership inside
            * this method do not use base.IsOwner, use
            * the code below instead. This difference exist
            * to support a clientHost condition. */
            if (base.Owner.IsLocalClient)
            {
                cameraRoot.AddComponent<CameraFollow>();
                foreach (Transform gears in Root)
                {
                    var childGameObjects = gears.GetComponentsInChildren<Transform>();
                    foreach (Transform allObjects in childGameObjects)
                    {
                        allObjects.gameObject.layer = LayerMask.NameToLayer("Player Root");
                    }
                }


            }
            else
            {

                // UICanvas.SetActive(false);
                //gameObject.AddComponent<Enemy>();
                // gameObject.AddComponent<EnemyHealth>(); 
            }
            
            
        }

        //slide value
        public float speed = 8f;
        public float gravity = -9.81f;
        public float value;
        Vector3 velocity;
        bool isGrounded;
 
        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;


        public float slideTimeRemaining = 10;
        bool timerIsRunning;
        public float slideSpeed = 7f;

        public bool extraJump;
       
        public bool lookAtPlayer = true;
        public Vector2 look;
        public float x, z;
        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }
        public void OnLook(InputValue value)
        {
            
                LookInput(value.Get<Vector2>());
            
        }

        private void Awake()
        {

            //myActionAsset.bindingMask = new InputBinding { groups = "KeyboardMouse" };
            //playerInput.SwitchCurrentControlScheme(Keyboard.current, Mouse.current);
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            m_MainCamera = GameObject.FindWithTag("Follow Camera").GetComponent<CinemachineVirtualCamera>();
            m_AimCamera = GameObject.FindWithTag("Aim Camera").GetComponent<CinemachineVirtualCamera>();
            _controller = GetComponent<CharacterController>();
            //rb = GetComponent<Rigidbody>();
            //Root = GetComponentInChildren<Transform>();
            //Root.layer = LayerMask.NameToLayer("Player Root");
            
            isCrouching = false;
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _hasAnimator = TryGetComponent(out _animator);
            AssignAnimationIDs();
            SetRigWeight();
            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;           
        }

        private void Update()
        {
            if (!base.IsOwner)
                return;

            Vector3 screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);
            ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            GroundedCheck();
            MoveOld();
            //Move();
            z = ultimateJoystick.GetVerticalAxis();
            //if(base.IsServer)
            changingGun = weaponSwitching.GunSwaping();
            //Jump();
            if (firedBullet && fireBulletTime >= 0)
            {
                if (!firing)
                    fireBulletTime -= Time.deltaTime;
                if (fireBulletTime <= 0)
                {
                    firedBullet = false;
                }
            }

            playerDead = playerHealth.PlayerDeathState();
            
            playerGunSelector.SetLookInput(look.x, look.y, x, z);
            //playerGunSelector.SetLookInput(mouseX, mouseY,x,z);
           
            SetRigWeight();
            JumpAndGravity();

            if(_animationBlend > 1)
                Slide();
            if (Input.GetMouseButtonDown(2))
                Crouch();

            ControllerChanges();
        }
        

        private void LateUpdate()
        {
            if (!base.IsOwner)
                return;
            CameraRotationOld();
            //CameraRotation();
            
        }
        public void SetRigWeight()
        {
            if(base.IsServer)
                SetRigObserver();

            if(base.IsOwner)
                SetRigServer(); 
        }
        
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDIdleJump = Animator.StringToHash("Idle Jump");
            _animIDWalkJump = Animator.StringToHash("Walk Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }
        public void CheckJump()
        {
            isPressedJump = true;
        }
        public LayerMask IdentifyEnemy;
        private Ray ray;

        public void CameraRotation()
        {
            if (playerHealth.PlayerDeathState())
                return;
            mouseX = screenTouch.lookInput.x;
            mouseY = screenTouch.lookInput.y;
            if (screenTouch.rightFingerID == -1)
            {
                mouseX = 0;
                mouseY = 0;
            }
            if (screenTouch.rightFingerID != -1)
            {
                //float h = UltimateTouchpad.GetHorizontalAxis("Look");
                //float v = UltimateTouchpad.GetVerticalAxis("Look");
                //Vector3 direction = new Vector3(h, v, 0f).normalized;
                //Debug.Log(direction.x);
                // if there is an input and camera position is not fixed
                if (screenTouch.lookInput.sqrMagnitude >= _threshold && !LockCameraPosition)
                {
                    //Don't multiply mouse input by Time.deltaTime;
                    //float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                    //_cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * sensitivity;
                    //_cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * sensitivity;
                    _cinemachineTargetYaw += mouseX * Time.deltaTime * 100;
                    _cinemachineTargetPitch -= mouseY * Time.deltaTime * 100;
                }

                // clamp our rotations so our values are limited 360 degrees
                _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
                if (Physics.SphereCast(ray, 0.2f, out RaycastHit hitnew, float.MaxValue, IdentifyEnemy))
                {
                    Vector3 hitpoint = hitnew.collider.ClosestPointOnBounds(hitnew.point);

                    //
                    //CinemachineCameraTarget.transform.rotation 
                    //Vector3 targetDirection = hitpoint - CinemachineCameraTarget.transform.localPosition;
                    //Vector3 lookDirection = Vector3.RotateTowards(CinemachineCameraTarget.transform.forward, targetDirection, 1f, 0.0f);
                    //CinemachineCameraTarget.transform.localRotation = Quaternion.LookRotation(lookDirection);
                    sensitivity = 70f;
                    screenTouch.SetSensitivity(6);
                    //CinemachineCameraTarget.transform.rotation = Quaternion.Euler(lookDirection.x, lookDirection.z, 0.0f);
                    //CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                    // _cinemachineTargetYaw, 0.0f);
                    //Debug.Log("work");
                    // CinemachineCameraTarget.transform.rotation = Quaternion.Euler(hitpoint.x , targetDirection.y, 0.0f);
                    Debug.DrawLine(_mainCamera.transform.position, hitpoint, Color.red);
                    Vector3 enemyPoint = hitpoint;
                    lookAtPlayer = true;
                    //Vector3 aimAssistPoint = ray.GetPoint(Vector3.Distance(enemyPoint, CinemachineCameraTarget.transform.position));
                    //if (Vector3.Distance(aimAssistPoint, enemyPoint) <= 1.5f)
                    //{
                    //StartCoroutine(ToggleMouseLook(0.5f));
                    //CinemachineCameraTarget.transform.LookAt(hitpoint);

                    }

            }
                else
                {
                    if (!isAiming)
                    {
                        screenTouch.SetSensitivity(8);
                        sensitivity = 100f;
                    }

                    //Cinemachine will follow this target

                }
                //if(lookAtPlayer)
                //{
                
                //}
           // }

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                    _cinemachineTargetYaw, 0.0f);
        }

        private void CameraRotationOld()
        {
            if (playerHealth.PlayerDeathState())
                return;
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = 1.0f;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            if (Physics.SphereCast(ray, 0.2f, out RaycastHit hitnew, float.MaxValue, IdentifyEnemy))
            {
                Vector3 hitpoint = hitnew.collider.ClosestPointOnBounds(hitnew.point);

                //
                //CinemachineCameraTarget.transform.rotation 
                 //Vector3 targetDirection = hitpoint - CinemachineCameraTarget.transform.localPosition;
                 //Vector3 lookDirection = Vector3.RotateTowards(CinemachineCameraTarget.transform.forward, targetDirection, 1f, 0.0f);
                 //CinemachineCameraTarget.transform.localRotation = Quaternion.LookRotation(lookDirection);
                sensitivity = 70f;
                screenTouch.SetSensitivity(6);
                //CinemachineCameraTarget.transform.rotation = Quaternion.Euler(lookDirection.x, lookDirection.z, 0.0f);
                //CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                // _cinemachineTargetYaw, 0.0f);
                //Debug.Log("work");
                // CinemachineCameraTarget.transform.rotation = Quaternion.Euler(hitpoint.x , targetDirection.y, 0.0f);
                Debug.DrawLine(_mainCamera.transform.position, hitpoint, Color.red);
                Vector3 enemyPoint = hitpoint ;
                lookAtPlayer = true;
                //Vector3 aimAssistPoint = ray.GetPoint(Vector3.Distance(enemyPoint, CinemachineCameraTarget.transform.position));
                //if (Vector3.Distance(aimAssistPoint, enemyPoint) <= 1.5f)
                //{
                    //StartCoroutine(ToggleMouseLook(0.5f));
                //CinemachineCameraTarget.transform.LookAt(hitpoint);

                //}
                   
            }
            else
            {
                if (!isAiming)
                {
                    screenTouch.SetSensitivity(8);
                    sensitivity = 100f;
                }
                    
                //Cinemachine will follow this target

            }
            //if(lookAtPlayer)
            //{
                CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                    _cinemachineTargetYaw, 0.0f);
            //}
            

        }
        private void MoveOld()
        {
            _animator.SetFloat("MoveX", _input.move.x);
            _animator.SetFloat("MoveZ", _input.move.y);
            Vector2 directionNew = new Vector2(_input.move.x, _input.move.y);
            if (directionNew.y > 0.2f && !isAiming && !firedBullet && !changingGun && !isCrouching && !isSliding)
                MoveSpeed = 7f;

            else
                MoveSpeed = 5;

            if (weaponSwitching.selectedWeapon == 0)
            {
                if (!isAiming)
                {
                    if (!firedBullet)
                    {

                        _animator.SetLayerWeight(1, 0);
                        _animator.SetLayerWeight(3, 0);
                    }

                }
                else
                {

                    _animator.SetLayerWeight(1, 1);
                    _animator.SetLayerWeight(3, 0);
                }

            }
            else
            {
                if (!isAiming)
                {
                    if (!firedBullet)
                    {

                        _animator.SetLayerWeight(1, 0);
                        _animator.SetLayerWeight(3, 0);
                    }

                }
                else
                {

                    _animator.SetLayerWeight(3, 1);
                    _animator.SetLayerWeight(1, 0);

                }

            }


            
          

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero && !isSliding)
            {
                targetSpeed = 0.0f;

            }
            //targetSpeed = MoveSpeed;
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
               // transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            if (!isSliding)
            {
                // move the player
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }
           

            // update animator if using character
            if (_hasAnimator)
            {
                //Animations State
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetBool("Aim Walk", isAimWalking);
                if (targetSpeed > 5f && !isAiming)
                {
                    running = true;
                    if (!isSliding)
                    {
                        pistolRig.weight = 0f;
                        rifleRig.weight = 0f;
                    }


                }
                else
                {
                    running = false;

                    if (weaponSwitching.selectedWeapon == 0)
                        rifleRig.weight = 1f;

                    else
                        pistolRig.weight = 1f;
                }

                //fPSController.GetComponent<FPSController>().SetMovementSpeed(_animationBlend);            
            }
            //Aim and Walking
            if (isAiming && _animationBlend > 1)
            {
                isAimWalking = true;
            }
            else
            {
                isAimWalking = false;
            }
        }
        public void Move()
        {
            //if (playerHealth.PlayerDeathState())
            //    return;
            //_controller.detectCollisions = false;
            x = ultimateJoystick.GetHorizontalAxis();
            z = ultimateJoystick.GetVerticalAxis();
            direction = new Vector3(x, 0f, z).normalized;

            float neutralize = 1f;
     
            _animator.SetFloat("MoveX", x);
            _animator.SetFloat("MoveZ", z);

        
            if (direction.z > 0.2f && !isAiming && !firedBullet && !changingGun && !isCrouching && !isSliding)
                MoveSpeed = 7f;               

            else
                MoveSpeed = 5;

            if (weaponSwitching.selectedWeapon == 0)
            {
                if (!isAiming)
                {
                    if(!firedBullet)
                    {
                       
                        _animator.SetLayerWeight(1, 0);
                        _animator.SetLayerWeight(3, 0);
                    }
                    
                }
                else
                {
                   
                    _animator.SetLayerWeight(1, 1);
                    _animator.SetLayerWeight(3, 0);
                }
                    
            }
            else
            {
                if (!isAiming)
                {
                    if (!firedBullet)
                    {
                       
                        _animator.SetLayerWeight(1, 0);
                        _animator.SetLayerWeight(3, 0);
                    }
                       
                }
                else
                {
                   
                    _animator.SetLayerWeight(3, 1);
                    _animator.SetLayerWeight(1, 0);
                    
                }

            }


            float targetSpeed = MoveSpeed;
    
            if(direction == Vector3.zero && !isSliding)
            {
                targetSpeed = 0.0f;               
                neutralize = 0f;
            }
                
            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                //_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                //    Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * 1,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
               // _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (direction != Vector3.zero)
            {
                //_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                //                  _mainCamera.transform.eulerAngles.y;
                _targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                //transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            if(!isSliding)
            {
               // _controller.enabled = true;
                // move the player
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) * neutralize +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }
            //else
            //{
            //    // move the player
            //    // _controller.enabled = false;
            //    // _controller.detectCollisions = true;
            //    _controller.Move(targetDirection.normalized * 0);
            //}


            

            // update animator if using character
            if (_hasAnimator)
            {
                //Animations State
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetBool("Aim Walk", isAimWalking);
                if(targetSpeed > 5f && !isAiming)
                {
                    running = true;
                    if(!isSliding)
                    {
                        pistolRig.weight = 0f;
                        rifleRig.weight = 0f;
                    }
                    
                   
                }
                else
                {
                    running = false;

                    if(weaponSwitching.selectedWeapon == 0)
                        rifleRig.weight = 1f;

                    else
                        pistolRig.weight = 1f;
                }

                //fPSController.GetComponent<FPSController>().SetMovementSpeed(_animationBlend);            
            }
           
            //Aim and Walking
            if (isAiming && _animationBlend > 1)
            {
                isAimWalking = true;                          
            }
            else
            {
                isAimWalking = false;
            }                    
        }
        
        public void CrouchInput()
        {
            if(Grounded)
            {
                if (!isCrouching)
                    isCrouching = true;
                else
                    isCrouching = false;
    
            }
            _animator.SetBool("Crouch", isCrouching);
        }
        
        public void ControllerChanges()
        {
            //if (playerHealth.PlayerDeathState())
            //    return;
            if (Grounded)
            {
                float yVelocity = 0f;

                if (!isCrouching && !isSliding)
                {

                    float oldPos;
                    if (!weaponSwitching.gunChanging)
                    {
                        if (!isSliding)
                        {
                            oldPos = Mathf.SmoothDamp(1.375f, 1f, ref yVelocity, Time.deltaTime * 30f);
                        }
                        else
                        {
                            oldPos = Mathf.SmoothDamp(1.375f, 0.8f, ref yVelocity, Time.deltaTime * 30f);
                        }

                        CinemachineCameraTarget.transform.localPosition = new Vector3(0, oldPos, 0);
                        _controller.height = 1.85f;
                        _controller.center = new Vector3(0, 0.92f, 0);
                    }
                }
                else
                {

                    if (!weaponSwitching.gunChanging)
                    {
                        float newPos;
                        if (!isSliding)
                        {
                            newPos = Mathf.SmoothDamp(1f, 1.375f, ref yVelocity, Time.deltaTime * 30f);
                        }
                        else
                        {
                            newPos = Mathf.SmoothDamp(0.8f, 1.375f, ref yVelocity, Time.deltaTime * 30f);
                        }
                        //float newPos = Mathf.SmoothDamp(1f, 1.375f, ref yVelocity, Time.deltaTime * 30f);
                        CinemachineCameraTarget.transform.localPosition = new Vector3(0, newPos, 0);
                        _controller.height = crouchHeight;
                        _controller.center = crouchingCenter;
                    }
                }
            }
           
            _controller.height = Mathf.Lerp(_controller.height, _controller.height, Time.deltaTime * 10f);
        }
        public void Crouch()
        {
            //if (playerHealth.PlayerDeathState())
            //    return;
            if (z > 0.6f && !isAiming && !isCrouching)
                StartSlide();

            //ForPcControls
            if (_input.move.y > 0.6f && !isAiming && !isCrouching)
                StartSlide();
            
            else
                CrouchInput();
            
        }
       
        public void Slide()
        {
            
            
            if (!timerIsRunning)
            {
            
                if (Grounded)
                {
                    timerIsRunning = false;
                }
            }
     
            // This is a slidekick timer
            if (timerIsRunning)
            {
                //extraJump = true;
                //value = slideTimeRemaining;
                if (value > 0)
                {
                    Debug.Log(direction.z);
                    
                    value -= 1 * Time.deltaTime;
                    slideSpeed -= 3 * Time.deltaTime;
                    
                    _controller.Move(transform.forward * slideSpeed * Time.deltaTime +
                                 new Vector3(0.0f, _verticalVelocity * 1.5f, 0.0f) * Time.deltaTime);
                   

                }

                else
                {
                    value = 0;
                    slideTimeRemaining = 0;
                    //isCrouching = false;
                    isSliding = false;
                    _animator.SetBool("Slide", isSliding);
                    ControllerChanges();
                    timerIsRunning = false;
                    
                    extraJump = false;
                    value = 1.8f;
                    slideSpeed = 10;
                }
            }
            
        }
        public void StartSlide()
        {
           
            StartCoroutine(slide());
        }
        IEnumerator slide()
        {
            if(Grounded)
            {
                //firedBullet = false;
                timerIsRunning = true;
                isSliding = true;


                _animator.SetBool("Slide", isSliding);
                slideTimeRemaining = 0.5f;
            }
            
            // _controller.height = reducedHeight;
            yield return new WaitForSeconds(0.5f);
            // _controller.height = originalHeight;
        }
           
        public void JumpAndGravity()
        {
            if(_input.jump && isCrouching)
            {
                isCrouching = false;
                _animator.SetBool("Crouch", isCrouching);
            }
            else if(Grounded && !isCrouching)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _animator.SetBool(_animIDIdleJump, false);
                    _animator.SetBool(_animIDWalkJump, false);
                    isPressedJump = false;
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f )
                {
                    // set sphere position, with offset
                    Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                        transform.position.z);
                    Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                        QueryTriggerInteraction.Ignore);
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character

                    if (_hasAnimator)
                    {
                        if (_animationBlend > 1)
                        {
                            _animator.SetBool(_animIDWalkJump, true);
                        }
                        else
                            _animator.SetBool(_animIDIdleJump, true);
                    }

                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        // _animator.SetBool(_animIDFreeFall, true);
                    }
                }
                // set sphere position, with offset
                Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                    transform.position.z);
                Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                    QueryTriggerInteraction.Ignore);
                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }
       
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        //private void OnDrawGizmosSelected()
        //{
        //    Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        //    Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        //    if (Grounded) Gizmos.color = transparentGreen;
        //    else Gizmos.color = transparentRed;

        //    // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        //    Gizmos.DrawSphere(
        //        new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
        //        GroundedRadius);
        //}

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
        public void SetSensitivity(float newSensitivity)
        {
            sensitivity = newSensitivity;
        }  
        public void Aiming(bool state)
        {
            isAiming = state;
        }
        public void FPSMode(bool state)
        {
            inFPSMode = state;
        }
       
        public void ShotFired(bool state)
        {
            fireBulletTime = 1.3f;
            //if(!isSliding)
                firedBullet = state;
            if (weaponSwitching.selectedWeapon == 0)
            {               
                _animator.SetLayerWeight(1, 1);
                _animator.SetLayerWeight(3, 0);
            }
            else
            {
               
                _animator.SetLayerWeight(3, 1); 
                _animator.SetLayerWeight(1, 0);
            }
                
        }
        public void FiringContinous(bool state)
        {
            firing = state;

            if (_animationBlend <= 0)
                _animator.SetBool("Rifle Idle Firing", state);
        }
        public void GunSwapingGunChangeIn()
       {
            if(base.IsServer)           
                weaponSwitching.GunSwapVisualTakeInObserver();

            else
                weaponSwitching.GunSwapVisualTakeInServer();

        }
        public void GunSwapingGunChangeOut()
        {
            if(base.IsServer)
                weaponSwitching.GunSwapVisualTakeOutObserver();

            
            else
                weaponSwitching.GunSwapVisualTakeOutServer();

        }
        public void ReloadCheck(bool state)
        {
            isReloading = state;
        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void SetRigServer()
        {
           
            if (weaponSwitching.selectedWeapon == 0)
            {
                //MainRig.weight = 1f;
                if (!running)
                {
                    if (!changingGun)
                    {
                        if (!isReloading)
                        {
                            rifleRig.weight = 1f;
                            pistolRig.weight = 0f;
                        }
                        else
                        {
                            pistolRig.weight = 0f;
                            rifleRig.weight = 0f;
                        }
                    }
                    else
                    {
                        pistolRig.weight = 0f;
                        rifleRig.weight = 0f;
                    }
                }
            }

            else
            {
                //MainRig.weight = 1f;
                if (!running)
                {
                    if (!changingGun)
                    {
                        if (!isReloading)
                        {
                            pistolRig.weight = 1f;
                            rifleRig.weight = 0f;
                        }
                        else
                        {
                            pistolRig.weight = 0f;
                            rifleRig.weight = 0f;
                        }
                    }
                    else
                    {
                        pistolRig.weight = 0f;
                        rifleRig.weight = 0f;
                    }
                }
            }
            if (weaponSwitching.selectedWeapon == 0)
            {
                _animator.SetLayerWeight(0, 1);
                _animator.SetLayerWeight(2, 0);
            }
            else
            {
                _animator.SetLayerWeight(2, 1);
                _animator.SetLayerWeight(0, 0);
            }
        }
        [ObserversRpc(BufferLast = true)]
        public void SetRigObserver()
        {
            
            if (weaponSwitching.selectedWeapon == 0)
            {
               // MainRig.weight = 1f;
                if (!running)
                {
                    if (!changingGun)
                    {
                        if (!isReloading)
                        {
                            rifleRig.weight = 1f;
                            pistolRig.weight = 0f;
                        }
                        else
                        {
                            pistolRig.weight = 0f;
                            rifleRig.weight = 0f;
                        }
                    }
                    else
                    {
                        pistolRig.weight = 0f;
                        rifleRig.weight = 0f;
                    }
                }
            }

            else
            {
                //MainRig.weight = 1f;
                if (!running)
                {
                    if (!changingGun)
                    {
                        if (!isReloading)
                        {
                            pistolRig.weight = 1f;
                            rifleRig.weight = 0f;
                        }
                        else
                        {
                            pistolRig.weight = 0f;
                            rifleRig.weight = 0f;
                        }
                    }
                    else
                    {
                        pistolRig.weight = 0f;
                        rifleRig.weight = 0f;
                    }
                }
            }
            if (weaponSwitching.selectedWeapon == 0)
            {
                _animator.SetLayerWeight(0, 1);
                _animator.SetLayerWeight(2, 0);
            }
            else
            {
                _animator.SetLayerWeight(2, 1);
                _animator.SetLayerWeight(0, 0);
            }
        }
    }
}
