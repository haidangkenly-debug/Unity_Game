using UnityEngine;

public class IdleState : PlayerState
{
    public override void Update()
    {
        UpdateAnimationState();

        if (Mathf.Abs(playerController.moveInput.x) > 0.1f && playerController.isGrounded)
        {
            playerController.TransitionToState(playerController.runState);
        }
    }

    public override void FixedUpdate()
    {
        Move(0);
        if (!playerController.isGrounded && rb.linearVelocity.y < -0.1f)
        {
            playerController.TransitionToState(playerController.fallState);
        }
    }
}

public class RunState : PlayerState
{
    public override void Update()
    {
        UpdateAnimationState();

        if (Mathf.Abs(playerController.moveInput.x) < 0.1f && playerController.isGrounded)
        {
            playerController.TransitionToState(playerController.idleState);
        }
    }

    public override void FixedUpdate()
    {
        Move(playerData.runSpeed);

        // Fall transition
        if (!playerController.isGrounded && rb.linearVelocity.y < -0.1f)
        {
            playerController.TransitionToState(playerController.fallState);
        }
    }
}
