using UnityEngine;
using UnityEngine.InputSystem;

public class AttackState : PlayerState
{
    private string skillName;
    private bool canCancelAttack;

    public void SetupAttack(string skill)
    {
        skillName = skill;
    }

    public override void Enter()
    {
        canCancelAttack = false;
        animator.SetTrigger(skillName);
        rb.gravityScale = playerData.normalGravity;
        Debug.Log($"<color=orange>[AttackState] ⚔️ Bắt đầu: {skillName}</color>");
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