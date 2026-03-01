using UnityEngine;

/// <summary>
/// Enemy'nin player'a çarpınca hasar vermesini sağlar
/// Enemy de çarpışmadan hasar alır ve canı bitince ölür
/// </summary>
public class EnemyCollisionDamage : MonoBehaviour
{
    [Header("Collision Damage Settings")]
    [SerializeField] private float damageToPlayer = 10f;
    [SerializeField] private float damageToSelf = 15f;
    [SerializeField] private float damageInterval = 1f; // Sürekli hasar için cooldown

    private float lastDamageTime = 0f;
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogError("EnemyCollisionDamage: EnemyHealth component not found! This script requires EnemyHealth.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Player'a çarptı mı kontrol et
        if (collision.gameObject.CompareTag("Player"))
        {
            DealDamage(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Player ile temas halindeyken sürekli hasar ver (cooldown ile)
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageInterval)
            {
                DealDamage(collision.gameObject);
            }
        }
    }

    private void DealDamage(GameObject player)
    {
        lastDamageTime = Time.time;

        // Player'a hasar ver
        IDamageable playerDamageable = player.GetComponent<IDamageable>();
        if (playerDamageable != null && !playerDamageable.IsDead)
        {
            playerDamageable.TakeDamage(damageToPlayer);
            Debug.Log($"Enemy dealt {damageToPlayer} damage to Player!");
        }

        // Enemy kendine de hasar alsın
        if (enemyHealth != null && !enemyHealth.IsDead)
        {
            enemyHealth.TakeDamage(damageToSelf);
            Debug.Log($"Enemy took {damageToSelf} self-damage from collision!");
        }
    }
}
