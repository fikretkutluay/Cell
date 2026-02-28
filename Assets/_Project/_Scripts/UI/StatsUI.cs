using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class StatsUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerStats playerStats;

    [Header("HealthUI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI fireRateText;

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthUI;
            playerStats.OnStatsChanged += UpdateStatsUI;

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
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText != null)
        {
            healthText.text = $"{current:0} / {max:0}";
        }
    }
    private void UpdateStatsUI()
    {
        if (damageText != null) damageText.text = $"DMG: {playerStats.currentDamage}";
        if (speedText != null) speedText.text = $"SPD: {playerStats.currentMoveSpeed}";
        if (fireRateText != null) fireRateText.text = $"FRT: {playerStats.currentFireRate}";
    }

 
}
