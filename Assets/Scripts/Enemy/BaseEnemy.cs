using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHP = 50f;
    protected float currentHP;
    public float moveSpeed = 2f;

    [Header("Health Regen")]
    public float regenAmount = 5f;        // Lượng máu hồi phục mỗi giây
    public float regenDelay = 3f;         // Thời gian chờ (giây) trước khi bắt đầu hồi phục
    private Coroutine regenCoroutine;
    
    [Header("References")]
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer sr;

    [Header("UI")]
    public Image hpFillImage;
    public GameObject hpBarObject;        // Kéo thả GameObject "HpBar" (Canvas cha của thanh máu) vào đây

    [Header("Combat Physics")]
    public Transform attackPoint;
    public Vector2 attackSize = new Vector2(1.2f, 1.2f); 
    public LayerMask playerLayer;

    [Header("Platformer Checks")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public Transform ledgeCheck;

    protected Transform player;
    protected bool isDead = false;
    protected bool isGrounded;
    protected bool isNearLedge;

    protected virtual void Awake()
    {
        currentHP = maxHP;
        UpdateHealthBar();
        SetHpBarVisible(false); // Mặc định ẩn thanh máu khi mới sinh ra
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected virtual void OnEnable()
    {
        CharacterPrefabManager.onCharacterChanged += UpdatePlayerTarget;
    }

    protected virtual void OnDisable()
    {
        CharacterPrefabManager.onCharacterChanged -= UpdatePlayerTarget;
    }

    private void UpdatePlayerTarget(GameObject newCharacter, PlayerData data)
    {
        if (newCharacter != null)
        {
            player = newCharacter.transform;
            Debug.Log($"<color=orange>[{gameObject.name}] Đã cập nhật mục tiêu mới: {newCharacter.name}</color>");
        }
    }

    protected virtual void Update()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        
        if (ledgeCheck != null)
        {
            isNearLedge = !Physics2D.Raycast(ledgeCheck.position, Vector2.down, 0.5f, groundLayer);
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHP -= damage;
        UpdateHealthBar();
        
        // Khi bị đánh trúng, bắt buộc phải hiện thanh máu ngay lập tức
        SetHpBarVisible(true);

        // Quản lý cơ chế đếm ngược hồi máu
        if (regenCoroutine != null) StopCoroutine(regenCoroutine);
        if (!isDead) regenCoroutine = StartCoroutine(RegenHealthRoutine());

        StartCoroutine(FlashRed());
        
        if (currentHP <= 0) Die();
    }

    private void UpdateHealthBar()
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = currentHP / maxHP;
        }
    }

    // Hàm ẩn/hiện thanh HP công khai để lớp con gọi sử dụng
    public void SetHpBarVisible(bool visible)
    {
        if (hpBarObject != null && hpBarObject.activeSelf != visible)
        {
            hpBarObject.SetActive(visible);
        }
    }

    // Coroutine tự động phục hồi máu sau khoảng thời gian trì hoãn
    private IEnumerator RegenHealthRoutine()
    {
        yield return new WaitForSeconds(regenDelay);

        while (currentHP < maxHP && !isDead)
        {
            currentHP += regenAmount * Time.deltaTime;
            if (currentHP > maxHP) currentHP = maxHP;
            
            UpdateHealthBar();
            yield return null;
        }
    }

    private IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = Color.white;
    }

    protected virtual void Die()
    {
        isDead = true;
        
        if (regenCoroutine != null) StopCoroutine(regenCoroutine); 
        
        SetHpBarVisible(false); 

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (anim != null) anim.SetTrigger("Die");
        
        Destroy(gameObject, 1f); 
    }

    protected void Flip(float direction)
    {
        if (direction > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (direction < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
    }

    protected virtual void OnDrawGizmos() 
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackSize); 
        }
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + Vector3.down * 0.5f);
        }
    }
}