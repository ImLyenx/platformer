using UnityEngine;
using UnityEngine.Serialization;

public class HeroEntity : MonoBehaviour
{
    private CameraFollowable _cameraFollowable;

    [Header("Physics")]
    [SerializeField]
    private Rigidbody2D _rigidbody;

    [Header("Vertical Movements")]
    private float _verticalSpeed = 0f;

    [Header("Fall")]
    [SerializeField]
    private HeroFallSettings _fallSettings;

    [Header("Jump")]
    [SerializeField]
    private HeroJumpSettings[] _jumpSettings;
    public int MultiJumpCountMax => _jumpSettings.Length;

    private int _multiJumpCount = 0;

    [SerializeField]
    private HeroFallSettings _jumpFallSettings;

    enum JumpState
    {
        NotJumping,
        JumpImpulsion,
        Falling
    }

    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer = 0f;

    [Header("Ground")]
    [SerializeField]
    private GroundDetector _groundDetector;
    [SerializeField]
    private LeftWallDetector _leftWallDetector;
    [SerializeField]
    private RightWallDetector _rightWallDetector;
    public bool IsTouchingGround { get; private set; } = false;
    public bool IsTouchingLeftWall { get; private set; } = false;
    public bool IsTouchingRightWall { get; private set; } = false;

    [Header("Horizontal Movements")]
    [FormerlySerializedAs("_movementsSettings")]
    [SerializeField]
    private HeroHorizontalMovementsSettings _groundHorizontalMovementsSettings;

    [SerializeField]
    private HeroHorizontalMovementsSettings _airHorizontalMovementsSettings;
    private float _horizontalSpeed = 0f;
    private float _moveDirX = 0f;

    [Header("Dash")]
    [SerializeField]
    private HeroDashSettings _groundDashSettings;

    [SerializeField]
    private HeroDashSettings _airDashSettings;
    private bool _isDashing = false;
    private float _dashTimer = 0f;

    [Header("Orientation")]
    [SerializeField]
    private Transform _orientVisualRoot;
    private float _orientX = 1f;

