using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public PlayerData playerData;

    [Header("Current Stats")]
    public float currentHP;
    public float currentMana;

    // Events
    public event Action<float, float> OnHPChanged;
    public event Action<float, float> OnManaChanged;
    public event Action OnDied;

    private void Awake()
    {
        if (playerData != null)
        {
            currentHP = playerData.maxHP;
            currentMana = playerData.maxMana;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        OnHPChanged?.Invoke(currentHP, playerData.maxHP);

        if (currentHP <= 0)
        {
            currentHP = 0;
            OnDied?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, playerData.maxHP);
        OnHPChanged?.Invoke(currentHP, playerData.maxHP);
    }

    public void UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            OnManaChanged?.Invoke(currentMana, playerData.maxMana);
            Debug.Log($"<color=cyan>[PlayerStats] Đã dùng {amount} Mana. Mana còn lại: {currentMana}</color>");
            return;
        }
        Debug.LogWarning($"<color=red>[PlayerStats] KHÔNG ĐỦ MANA! Cần {amount}, nhưng chỉ có {currentMana}.</color>");
    }
    

    public void RestoreMana(float amount)
    {
        currentMana = Mathf.Min(currentMana + amount, playerData.maxMana);
        OnManaChanged?.Invoke(currentMana, playerData.maxMana);
    }

    public bool CanUseMana(float amount)
    {
        bool hasEnough = currentMana >= amount;
        if (!hasEnough)
        {
            // Bổ sung cảnh báo ngay tại hàm check để chắc chắn Console sẽ báo lỗi
            Debug.LogWarning($"<color=red>[PlayerStats] Từ chối dùng kỹ năng: Không đủ Mana! (Cần {amount})</color>");
        }
        return hasEnough;
    }

    public bool IsAlive()
    {
        return currentHP > 0;
    }

    public float CalculateDamage(float baseDamage)
    {
        float finalDamage = baseDamage;
        
        if (UnityEngine.Random.value < playerData.critChance)
        {
            finalDamage *= playerData.critDamage;
            Debug.Log($"<color=orange>[PlayerStats] CHÍ MẠNG!!! Sát thương nhân lên thành: {finalDamage}</color>");
        }
        else
        {
            Debug.Log($"<color=white>[PlayerStats] Đánh thường: Gây ra {finalDamage} sát thương.</color>");
        }
        
        return finalDamage;
    }

    public void UpdateCharacterData(PlayerData newData)
    {
        playerData = newData;
        
        // Cập nhật lại thanh máu, mana theo nhân vật mới
        currentHP = playerData.maxHP;
        currentMana = playerData.maxMana;

        OnHPChanged?.Invoke(currentHP, playerData.maxHP);
        OnManaChanged?.Invoke(currentMana, playerData.maxMana);
    }
}