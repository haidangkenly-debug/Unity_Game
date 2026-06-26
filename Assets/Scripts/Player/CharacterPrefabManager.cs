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

    // ✅ REFACTORED: Main spawn logic (simplified)
    private void SpawnCharacter(int index)
    {
        DestroyCurrentCharacter();

        currentCharacterIndex = index;
        CharacterPrefabEntry entry = characterPrefabs[index];

        if (!ValidateEntry(entry)) return;

        // Instantiate new character
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

        SetupCharacter(currentCharacterInstance, entry);

        Debug.Log($"<color=cyan>✅ [CharacterPrefabManager] Spawned: {entry.characterName} (Index: {index})</color>");
        onCharacterChanged?.Invoke(currentCharacterInstance, entry.playerData);
    }

    // ✅ HELPER METHOD 1: Cleanup
    private void DestroyCurrentCharacter()
    {
        if (currentCharacterInstance == null) return;

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

    // ✅ HELPER METHOD 2: Validation
    private bool ValidateEntry(CharacterPrefabEntry entry)
    {
        if (entry == null || entry.prefab == null)
        {
            Debug.LogError("[CharacterPrefabManager] ❌ Invalid prefab entry!");
            return false;
        }

        if (entry.playerData == null)
        {
            Debug.LogError("[CharacterPrefabManager] ❌ playerData is null!");
            return false;
        }

        return true;
    }

    // ✅ HELPER METHOD 3: Setup components
    private void SetupCharacter(GameObject instance, CharacterPrefabEntry entry)
    {
        var controller = instance.GetComponent<PlayerController>();
        var stats = instance.GetComponent<PlayerStats>();
        var rb = instance.GetComponent<Rigidbody2D>();
        var anim = instance.GetComponentInChildren<Animator>();

        // Validate critical components
        if (controller == null)
        {
            Debug.LogError("[CharacterPrefabManager] ❌ PlayerController not found on prefab!");
            Destroy(instance);
            return;
        }

        if (controller.groundCheck == null)
        {
            Debug.LogError("[CharacterPrefabManager] ❌ groundCheck not assigned in prefab!");
            Destroy(instance);
            return;
        }

        if (rb == null)
        {
            Debug.LogError("[CharacterPrefabManager] ❌ Rigidbody2D not found on prefab!");
            Destroy(instance);
            return;
        }

        // Initialize PlayerController with data
        controller.OnPlayerDataAssigned(entry.playerData);

        // Update stats if present
        if (stats != null)
        {
            stats.UpdateCharacterData(entry.playerData);
            Debug.Log($"<color=cyan>📊 Updated Stats: HP={entry.playerData.maxHP}, Mana={entry.playerData.maxMana}</color>");
        }
        else
        {
            Debug.LogWarning("[CharacterPrefabManager] ⚠️ PlayerStats not found on prefab!");
        }

        // Warn if Animator missing
        if (anim == null)
        {
            Debug.LogWarning("[CharacterPrefabManager] ⚠️ Animator not found on prefab!");
        }
    }

    // ========== GETTER METHODS ==========
    public GameObject GetCurrentCharacter() => currentCharacterInstance;
    public int GetCurrentCharacterIndex() => currentCharacterIndex;
    public string GetCurrentCharacterName() => characterPrefabs[currentCharacterIndex].characterName;
    public PlayerData GetCurrentPlayerData() => characterPrefabs[currentCharacterIndex].playerData;
}