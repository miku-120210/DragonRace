using UnityEngine;
using Fusion;
using NetworkRigidbody2D = Fusion.Addons.Physics.NetworkRigidbody2D;

public class PlayerRigidBodyMovement : NetworkBehaviour
{
    [Header("Movement")]
    private PlayerBehaviour _behaviour;
    [SerializeField] private LayerMask _groundLayer;
    private NetworkRigidbody2D _rb;
    private InputController _inputController;

    [SerializeField] float _speed = 1500f;
    [SerializeField] float _jumpForce = 10f;
    [SerializeField] float _maxVelocity = 5f;

    [SerializeField] private float fallMultiplier = 7f;
    [SerializeField] private float lowJumpMultiplier = 7f;
    private readonly float wallSlidingMultiplier = 1f;

    private Vector2 _groundHorizontalDragVector = new Vector2(0.1f, 1);
    private Vector2 _airHorizontalDragVector = new Vector2(0.98f, 1);
    private Vector2 _horizontalSpeedReduceVector = new Vector2(0.95f, 1);
    private Vector2 _verticalSpeedReduceVector = new Vector2(1, 0.95f);

    private Collider2D _collider;
    [Networked]
    private NetworkBool IsGrounded { get; set; }
    private bool _wallSliding;
    private Vector2 _wallSlidingNormal;

    private float _jumpBufferThreshold = 0.2f;
    private float _jumpBufferTime;

    [Networked]
    private float CoyoteTimeThreshold { get; set; } = 0.1f;
    [Networked]
    private float TimeLeftGrounded { get; set; }
    [Networked]
    private NetworkBool CoyoteTimeCD { get; set; }
    [Networked]
    private NetworkBool WasGrounded { get; set; }

    [Networked] public Vector3 Velocity { get; set; }

    //[Space()]
    //[Header("Particle")]
    //[SerializeField] private ParticleManager _particleManager;

    //[Space()]
    //[Header("Sound")]
    //[SerializeField] private SoundChannelSO _sfxChannel;
    //[SerializeField] private SoundSO _jumpSound;
    //[SerializeField] private AudioSource _playerSource;

    void Awake()
    {
        _rb = GetComponent<NetworkRigidbody2D>();
        _collider = GetComponentInChildren<Collider2D>();
        _behaviour = GetBehaviour<PlayerBehaviour>();
        _inputController = GetBehaviour<InputController>();
    }

    public override void Spawned()
    {
        Runner.SetPlayerAlwaysInterested(Object.InputAuthority, Object, true);
    }

    /// <summary>
    /// Detects grounded and wall sliding state
    /// </summary>
    private void DetectGroundAndWalls()
    {
        WasGrounded = IsGrounded;
        IsGrounded = default;
        _wallSliding = default;

        IsGrounded = (bool)Runner.GetPhysicsScene2D().OverlapBox((Vector2)transform.position + Vector2.down * (_collider.bounds.extents.y - .3f), Vector2.one * .85f, 0, _groundLayer);
        if (IsGrounded)
        {
            CoyoteTimeCD = false;
            return;
        }

        if (WasGrounded)
        {
            if (CoyoteTimeCD)
            {
                CoyoteTimeCD = false;
            }
            else
            {
                TimeLeftGrounded = Runner.SimulationTime;
            }
        }

        RaycastHit2D hitRight = Runner.GetPhysicsScene2D().CircleCast(
            transform.position + Vector3.right * (_collider.bounds.extents.x), // 円の中心
            0.3f, // 円の半径
            Vector2.zero, // キャスト方向（今回は移動させないので Vector2.zero）
            0f, // キャスト距離（その場での判定なので0）
            _groundLayer // 対象のレイヤー
            );

        if (hitRight.collider != null) // 右壁に接触しているか確認
        {
            _wallSliding = true;
            _wallSlidingNormal = Vector2.left; // 右壁に接触しているので、法線は左向き
#if UNITY_EDITOR
            Debug.Log("右壁接触したよー");

            // ヒットした場所を可視化（円の中心からヒット位置まで線を引く）
            Debug.DrawLine(
                transform.position + Vector3.right * (_collider.bounds.extents.x), // キャストした円の中心
                hitRight.point, // ヒットした場所
                Color.red, // 線の色
                1.0f // 線が表示される時間（秒）
            );
#endif
            return;
        }
        else
        {
            RaycastHit2D hitLeft = Runner.GetPhysicsScene2D().CircleCast(
                transform.position - Vector3.right * (_collider.bounds.extents.x),
                0.3f,
                Vector2.zero,
                0f,
                _groundLayer
                );

            if (hitLeft.collider != null)
            {
                _wallSliding = true;
                _wallSlidingNormal = Vector2.right;

#if UNITY_EDITOR
                Debug.Log("左壁接触したよー");
                // ヒットした場所を可視化（円の中心からヒット位置まで線を引く）
                Debug.DrawLine(
                    transform.position - Vector3.right * (_collider.bounds.extents.x),
                    hitLeft.point,
                    Color.blue,
                    1.0f
                );
#endif
            }
        }
    }

