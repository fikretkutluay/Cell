using UnityEngine;
using System;
using UnityEditor.Overlays;
using System.IO;


[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "CellGame/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public event Action<float, float> OnHealthChanged;
    public event Action OnStatsChanged;

    [ContextMenu("Test: Give Player 10hp damage")]
    public void TestTakeDamage()
    {
        TakeDamage(50);
    }

    [ContextMenu("Test: Heal Player")]
    public void TestHeal()
    {
        Heal(10);
    }
    [Header("Temel Deðerler (Baþlangýç)")]
    public float baseMaxHealth = 100f;
    public float baseMoveSpeed = 5f;
    public float baseDamage = 10f;
    public float baseFireRate = 0.5f;

    [Header("Anlýk Deðerler (Oyun Ýçi)")]
    public float currentHealth;
    public float maxHealth;
    public float currentMoveSpeed;
    public float currentDamage;
    public float currentFireRate;

    // Oyun baþladýðýnda veya öldüðümüzde deðerleri sýfýrlamak için
    public void ResetValues()
    {
        maxHealth = baseMaxHealth;
        currentHealth = maxHealth;

        currentMoveSpeed = baseMoveSpeed;
        currentDamage = baseDamage;
        currentFireRate = baseFireRate;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStatsChanged?.Invoke();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if(currentHealth < 0) currentHealth = 0;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void UpgradeStat(float amount, string statType)
    {
        switch (statType)
        {
            case "Damage":
                currentDamage += amount;
                break;
            case "Speed":
                currentMoveSpeed += amount;
                break;
            case "FireRate":
                currentFireRate -= 0.05f;
                if (currentFireRate < 0.1f) currentFireRate = 0.1f;
                break;
        }

        OnStatsChanged?.Invoke();
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;

        currentHealth += amount;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

}