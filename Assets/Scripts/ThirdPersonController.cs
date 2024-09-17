using System;
using InputSystem;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class ThirdPersonController : MonoBehaviourPunCallbacks
{
    [Header("General")] [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PhotonView view;
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController controller;
    [SerializeField] private InputController input;
    public Transform centerOfPlayer;

    [Header("Player")] [Tooltip("Move speed of the character in m/s")] [SerializeField]
    private float moveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")] [SerializeField]
    private float sprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")] [Range(0.0f, 0.3f)] [SerializeField]
    private float rotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")] [SerializeField]
    private float speedChangeRate = 10.0f;

    [SerializeField] private AudioClip landingAudioClip;
    [SerializeField] private AudioClip[] footstepAudioClips;
    [Range(0, 1)] [SerializeField] private float footstepAudioVolume = 0.5f;


    [Space(10)] [Tooltip("The height the player can jump")] [SerializeField]
    private float jumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")] [SerializeField]
    private float gravity = -15.0f;


    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    [SerializeField]
    private float jumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")] [SerializeField]
    private float fallTimeout = 0.15f;


    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool grounded = true;

    [Tooltip("Useful for rough ground")] public float groundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")] [SerializeField]
    private float groundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")] [SerializeField]
    private LayerMask groundLayers;


    [Header("Cinemachine")] [SerializeField]
    private Camera mainCamera;

    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")] [SerializeField]
    private GameObject cinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")] [SerializeField]
    private float topClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")] [SerializeField]
    private float bottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    [SerializeField]
    private float cameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")] [SerializeField]
    private bool lockCameraPosition = false;

    [SerializeField] TauntMenuController tauntMenuController;

    [Header("Interactable Objects")] [SerializeField]
    private float interactableObjectRange = 3.0f;

    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private float checkRate = 0.2f;
    [SerializeField] private float nextCheckTime = 0f;
    private IInteractable _previousInteractableInRange;

    [Header("Follow behaviour")] [SerializeField]
    private LineRenderer lineRenderer;

    private bool _isFollowing;
    private Transform _followTarget;
    private PhotonView _followTargetView;
    private Transform _followTargetCenterOfPlayer;


    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float CameraThreshold = 0.00001f;
    private bool _cursorLocked;

    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    public bool isAnimationPlaying;
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private bool _hasAnimator;
    private bool _openTauntMenu;
    private bool _isInteracting;
    private Vector3 _sitPosition;

    public static event Action OnPlayerSpawned;

    private bool IsCurrentDeviceMouse => playerInput.currentControlScheme == "KeyboardMouse";


    private void Awake()
    {
        SetupPlayer();
    }

    private void Update()
    {
        if (!view.IsMine || isAnimationPlaying)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            Teleport();
        }

//        Debug.Log(PlayerList.Players.Count);
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out var hit, 100))
            {
                PhotonView targetView = hit.collider.gameObject.GetComponent<PhotonView>();
                if (targetView != null)
                {
                    targetView.RPC("SetupFollow", RpcTarget.AllBufferedViaServer, view.ViewID);
                }
            }
        }

        DetectInteractableObjects();

        if (_isInteracting)
        {
            //Sit();
        }
        else
        {
            JumpAndGravity();
            GroundedCheck();
            Move();
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (_isFollowing && _followTargetView != null && otherPlayer.ActorNumber == _followTargetView.OwnerActorNr)
        {
            CleanupFollowState();
        }
    }


    [PunRPC]
    public void DrawLine()
    {
        lineRenderer.SetPosition(0, centerOfPlayer.position);
        lineRenderer.SetPosition(1, _followTargetCenterOfPlayer.position);
    }

    [PunRPC]
    public void SetupFollow(int targetViewID)
    {
        if (!PhotonView.Find(targetViewID)) return;

        if (_followTargetView != null && _followTargetView.ViewID == targetViewID)
        {
            CleanupFollowState();
        }
        else
        {
            _isFollowing = true;
            lineRenderer.enabled = true;
            _followTargetView = PhotonView.Find(targetViewID);
            _followTarget = _followTargetView.transform;
            _followTargetCenterOfPlayer = _followTarget.GetComponent<ThirdPersonController>().centerOfPlayer;
        }
    }

    private void CleanupFollowState()
    {
        _isFollowing = false;
        lineRenderer.enabled = false;
        _followTarget = null;
        _followTargetView = null;
        _followTargetCenterOfPlayer = null;
    }

    private void Follow()
    {
        Vector3 directionToTarget = _followTarget.position - transform.position;
        directionToTarget.y = 0;

        float currentDistance = directionToTarget.magnitude;
        float stopDistance = 2;


        if (currentDistance > stopDistance)
        {
            Vector3 targetPosition = _followTarget.position;
            targetPosition.y = transform.position.y;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                rotationSmoothTime * 60 * Time.deltaTime);

            controller.Move(new Vector3(0, -5, 0) * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, sprintSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetFloat(_animIDSpeed, 0);
            animator.SetFloat(_animIDMotionSpeed, 0);
        }


        float animationSpeed = currentDistance > stopDistance ? 1 : 0;
        animator.SetFloat(_animIDSpeed, animationSpeed * moveSpeed);
        animator.SetFloat(_animIDMotionSpeed, animationSpeed);

        if (_followTargetCenterOfPlayer)
            view.RPC(nameof(DrawLine), RpcTarget.All);
    }


    private void InteractWithObject()
    {
        if (_previousInteractableInRange == null) return;
        _previousInteractableInRange.Interact(view);
        if (_previousInteractableInRange.IsUiInteraction)
            input.SetCursorState(false);
        _previousInteractableInRange = null;
    }

    public void Sit(bool state)
    {
        view.RPC(nameof(SetBoolAnimation), RpcTarget.AllBufferedViaServer, "Sitting", state);
    }

    [PunRPC]
    private void TriggerAnimation(string animationName)
    {
        animator.SetTrigger(animationName);
    }

    [PunRPC]
    private void SetBoolAnimation(string animationName, bool state)
    {
        animator.SetBool(animationName, state);
    }

    public void SetupSit(Vector3 newPosition)
    {
        _isInteracting = true;
        _sitPosition = newPosition;
        Sit(true);
    }

    public void StandUp()
    {
        _isInteracting = false;
        Sit(false);
    }

    private void Teleport()
    {
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out var hit, 100))
        {
            controller.enabled = false;
            transform.position = hit.point;
            controller.enabled = true;
        }
    }

    private void DetectInteractableObjects()
    {
        if (Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + checkRate;

        Collider[] hitColliders =
            Physics.OverlapSphere(transform.position, interactableObjectRange, interactableLayerMask);
        IInteractable closestInteractable = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (var hitCollider in hitColliders)
        {
            // Check if the object has an IInteractable component and if it is interactable
            if (!hitCollider.TryGetComponent(out IInteractable interactable) || !interactable.IsInteractable)
                continue;

            // Start with the hitCollider's transform position
            Vector3 targetPosition = hitCollider.transform.position;

            // Check if the object has a PopUpHandler to adjust the position using the popUpOffset
            if (hitCollider.TryGetComponent(out PopUpHandler popUpHandler))
            {
                // Adjust the target position by adding the popUpOffset
                targetPosition += popUpHandler.popUpOffset;
            }

            // Calculate the squared distance between the current position and the adjusted target position
            float dSqrToTarget = (targetPosition - currentPosition).sqrMagnitude;

            // Find the closest interactable object based on the distance
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestInteractable = interactable;
            }
        }

        // If a new interactable object is found and it's different from the previous one
        if (closestInteractable != null && closestInteractable != _previousInteractableInRange)
        {
            // Hide the info for the previously detected interactable
            _previousInteractableInRange?.HideInfo();

            // Use the interactable's ShowInfo method
            closestInteractable.ShowInfo();
            Debug.Log("Interactable object in range: " + closestInteractable);

            // Update the previous interactable reference
            _previousInteractableInRange = closestInteractable;
        }
        else if (closestInteractable == null && _previousInteractableInRange != null)
        {
            // If no interactable is found, hide the previous interactable's info
            _previousInteractableInRange.HideInfo();
            Debug.Log("No interactable object in range");

            _previousInteractableInRange = null;
        }
    }


    private void CameraRotation()
    {
        if (input.Look.sqrMagnitude >= CameraThreshold && !lockCameraPosition)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += input.Look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += input.Look.y * deltaTimeMultiplier;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {
        float targetSpeed = input.Sprint ? sprintSpeed : moveSpeed;

        if (input.Move == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = input.analogMovement ? input.Move.magnitude : 1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * speedChangeRate);

            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        Vector3 inputDirection = new Vector3(input.Move.x, 0.0f, input.Move.y).normalized;

        if (input.Move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                        new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        if (_hasAnimator)
        {
            animator.SetFloat(_animIDSpeed, _animationBlend);
            animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    private void JumpAndGravity()
    {
        if (grounded)
        {
            _fallTimeoutDelta = fallTimeout;

            if (_hasAnimator)
            {
                animator.SetBool(_animIDJump, false);
                animator.SetBool(_animIDFreeFall, false);
            }

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            if (input.Jump && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (_hasAnimator)
                {
                    animator.SetBool(_animIDJump, true);
                }
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = jumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (_hasAnimator)
                {
                    animator.SetBool(_animIDFreeFall, true);
                }
            }
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (footstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, footstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.TransformPoint(controller.center),
                    footstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(landingAudioClip, transform.TransformPoint(controller.center),
                footstepAudioVolume);
        }
    }

    private void ToggleHideTauntMenuInput()
    {
        if (!_openTauntMenu && CheckBlockingStates() && grounded)
        {
            OpenTauntMenu();
        }
        else if (_openTauntMenu)
        {
            HideTauntMenu();
        }
    }

    private void OpenTauntMenu()
    {
        tauntMenuController.canvasGameObject.SetActive(true);
        input.SetCursorState(false);
        _openTauntMenu = true;
    }

    public void HideTauntMenu()
    {
        tauntMenuController.canvasGameObject.SetActive(false);
        input.SetCursorState(true);
        _openTauntMenu = false;
    }

    public void AnimationFinished()
    {
        isAnimationPlaying = false;
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset,
            transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
            QueryTriggerInteraction.Ignore);

        if (_hasAnimator)
        {
            animator.SetBool(_animIDGrounded, grounded);
        }
    }

    private void SetupPlayer()
    {
        view = GetComponent<PhotonView>();
        if (view.IsMine)
        {
            Debug.Log("Setting up for my player");
            mainCamera.enabled = true;
            _cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out animator);


            input.SetCursorState(true);
            
            CutsceneManager.OnCutsceneStarted += () => isAnimationPlaying = true;
            CutsceneManager.OnCutsceneEnded += () => isAnimationPlaying = false;
            input.OnOpenHideTauntMenu += ToggleHideTauntMenuInput;
            input.OnInteract += InteractWithObject;
            input.OnMouseClick += ClickMouse;
            ReviveAnimation.OnPlayerRevived += OnPlayerSpawned;
            UIEventManager.OnUIInteractionEnded += () => input.SetCursorState(true);
            
            AssignAnimationIDs();

            _jumpTimeoutDelta = jumpTimeout;
            _fallTimeoutDelta = fallTimeout;
        }
        else
        {
            Debug.Log("Disabling camera for other player's prefab");
            Camera otherPlayerCamera = GetComponentInChildren<Camera>();
            if (otherPlayerCamera != null)
                otherPlayerCamera.gameObject.SetActive(false);
        }
    }

    private void ClickMouse()
    {
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out var hit, 100))
        {
            Debug.Log(hit.transform.name);
            ILeftMouseClick clickComponent = hit.transform.GetComponent<ILeftMouseClick>();
            clickComponent?.ClickMouse(hit);
        }
    }

    private bool CheckBlockingStates()
    {
        return !isAnimationPlaying && !_isFollowing;
    }
    
}