    [Header("Debug")]
    [SerializeField]
    private bool _guiDebug = false;

    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }

    public void JumpStart(int multiJumpIndex)
    {
        _multiJumpCount = multiJumpIndex;
        _jumpState = JumpState.JumpImpulsion;
        _jumpTimer = 0f;
    }

    public bool IsJumping => _jumpState != JumpState.NotJumping;

    // MARK: FixedUpdate
    private void FixedUpdate()
    {
        _ApplyGroundDetection();
        _ApplyLeftWallDetection();
        _ApplyRightWallDetection();
        _UpdateCameraFollowPosition();

        HeroHorizontalMovementsSettings settings = _GetCurrentHorizontalMovementsSettings();
        if (_AreOrientAndMovementOpposite())
        {
            _TurnBack(settings);
        }
        else
        {
            _UpdateHorizontalSpeed(settings);
            _ChangeOrientFromHorizontalMovement();
        }

        if (IsJumping)
        {
            UpdateJump();
        }
        else
        {
            if (!IsTouchingGround)
            {
                _ApplyFallGravity(_fallSettings);
            }
            else
            {
                _ResetVerticalSpeed();
            }
        }

        _ApplyHorizontalSpeed();
        _ApplyVerticalSpeed();

        _UpdateDash();
    }

    private HeroHorizontalMovementsSettings _GetCurrentHorizontalMovementsSettings()
    {
        return IsTouchingGround
            ? _groundHorizontalMovementsSettings
            : _airHorizontalMovementsSettings;
    }

    private HeroDashSettings _GetCurrentDashSettings()
    {
        return IsTouchingGround ? _groundDashSettings : _airDashSettings;
    }

    private void _ChangeOrientFromHorizontalMovement()
    {
        if (_moveDirX == 0f)
            return;

        _orientX = Mathf.Sign(_moveDirX);
    }

    private void _ApplyHorizontalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed * _orientX;
        if (IsTouchingLeftWall && velocity.x < 0f)
        {
            velocity.x = 0f;
            _horizontalSpeed = 0f;
        }
        if (IsTouchingRightWall && velocity.x > 0f)
        {
            velocity.x = 0f;
            _horizontalSpeed = 0f;
        }
        _rigidbody.velocity = velocity;
    }

    private void _UpdateHorizontalSpeed(HeroHorizontalMovementsSettings settings)
    {
        if (_moveDirX != 0f)
        {
            _Accelerate(settings);
        }
        else
        {
            _Decelerate(settings);
        }
    }

    private void _Accelerate(HeroHorizontalMovementsSettings settings)
    {
        _horizontalSpeed += settings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > settings.maxSpeed && !_isDashing)
        {
            _horizontalSpeed = settings.maxSpeed;
        }
    }

    private void _Decelerate(HeroHorizontalMovementsSettings settings)
    {
        _horizontalSpeed -= settings.deceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
        }
    }

    private void _TurnBack(HeroHorizontalMovementsSettings settings)
    {
        _horizontalSpeed -= settings.turnBackFriction * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
            _ChangeOrientFromHorizontalMovement();
        }
    }

    private bool _AreOrientAndMovementOpposite()
    {
        return _moveDirX * _orientX < 0f;
    }

    // MARK: Update
    private void Update()
    {
        HeroDashSettings dashSettings = _GetCurrentDashSettings();
        if (Input.GetKeyDown(KeyCode.F))
        {
            _Dash(dashSettings);
        }
        _UpdateOrientVisual();
    }

    private void _UpdateOrientVisual()
    {
        if (_moveDirX == 0f)
            return;

        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }

    private void _Dash(HeroDashSettings settings)
    {
        _horizontalSpeed = settings.dashSpeed;
        _dashTimer = settings.dashDuration;
        _isDashing = true;
    }

    private void _UpdateDash()
    {
        if (_isDashing)
        {
            _dashTimer -= Time.fixedDeltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                _horizontalSpeed = 0f;
            }
        }
    }

    private void _ApplyFallGravity(HeroFallSettings settings)
    {
        if (!_isDashing)
        {
            _verticalSpeed -= settings.fallGravity * Time.fixedDeltaTime;
            if (_verticalSpeed < -settings.maxFallSpeed)
            {
                _verticalSpeed = -settings.maxFallSpeed;
            }
        }
        else
        {
            _verticalSpeed = 0f;
        }
    }

    private void _ApplyVerticalSpeed()
    {
        if (_isDashing)
        {
            Vector2 velocity = _rigidbody.velocity;
            velocity.y = 0f;
            _rigidbody.velocity = velocity;
        }
        else
        {
            Vector2 velocity = _rigidbody.velocity;
            velocity.y = _verticalSpeed;
            _rigidbody.velocity = velocity;
        }
    }

    private void _ApplyGroundDetection()
    {
        IsTouchingGround = _groundDetector.DetectGroundNearby();
    }

    private void _ApplyLeftWallDetection()
    {
        IsTouchingLeftWall = _leftWallDetector.DetectLeftWallNearby();
    }

    private void _ApplyRightWallDetection()
    {
        IsTouchingRightWall = _rightWallDetector.DetectRightWallNearby();
    }

    private void _ResetVerticalSpeed()
    {
        _verticalSpeed = 0f;
    }

    private void _UpdateJumpStateImpulsion()
    {
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings[_multiJumpCount].jumpMaxDuration)
        {
            _verticalSpeed = _jumpSettings[_multiJumpCount].jumpSpeed;
        }
        else
        {
            _jumpState = JumpState.Falling;
        }
    }

    private void _UpdateJumpStateFalling()
    {
        if (!IsTouchingGround)
        {
            _ApplyFallGravity(_jumpFallSettings);
        }
        else
        {
            _ResetVerticalSpeed();
            _jumpState = JumpState.NotJumping;
        }
    }

    private void UpdateJump()
    {
        switch (_jumpState)
        {
            case JumpState.JumpImpulsion:
                _UpdateJumpStateImpulsion();
                break;
            case JumpState.Falling:
                _UpdateJumpStateFalling();
                break;
        }
    }

    public void StopJumpImpulsion()
    {
        _jumpState = JumpState.Falling;
    }

    public bool IsJumpImpulsing => _jumpState == JumpState.JumpImpulsion;
    public bool IsJumpMinDurationReached => _jumpTimer >= _jumpSettings[_multiJumpCount].jumpMinDuration;

    private void _UpdateCameraFollowPosition()
    {
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        if (IsTouchingGround && !IsJumping)
        {
            _cameraFollowable.FollowPositionY = _rigidbody.position.y;
        }
    }

    private void Awake()
    {
        _cameraFollowable = GetComponent<CameraFollowable>();
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        _cameraFollowable.FollowPositionY = _rigidbody.position.y;
    }

    private void OnGUI()
    {
        if (!_guiDebug)
            return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"MoveDirX = {_moveDirX}");
        GUILayout.Label($"OrientX = {_orientX}");
        GUILayout.Label($"Jump State = {_jumpState}");
        GUILayout.Label($"MultiJumpCount = {_multiJumpCount}");
        if (IsTouchingGround)
        {
            GUILayout.Label("On Ground");
        }
        else
        {
            GUILayout.Label("In Air");
        }
        if (IsTouchingLeftWall)
        {
            GUILayout.Label("Touching Left Wall");
        }
        else
        {
            GUILayout.Label("Not Touching Left Wall");
        }
        if (IsTouchingRightWall)
        {
            GUILayout.Label("Touching Right Wall");
        }
        else
        {
            GUILayout.Label("Not Touching Right Wall");
        }
        if (_isDashing)
        {
            GUILayout.Label("Dashing");
        }
        else
        {
            GUILayout.Label("Not Dashing");
        }
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label($"Vertical Speed = {_verticalSpeed}");
        GUILayout.EndVertical();
    }
}
