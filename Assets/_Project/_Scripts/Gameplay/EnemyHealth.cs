using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Vitality")]
    [SerializeField] private float maxHealth = 30f;

    [Header("Reward System")]
    [SerializeField] private LootTable lootTable;

    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public bool IsDead => currentHealth <= 0;

    private void Die()
    {
        if (lootTable != null)
        {
            GameObject itemToDrop = lootTable.GetRandomLoot();

            if (itemToDrop != null)
            {
                Instantiate(itemToDrop, transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }
}
