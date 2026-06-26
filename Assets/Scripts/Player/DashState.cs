using UnityEngine;

public class DashState : PlayerState
{
    private float dashTimer;

    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);
    }

    public override void Enter()
    {
        dashTimer = playerData.dashDuration;
        
        animator.SetBool("isDashing", true);
        skillManager.UseDash();
        
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(playerData.dashSpeed * playerController.facingDirection, 0f);
    }

    public override void Update()
    {
        dashTimer -= Time.deltaTime;

        if (dashTimer > 0.1f) 
        {
            float moveInput = playerController.moveInput.x;
            if (Mathf.Abs(moveInput) > 0.1f && (Mathf.Sign(moveInput) != playerController.facingDirection))
            {
                dashTimer = 0; 
            }
        }

        if (dashTimer <= 0)
        {
            ReturnToNormalState();
        }
    }

    public override void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(playerData.dashSpeed * playerController.facingDirection, 0f);
    }

    public override void Exit()
    {
        animator.SetBool("isDashing", false);
        rb.gravityScale = playerData.normalGravity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);
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
}