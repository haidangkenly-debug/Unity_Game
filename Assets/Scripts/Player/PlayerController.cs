using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Data")]
    public PlayerData playerData;

    [Header("Components")]
    public Rigidbody2D rb;
    public PlayerInput playerInput;
    public Animator anim;
    public Transform attackPoint;
    public LayerMask attackLayer;

    [Header("Ground Check")]
    public Transform groundCheck;  // 🔧 CRITICAL: Must be assigned in prefab!
    public LayerMask groundLayer;
    public bool isGrounded;

    [Header("States Logic")]
    public PlayerState currentState { get; private set; }
    public IdleState idleState { get; private set; }
    public RunState runState { get; private set; }
    public JumpState jumpState { get; private set; }
    public FallState fallState { get; private set; }
    public AttackState attackState { get; private set; }
    public DashState dashState { get; private set; }

    // Input tracking
    public Vector2 moveInput;
    private bool jumpPressed;
    private int selectedAttackSlot = 1;
    public int facingDirection = 1;
    private bool isInitialized = false;
    private bool IsReady => playerData != null && currentState != null && isInitialized;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        playerInput = GetComponent<PlayerInput>();

        // ✅ CRITICAL: Validate groundCheck first
        if (groundCheck == null)
        {
            Debug.LogError("[PlayerController] ❌ CRITICAL: groundCheck Transform is NOT assigned!");
            Debug.LogError("[PlayerController] ❌ Please assign groundCheck in the prefab's PlayerController component");
            enabled = false;
            return;
        }

        // ✅ Don't initialize states here - wait for CharacterPrefabManager
        if (playerData == null)
        {
            Debug.Log("[PlayerController] 🕐 Waiting for playerData from CharacterPrefabManager...");
        }
    }

    private void InitializeStates()
    {
        if (playerData == null)
        {
            Debug.LogError("[PlayerController] ❌ Cannot initialize states - playerData is NULL!");
            return;
        }

        // ✅ Create all states
        idleState = new IdleState();
        runState = new RunState();
        jumpState = new JumpState();
        fallState = new FallState();
        dashState = new DashState();
        attackState = new AttackState();

        // ✅ Initialize all states
        foreach (var state in new PlayerState[] { idleState, runState, jumpState, fallState, dashState, attackState })
        {
            state.Initialize(this);
        }
    }

    private void Update()
    {
        // ✅ OPTIMIZED: Single IsReady check instead of 11 null checks
        if (!IsReady) return;

        if (currentState is not DashState)
        {
            Flip();
        }
        currentState.Update();
    }

    private void FixedUpdate()
    {
        // ✅ OPTIMIZED: Single IsReady check
        if (!IsReady) return;

        CheckGrounded();

        if (currentState is not DashState)
        {
            ApplyVariableGravity();
        }

        currentState.FixedUpdate();
        HandleJumpInput();
    }

    public void TransitionToState(PlayerState newState)
    {
        if (newState == null)
        {
            Debug.LogError("[PlayerController] ❌ Attempted to transition to NULL state!");
            return;
        }

        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    // ✅ OPTIMIZED: Single initialization point called by CharacterPrefabManager
    public void OnPlayerDataAssigned(PlayerData newData)
    {
        if (newData == null)
        {
            Debug.LogError("[PlayerController] ❌ Tried to assign NULL PlayerData!");
            return;
        }

        playerData = newData;

        // ✅ Initialize ONLY once
        if (!isInitialized)
        {
            InitializeStates();
            TransitionToState(idleState);
            isInitialized = true;

            // ✅ Initialize SkillManager with data
            SkillManager skillMgr = GetComponent<SkillManager>();
            if (skillMgr != null)
            {
                skillMgr.InitializeSkillManager(playerData);
            }

            Debug.Log($"<color=green>✅ [PlayerController] Ready: {playerData.characterName}</color>");
        }
    }

    #region Input Handlers (New Input System)

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        currentState?.CaptureMoveInput(moveInput);
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed) jumpPressed = true;
    }

    public void OnSelectSlot1(InputValue value)
    {
        if (value.isPressed) selectedAttackSlot = 1;
    }

    public void OnSelectSlot2(InputValue value)
    {
        if (value.isPressed) selectedAttackSlot = 2;
    }

    public void OnSwitchSkill(InputValue value)
    {
        if (value.isPressed) selectedAttackSlot = (selectedAttackSlot == 1) ? 2 : 1;
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            if (!IsReady)
            {
                Debug.LogWarning("[PlayerController] Cannot attack - system not ready!");
                return;
            }
            if (currentState is DashState || currentState is AttackState) return;
            SkillManager skillManager = GetComponent<SkillManager>();
            if (skillManager == null)
            {
                Debug.LogError("[PlayerController] ❌ SkillManager not found!");
                return;
            }
            if (selectedAttackSlot == 1)
            {
                if (skillManager.CanUseSkill1())
                {
                    attackState.SetupAttack("Attack_1", 1.0f);
                    TransitionToState(attackState);
                    skillManager.UseSkill1();
                }
                else
                {
                    Debug.Log("<color=red>[ATTACK 1] ⏳ Đang cooldown!</color>");
                }
            }
            else if (selectedAttackSlot == 2)
            {
                if (skillManager.CanUseSkill2())
                {
                    attackState.SetupAttack("Attack_2", 1.5f);
                    TransitionToState(attackState);
                    skillManager.UseSkill2();
                }
                else
                {
                    Debug.Log("<color=red>[ATTACK 2] ⏳ Đang cooldown!</color>");
                }
            }
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            if (!IsReady)
            {
                Debug.LogWarning("[PlayerController] Cannot dash - system not ready!");
                return;
            }

            SkillManager skillManager = GetComponent<SkillManager>();

            if (skillManager == null)
            {
                Debug.LogError("[PlayerController] ❌ SkillManager not found!");
                return;
            }

            bool isMovingHorizontally = Mathf.Abs(moveInput.x) > 0.1f;

            if (isMovingHorizontally && currentState is not DashState && currentState is not AttackState)
            {
                if (skillManager.CanDash())
                {
                    TransitionToState(dashState);
                }
                else
                {
                    Debug.Log("<color=orange>[Dash] ⏳ Cooldown hoặc không đủ Mana!</color>");
                }
            }
        }
    }

    public void OnSwitchCharacter(InputValue value)
    {
        if (value.isPressed)
        {
            if (!IsReady)
            {
                Debug.LogWarning("[PlayerController] Cannot switch - system not ready!");
                return;
            }

            if (currentState is DashState || currentState is AttackState)
            {
                Debug.Log("<color=orange>[SwitchCharacter] ⚠️ Cannot switch during Dash or Attack!</color>");
                return;
            }

            CharacterPrefabManager manager = FindAnyObjectByType<CharacterPrefabManager>();

            if (manager == null)
            {
                Debug.LogError("[PlayerController] ❌ CharacterPrefabManager not found in scene!");
                return;
            }

            Debug.Log("<color=cyan>[PlayerController] 🔄 Switching character...</color>");
            manager.NextCharacter();
        }
    }

    private void HandleJumpInput()
    {
        if (jumpPressed)
        {
            jumpPressed = false;
            if (currentState is IdleState || currentState is RunState)
            {
                TransitionToState(jumpState);
            }
        }
    }

    #endregion

    #region Helper Methods

    public void CheckGrounded()
    {
        // ✅ groundCheck is validated in Start()
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, playerData.groundCheckRadius, groundLayer);
    }

    public void Flip()
    {
        if (moveInput.x > 0.1f)
        {
            facingDirection = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput.x < -0.1f)
        {
            facingDirection = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    public void ApplyVariableGravity()
    {
        if (rb.linearVelocity.y < -0.01f)
        {
            rb.gravityScale = playerData.fallGravity;
        }
        else if (rb.linearVelocity.y > 0.01f)
        {
            rb.gravityScale = playerData.jumpGravity;
        }
        else
        {
            rb.gravityScale = playerData.normalGravity;
        }
    }

    // ✅ DIRECT CALLS: No AnimationBridge needed
    public void AnimEvent_EnableCancel()
    {
        if (currentState is AttackState attack) attack.EnableAttackCancel();
    }

    public void AnimEvent_FinishAttack()
    {
        if (currentState is AttackState attack) attack.FinishAttack();
    }
    public void AnimEvent_TriggerAttack()
    {
        if (currentState is AttackState attack)
        {
            attack.ExecuteAttackHit();
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (groundCheck != null && playerData != null)
        {
            Gizmos.DrawWireSphere(groundCheck.position, playerData.groundCheckRadius);
        }

        // Vẽ vùng tấn công (Box) màu vàng
        if (attackPoint != null && playerData != null)
        {
            Gizmos.color = Color.yellow;
            Vector2 boxSize = new Vector2(playerData.attackRange * 2f, playerData.attackRange * 2f);
            Gizmos.DrawWireCube(attackPoint.position, boxSize);
        }
    }
}