    public bool GetGrounded()
    {
        return IsGrounded;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput<InputData>(out var input))
        {
            var pressed = input.GetButtonPressed(_inputController.PrevButtons);
            _inputController.PrevButtons = input.Buttons;
            UpdateMovement(input);
            Jump(pressed);
            BetterJumpLogic(input);
        }

        Velocity = _rb.Rigidbody.velocity;
    }

    void UpdateMovement(InputData input)
    {
        DetectGroundAndWalls();

        if (input.GetButton(InputButton.LEFT) && _behaviour.InputsAllowed)
        {
            //Reset x velocity if start moving in oposite direction.
            if (_rb.Rigidbody.velocity.x > 0 && IsGrounded)
            {
                _rb.Rigidbody.velocity *= Vector2.up;
            }
            _rb.Rigidbody.AddForce(Vector2.left * _speed * Runner.DeltaTime, ForceMode2D.Force);
        }
        else if (input.GetButton(InputButton.RIGHT) && _behaviour.InputsAllowed)
        {
            //Reset x velocity if start moving in oposite direction.
            if (_rb.Rigidbody.velocity.x < 0 && IsGrounded)
            {
                _rb.Rigidbody.velocity *= Vector2.up;
            }
            _rb.Rigidbody.AddForce(Vector2.right * _speed * Runner.DeltaTime, ForceMode2D.Force);
        }
        else
        {
            //Different horizontal drags depending if grounded or not.
            if (IsGrounded)
                _rb.Rigidbody.velocity *= _groundHorizontalDragVector;
            else
                _rb.Rigidbody.velocity *= _airHorizontalDragVector;
        }

        LimitSpeed();
    }

    private void LimitSpeed()
    {
        //Limit horizontal velocity
        if (Mathf.Abs(_rb.Rigidbody.velocity.x) > _maxVelocity)
        {
            _rb.Rigidbody.velocity *= _horizontalSpeedReduceVector;
        }

        if (Mathf.Abs(_rb.Rigidbody.velocity.y) > _maxVelocity * 2)
        {
            _rb.Rigidbody.velocity *= _verticalSpeedReduceVector;
        }
    }

    #region Jump
    private void Jump(NetworkButtons pressedButtons)
    {

        //Jump
        if (pressedButtons.IsSet(InputButton.JUMP) || CalculateJumpBuffer())
        {
            if (_behaviour.InputsAllowed)
            {
                if (!IsGrounded && pressedButtons.IsSet(InputButton.JUMP))
                {
                    _jumpBufferTime = Runner.SimulationTime;
                }

                if (IsGrounded || CalculateCoyoteTime())
                {
                    _rb.Rigidbody.velocity *= Vector2.right; //Reset y Velocity
                    _rb.Rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                    CoyoteTimeCD = true;
                    if (Runner.IsForward && Object.HasInputAuthority)
                    {
                        RPC_PlayJumpEffects((Vector2)transform.position - Vector2.up * .5f);
                    }
                }
                else if (_wallSliding)
                {
                    _rb.Rigidbody.velocity *= Vector2.zero; //Reset y and x Velocity
                    _rb.Rigidbody.AddForce((Vector2.up + (_wallSlidingNormal)) * _jumpForce, ForceMode2D.Impulse);
                    //Debug.Log(_wallSlidingNormal);
                    CoyoteTimeCD = true;
                    if (Runner.IsForward && Object.HasInputAuthority)
                    {
                        RPC_PlayJumpEffects((Vector2)transform.position - _wallSlidingNormal * .5f);
                    }
                }
            }
        }
    }

    private bool CalculateJumpBuffer()
    {
        return (Runner.SimulationTime <= _jumpBufferTime + _jumpBufferThreshold) && IsGrounded;
    }

    private bool CalculateCoyoteTime()
    {
        return (Runner.SimulationTime <= TimeLeftGrounded + CoyoteTimeThreshold);
    }

    [Rpc(sources: RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_PlayJumpEffects(Vector2 particlePos)
    {
        PlayJumpSound();
        PlayJumpParticle(particlePos);
    }

    private void PlayJumpSound()
    {
        //_sfxChannel.CallSoundEvent(_jumpSound, Object.HasInputAuthority ? null : _playerSource);
    }

    private void PlayJumpParticle(Vector2 pos)
    {
        //_particleManager.Get(ParticleManager.ParticleID.Jump).transform.position = pos;
    }

    /// <summary>
    /// Increases gravity force on the player based on input and current fall progress.
    /// </summary>
    /// <param name="input"></param>
    private void BetterJumpLogic(InputData input)
    {
        if (IsGrounded) return;
        if (_rb.Rigidbody.velocity.y < 0)
        {
            if (_wallSliding && input.AxisPressed())
            {
                _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (wallSlidingMultiplier - 1) * Runner.DeltaTime;
            }
            else
            {
                _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Runner.DeltaTime;
            }
        }
        else if (_rb.Rigidbody.velocity.y > 0 && !input.GetButton(InputButton.JUMP))
        {
            _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Runner.DeltaTime;
        }
    }
    #endregion
}
