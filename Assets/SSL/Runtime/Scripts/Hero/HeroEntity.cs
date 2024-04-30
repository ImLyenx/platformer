using UnityEngine;

public class HeroEntity : MonoBehaviour
{
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
    private HeroJumpSettings _jumpSettings;

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
    public bool IsTouchingGround { get; private set; } = false;

    [Header("Horizontal Movements")]
    [SerializeField]
    private HeroHorizontalMovementsSettings _movementsSettings;
    private float _horizontalSpeed = 0f;
    private float _moveDirX = 0f;

    [Header("Dash")]
    [SerializeField]
    private float _DashSpeed = 40f;

    [SerializeField]
    private float _DashDuration = 0.1f;
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

    public void JumpStart()
    {
        _jumpState = JumpState.JumpImpulsion;
        _jumpTimer = 0f;
    }

    public bool IsJumping => _jumpState != JumpState.NotJumping;

    // MARK: FixedUpdate
    private void FixedUpdate()
    {
        _ApplyGroundDetection();

        if (_AreOrientAndMovementOpposite())
        {
            _TurnBack();
        }
        else
        {
            _UpdateHorizontalSpeed();
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

        if (Input.GetKeyDown(KeyCode.F))
        {
            _Dash();
        }
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
        _rigidbody.velocity = velocity;
    }

    private void _UpdateHorizontalSpeed()
    {
        if (_moveDirX != 0f)
        {
            _Accelerate();
        }
        else
        {
            _Decelerate();
        }
    }

    private void _Accelerate()
    {
        _horizontalSpeed += _movementsSettings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > _movementsSettings.maxSpeed && !_isDashing)
        {
            _horizontalSpeed = _movementsSettings.maxSpeed;
        }
    }

    private void _Decelerate()
    {
        _horizontalSpeed -= _movementsSettings.deceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
        }
    }

    private void _TurnBack()
    {
        _horizontalSpeed -= _movementsSettings.turnBackFriction * Time.fixedDeltaTime;
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
        _UpdateOrientVisual();
        _UpdateDash();
    }

    private void _UpdateOrientVisual()
    {
        if (_moveDirX == 0f)
            return;

        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }

    private void _Dash()
    {
        _horizontalSpeed = _DashSpeed;
        _dashTimer = _DashDuration;
        _isDashing = true;
    }

    private void _UpdateDash()
    {
        if (!_isDashing)
            return;
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
        _verticalSpeed -= settings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -settings.maxFallSpeed)
        {
            _verticalSpeed = -settings.maxFallSpeed;
        }
    }

    private void _ApplyVerticalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.y = _verticalSpeed;
        _rigidbody.velocity = velocity;
    }

    private void _ApplyGroundDetection()
    {
        IsTouchingGround = _groundDetector.DetectGroundNearby();
    }

    private void _ResetVerticalSpeed()
    {
        _verticalSpeed = 0f;
    }

    private void _UpdateJumpStateImpulsion()
    {
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings.jumpMaxDuration)
        {
            _verticalSpeed = _jumpSettings.jumpSpeed;
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

    private void OnGUI()
    {
        if (!_guiDebug)
            return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"MoveDirX = {_moveDirX}");
        GUILayout.Label($"OrientX = {_orientX}");
        GUILayout.Label($"Jump State = {_jumpState}");
        if (IsTouchingGround)
        {
            GUILayout.Label("On Ground");
        }
        else
        {
            GUILayout.Label("In Air");
        }
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label($"Vertical Speed = {_verticalSpeed}");
        GUILayout.EndVertical();
    }
}
