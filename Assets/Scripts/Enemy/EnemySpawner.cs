using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Cài đặt Spawn")]
    public GameObject enemyPrefab;    // Bản mẫu quái vật (Prefab)
    public float respawnDelay = 3f;   // Thời gian chờ để hồi sinh sau khi quái chết (giây)

    private GameObject spawnedEnemy;  // Biến lưu trữ con quái hiện tại do Spawner này tạo ra
    private bool isWaitingToRespawn = false;

    void Start()
    {
        // Khi game bắt đầu, đẻ con quái đầu tiên luôn
        SpawnNewEnemy();
    }

    void Update()
    {
        // Nếu đã từng đẻ quái, và hiện tại con quái đó KHÔNG CÒN TỒN TẠI (đã bị Destroy)
        // ĐỒNG THỜI Spawner chưa nằm trong trạng thái đợi hồi sinh
        if (spawnedEnemy == null && !isWaitingToRespawn)
        {
            // Kích hoạt tiến trình chờ 3 giây rồi đẻ quái mới
            StartCoroutine(RespawnRoutine());
        }
    }

    private void SpawnNewEnemy()
    {
        if (enemyPrefab == null) return;

        // Tạo quái mới tại đúng vị trí của cục Spawner này
        spawnedEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
    }

    // Tiến trình đếm ngược thời gian
    private IEnumerator RespawnRoutine()
    {
        isWaitingToRespawn = true; // Đánh dấu là đang trong chế độ chờ, tránh Update gọi trùng lặp

        yield return new WaitForSeconds(respawnDelay); // Chờ đúng số giây quy định (3 giây)

        SpawnNewEnemy(); // Hết thời gian thì đẻ quái mới

        isWaitingToRespawn = false; // Reset trạng thái để sẵn sàng cho lần chết tiếp theo
    }

    // Vẽ một hình tròn nhỏ màu hồng trong cửa sổ Scene để bạn dễ quản lý các điểm Spawn
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}