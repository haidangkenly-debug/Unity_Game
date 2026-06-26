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

    // 🔧 FIX: Track initialization state
    private bool isInitialized = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        playerInput = GetComponent<PlayerInput>();

        // 🔧 CRITICAL FIX: Check groundCheck assignment first!
        if (groundCheck == null)
        {
            Debug.LogError("[PlayerController] ❌ CRITICAL: groundCheck Transform is NOT assigned!");
            Debug.LogError("[PlayerController] ❌ Please assign groundCheck in the prefab's PlayerController component");
            enabled = false;  // Disable script to prevent further errors
            return;
        }

        // 🔧 FIX: Warn if playerData is null (will be set by CharacterPrefabManager)
        if (playerData == null)
        {
            Debug.LogWarning("[PlayerController] ⚠️ playerData is NULL (CharacterPrefabManager will assign it)");
            // Don't return - we'll initialize states anyway
            // CharacterPrefabManager will set playerData in SpawnCharacter()
        }

        // 🔧 FIX: Only initialize states if playerData is available
        if (playerData != null)
        {
            InitializeStates();
            TransitionToState(idleState);
            isInitialized = true;

            Debug.Log($"<color=green>✅ [PlayerController] Initialized for: {playerData.characterName}</color>");
        }
        else
        {
            Debug.Log("[PlayerController] 🕐 Waiting for playerData from CharacterPrefabManager...");
        }
    }

    private void InitializeStates()
    {
        if (playerData == null)
        {
            Debug.LogError("[PlayerController] Cannot initialize states - playerData is NULL!");
            return;
        }

        idleState = new IdleState();
        runState = new RunState();
        jumpState = new JumpState();
        fallState = new FallState();
        dashState = new DashState();
        attackState = new AttackState();

        foreach (var state in new PlayerState[] { idleState, runState, jumpState, fallState, dashState, attackState })
        {
            state.Initialize(this);
        }
    }

    private void Update()
    {
        // 🔧 FIX: Guard clause - ensure playerData and states exist
        if (playerData == null || currentState == null)
            return;

        if (currentState is not DashState)
        {
            Flip();
        }
        currentState?.Update();
    }

    private void FixedUpdate()
    {
        // 🔧 FIX: Guard clause for FixedUpdate
        if (playerData == null || groundCheck == null)
            return;

        CheckGrounded();

        if (currentState is not DashState)
        {
            ApplyVariableGravity();
        }

        currentState?.FixedUpdate();
        HandleJumpInput();
    }

    public void TransitionToState(PlayerState newState)
    {
        if (newState == null)
        {
            Debug.LogError("[PlayerController] Attempted to transition to NULL state!");
            return;
        }

        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    // 🔧 NEW: Public method for CharacterPrefabManager to call after setting playerData
    public void OnPlayerDataAssigned(PlayerData newData)
    {
        playerData = newData;

        if (!isInitialized && playerData != null)
        {
            InitializeStates();
            TransitionToState(idleState);
            isInitialized = true;

            Debug.Log($"<color=green>✅ [PlayerController] Initialized for: {playerData.characterName}</color>");
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
            if (playerData == null || currentState == null)
            {
                Debug.LogWarning("[PlayerController] Cannot attack - playerData or state not initialized!");
                return;
            }

            if (currentState is DashState || currentState is AttackState) return;

            SkillManager skillManager = GetComponent<SkillManager>();
            PlayerStats stats = GetComponent<PlayerStats>();

            if (skillManager == null || stats == null)
            {
                Debug.LogError("[PlayerController] ❌ SkillManager or PlayerStats not found!");
                return;
            }

            float baseDamage = playerData.attackPower;

            // ================= SLOT 1 (ATTACK_1) =================
            if (selectedAttackSlot == 1)
            {
                if (skillManager.CanUseSkill1())
                {
                    attackState.SetupAttack("Attack_1");
                    TransitionToState(attackState);
                    skillManager.UseSkill1();

                    float damageDealt = stats.CalculateDamage(baseDamage * 1.0f);
                    Debug.Log($"<color=green>[ATTACK 1] ✅ Gây: {damageDealt} DMG</color>");
                }
                else
                {
                    Debug.Log("<color=red>[ATTACK 1] ⏳ Đang cooldown!</color>");
                    return;
                }
            }
            // ================= SLOT 2 (ATTACK_2) =================
            else if (selectedAttackSlot == 2)
            {
                if (skillManager.CanUseSkill2())
                {
                    attackState.SetupAttack("Attack_2");
                    TransitionToState(attackState);
                    skillManager.UseSkill2();

                    float damageDealt = stats.CalculateDamage(baseDamage * 1.5f);
                    Debug.Log($"<color=green>[ATTACK 2] ✅ Gây: {damageDealt} DMG</color>");
                }
                else
                {
                    Debug.Log("<color=red>[ATTACK 2] ⏳ Đang cooldown!</color>");
                    return;
                }
            }
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            if (playerData == null || currentState == null)
            {
                Debug.LogWarning("[PlayerController] Cannot dash - playerData not initialized!");
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
            if (playerData == null || currentState == null)
            {
                Debug.LogWarning("[PlayerController] Cannot switch - playerData not initialized!");
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
        // 🔧 CRITICAL: groundCheck must not be null
        if (groundCheck == null)
        {
            Debug.LogError("[PlayerController] groundCheck is NULL in CheckGrounded()!");
            isGrounded = false;
            return;
        }

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
        if (playerData == null) return;

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

    public void AnimEvent_EnableCancel()
    {
        if (currentState is AttackState attack) attack.EnableAttackCancel();
    }

    public void AnimEvent_FinishAttack()
    {
        if (currentState is AttackState attack) attack.FinishAttack();
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (groundCheck != null && playerData != null)
        {
            Gizmos.DrawWireSphere(groundCheck.position, playerData.groundCheckRadius);
        }
    }
}