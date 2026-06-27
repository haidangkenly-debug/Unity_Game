using UnityEngine;

public class CameraConfinerGenerator : MonoBehaviour
{
    [SerializeField] private GameObject backgroundContainer;
    [SerializeField] private PolygonCollider2D confiner;
    [SerializeField] private float padding = 0.5f;

    private void Start()
    {
        GenerateConfinerFromBackground();
    }

    private void GenerateConfinerFromBackground()
    {
        if (backgroundContainer == null || confiner == null)
        {
            Debug.LogError("[CameraConfinerGenerator] ❌ Background hoặc Confiner not assigned!");
            return;
        }

        Bounds bounds = GetChildrenBounds(backgroundContainer);

        if (bounds.size == Vector3.zero)
        {
            Debug.LogError("[CameraConfinerGenerator] ❌ Không tìm được background sprite!");
            return;
        }
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        Vector2[] points = new Vector2[]
        {
            new Vector2(min.x - padding, min.y - padding),
            new Vector2(max.x + padding, min.y - padding),
            new Vector2(max.x + padding, max.y + padding), 
            new Vector2(min.x - padding, max.y + padding)
        };

        // ✅ Gán vào Confiner
        confiner.points = points;

        Debug.Log($"<color=cyan>✅ [CameraConfinerGenerator] Generated confiner: {bounds.size}</color>");
        Debug.Log($"   Min: {min}, Max: {max}");
    }

    private Bounds GetChildrenBounds(GameObject parent)
    {
        Bounds bounds = new Bounds(parent.transform.position, Vector3.zero);
        bool foundRenderer = false;

        foreach (SpriteRenderer renderer in parent.GetComponentsInChildren<SpriteRenderer>())
        {
            if (!foundRenderer)
            {
                bounds = renderer.bounds;
                foundRenderer = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return bounds;
    }

    private void OnDrawGizmosSelected()
    {
        if (backgroundContainer == null) return;

        Bounds bounds = GetChildrenBounds(backgroundContainer);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}