using UnityEngine;

public class EnemyAI : BaseEnemy
{
    public enum State { Patrol, Chase, Attack, Idle }
    public State currentState = State.Patrol;

    [Header("AI & Territory Settings")]
    public float territoryRadius = 5f; 
    public float detectRange = 4f;
    public float attackRange = 1f; 
    public float attackCooldown = 1.5f;
    
    [Header("Idle Settings")]
    public float idleDuration = 2f; 
    private float idleTimer;
    private float lastAttackTime;
    private Vector2 startPosition;
    private float patrolDirection = 1f; 

    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position; 
    }

    protected override void Update()
    {
        base.Update(); 
        
        if (isDead || player == null || !isGrounded) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        bool isPlayerInTerritory = Vector2.Distance(startPosition, player.position) <= territoryRadius;

        // XỬ LÝ ẨN/HIỆN THANH HP THEO LÃNH THỔ
        if (isPlayerInTerritory || currentState == State.Chase || currentState == State.Attack)
        {
            SetHpBarVisible(true);
        }
        else
        {
            // Chỉ ẩn đi khi Player đã rời khỏi vùng lãnh thổ VÀ quái đã tự hồi đầy máu
            if (currentHP >= maxHP)
            {
                SetHpBarVisible(false);
            }
        }

        // CHUYỂN ĐỔI TRẠNG THÁI (STATE MACHINE)
        if (distToPlayer <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (isPlayerInTerritory) 
        {
            currentState = State.Chase;
        }
        else if (currentState != State.Idle)
        {
            currentState = State.Patrol;
        }

        // THỰC THI HÀNH ĐỘNG
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                ChasePlayer();
                break;
            case State.Attack:
                TryAttack();
                break;
            case State.Idle:
                Idle();
                break;
        }
    }

    private void Patrol()
    {
        float distFromStart = transform.position.x - startPosition.x;
        
        bool hitRightBoundary = (distFromStart >= territoryRadius && patrolDirection > 0);
        bool hitLeftBoundary = (distFromStart <= -territoryRadius && patrolDirection < 0);

        if (hitRightBoundary || hitLeftBoundary || isNearLedge)
        {
            currentState = State.Idle;
            idleTimer = idleDuration; 
            anim.SetBool("isMoving", false);
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoveInDirection(patrolDirection);
    }

    private void Idle()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isMoving", false);

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            patrolDirection *= -1; 
            Flip(patrolDirection); 
            currentState = State.Patrol;
        }
    }

    private void ChasePlayer()
    {
        float dirToPlayer = Mathf.Sign(player.position.x - transform.position.x);

        if (isNearLedge && Mathf.Sign(transform.localScale.x) == dirToPlayer)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("isMoving", false);
            return;
        }

        MoveInDirection(dirToPlayer);
    }

    private void MoveInDirection(float dir)
    {
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
        Flip(dir);
        anim.SetBool("isMoving", true);
    }

    private void TryAttack()
    {
        rb.linearVelocity = Vector2.zero; 
        anim.SetBool("isMoving", false);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            anim.SetTrigger("Attack");
        }
    }

    // GỌI TỪ ANIMATION EVENT
    public void AnimEvent_AttackHit()
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPoint.position, attackSize, 0f, playerLayer);
        
        foreach (Collider2D hit in hits)
        {
            hit.SendMessage("TakeDamage", 10f, SendMessageOptions.DontRequireReceiver);
            Debug.Log("<color=red>Enemy chém trúng Player!</color>");
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.cyan;
        Vector2 drawStartPos = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Gizmos.DrawLine(drawStartPos + Vector2.left * territoryRadius, drawStartPos + Vector2.right * territoryRadius);
        Gizmos.DrawWireCube(drawStartPos, new Vector3(territoryRadius * 2, 0.5f, 0));

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}