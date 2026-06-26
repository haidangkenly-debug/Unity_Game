using UnityEngine;

public abstract class PlayerState 
{
    protected PlayerController playerController;
    protected Animator animator;
    protected Rigidbody2D rb;
    protected PlayerData playerData;
    protected PlayerStats playerStats;
    protected SkillManager skillManager;

    public virtual void Initialize(PlayerController controller)
    {
        playerController = controller;
        animator = controller.anim;
        rb = controller.rb;
        playerData = controller.playerData;
        playerStats = controller.GetComponent<PlayerStats>();
        skillManager = controller.GetComponent<SkillManager>();
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }

    public virtual void CaptureMoveInput(Vector2 input)
    {
        playerController.moveInput = input;
    }

    protected void UpdateAnimationState()
    {
        animator.SetBool("isGrounded", playerController.isGrounded);
        animator.SetBool("isIdle", Mathf.Abs(playerController.moveInput.x) < 0.1f && playerController.isGrounded);
        animator.SetBool("isRunning", Mathf.Abs(playerController.moveInput.x) > 0.1f && playerController.isGrounded);
        animator.SetBool("isJumping", rb.linearVelocity.y > 0.1f);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    protected void Move(float speed)
    {
        rb.linearVelocity = new Vector2(speed * playerController.moveInput.x, rb.linearVelocity.y);
    }
    
}