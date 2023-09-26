using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private static PlayerController Singleton;

    [Header("Movement Settings")]
    [SerializeField] private float sideMovementSpeed = 1f;
    [SerializeField] private float forwardMovementSpeed = 1f;
    [SerializeField] private float dashSpeedMultiplier = 2f;

    [Header("Dash Settings")]
    [SerializeField] private float dashDuration = 1f;
    [SerializeField] private float dashCooldownDuration = 1f;

    private float _currentTargetX;
    private bool _dashing = false;
    private float _dashTimer;
    private bool _dashCooldown = false;

    private Rigidbody _rb;
    private PlayerInput _playerInput;

    public TextMeshProUGUI debugText1;
    public TextMeshProUGUI debugText2;
    public TextMeshProUGUI debugText3;

    public static int CurrentLaneInt { get; set; } = 1;
    public static float ForwardMovementSpeed { get => Singleton.forwardMovementSpeed; set => Singleton.forwardMovementSpeed = value; }
    public static float SideMovementSpeed { get => Singleton.sideMovementSpeed; set => Singleton.sideMovementSpeed = value; }
    public static float CurrentDistance => Singleton.transform.position.z;

    public static Action OnLaneSwitch;  // Started a lane switch.
    public static Action OnSwitchedLane; // Reached a new lane.


    #region Initiation
    private void Awake()
    {
        if (Singleton)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        _rb = GetComponent<Rigidbody>();
        _playerInput = new();
    }
    private void Start()
    {
        _currentTargetX = LaneManager.GetLaneX(CurrentLaneInt);
        _rb.position = LaneManager.GetTargetLanePosition(_rb.position, CurrentLaneInt);
    }

    private void OnEnable() => GameController.OnGameStart += OnGameStart;

    private void OnDisable()
    {
        _playerInput.Disable();

        // Events
        GameController.OnGameStart -= OnGameStart;

        // Input reading
        _playerInput.Standard.SwitchLane.started -= ctx => OnSwitchLane(ctx);
        _playerInput.Standard.Dash.started -= ctx => OnDash(ctx);
    }
    #endregion

    /// <summary>
    /// Called when the 'GameStarted' value in the Game Controller is set to true.
    /// </summary>
    private void OnGameStart()
    {
        // Enable Input Reading
        _playerInput.Enable();
        _playerInput.Standard.SwitchLane.started += ctx => OnSwitchLane(ctx);
        _playerInput.Standard.Dash.started += ctx => OnDash(ctx);
    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        if (!_dashing && !_dashCooldown)
        {
            _dashing = true;
            _dashTimer = Time.time + dashDuration;
        }
    }

    private void Update()
    {
        if (!GameController.GameStarted) return;

        debugText1.SetText($"Forward speed: {forwardMovementSpeed}");
        debugText2.SetText($"Side speed: {sideMovementSpeed}");

        ForwardMovement();
        Dash();
        LaneSwitching();

        void ForwardMovement()
        {
            Vector3 currentPos = _rb.position;

            float step = (_dashing ? ForwardMovementSpeed * dashSpeedMultiplier : ForwardMovementSpeed) * Time.deltaTime;
            float targetZ = currentPos.z + 1f;
            float curZ = Mathf.MoveTowards(_rb.position.z, targetZ, step);

            currentPos.z = curZ;
            _rb.MovePosition(currentPos);
        }
        void Dash()
        {
            if (_dashing && Time.time >= _dashTimer)
            {
                _dashing = false;
                _dashCooldown = true;
                _dashTimer = Time.time + dashCooldownDuration;
            }
            if (_dashCooldown && Time.time >= _dashTimer)
            {
                _dashCooldown = false;
            }
        }
        void LaneSwitching()
        {
            float step = (_dashing ? ForwardMovementSpeed * dashSpeedMultiplier : ForwardMovementSpeed) * Time.deltaTime;
            float curX = Mathf.MoveTowards(_rb.position.x, _currentTargetX, step);

            Vector3 currentPos = _rb.position;
            currentPos.x = curX;
            _rb.MovePosition(currentPos);
        }
    }

    private void OnSwitchLane(InputAction.CallbackContext ctx)
    {
        if (!GameController.GameStarted || PauseMenu.isPaused) return;

        int input = (int)ctx.ReadValue<float>();
        TrySwitchLane(input);
    }

    private void TrySwitchLane(int input)
    {
        // If the lane switch attempt is valid...
        int _targetLane = CurrentLaneInt + input;

        if (LaneManager.LaneExists(_targetLane))
        {
            CurrentLaneInt = _targetLane;

            // Calculate the target position based on the new lane
            _currentTargetX = LaneManager.GetLaneX(_targetLane);

            OnLaneSwitch?.Invoke();

            debugText1.SetText($"Current Lane: {CurrentLaneInt}");
        }
    }

    private void OnDestroy()
    {
        Singleton = null;
    }
}
