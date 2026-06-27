using UnityEngine;
using UnityEngine.InputSystem;

public class AttackState : PlayerState
{
    private string skillName;
    private bool canCancelAttack;
    private float currentSkillDamageMultiplier = 1f;

    public void SetupAttack(string skill, float damageMultiplier = 1f)
    {
        skillName = skill;
        currentSkillDamageMultiplier = damageMultiplier;
    }

    public override void Enter()
    {
        canCancelAttack = false;
        animator.SetTrigger(skillName);
        rb.gravityScale = playerData.normalGravity;
        Debug.Log($"<color=orange>[AttackState] ⚔️ Bắt đầu: {skillName}</color>");
    }

    public void ExecuteAttackHit()
    {
        if (playerController.attackPoint == null) return;

        Vector2 boxSize = new Vector2(playerData.attackRange * 2f, playerData.attackRange * 2f);

        // Dùng OverlapBoxAll để quét vùng đánh hình chữ nhật
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
            playerController.attackPoint.position,
            boxSize,
            0f,
            playerController.attackLayer
        );

        // 🔥 KÍCH HOẠT RUNG CAMERA KHI CHÉM TRÚNG QUÁI
        if (hitEnemies.Length > 0)
        {
            CameraController cam = Object.FindAnyObjectByType<CameraController>();
            if (cam != null)
            {
                // Thời gian rung: 0.15 giây, Cường độ lệch: 0.15 đơn vị (Tốt cho game pixel)
                cam.Shake(0.15f, 0.15f);
            }
        }

        PlayerStats stats = playerController.GetComponent<PlayerStats>();
        float finalDamage = stats != null
            ? stats.CalculateDamage(playerData.attackPower * currentSkillDamageMultiplier)
            : playerData.attackPower * currentSkillDamageMultiplier;

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.SendMessage("TakeDamage", finalDamage, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"<color=red>💥 Chém trúng: {enemy.name} - Gây {finalDamage} DMG!</color>");
        }
    }

    public override void Update()
    {
        if (canCancelAttack)
        {
            bool isMoving = Mathf.Abs(playerController.moveInput.x) > 0.1f;

            if (isMoving)
            {
                ReturnToNormalState();
            }
        }
    }

    // Gọi từ animation event
    public void EnableAttackCancel()
    {
        canCancelAttack = true;
        Debug.Log("<color=cyan>[AttackState] 🔓 Có thể hủy tấn công</color>");
    }

    // Gọi từ animation event
    public void FinishAttack()
    {
        Debug.Log("<color=lime>[AttackState] ✅ Kết thúc tấn công</color>");
        ReturnToNormalState();
    }

    private void ReturnToNormalState()
    {
        if (playerController.isGrounded)
        {
            if (Mathf.Abs(playerController.moveInput.x) > 0.1f)
                playerController.TransitionToState(playerController.runState);
            else
                playerController.TransitionToState(playerController.idleState);
        }
        else
        {
            playerController.TransitionToState(playerController.fallState);
        }
    }

    public override void FixedUpdate()
    {
        Move(playerData.runSpeed);
    }
}