using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Platformer Camera Settings")]
    [SerializeField] private float lookAheadDistance = 3f;
    [SerializeField] private float lookAheadSmoothing = 0.15f;
    [SerializeField] private float velocityThreshold = 0.1f;  // ← THÊM: Ngưỡng velocity
    [SerializeField] private bool usePixelPerfectSnapping = true;
    [SerializeField] private float pixelsPerUnit = 16f;
    
    [Header("Background Bounds")]
    [SerializeField] private GameObject backgroundContainer;
    [SerializeField] private float padding = 0.5f;
    [Header("Wall Bounds (Invisible Camera Limits)")]
    [SerializeField] private BoxCollider2D leftWall;
    [SerializeField] private BoxCollider2D rightWall;
    
    private Transform targetTransform;
    private Vector3 targetPosition;
    private Camera mainCamera;
    private Vector3 snappedPos;
    private const float SNAP_THRESHOLD = 0.01f;
    
    private float targetFacingDirection = 1f;
    private float currentFacingDirection = 1f;
    private Bounds backgroundBounds;
    
    // ✅ THÊM: Cache Rigidbody2D để lấy velocity
    private Rigidbody2D characterRigidbody;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("[CameraController] ❌ Camera component not found!");
            enabled = false;
            return;
        }

        mainCamera.allowHDR = false;
        mainCamera.allowMSAA = false;

        if (backgroundContainer != null)
        {
            backgroundBounds = GetChildrenBounds(backgroundContainer);
        }

        CharacterPrefabManager.onCharacterChanged += OnCharacterChanged;
    }

    private void OnCharacterChanged(GameObject newCharacter, PlayerData data)
    {
        if (newCharacter == null)
        {
            Debug.LogError("[CameraController] ❌ New character is null!");
            return;
        }

        targetTransform = newCharacter.transform;
        
        // ✅ THÊM: Lấy Rigidbody2D của nhân vật mới
        characterRigidbody = newCharacter.GetComponent<Rigidbody2D>();
        
        SetCameraImmediate();
        
        Debug.Log($"📷 [CameraController] Following: {data.characterName}");
    }

    private void LateUpdate()
    {
        if (targetTransform == null) return;

        // ✅ THAY: Detect facing dựa vào VELOCITY
        DetectFacingDirectionByVelocity();

        // ✅ Smooth lerp facing direction
        currentFacingDirection = Mathf.Lerp(
            currentFacingDirection, 
            targetFacingDirection, 
            lookAheadSmoothing
        );

        // ✅ Tính target position với smooth look ahead
        Vector3 characterFeetPos = GetCharacterFeetPosition(targetTransform);
        Vector3 smoothLookAhead = Vector3.right * currentFacingDirection * lookAheadDistance;
        
        targetPosition = characterFeetPos + smoothLookAhead;
        targetPosition.z = -10f;

        transform.position = targetPosition;
        ClampCameraInBounds();

        if (usePixelPerfectSnapping)
        {
            SnapToPixelGrid();
        }
    }

    // ✅ THAY: Detect facing dựa vào velocity (không phải scale)
    private void DetectFacingDirectionByVelocity()
    {
        if (targetTransform == null) return;

        // ✅ Nếu có Rigidbody2D, dùng velocity
        if (characterRigidbody != null)
        {
            // ✅ Chỉ update direction khi velocity X > ngưỡng
            if (Mathf.Abs(characterRigidbody.linearVelocity.x) > velocityThreshold)
            {
                // Nhân vật đang di chuyển
                if (characterRigidbody.linearVelocity.x < 0)
                {
                    targetFacingDirection = -1f;  // Chạy sang trái
                }
                else if (characterRigidbody.linearVelocity.x > 0)
                {
                    targetFacingDirection = 1f;   // Chạy sang phải
                }
            }
            else
            {
                // ✅ Nhân vật đứng yên - fallback sang scale
                // Nhưng không update nếu vừa mới đứng lại
                // (giữ direction cuối cùng từ velocity)
            }
        }
        else
        {
            // ✅ Nếu không có Rigidbody2D, fallback sang scale (như cũ)
            if (targetTransform.localScale.x < 0)
            {
                targetFacingDirection = -1f;
            }
            else if (targetTransform.localScale.x > 0)
            {
                targetFacingDirection = 1f;
            }
        }
    }

    private Vector3 GetCharacterFeetPosition(Transform character)
    {
        SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            Bounds bounds = spriteRenderer.bounds;
            Vector3 feetPos = character.position;
            feetPos.y = character.position.y - (bounds.size.y / 2);
            return feetPos;
        }
        
        return character.position;
    }

   private void ClampCameraInBounds()
    {
        Vector3 pos = transform.position;
        float camHeight = mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect; 
        if (leftWall != null)
        {
            float minX = leftWall.bounds.max.x + camWidth;
            pos.x = Mathf.Max(pos.x, minX);
        }
        if (rightWall != null)
        {
            float maxX = rightWall.bounds.min.x - camWidth;
            pos.x = Mathf.Min(pos.x, maxX);
        }
        if (backgroundContainer != null)
        {
            backgroundBounds = GetChildrenBounds(backgroundContainer);
            float minY = backgroundBounds.min.y + camHeight - padding;
            float maxY = backgroundBounds.max.y - camHeight + padding;
            
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }
        transform.position = pos;
    }
    private void SnapToPixelGrid()
    {
        snappedPos = transform.position;
        
        float snappedX = Mathf.Round(snappedPos.x * pixelsPerUnit) / pixelsPerUnit;
        float snappedY = Mathf.Round(snappedPos.y * pixelsPerUnit) / pixelsPerUnit;
        
        if (Mathf.Abs(snappedPos.x - snappedX) > SNAP_THRESHOLD || 
            Mathf.Abs(snappedPos.y - snappedY) > SNAP_THRESHOLD)
        {
            snappedPos.x = snappedX;
            snappedPos.y = snappedY;
            transform.position = snappedPos;
        }
    }

    private void SetCameraImmediate()
    {
        if (targetTransform == null) return;

        Vector3 characterFeetPos = GetCharacterFeetPosition(targetTransform);
        transform.position = characterFeetPos;
    }

    private Bounds GetChildrenBounds(GameObject parent)
    {
        Bounds bounds = new Bounds(parent.transform.position, Vector3.zero);
        bool found = false;

        foreach (SpriteRenderer renderer in parent.GetComponentsInChildren<SpriteRenderer>())
        {
            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return bounds;
    }

    public void SetTarget(Transform target)
    {
        if (target == null) return;
        
        targetTransform = target;
        characterRigidbody = target.GetComponent<Rigidbody2D>();
        SetCameraImmediate();
    }

    private void OnDestroy()
    {
        CharacterPrefabManager.onCharacterChanged -= OnCharacterChanged;
    }

    private void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        
        if (Application.isPlaying && targetTransform != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 lookAheadPos = GetCharacterFeetPosition(targetTransform) + 
                                   Vector3.right * currentFacingDirection * lookAheadDistance;
            Gizmos.DrawLine(targetTransform.position, lookAheadPos);
            Gizmos.DrawWireCube(lookAheadPos, Vector3.one * 0.5f);
            
            // ✅ Debug: Vẽ velocity
            if (characterRigidbody != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(targetTransform.position, 
                               targetTransform.position + new Vector3(characterRigidbody.linearVelocity.x * 0.1f, 0, 0));
            }
        }
        #endif
    }
}