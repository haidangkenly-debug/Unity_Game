using System.Collections;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    [Header("Cài đặt Bóng mờ")]
    [Tooltip("Kéo thả GhostPrefab từ cửa sổ Project vào đây")]
    public GameObject ghostPrefab; 
    
    [Tooltip("Khoảng thời gian sinh ra từng cái bóng (giây)")]
    public float ghostDelay = 0.05f; 
    
    [Tooltip("Màu của tàn ảnh (Mặc định: Trắng hơi trong suốt)")]
    public Color ghostColor = new Color(1f, 1f, 1f, 0.6f); 

    private bool isGhosting;
    private SpriteRenderer playerSpriteRenderer;

    void Start()
    {
        // Lấy SpriteRenderer của nhân vật chính để copy hình ảnh
        playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (playerSpriteRenderer == null)
        {
            Debug.LogError("Không tìm thấy SpriteRenderer ở object con!");
        }
    }
    // Hàm này được gọi trong DashState.Enter()
    public void StartGhosting()
    {
        if (!isGhosting)
        {
            isGhosting = true;
            StartCoroutine(SpawnGhosts());
        }
    }
    // Hàm này được gọi trong DashState.Exit()
    public void StopGhosting()
    {
        isGhosting = false;
    }
    private IEnumerator SpawnGhosts()
    {
        while (isGhosting)
        {
            // 1. Sinh ra một cái bóng tại vị trí hiện tại của Player
            GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);     
            // 2. Lấy SpriteRenderer của bóng
            SpriteRenderer ghostSprite = ghost.GetComponent<SpriteRenderer>();    
            // 3. Chép khung hình (sprite) hiện tại của Player sang bóng
            ghostSprite.sprite = playerSpriteRenderer.sprite;   
            // Đảm bảo bóng quay đúng hướng với Player (nếu bạn dùng flipX để quay mặt)
            ghostSprite.flipX = playerSpriteRenderer.flipX;    
            // Đảm bảo scale (kích thước) cũng giống Player (nếu bạn dùng scale.x để quay mặt)
            ghost.transform.localScale = transform.localScale;    
            // 4. Đổi màu cái bóng
            ghostSprite.color = ghostColor;
            // 5. Chỉnh Order in Layer để cái bóng luôn nằm DƯỚI nhân vật
            ghostSprite.sortingLayerID = playerSpriteRenderer.sortingLayerID;
            ghostSprite.sortingOrder = playerSpriteRenderer.sortingOrder - 1;
            // 6. Đợi một khoảng thời gian (ghostDelay) trước khi sinh bóng tiếp theo
            yield return new WaitForSeconds(ghostDelay);
        }
    }
}