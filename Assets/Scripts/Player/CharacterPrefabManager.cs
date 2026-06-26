using UnityEngine;

public class CharacterPrefabManager : MonoBehaviour
{
    [SerializeField] private CharacterPrefabEntry[] characterPrefabs = new CharacterPrefabEntry[5];
    
    private int currentCharacterIndex = 0;
    private GameObject currentCharacterInstance;
    private Transform spawnPoint;

    // Events
    public delegate void OnCharacterSwapped(GameObject newCharacter, PlayerData data);
    public static OnCharacterSwapped onCharacterChanged;

    private void Start()
    {
        spawnPoint = transform;
        
        if (characterPrefabs != null && characterPrefabs.Length > 0 && characterPrefabs[0].prefab != null)
        {
            SpawnCharacter(0);
        }
        else
        {
            Debug.LogError("[CharacterPrefabManager] ❌ Character Prefabs array is empty or first prefab is null!");
        }
    }

    public void NextCharacter()
    {
        if (characterPrefabs == null || characterPrefabs.Length <= 1) 
        {
            Debug.LogWarning("[CharacterPrefabManager] ⚠️ No characters to switch to");
            return;
        }
        
        int nextIndex = (currentCharacterIndex + 1) % characterPrefabs.Length;
        SpawnCharacter(nextIndex);
    }

    public void PreviousCharacter()
    {
        if (characterPrefabs == null || characterPrefabs.Length <= 1) 
        {
            Debug.LogWarning("[CharacterPrefabManager] ⚠️ No characters to switch to");
            return;
        }
        
        int prevIndex = (currentCharacterIndex - 1 + characterPrefabs.Length) % characterPrefabs.Length;
        SpawnCharacter(prevIndex);
    }

    public void SelectCharacter(int index)
    {
        if (characterPrefabs == null || index < 0 || index >= characterPrefabs.Length) 
        {
            Debug.LogError($"[CharacterPrefabManager] ❌ Invalid character index: {index}");
            return;
        }
        SpawnCharacter(index);
    }

    private void SpawnCharacter(int index)
    {
        if (currentCharacterInstance != null)
        {
            // 🔧 FIX: Deselect dalam editor sebelum destroy
            #if UNITY_EDITOR
            if (UnityEditor.Selection.activeGameObject == currentCharacterInstance)
            {
                UnityEditor.Selection.activeGameObject = null;
                Debug.Log("[CharacterPrefabManager] 🔓 Deselected current character");
            }
            #endif

            Destroy(currentCharacterInstance);
            Debug.Log("[CharacterPrefabManager] 🗑️ Destroyed previous character instance");
        }

        currentCharacterIndex = index;
        CharacterPrefabEntry entry = characterPrefabs[index];

        if (entry.prefab == null)
        {
            Debug.LogError($"[CharacterPrefabManager] ❌ Prefab at index {index} is null!");
            return;
        }

        currentCharacterInstance = Instantiate(
            entry.prefab,
            spawnPoint.position,
            Quaternion.identity,
            spawnPoint 
        );

        if (currentCharacterInstance == null)
        {
            Debug.LogError("[CharacterPrefabManager] ❌ Failed to instantiate character!");
            return;
        }

        // 🔧 CRITICAL FIX: Get components and setup data
        PlayerController controller = currentCharacterInstance.GetComponent<PlayerController>();
        if (controller == null)
        {
            Debug.LogError("[CharacterPrefabManager] ❌ PlayerController not found on prefab!");
            Destroy(currentCharacterInstance);
            return;
        }

        // 🔧 Verify groundCheck is assigned in prefab
        if (controller.groundCheck == null)
        {
            Debug.LogError("[CharacterPrefabManager] ❌ CRITICAL: groundCheck not assigned in prefab!");
            Debug.LogError("[CharacterPrefabManager] ❌ Please assign groundCheck Transform in PlayerController!");
            Destroy(currentCharacterInstance);
            return;
        }

        // 🔧 NEW: Call OnPlayerDataAssigned to properly initialize
        if (entry.playerData != null)
        {
            controller.OnPlayerDataAssigned(entry.playerData);
        }
        else
        {
            Debug.LogError($"[CharacterPrefabManager] ❌ playerData at index {index} is null!");
            Destroy(currentCharacterInstance);
            return;
        }

        // Update PlayerStats if present
        PlayerStats stats = controller.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.UpdateCharacterData(entry.playerData);
            Debug.Log($"<color=cyan>📊 Updated Stats: HP={entry.playerData.maxHP}, Mana={entry.playerData.maxMana}</color>");
        }
        else
        {
            Debug.LogWarning("[CharacterPrefabManager] ⚠️ PlayerStats not found on prefab!");
        }

        // Verify other critical components
        Rigidbody2D rb = currentCharacterInstance.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[CharacterPrefabManager] ❌ Rigidbody2D not found on prefab!");
        }

        Animator anim = currentCharacterInstance.GetComponentInChildren<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("[CharacterPrefabManager] ⚠️ Animator not found on prefab!");
        }

        Debug.Log($"<color=cyan>✅ [CharacterPrefabManager] Spawned: {entry.characterName} (Index: {index})</color>");
        
        onCharacterChanged?.Invoke(currentCharacterInstance, entry.playerData);
    }

    // ========== GETTER METHODS ==========
    public GameObject GetCurrentCharacter() => currentCharacterInstance;
    public int GetCurrentCharacterIndex() => currentCharacterIndex;
    public string GetCurrentCharacterName() => characterPrefabs[currentCharacterIndex].characterName;
    public PlayerData GetCurrentPlayerData() => characterPrefabs[currentCharacterIndex].playerData;
}