using System;
using UnityEngine;



public class StatPickup : MonoBehaviour
{
    public enum UpgradeType
    {
        Damage,
        MoveSpeed,
        FireRate,
        Heal,
        MaxHealth
    }

    [Header("Stats")]
    [SerializeField] private PlayerStats statsToUpgrade;
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private float amount = 1f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyUpgrade();

            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }

    private void ApplyUpgrade()
    {
        switch (upgradeType)
        {
            case UpgradeType.Damage:
                statsToUpgrade.UpgradeStat(amount, "Damage");
                break;
            case UpgradeType.MoveSpeed:
                statsToUpgrade.UpgradeStat(amount, "Speed");
                break;
            case UpgradeType.FireRate:
                statsToUpgrade.UpgradeStat(amount, "FireRate");
                break;
            case UpgradeType.Heal:
                statsToUpgrade.Heal(amount);
                break;
            case UpgradeType.MaxHealth:
                statsToUpgrade.IncreaseMaxHealth(amount);
                break;
        }
    }
}
