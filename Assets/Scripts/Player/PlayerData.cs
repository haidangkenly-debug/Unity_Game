using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Character Info")]
    public string characterName;
    public Sprite characterIcon;

    [Header("Movement")]
    public float runSpeed = 8f;
    public float jumpForce = 20f;
    public float jumpMultiplier = 0.5f;
    public float normalGravity = 4f;
    public float fallGravity = 5f;
    public float jumpGravity = 3f;

    [Header("Base Stats")]
    public float maxHP = 100f;
    public float maxMana = 100f;
    public float attackPower = 10f;
    public float critChance = 0.1f;        // 10%
    public float critDamage = 1.5f;        // 150%

    [Header("Dash")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 1f;
    public float dashManaCost = 20f;

    [Header("Skill_1 (Attack_1)")]
    public string skill1Name = "Attack_1";
    public float skill1Damage = 15f;
    public float skill1Cooldown = 0.33f;

    [Header("Skill_2 (Attack_2)")]
    public string skill2Name = "Attack_2";
    public float skill2Damage = 20f;
    public float skill2Cooldown = 1.5f;
    [Header("Attack Settings")]
    public float attackRange = 0.8f;      // Bán kính vùng chém
    public LayerMask enemyLayer;          // Tick chọn layer "Enemy" trong Unity
    [Header("Ground Check")]
    public float groundCheckRadius = 0.2f;
}