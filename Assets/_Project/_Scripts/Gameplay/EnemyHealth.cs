using System;
using UnityEngine;

/// <summary>
/// Enemy'nin can sistemini yönetir
/// Hasar alır, ölür ve loot drop eder
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Vitality")]
    [SerializeField] private float maxHealth = 30f;

    [Header("Reward System")]
    [SerializeField] private LootTable lootTable;

    [Header("Death Settings")]
    [SerializeField] private float destroyDelay = 0.1f;

    private float currentHealth;
    private bool isDying = false;

    public bool IsDead => currentHealth <= 0;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        // Zaten ölüyorsa tekrar hasar alma
        if (isDying) return;

        currentHealth -= amount;
        Debug.Log($"Enemy took {amount} damage. Current health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDying) return;
        isDying = true;

        Debug.Log("Enemy died!");

        // Loot drop
        if (lootTable != null)
        {
            GameObject itemToDrop = lootTable.GetRandomLoot();

            if (itemToDrop != null)
            {
                Instantiate(itemToDrop, transform.position, Quaternion.identity);
            }
        }

        // Collider'ı devre dışı bırak (tekrar çarpışma olmasın)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Rigidbody'yi durdur
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Objeyi yok et
        Destroy(gameObject, destroyDelay);
    }
}
