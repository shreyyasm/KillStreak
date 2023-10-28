using UnityEngine;
//using Unity.Netcode;
//using Unity.Netcode.Components;
using FishNet.Object;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using System.Collections;
using FishNet.Object.Synchronizing;
using Cinemachine;
using FishNet;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using StarterAssets;
using System.Collections.Generic;
using EOSLobbyTest;

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
        
        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public string PlayerName { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

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

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public bool Grounded { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

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
        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public float _cinemachineTargetYaw { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public float _cinemachineTargetPitch { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }


        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public float _cinemachineTargetX { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

        // player
        private float _speed;

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public float _animationBlend { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

        public float _targetRotation = 0.0f;
        public Vector3 movement;
        public Vector3 targetDirection;
        private float _rotationVelocity;
        public float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        public float _jumpTimeoutDelta;
        public float _fallTimeoutDelta;

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

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public bool firedBullet { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public bool firing { get;[ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public bool isCrouching { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]

        public bool isSliding { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public bool running { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

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

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        float mouseX { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        float  mouseY { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

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
        public AudioListener audioListener;

        public float value;
        public float slideSpeed = 7f;

        public float zValue;
        public float lookSensitivity = 100;
        public AudioSource audioSource;
        public GameObject RespawnPrefab;
        //MoveData for client simulation
        private MoveData _clientMoveData;
        public float _CameraEulerY;
        public LoadOutManager loadOutManager;
        public GameObject root;
        public GameObject loadOutButton;
        //MoveData for replication
        public struct MoveData : IReplicateData
        {
                     
            public Vector3 Move;
            public Vector3 Look;
            public Vector3 ResetPosRed;
            public Vector3 ResetPosBlue;
            public bool Jump;
            public bool Slide;
            public float CameraEulerY;
            public bool Sprint;

            
            private uint _tick;
            public void Dispose() { }
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }

        //ReconcileData for Reconciliation
        public struct ReconcileData : IReconcileData
        {
            public Vector3 Position;
            public Quaternion Rotation;

            public float VerticalVelocity;
            public float FallTimeout;
            public float JumpTimeout;
            public float valueSlide;
            public float slideSpeed;
            public bool Grounded;
            public bool timeIsRunning;

            public float _cinemachineTargetX;
            public float _targetRotation;
            public Vector3 movement;
            public Vector3 targetDirection;

            public float CameraEulerY;
            public Vector3 screenCenterPoint;
            public float lookSensitivity;


            
            public ReconcileData(Vector3 position, Quaternion rotation,float verticalVelocity, float fallTimeout, float jumpTimeout,float slideValue, float slideSpeedValue, bool grounded, bool timeRunning, float _cinemachineTargetXNew, float targetRotation, Vector3 _movement, Vector3 _targetDirection, float cameraEuler, Vector3 _screenCenterPoint, float lookSens)
            {
                Position = position;
                Rotation = rotation;
                VerticalVelocity = verticalVelocity;
                FallTimeout = fallTimeout;
                JumpTimeout = jumpTimeout;
                valueSlide = slideValue;
                slideSpeed = slideSpeedValue;
                
                Grounded = grounded;
                timeIsRunning = timeRunning;

                _cinemachineTargetX = _cinemachineTargetXNew;
                

                movement = _movement;
                targetDirection = _targetDirection;


                _targetRotation = targetRotation;
                CameraEulerY = cameraEuler;
                screenCenterPoint = _screenCenterPoint;
                lookSensitivity = lookSens;

            _tick = 0;
            }
            private uint _tick;
            public void Dispose() { }
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }
       
        private void Awake()
        {
            //SetJoystickPos();
            //SpawnCountDown();
            //PointSystem.Instance.GameStartCountdown();
            //myActionAsset.bindingMask = new InputBinding { groups = "KeyboardMouse" };
            //playerInput.SwitchCurrentControlScheme(Keyboard.current, Mouse.current);
            // get a reference to our main camera

            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            Grounded = true;
            audioRunSource.Play(0);
            audioCrouchSource.Play(0);
            // _controller = GetComponent<CharacterController>();
            audioRunSource.Pause();
            audioCrouchSource.Pause();
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
           
            m_MainCamera = GameObject.FindWithTag("Follow Camera").GetComponent<CinemachineVirtualCamera>();
            m_AimCamera = GameObject.FindWithTag("Aim Camera").GetComponent<CinemachineVirtualCamera>();
            playerManager = FindObjectOfType<PlayerManager>();
            _controller = GetComponent<CharacterController>();

            //rb = GetComponent<Rigidbody>();
            //Root = GetComponentInChildren<Transform>();
            //Root.layer = LayerMask.NameToLayer("Player Root");
            
            isCrouching = false;
        }
        private void OnEnable()
        {
            ResetPosition = false;
        }
        private void OnDestroy()
        {
            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;               
            }
        }
        
  
        public void SpawnCountDown()
        {
            
            if(base.IsClientOnly)
            {
                //Debug.Log("work");
                StartCountDownServer();
                StartCountDownObserver();
            }
               
           
            //GameObject countdown = Instantiate(pointSystem, transform.position, Quaternion.identity);
            //InstanceFinder.ServerManager.Spawn(countdown);
            
        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void StartCountDownServer()
        {
            PointSystem.Instance.GameStartCountdown();
        }
        [ObserversRpc(BufferLast = true, RunLocally = true)]
        public void StartCountDownObserver()
        {
            PointSystem.Instance.GameStartCountdown();
        }

        public PlayerManager playerManager;
        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            /* If you wish to check for ownership inside
            * this method do not use base.IsOwner, use
            * the code below instead. This difference exist
            * to support a clientHost condition. */
            SpawnCountDown();

            if (base.Owner.IsLocalClient)
            {
                //RespawnManager();
                cameraRoot.AddComponent<CameraFollow>();
               // gameObject.AddComponent<AudioListener>();
                foreach (Transform gears in Root)
                {
                    var childGameObjects = gears.GetComponentsInChildren<Transform>();
                    foreach (Transform allObjects in childGameObjects)
                    {
                        allObjects.gameObject.layer = LayerMask.NameToLayer("Player Root");
                    }
                }
                SetName();
                Invoke("ChangeToGreenColor", 0.5f);
                //ChangeToGreenColor();
            }
            ResetPositionPlayer();
            // reset our timeouts on start
            _fallTimeoutDelta = FallTimeout;
            _jumpTimeoutDelta = JumpTimeout;
            
            
        }
        public void SetName()
        {
            PlayerName = playerManager.myPlayerName;
            //if (base.IsServer)
            //    SetNameObserver();
            //else
            //    SetNameServer();

        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]        
        public void SetNameServer()
        {
            PlayerName = playerManager.myPlayerName;
        }      
        [ObserversRpc(BufferLast = true, RunLocally = true)]
        public void SetNameObserver()
        {
            PlayerName = playerManager.myPlayerName;
        }

        private void TimeManager_OnTick()
        {
            if (base.IsOwner)
            {
                Reconciliation(default, false);
                CheckInput(out MoveData md);
                MoveWithData(md, false);

                _clientMoveData = md;
            }
            if (base.IsServer)
            {

                MoveWithData(default, true);
                ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _verticalVelocity, _fallTimeoutDelta, _jumpTimeoutDelta, value, slideSpeed, Grounded, timerIsRunning, _cinemachineTargetX, _targetRotation, movement, targetDirection, _CameraEulerY, screenCenterPoint, lookSensitivity);
                Reconciliation(rd, true);
            }
        }




        [Reconcile]     
        private void Reconciliation(ReconcileData rd, bool asServer, Channel channel = Channel.Unreliable)
        {
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
            _verticalVelocity = rd.VerticalVelocity;
            _fallTimeoutDelta = rd.FallTimeout;
            _jumpTimeoutDelta = rd.JumpTimeout;
            value = rd.valueSlide;
            slideSpeed = rd.slideSpeed;
            Grounded = rd.Grounded;
            timerIsRunning = rd.timeIsRunning;
            //_cinemachineTargetYaw = rd._cinemachineTargetYaw;
            ///_cinemachineTargetPitch = rd._cinemachineTargetPitch;

            _targetRotation = rd._targetRotation;
            movement = rd.movement;
            targetDirection = rd.targetDirection;
            _CameraEulerY = rd.CameraEulerY;
            screenCenterPoint = rd.screenCenterPoint;
            lookSensitivity = rd.lookSensitivity;
            _cinemachineTargetX = rd._cinemachineTargetX;
        }

        private void CheckInput(out MoveData md)
        {
       
            md = new MoveData()
            {

                Move = new Vector3(ultimateJoystick.GetHorizontalAxis(), 0f, ultimateJoystick.GetVerticalAxis()).normalized,
                Look = new Vector3(screenTouch.lookInput.x, screenTouch.lookInput.y),
                ResetPosRed = new Vector3(GameManagerEOS.Instance.RedTeamSpawnPoints[playerGunSelector.PlayerRedPosIndex].position.x, -0.46f, GameManagerEOS.Instance.RedTeamSpawnPoints[playerGunSelector.PlayerRedPosIndex].position.z),
                ResetPosBlue = new Vector3(GameManagerEOS.Instance.BlueTeamSpawnPoints[playerGunSelector.PlayerBluePosIndex].position.x, -0.46f, GameManagerEOS.Instance.BlueTeamSpawnPoints[playerGunSelector.PlayerBluePosIndex].position.z),
                CameraEulerY = _mainCamera.transform.eulerAngles.y,
                Jump = _input.jump,
                Slide = _input.slide,

                Sprint = _input.sprint,
            };

            _input.jump = false;
            _input.slide = false;
        }

        //slide value
        public float speed = 8f;
        public float gravity = -9.81f;
       
        Vector3 velocity;
        bool isGrounded;
 
        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;


        public float slideTimeRemaining = 10;
        public bool timerIsRunning;
        

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

        
        private void Start()
        {
            
            loadOutManager.PlayLoadoutSfX();
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _hasAnimator = TryGetComponent(out _animator);
            AssignAnimationIDs();
            SetRigWeight();
            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
           
        }
     
        public Vector3 screenCenterPoint;
        Ray rayNew;
        private void Update()
        {
            if (!base.IsOwner)
                return;

            //SetJoystickPos();
            //MoveOld();
            //Move();
            //screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);
            //rayNew = Camera.main.ScreenPointToRay(screenCenterPoint);

            if (firedBullet && fireBulletTime >= 0)
            {
                if (!firing)
                    fireBulletTime -= Time.deltaTime;
                if (fireBulletTime <= 0)
                {
                    firedBullet = false;
                }
            }
          
            PlayRunSound(); 
            //Pc input
            //playerGunSelector.SetLookInput(look.x, look.y, x, z);

            //touchinput 
            playerGunSelector.SetLookInput(mouseX, mouseY, x, z);

            //SetRigWeight();
            //if (Input.GetMouseButtonDown(1))
            //    Crouch();
            //if (Input.GetMouseButtonDown(2))
            //    shooterController.Aim();

            ControllerChanges();
        }


        private void LateUpdate()
        {
            if (!base.IsOwner)
                return;
            //CameraRotationOld();
           CameraRotation();

        }
        public void SetRigWeight()
        {
            if (base.IsServer)
                SetRigObserver();

            else
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
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

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


                CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                        _cinemachineTargetYaw, 0.0f);
            }
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
            {
                MoveSpeed = 7f;
                pistolRig.weight = 0f;
                rifleRig.weight = 0f;
            }
            else
            {
                MoveSpeed = 5;
                pistolRig.weight = 1f;
                rifleRig.weight = 1;
            }
                

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
        bool startSlide;
        public bool ResetPosition;
        [Replicate]
        private void MoveWithData(MoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
        {

            GroundedCheck();
            if(ResetPosition)
            {
                if (playerGunSelector.redTeamPlayer)
                {
                    transform.position = md.ResetPosRed;
                    
                }
                   
                else
                {
                    transform.position = md.ResetPosBlue;
                    
                }
                    
            }

            //if (playerGunSelector.aimAssist)
            //{
            //    if (Physics.SphereCast(rayNew, playerGunSelector.sphereCastRadiusAimAssist, out RaycastHit hitnew, float.MaxValue, playerGunSelector.AimAssistHitMask))
            //    {

            //        if (Physics.Raycast(rayNew, out RaycastHit hit, float.MaxValue, playerGunSelector.ActiveGun.ShootConfig.HitMask))
            //        {
            //            //rayHitPoint = hit.point;
            //        }

            //        if (hitnew.collider.gameObject.TryGetComponent<CapsuleCollider>(out CapsuleCollider collider))
            //        {
            //            if (!hit.collider.gameObject.TryGetComponent<CapsuleCollider>(out CapsuleCollider colliderNew))
            //            {
            //                if (_animationBlend > 2)
            //                {
            //                    if (!isSliding)
            //                    {
            //                        if (!playerGunSelector.ActiveGun.sniper)
            //                        {

            //                            //Debug.Log(hitnew.point.x + "New");
            //                            //Debug.Log(hit.point.x + "Old");
            //                            float Distance = Vector3.Distance(hit.collider.transform.position, transform.position);
            //                            if(Distance < 15f)
            //                            {
            //                                if (hitnew.point.x > hit.point.x)
            //                                {
            //                                    _cinemachineTargetYaw = Mathf.Lerp(_cinemachineTargetYaw, _cinemachineTargetYaw += 0.14f, delta * 30f);
            //                                }


            //                                if (hitnew.point.x < hit.point.x)
            //                                {
            //                                    _cinemachineTargetYaw = Mathf.Lerp(_cinemachineTargetYaw, _cinemachineTargetYaw -= 0.14f, delta * 30f);

            //                                }
            //                            }
            //                        }

            //                    }


            //                }

            //            }
            //            //
            //        }

            //    }
            //}

            if (playerHealth.PlayerDeathState())
                return;
      
           
                //float h = UltimateTouchpad.GetHorizontalAxis("Look");
                //float v = UltimateTouchpad.GetVerticalAxis("Look");
                //Vector3 direction = new Vector3(h, v, 0f).normalized;
                //Debug.Log(direction.x);
                // if there is an input and camera position is not fixed
                if (md.Look.sqrMagnitude >= _threshold && !LockCameraPosition)
                {
                //Don't multiply mouse input by Time.deltaTime;
                //float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                //_cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * sensitivity;
                //_cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * sensitivity;
                _cinemachineTargetX += md.Look.x * lookSensitivity * (float)base.TimeManager.TickDelta;
               // _cinemachineTargetPitch -= md.Look.y * lookSensitivity * delta;
                }

                // clamp our rotations so our values are limited 360 degrees
                _cinemachineTargetX = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
                //_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);


            

            //CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            //        _cinemachineTargetYaw, 0.0f);


            transform.rotation = Quaternion.Euler(0, _cinemachineTargetX, 0f);
            //transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation, 10 *delta);
            //Vector3 newRot = new Vector3(_cinemachineTargetPitch, 0,0 );
            //transform.forward = Vector3.Lerp(transform.forward, newRot, 10f);
            //transform.forward = Vector3.Lerp(transform.forward, _cinemachineTargetYaw, Time.deltaTime * 10f);
            //_controller.transform.forward = Vector3.Lerp(transform.forward, new Vector3(_cinemachineTargetPitch, 0 , _cinemachineTargetYaw), delta * 10f);
            // _controller.transform.forward = Vector3.Lerp(transform.forward, new Vector3(_mainCamera.transform.forward.x, 0, _mainCamera.transform.forward.z) , delta * 10f);


            //Jump Function 
            if (md.Jump && isCrouching)
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
                if (md.Jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // set sphere position, with offset
                    isSliding = false;
                    value = 0f;
                    slideSpeed = 0;
                    startSlide = false;
                    slideTimeRemaining = 0;
                    timerIsRunning = false;
                    
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    //PlayJumpSound();
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
                    _jumpTimeoutDelta -= (float)base.TimeManager.TickDelta;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= (float)base.TimeManager.TickDelta;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        // _animator.SetBool(_animIDFreeFall, true);
                    }
                }
                
                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * (float)base.TimeManager.TickDelta;
            }
            
            if (md.Slide && md.Move.z > 0.6f && !isAiming && !isCrouching)
            {
                startSlide = true;
                value = 1.8f;
                slideSpeed = 10f;
            }         
            if (Grounded && startSlide)
            {
                //firedBullet = false;
                timerIsRunning = true;
                isSliding = true;
                
                _animator.SetBool("Slide", isSliding);
                slideTimeRemaining = 0.5f;
            }

            if (value >  0 )
            {
                value -= (float)base.TimeManager.TickDelta;
                slideSpeed -= 3 * (float)base.TimeManager.TickDelta;

                Vector3 slideMovement = transform.forward * slideSpeed;
                slideMovement += new Vector3(0.0f, _verticalVelocity * 1.5f, 0.0f);

                _controller.Move(slideMovement * (float)base.TimeManager.TickDelta);
            }

            else
            {
                startSlide = false;
                slideTimeRemaining = 0;
                timerIsRunning = false;
                //isCrouching = false;
                isSliding = false;
                _animator.SetBool("Slide", isSliding);
                ControllerChanges();
               

                extraJump = false;
                value = 0f;
                slideSpeed = 0;

            }
            //Movement Function

            //if (playerHealth.PlayerDeathState())
            //    return;
            //_controller.detectCollisions = false;



            float neutralize = 1f;
           
            _animator.SetFloat("MoveX", md.Move.x);
            _animator.SetFloat("MoveZ", md.Move.z);

        
            if (md.Move.z > 0.2f && !isAiming && !isReloading && !firedBullet && !changingGun && !isCrouching && !isSliding)
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
    
            if(md.Move == Vector3.zero && !isSliding)
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
                    (float)base.TimeManager.TickDelta * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
               // _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, (float)base.TimeManager.TickDelta * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (md.Move != Vector3.zero)
            {
                //_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                //                  _mainCamera.transform.eulerAngles.y;
                _targetRotation = Mathf.Atan2(md.Move.x, md.Move.z) * Mathf.Rad2Deg +
                                  md.CameraEulerY;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                //transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            movement = targetDirection.normalized * targetSpeed * neutralize;
            movement += new Vector3(0.0f, _verticalVelocity, 0.0f);
            if (!isSliding)
            {

                 _controller.enabled = true;
                // move the player
                if (!ResetPosition)
                    _controller.Move(movement * (float)base.TimeManager.TickDelta);
            }
            
            // update animator if using character
            if (_hasAnimator)
            {
                //Animations State
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetBool("Aim Walk", isAimWalking);
                //if(targetSpeed > 5f && !isAiming)
                //{
                //    running = true;
                //    if(!isSliding)
                //    {
                //        pistolRig.weight = 0f;
                //        rifleRig.weight = 0f;
                //    }
             
                //}
                //else
                //{
                //    running = false;
                //    if (!isReloading && !changingGun)
                //    {
                //        if (weaponSwitching.selectedWeapon == 0)
                //            rifleRig.weight = 1f;

                //        else
                //            pistolRig.weight = 1f;
                //    }
                    
                //}
                RunningRigUpdate(targetSpeed);
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

        public void RunningRigUpdate(float targetSpeed)
        {
            if (base.IsServer)
                RunningRigUpdateObserver(targetSpeed);
            else
                RunningRigUpdateServer(targetSpeed);
        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void RunningRigUpdateServer(float targetSpeed)
        {
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
                if (!isReloading && !changingGun)
                {
                    if (weaponSwitching.selectedWeapon == 0)
                        rifleRig.weight = 1f;

                    else
                        pistolRig.weight = 1f;
                }
            }
        }
        [ObserversRpc(BufferLast = true, RunLocally = true)]
        public void RunningRigUpdateObserver(float targetSpeed)
        {
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
                if (!isReloading && !changingGun)
                {
                    if (weaponSwitching.selectedWeapon == 0)
                        rifleRig.weight = 1f;

                    else
                        pistolRig.weight = 1f;
                }
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
            if (playerHealth.PlayerDeathState())
                return;

            if(ultimateJoystick.GetVerticalAxis() < 0.6f)
            {
                CrouchInput();
            }
            //if (ultimateJoystick.GetVerticalAxis() > 0.6f && !isAiming && !isCrouching)
            //    StartSlide();

            //ForPcControls
            //if (_input.move.y > 0.6f && !isAiming && !isCrouching)
            //    StartSlide();
        
        }
       
     
        public void JumpAndGravity(MoveData md, float delta)
        {
            if (md.Jump && isCrouching)
            {
                isCrouching = false;
                _animator.SetBool("Crouch", isCrouching);
            }
            else if (Grounded && !isCrouching)
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
                if (md.Jump && _jumpTimeoutDelta <= 0.0f)
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
                    _jumpTimeoutDelta -= delta;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= delta;
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
                _verticalVelocity += Gravity * delta;
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
            audioSource.PlayOneShot(gunOutSound, 1);
            //audioSource.PlayOneShot(gunOutSound, 1);
            if (base.IsServer)           
                weaponSwitching.GunSwapVisualTakeInObserver();

            else
                weaponSwitching.GunSwapVisualTakeInServer();

        }
        public void GunSwapingGunChangeOut()
        {
            
            audioSource.PlayOneShot(gunInSound, 1);
            if (base.IsServer)
                weaponSwitching.GunSwapVisualTakeOutObserver();

            
            else
                weaponSwitching.GunSwapVisualTakeOutServer();

        }
        public void ReloadCheck(bool state)
        {
            isReloading = state;
            SetRigWeight();

        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void SetRigServer()
        {
            
            if (weaponSwitching.selectedWeapon == 0)
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

            else
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
        [ObserversRpc(BufferLast = true, RunLocally = true)]
        public void SetRigObserver()
        {
            
            if (weaponSwitching.selectedWeapon == 0)
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

            else
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
        public void GunChangeAnimationCheck()
        {
            weaponSwitching.GunChangeAnimationCheck();
        }
        public void GunChangeAnimationCheckBackwards()
        {
            weaponSwitching.GunChangeAnimationCheckBackwards();
        }
        public AudioSource audioRunSource;
        public AudioSource audioCrouchSource;
        public void PlayRunSound()
        {

            if (base.IsServer)
                PlayRunSoundObserver();
            else
                PlayRunSoundServer();
        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void PlayRunSoundServer()
        {
            if (Grounded)
            {
                if (!isCrouching && _animationBlend > 2)
                {
                    audioCrouchSource.Pause();
                    audioRunSource.UnPause();
                }


                else if (isCrouching && _animationBlend > 2)
                {
                    audioRunSource.Pause();
                    audioCrouchSource.UnPause();
                }

                else
                {
                    audioRunSource.Pause();
                    audioCrouchSource.Pause();
                }
            }
            else
            {
                audioRunSource.Pause();
                audioCrouchSource.Pause();
            }
        }
        [ObserversRpc(BufferLast = false, RunLocally = true)]
        public void PlayRunSoundObserver()
        {
            if (Grounded)
            {
                if (!isCrouching && _animationBlend > 2)
                {
                    audioCrouchSource.Pause();
                    audioRunSource.UnPause();
                }


                else if (isCrouching && _animationBlend > 2)
                {
                    audioRunSource.Pause();
                    audioCrouchSource.UnPause();
                }

                else
                {
                    audioRunSource.Pause();
                    audioCrouchSource.Pause();
                }
            }
            else
            {
                audioRunSource.Pause();
                audioCrouchSource.Pause();
            }
        }
        
        public AudioClip jumpSound;
        public AudioClip gunInSound;
        public AudioClip gunOutSound;
        public void PlayJumpSound()
        {

            if (base.IsServer)
                PlayJumpSoundObserver();

            else
                PlayJumpSoundServer();
        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void PlayJumpSoundServer()
        {
          
            if(!isCrouching)
            {
                if(Grounded)
                {
                    if (_jumpTimeoutDelta <= -0.1)
                        audioSource.PlayOneShot(jumpSound, 0.8f);
                }
                    
            }
                
        }
        [ObserversRpc(BufferLast = false, RunLocally = true)]
        public void PlayJumpSoundObserver()
        {
            
            if (!isCrouching)
            {
                if (Grounded)
                {
                    if (_jumpTimeoutDelta <= 0)
                        audioSource.PlayOneShot(jumpSound, 0.8f);
                }
            }
   
        }
        public void ResetPositionPlayer()
        {
            if (base.IsServer)
                ResetPositionPlayerObserver();
            else
                ResetPositionPlayerServer();

            if(base.IsOwner)
                loadOutManager.PlayLoadoutSfX();
        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void ResetPositionPlayerServer()
        {
            StartCoroutine(DelayResetPosition());
        }
        [ObserversRpc(BufferLast = false, RunLocally = true)]
        public void ResetPositionPlayerObserver()
        {
            StartCoroutine(DelayResetPosition());
        }

        IEnumerator DelayResetPosition()
        {
            ResetPosition = true;

            if (playerGunSelector.redTeamPlayer)
                _cinemachineTargetYaw = 0;
            else
                _cinemachineTargetYaw = 180;

            if (loadOutButton != null)
                loadOutButton.SetActive(true);
            

            if (playerGunSelector.blueTeamPlayer)
                _cinemachineTargetYaw = 180;

            yield return new WaitForSeconds(3f);
            ResetPosition = false;

            yield return new WaitForSeconds(3f);
            if(loadOutButton != null)
                loadOutButton.SetActive(false);
        }
        public void ChangeToGreenColor()
        {
            
            if (playerGunSelector.redTeamPlayer)
            {
                //Debug.Log("Work");
                foreach (GameObject i in PlayerRespawn.Instance.RedPlayers)
                {
                    PlayerCustomization playerCustomization = i.GetComponent<PlayerCustomization>();
                    //Debug.Log("Color");
                    playerCustomization.Characters[playerCustomization.GenderIndex].MainBody[playerCustomization.characterIndex[playerCustomization.GenderIndex].MainBodyIndex].gameObject.GetComponent<Outline>().enabled = false;
                }
            }
            else
            {
                foreach (GameObject i in PlayerRespawn.Instance.BluePlayers)
                {
                    PlayerCustomization playerCustomization = i.GetComponent<PlayerCustomization>();
                    playerCustomization.Characters[playerCustomization.GenderIndex].MainBody[playerCustomization.characterIndex[playerCustomization.GenderIndex].MainBodyIndex].gameObject.GetComponent<Outline>().enabled = false;
                }
            }
        }

        public void SeeInvincibilty()
        {
            //StartCoroutine(SeeInvincibiltyDelay());
            if (base.IsServer)
                SeeInvincibiltyObserver();

            else
                SeeInvincibiltyServer();
        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void SeeInvincibiltyServer()
        {
            StartCoroutine(SeeInvincibiltyDelay());
        }
        [ObserversRpc(BufferLast = true, RunLocally = true)]
        public void SeeInvincibiltyObserver()
        {
            StartCoroutine(SeeInvincibiltyDelay());
        }
        public GameObject playerMainBody;
        IEnumerator SeeInvincibiltyDelay()
        {
            PlayerCustomization playerCustomization = GetComponent<PlayerCustomization>();
            GameObject player = playerCustomization.Characters[playerCustomization.GenderIndex].MainBody[playerCustomization.characterIndex[playerCustomization.GenderIndex].MainBodyIndex]; 
            playerMainBody = playerCustomization.Characters[playerCustomization.GenderIndex].MainBody[playerCustomization.characterIndex[playerCustomization.GenderIndex].MainBodyIndex];

            player.GetComponent<Outline>().OutlineColor = Color.white;

            player.GetComponent<Outline>().enabled = true;
            

           
            yield return new WaitForSeconds(5f);
            if (base.IsOwner)
            {
                //player.GetComponent<Outline>().enabled = false;
                if (playerGunSelector.redTeamPlayer)
                    PlayerRespawn.Instance.DisableOutlineRedPlayers();
                else
                    PlayerRespawn.Instance.DisableOutlineBluePlayers();

            }
            //if (playerGunSelector.redTeamPlayer)
            //    PlayerRespawn.Instance.DisableOutlineRedPlayers();
            //else
            //    PlayerRespawn.Instance.DisableOutlineBluePlayers();

            player.GetComponent<Outline>().OutlineColor = Color.red;
        }
        public void SeeInvincibiltySpawn()
        {
            //StartCoroutine(SeeInvincibiltyDelay());
            if (base.IsServer)
                SeeInvincibiltyObserverSpawn();

            else
                SeeInvincibiltyServeSpawn();
        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void SeeInvincibiltyServeSpawn()
        {
            StartCoroutine(SeeInvincibiltyDelaySpawn());
        }
        [ObserversRpc(BufferLast = true, RunLocally = true)]
        public void SeeInvincibiltyObserverSpawn()
        {
            StartCoroutine(SeeInvincibiltyDelaySpawn());
        }
        
        IEnumerator SeeInvincibiltyDelaySpawn()
        {
            yield return new WaitForSeconds(2f);
            PlayerCustomization playerCustomization = GetComponent<PlayerCustomization>();
            GameObject player = playerCustomization.Characters[playerCustomization.GenderIndex].MainBody[playerCustomization.characterIndex[playerCustomization.GenderIndex].MainBodyIndex];
            playerMainBody = playerCustomization.Characters[playerCustomization.GenderIndex].MainBody[playerCustomization.characterIndex[playerCustomization.GenderIndex].MainBodyIndex];
            player.GetComponent<Outline>().OutlineColor = Color.white;

            player.GetComponent<Outline>().enabled = true;



            yield return new WaitForSeconds(5f);
            if (base.IsOwner)
            {
                //player.GetComponent<Outline>().enabled = false;
                if (playerGunSelector.redTeamPlayer)
                    PlayerRespawn.Instance.DisableOutlineRedPlayers();
                else
                    PlayerRespawn.Instance.DisableOutlineBluePlayers();

            }
            //if (playerGunSelector.redTeamPlayer)
            //    PlayerRespawn.Instance.DisableOutlineRedPlayers();
            //else
            //    PlayerRespawn.Instance.DisableOutlineBluePlayers();

            player.GetComponent<Outline>().OutlineColor = Color.red;
        }
        public GameObject joystick;
        public Rect joystickPos;
        public void SetJoystickPos()
        {
            joystick.transform.position = joystickPos.position;
        }
    }
    
}
