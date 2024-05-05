using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("Entity")]
    [SerializeField]
    private HeroEntity _entity;
    private bool _entityWasTouchingGround = false;

    [Header("Jump Buffer")]
    [SerializeField]
    private float _jumpBufferDuration = 0.2f;
    private float _jumpBufferTimer = 0f;

    [Header("Coyote Time")]
    [SerializeField]
    private float _coyoteTimeDuration = 0.2f;
    private float _coyoteTimeCountdown = -1f;

    [Header("Debug")]
    [SerializeField]
    private bool _guiDebug = false;

    private int _multiJumpCount = 0;

    private void OnGUI()
    {
        if (!_guiDebug)
            return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"Jump Buffer Timer = {_jumpBufferTimer}");
        GUILayout.Label($"Coyote Time Countdown = {_coyoteTimeCountdown}");
        GUILayout.EndVertical();
    }

    private void Start()
    {
        _CancelJumpBuffer();
    }

    private void Update()
    {
        _UpdateJumpBuffer();

        _entity.SetMoveDirX(GetInputMoveX());

        if (_EntityHasExitedGround())
        {
            _ResetCoyoteTime();
        }
        else
        {
            _UpdateCoyoteTime();
        }

        if (_GetInputDownJump())
        {
            if (_entity.IsTouchingGround || _IsCoyoteTimeActive())
            {
                _entity.JumpStart(0);
            }
            else if (!_entity.IsJumpImpulsing && _multiJumpCount < _entity.MultiJumpCountMax - 1)
            {
                _multiJumpCount++;
                _entity.JumpStart(_multiJumpCount);
            }
            else
            {
                _ResetJumpBuffer();
            }
        }
        if (IsJumpBufferActive())
        {
            if (_entity.IsTouchingGround && !_entity.IsJumping)
            {
                _entity.JumpStart(0);
            }
        }

        if (_entityWasTouchingGround)
        {
            _multiJumpCount = 0; 
        }

        if (_entity.IsJumpImpulsing)
        {
            if (!_GetInputJump() && _entity.IsJumpMinDurationReached)
            {
                _entity.StopJumpImpulsion();
            }
        }

        _entityWasTouchingGround = _entity.IsTouchingGround;
    }

    private float GetInputMoveX()
    {
        float inputMoveX = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q))
        {
            inputMoveX -= 1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputMoveX += 1f;
        }
        return inputMoveX;
    }

    private bool _GetInputDownJump()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    private bool _GetInputJump()
    {
        return Input.GetKey(KeyCode.Space);
    }

     private void _ResetJumpBuffer()
    {
        _jumpBufferTimer = 0f;
    }

    private bool IsJumpBufferActive()
    {
        return _jumpBufferTimer < _jumpBufferDuration;
    }

    private void _UpdateJumpBuffer()
    {
        if (!IsJumpBufferActive())
            return;
        _jumpBufferTimer += Time.deltaTime;
    }

    private void _CancelJumpBuffer()
    {
        _jumpBufferTimer = _jumpBufferDuration;
    }

    private void _UpdateCoyoteTime()
    {
        if (!_IsCoyoteTimeActive())
            return;
        _coyoteTimeCountdown -= Time.deltaTime;
    }

    private bool _IsCoyoteTimeActive()
    {
        return _coyoteTimeCountdown > 0f;
    }

    private void _ResetCoyoteTime()
    {
        _coyoteTimeCountdown = _coyoteTimeDuration;
    }

    private bool _EntityHasExitedGround()
    {
        return !_entity.IsTouchingGround && _entityWasTouchingGround;
    }
}
