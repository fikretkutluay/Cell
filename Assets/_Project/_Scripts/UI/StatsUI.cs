using UnityEngine;
using UnityEngine.UI; // Image için gerekli
using TMPro;

public class StatsUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Health UI")]
    [SerializeField] private Image healthBarImage; // Slider yerine Image (Filled)
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Stats UI")]
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI fireRateText;

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthUI;
            playerStats.OnStatsChanged += UpdateStatsUI;

            // Baþlangýçta güncelle
            UpdateHealthUI(playerStats.currentHealth, playerStats.maxHealth);
            UpdateStatsUI();
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthUI;
            playerStats.OnStatsChanged -= UpdateStatsUI;
        }
    }

    private void UpdateHealthUI(float current, float max)
    {
        // --- DEÐÝÞÝKLÝK BURADA ---
        if (healthBarImage != null)
        {
            // Fill Amount her zaman 0 ile 1 arasýnda olmalýdýr.
            // Örnek: 80 can / 100 max = 0.8 (%80 dolu)
            healthBarImage.fillAmount = current / max;
        }
        // -------------------------

        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(current)} / {max}";
        }
    }

    private void UpdateStatsUI()
    {
        if (damageText != null) damageText.text = $"DMG: {playerStats.currentDamage:F1}";
        if (speedText != null) speedText.text = $"SPD: {playerStats.currentMoveSpeed:F1}";
        if (fireRateText != null) fireRateText.text = $"FRT: {playerStats.currentFireRate:F2}";
    }
}