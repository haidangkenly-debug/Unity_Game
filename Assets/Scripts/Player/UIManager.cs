using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private SkillManager skillManager;

    [Header("HP UI")]
    [SerializeField] private Image hpBarFill;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Mana UI")]
    [SerializeField] private Image manaBarFill;
    [SerializeField] private TextMeshProUGUI manaText;

    [Header("Skill Cooldowns")]
    [SerializeField] private Image skill1CooldownFill;
    [SerializeField] private TextMeshProUGUI skill1Text;
    [SerializeField] private Button skill1Button;

    [SerializeField] private Image skill2CooldownFill;
    [SerializeField] private TextMeshProUGUI skill2Text;
    [SerializeField] private Button skill2Button;

    [Header("Dash Cooldown")]
    [SerializeField] private Image dashCooldownFill;
    [SerializeField] private TextMeshProUGUI dashText;

    [Header("Character Info")]
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI statsText;

    private void Start()
    {
        // Subscribe to events
        playerStats.OnHPChanged += UpdateHPUI;
        playerStats.OnManaChanged += UpdateManaUI;
        skillManager.OnSkill1CooldownChanged += UpdateSkill1UI;
        skillManager.OnSkill2CooldownChanged += UpdateSkill2UI;
        skillManager.OnDashCooldownChanged += UpdateDashUI;

        // Initial update
        UpdateCharacterInfo();
        UpdateHPUI(playerStats.currentHP, playerController.playerData.maxHP);
        UpdateManaUI(playerStats.currentMana, playerController.playerData.maxMana);
    }

    private void Update()
    {
        // Update cooldown fills every frame
        skill1CooldownFill.fillAmount = skillManager.GetSkill1CooldownPercent();
        skill2CooldownFill.fillAmount = skillManager.GetSkill2CooldownPercent();
        dashCooldownFill.fillAmount = skillManager.GetDashCooldownPercent();
    }

    private void UpdateHPUI(float currentHP, float maxHP)
    {
        hpBarFill.fillAmount = currentHP / maxHP;
        hpText.text = $"HP: {currentHP:F0}/{maxHP:F0}";
    }

    private void UpdateManaUI(float currentMana, float maxMana)
    {
        manaBarFill.fillAmount = currentMana / maxMana;
        manaText.text = $"Mana: {currentMana:F0}/{maxMana:F0}";
    }

    private void UpdateSkill1UI(float cooldown, float maxCooldown)
    {
        if (cooldown <= 0)
        {
            skill1Text.text = $"[1] {playerController.playerData.skill1Name}\nReady";
            skill1Button.interactable = true;
        }
        else
        {
            skill1Text.text = $"[1] {playerController.playerData.skill1Name}\n{cooldown:F1}s";
            skill1Button.interactable = false;
        }
    }

    private void UpdateSkill2UI(float cooldown, float maxCooldown)
    {
        if (cooldown <= 0)
        {
            skill2Text.text = $"[2] {playerController.playerData.skill2Name}\nReady";
            skill2Button.interactable = true;
        }
        else
        {
            skill2Text.text = $"[2] {playerController.playerData.skill2Name}\n{cooldown:F1}s";
            skill2Button.interactable = false;
        }
    }

    private void UpdateDashUI(float cooldown, float maxCooldown)
    {
        if (cooldown <= 0)
        {
            dashText.text = $"[Shift] Dash\nReady";
        }
        else
        {
            dashText.text = $"[Shift] Dash\n{cooldown:F1}s";
        }
    }

    private void UpdateCharacterInfo()
    {
        PlayerData data = playerController.playerData;
        characterNameText.text = data.characterName;

        string statsInfo = $"ATK: {data.attackPower}\n" +
                         $"Crit: {data.critChance * 100:F0}%\n" +
                         $"Crit DMG: {data.critDamage * 100:F0}%";
        statsText.text = statsInfo;
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHPChanged -= UpdateHPUI;
            playerStats.OnManaChanged -= UpdateManaUI;
        }
        if (skillManager != null)
        {
            skillManager.OnSkill1CooldownChanged -= UpdateSkill1UI;
            skillManager.OnSkill2CooldownChanged -= UpdateSkill2UI;
            skillManager.OnDashCooldownChanged -= UpdateDashUI;
        }
    }
}
