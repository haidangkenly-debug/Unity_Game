using UnityEngine;

public class JumpState : PlayerState
{
    private bool jumpReleased = false;

    public override void Enter()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, playerData.jumpForce);
        jumpReleased = false;
    }

    public override void Update()
    {
        UpdateAnimationState();
    }

    public override void FixedUpdate()
    {
        Move(playerData.runSpeed);
        if (!jumpReleased && rb.linearVelocity.y <= 0)
        {
            jumpReleased = true;
            playerController.TransitionToState(playerController.fallState);
        }
    }

    public override void Exit()
    {
        jumpReleased = false;
    }
}

public class FallState : PlayerState
{
    public override void Update()
    {
        UpdateAnimationState();
    }

    public override void FixedUpdate()
    {
        Move(playerData.runSpeed);

        if (playerController.isGrounded)
        {
            if (Mathf.Abs(playerController.moveInput.x) < 0.1f)
            {
                playerController.TransitionToState(playerController.idleState);
            }
            else
            {
                playerController.TransitionToState(playerController.runState);
            }
        }
    }
}
