using System.Collections;
using UnityEngine;

/// <summary>
/// Beyin Boss - 3 farklı saldırı patternine sahip final boss
/// 1. Spawn Leukocytes - Bulunduğu yere 3 akyuvar spawn eder
/// 2. 360 Degree Burst - Etrafına 360 derece top atar
/// 3. Targeted Shot - Player'a hızlı ve güçlü top atar
/// </summary>
public class BrainBoss : MonoBehaviour, IDamageable
{
    public bool IsDead { get; private set; }

    [Header("Stats")]
    [SerializeField] private float maxHealth = 1000f;
    private float currentHealth;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject levelExitDoor;

    [Header("Attack 1: Spawn Leukocytes")]
    [SerializeField] private GameObject leukocytePrefab;
    [SerializeField] private int spawnCount = 3;
    [SerializeField] private float spawnRadius = 2f;

    [Header("Attack 2: 360 Degree Burst")]
    [SerializeField] private GameObject burstProjectilePrefab;
    [SerializeField] private int burstProjectileCount = 16;
    [SerializeField] private float burstProjectileSpeed = 8f;

    [Header("Attack 3: Targeted Shot")]
    [SerializeField] private GameObject targetedProjectilePrefab;
    [SerializeField] private float targetedProjectileSpeed = 15f;
    [SerializeField] private float targetedProjectileDamage = 25f;

    [Header("AI Settings")]
    [SerializeField] private float actionCooldown = 2.5f;
    [SerializeField] private float idleTime = 1f;

    [Header("Shake Effect")]
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float shakeSpeed = 20f;

    private Vector3 originalPosition;
    private bool isAttacking = false;

    private void Start()
    {
        currentHealth = maxHealth;
        IsDead = false;
        originalPosition = transform.position;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        StartCoroutine(CombatLoop());
        StartCoroutine(ShakeLoop());
    }

