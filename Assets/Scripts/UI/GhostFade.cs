using UnityEngine;

public class GhostFade : MonoBehaviour
{
    [Tooltip("Tốc độ mờ dần của bóng (càng lớn mờ càng nhanh)")]
    public float fadeSpeed = 3f; 
    
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        Color currentColor = spriteRenderer.color;      
        currentColor.a -= fadeSpeed * Time.deltaTime;
        spriteRenderer.color = currentColor;
        if (currentColor.a <= 0f)
        {
            Destroy(gameObject);
        }
    }
}