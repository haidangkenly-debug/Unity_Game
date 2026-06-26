using UnityEngine;
using System;

public class SkillManager : MonoBehaviour
{
    private PlayerData playerData;
    private PlayerStats playerStats;

    private float skill1Cooldown = 0f;
    private float skill2Cooldown = 0f;
    private float dashCooldown = 0f;

    // Events for UI
    public event Action<float, float> OnSkill1CooldownChanged;
    public event Action<float, float> OnSkill2CooldownChanged;
    public event Action<float, float> OnDashCooldownChanged;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    // 🔧 FIX: Di chuyển sang Start() để chắc chắn CharacterPrefabManager đã gán playerData
    private void Start()
    {
        PlayerController controller = GetComponent<PlayerController>();
        if (controller == null)
        {
            Debug.LogError("[SkillManager] ❌ PlayerController không tìm thấy!");
            return;
        }

        playerData = controller.playerData;

        if (playerData == null)
        {
            Debug.LogError("[SkillManager] ❌ playerData vẫn NULL! CharacterPrefabManager chưa gán?");
            return;
        }

        Debug.Log($"<color=cyan>[SkillManager] ✅ Khởi tạo thành công cho: {playerData.characterName}</color>");
    }

    private void Update()
    {
        // 🔧 FIX: Thêm null check để tránh NullReferenceException
        if (playerData == null) return;

        if (skill1Cooldown > 0)
        {
            skill1Cooldown -= Time.deltaTime;
            OnSkill1CooldownChanged?.Invoke(skill1Cooldown, playerData.skill1Cooldown);
        }

        if (skill2Cooldown > 0)
        {
            skill2Cooldown -= Time.deltaTime;
            OnSkill2CooldownChanged?.Invoke(skill2Cooldown, playerData.skill2Cooldown);
        }

        if (dashCooldown > 0)
        {
            dashCooldown -= Time.deltaTime;
            OnDashCooldownChanged?.Invoke(dashCooldown, playerData.dashCooldown);
        }
    }

    public bool CanUseSkill1()
    {
        if (playerData == null)
        {
            Debug.LogWarning("[SkillManager] playerData chưa được khởi tạo!");
            return false;
        }
        return skill1Cooldown <= 0;
    }

    public bool CanUseSkill2()
    {
        if (playerData == null)
        {
            Debug.LogWarning("[SkillManager] playerData chưa được khởi tạo!");
            return false;
        }
        return skill2Cooldown <= 0;
    }

    public bool CanDash()
    {
        if (playerData == null || playerStats == null)
        {
            Debug.LogWarning("[SkillManager] playerData hoặc playerStats chưa được khởi tạo!");
            return false;
        }
        return dashCooldown <= 0 && playerStats.CanUseMana(playerData.dashManaCost);
    }

    public void UseSkill1()
    {
        if (playerData == null) return;

        if (CanUseSkill1())
        {
            skill1Cooldown = playerData.skill1Cooldown;
            OnSkill1CooldownChanged?.Invoke(skill1Cooldown, playerData.skill1Cooldown);
            Debug.Log($"<color=yellow>[SkillManager] ⚔️ Dùng Skill 1 - Cooldown: {skill1Cooldown}s</color>");
        }
    }

    public void UseSkill2()
    {
        if (playerData == null) return;

        if (CanUseSkill2())
        {
            skill2Cooldown = playerData.skill2Cooldown;
            OnSkill2CooldownChanged?.Invoke(skill2Cooldown, playerData.skill2Cooldown);
            Debug.Log($"<color=yellow>[SkillManager] ⚔️ Dùng Skill 2 - Cooldown: {skill2Cooldown}s</color>");
        }
    }

    public void UseDash()
    {
        if (playerData == null || playerStats == null) return;

        if (CanDash())
        {
            playerStats.UseMana(playerData.dashManaCost);
            dashCooldown = playerData.dashCooldown;
            OnDashCooldownChanged?.Invoke(dashCooldown, playerData.dashCooldown);
            Debug.Log($"<color=cyan>[SkillManager] 💨 Dash được sử dụng - Cooldown: {dashCooldown}s</color>");
        }
    }

    public float GetSkill1CooldownPercent()
    {
        if (playerData == null) return 0f;
        return skill1Cooldown / playerData.skill1Cooldown;
    }

    public float GetSkill2CooldownPercent()
    {
        if (playerData == null) return 0f;
        return skill2Cooldown / playerData.skill2Cooldown;
    }

    public float GetDashCooldownPercent()
    {
        if (playerData == null) return 0f;
        return dashCooldown / playerData.dashCooldown;
    }
}