    /// <summary>
    /// Boss'un sürekli titremesini sağlar
    /// </summary>
    private IEnumerator ShakeLoop()
    {
        while (!IsDead)
        {
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeAmount;
            transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);
            yield return null;
        }
        transform.position = originalPosition;
    }

    /// <summary>
    /// Ana savaş döngüsü - rastgele saldırı seçer
    /// </summary>
    private IEnumerator CombatLoop()
    {
        yield return new WaitForSeconds(idleTime);

        while (!IsDead)
        {
            if (player == null) yield break;

            // Rastgele saldırı seç
            int attackChoice = Random.Range(0, 3);

            switch (attackChoice)
            {
                case 0:
                    yield return StartCoroutine(Attack1_SpawnLeukocytes());
                    break;
                case 1:
                    yield return StartCoroutine(Attack2_360DegreeBurst());
                    break;
                case 2:
                    yield return StartCoroutine(Attack3_TargetedShot());
                    break;
            }

            yield return new WaitForSeconds(actionCooldown);
        }
    }

    /// <summary>
    /// Saldırı 1: Boss'un etrafına 3 akyuvar spawn eder
    /// </summary>
    private IEnumerator Attack1_SpawnLeukocytes()
    {
        isAttacking = true;
        if (animator) animator.SetTrigger("Attack1");

        Debug.Log("Brain Boss: Spawning Leukocytes!");

        float angleStep = 360f / spawnCount;
        float angle = 0f;

        for (int i = 0; i < spawnCount; i++)
        {
            float xPos = transform.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius;
            float yPos = transform.position.y + Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius;
            Vector3 spawnPosition = new Vector3(xPos, yPos, 0);

            if (leukocytePrefab != null)
            {
                GameObject leukocyte = Instantiate(leukocytePrefab, spawnPosition, Quaternion.identity);
                
                // Boss collider'ı ile ignore collision
                Collider2D bossCollider = GetComponent<Collider2D>();
                Collider2D enemyCollider = leukocyte.GetComponent<Collider2D>();
                if (bossCollider != null && enemyCollider != null)
                {
                    Physics2D.IgnoreCollision(bossCollider, enemyCollider);
                }

                // Enemy AI'a player referansını ver
                LeukocyteAI enemyAI = leukocyte.GetComponent<LeukocyteAI>();
                if (enemyAI != null && player != null)
                {
                    enemyAI.player = player;
                    Debug.Log("Enemy AI player reference set!");
                }
                else
                {
                    Debug.LogWarning("LeukocyteAI component not found on spawned enemy!");
                }
            }

            angle += angleStep;
        }

        isAttacking = false;
        yield return null;
    }

    /// <summary>
    /// Saldırı 2: 360 derece etrafına top atar
    /// </summary>
    private IEnumerator Attack2_360DegreeBurst()
    {
        isAttacking = true;
        if (animator) animator.SetTrigger("Attack2");

        Debug.Log("Brain Boss: 360 Degree Burst!");

        yield return new WaitForSeconds(0.3f); // Animasyon için kısa bekleme

        float angleStep = 360f / burstProjectileCount;
        float angle = 0f;

        Vector3 centerPoint = firePoint != null ? firePoint.position : transform.position;
        Collider2D bossCollider = GetComponent<Collider2D>();

        for (int i = 0; i < burstProjectileCount; i++)
        {
            float xDir = Mathf.Cos(angle * Mathf.Deg2Rad);
            float yDir = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector2 direction = new Vector2(xDir, yDir).normalized;

            if (burstProjectilePrefab != null)
            {
                GameObject projectile = Instantiate(burstProjectilePrefab, centerPoint, Quaternion.identity);

                // Boss ile collision ignore
                Collider2D projCollider = projectile.GetComponent<Collider2D>();
                if (bossCollider != null && projCollider != null)
                {
                    Physics2D.IgnoreCollision(bossCollider, projCollider);
                }

                // Rotasyon ayarla
                float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                projectile.transform.rotation = Quaternion.Euler(0, 0, rotZ);

                // Hareket ettir - Rigidbody2D ayarlarını kontrol et
                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.gravityScale = 0f; // Gravity'yi kapat
                    rb.linearVelocity = direction * burstProjectileSpeed;
                    Debug.Log($"Projectile {i} fired with velocity: {rb.linearVelocity}");
                }
                else
                {
                    Debug.LogError("Projectile prefab doesn't have Rigidbody2D!");
                }
            }
            else
            {
                Debug.LogError("Burst Projectile Prefab is null!");
            }

            angle += angleStep;
        }

        isAttacking = false;
        yield return null;
    }

    /// <summary>
    /// Saldırı 3: Player'a hızlı ve güçlü top atar
    /// </summary>
    private IEnumerator Attack3_TargetedShot()
    {
        isAttacking = true;
        if (animator) animator.SetTrigger("Attack3");

        Debug.Log("Brain Boss: Targeted Shot!");

        yield return new WaitForSeconds(0.2f); // Nişan alma animasyonu

        if (player != null && targetedProjectilePrefab != null)
        {
            Vector3 centerPoint = firePoint != null ? firePoint.position : transform.position;
            Vector2 direction = (player.position - centerPoint).normalized;

            GameObject projectile = Instantiate(targetedProjectilePrefab, centerPoint, Quaternion.identity);

            // Boss ile collision ignore
            Collider2D bossCollider = GetComponent<Collider2D>();
            Collider2D projCollider = projectile.GetComponent<Collider2D>();
            if (bossCollider != null && projCollider != null)
            {
                Physics2D.IgnoreCollision(bossCollider, projCollider);
            }

            // Rotasyon ayarla
            float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, rotZ);

            // Hareket ettir - Rigidbody2D ayarlarını kontrol et
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f; // Gravity'yi kapat
                rb.linearVelocity = direction * targetedProjectileSpeed;
                Debug.Log($"Targeted projectile fired with velocity: {rb.linearVelocity}");
            }
            else
            {
                Debug.LogError("Targeted Projectile prefab doesn't have Rigidbody2D!");
            }

            // Özel hasar için BrainProjectile component'i varsa ayarla
            BrainProjectile brainProj = projectile.GetComponent<BrainProjectile>();
            if (brainProj != null)
            {
                brainProj.SetDamage(targetedProjectileDamage);
            }
        }
        else
        {
            if (player == null) Debug.LogError("Player is null!");
            if (targetedProjectilePrefab == null) Debug.LogError("Targeted Projectile Prefab is null!");
        }

        isAttacking = false;
        yield return null;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        Debug.Log($"Brain Boss took {amount} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        StopAllCoroutines();

        Debug.Log("Brain Boss defeated!");

        if (animator) animator.SetTrigger("Die");
        if (levelExitDoor) levelExitDoor.SetActive(true);

        Destroy(gameObject, 2f);
    }

    private void OnDrawGizmosSelected()
    {
        // Spawn radius göster
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Fire point göster
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.3f);
        }
    }
}
