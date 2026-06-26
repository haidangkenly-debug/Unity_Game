using UnityEngine;
using System;

public class SkillManager : MonoBehaviour
{
    private PlayerData playerData;
    private PlayerStats playerStats;

    private float skill1Cooldown = 0f;
    private float skill2Cooldown = 0f;
    private float dashCooldown = 0f;

    // ✅ NEW: Track ready state
    private bool isReady = false;

    // Events for UI
    public event Action<float, float> OnSkill1CooldownChanged;
    public event Action<float, float> OnSkill2CooldownChanged;
    public event Action<float, float> OnDashCooldownChanged;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    // ✅ NEW: Called by PlayerController after data assignment
    public void InitializeSkillManager(PlayerData data)
    {
        playerData = data;
        isReady = (playerData != null && playerStats != null);
        
        if (isReady)
        {
            Debug.Log($"<color=cyan>[SkillManager] ✅ Khởi tạo thành công cho: {playerData.characterName}</color>");
        }
        else
        {
            Debug.LogError("[SkillManager] ❌ Failed to initialize - null references!");
        }
    }

    private void Update()
    {
        // ✅ SINGLE CHECK: One property check instead of 9 individual checks
        if (!isReady) return;

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

    // ✅ SIMPLIFIED: No null checks in methods
    public bool CanUseSkill1() => skill1Cooldown <= 0;
    public bool CanUseSkill2() => skill2Cooldown <= 0;
    
    public bool CanDash()
    {
        return dashCooldown <= 0 && playerStats.HasEnoughMana(playerData.dashManaCost);
    }

    public void UseSkill1()
    {
        if (CanUseSkill1())
        {
            skill1Cooldown = playerData.skill1Cooldown;
            OnSkill1CooldownChanged?.Invoke(skill1Cooldown, playerData.skill1Cooldown);
            Debug.Log($"<color=yellow>[SkillManager] ⚔️ Dùng Skill 1 - Cooldown: {skill1Cooldown}s</color>");
        }
    }

    public void UseSkill2()
    {
        if (CanUseSkill2())
        {
            skill2Cooldown = playerData.skill2Cooldown;
            OnSkill2CooldownChanged?.Invoke(skill2Cooldown, playerData.skill2Cooldown);
            Debug.Log($"<color=yellow>[SkillManager] ⚔️ Dùng Skill 2 - Cooldown: {skill2Cooldown}s</color>");
        }
    }

    // ✅ IMPROVED: Now just consume mana, don't check again
    public void UseDash()
    {
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
        return skill1Cooldown / playerData.skill1Cooldown;
    }

    public float GetSkill2CooldownPercent()
    {
        return skill2Cooldown / playerData.skill2Cooldown;
    }

    public float GetDashCooldownPercent()
    {
        return dashCooldown / playerData.dashCooldown;
    }
}