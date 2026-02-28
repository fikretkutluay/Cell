using UnityEngine;
using System.Collections;
using System;
public class LungBoss : MonoBehaviour, IDamageable 
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 1000f;
    [SerializeField] private float currentHealth;
    public bool IsDead { get; private set; }
    private bool isAttacking = false;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject levelExitDoor;

    [Header("Attack1: Alveoli Burst (Normal)")]
    [SerializeField] private GameObject alveoliProjectile;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float timeBetweenShots = 0.2f;
    [SerializeField] private float projectileSpeed = 8f;

    [Header("Attack2: Mucus Nova (Skill)")]
    [SerializeField] private GameObject mucusSpearProjectile;
    [SerializeField] private int spearCount = 12;
    [SerializeField] private float skillCooldown = 4f;

    [Header("AI Settings")]
    [SerializeField] private float actionCooldown = 2f;
    [Range(0f, 1f)][SerializeField] private float skillChanceNormal = 0.3f;
    [Range(0f, 1f)][SerializeField] private float skillChanceEnraged = 0.6f;

    private Transform player;

    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(CombatLoop());
    }

    private IEnumerator CombatLoop()
    {
        while (!IsDead)
        {
            yield return new WaitForSeconds(actionCooldown);
            if (player == null) yield break;
            float currentSkillChance = (currentHealth / maxHealth <= 0.3f) ? skillChanceEnraged : skillChanceNormal;
            bool useSkill = UnityEngine.Random.value < currentSkillChance;

            if(useSkill)
            {
                yield return StartCoroutine(PerformSkillAttack());
            }
            else
            {
                yield return StartCoroutine(PerformBurstAttack());
            }
        }
    }

    private IEnumerator PerformBurstAttack()
    {
        isAttacking = true;
        for (int i = 0; i < burstCount; i++)
        {
            if (player == null) break;

            SpawnProjectile(alveoliProjectile, player.position);
            yield return new WaitForSeconds(timeBetweenShots);
        }
        isAttacking = false;
    }
    private IEnumerator PerformSkillAttack()
    {
        isAttacking = true;
        if (animator) animator.SetTrigger("Skill");
        yield return new WaitForSeconds(skillCooldown);
        isAttacking = false;
    }
    public void FireMucusNova()
    {
        float angleStep = 360f / spearCount;
        float angle = 0f;
        float spawnRadius = 2.5f;

        Vector3 centerPoint = (firePoint != null) ? firePoint.position : transform.position;
        Collider2D bossCollider = GetComponent<Collider2D>();

        for (int i = 0; i < spearCount; i++)
        {
            float xPos = centerPoint.x + Mathf.Cos((angle * Mathf.PI) / 180f) * spawnRadius;
            float yPos = centerPoint.y + Mathf.Sin((angle * Mathf.PI) / 180f) * spawnRadius;

            Vector3 spawnPosition = new Vector3 (xPos, yPos, 0);
            Vector3 moveDirection = (spawnPosition - centerPoint).normalized;

            GameObject spear = Instantiate(mucusSpearProjectile, spawnPosition, Quaternion.identity);

            Collider2D spearCollider = spear.GetComponent<Collider2D>();
            if (bossCollider != null && spearCollider != null)
            {
                Physics2D.IgnoreCollision(bossCollider, spearCollider);
            }
            float rotZ = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            spear.transform.rotation = Quaternion.Euler(0, 0, rotZ);
            
            if (spear.TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = moveDirection * projectileSpeed;
            }
            angle += angleStep;

        }
    }

    private void SpawnProjectile(GameObject prefab, Vector3 targetPos)
    {
        Vector2 direction = (targetPos - firePoint.position).normalized;
        GameObject proj = Instantiate(prefab, firePoint.position, Quaternion.identity);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (proj.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = direction * projectileSpeed;
        }

        if (proj.TryGetComponent(out Projectile p))
        {
            //p.Setup();
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        StopAllCoroutines();
        if (animator) animator.SetTrigger("Die");
        if (levelExitDoor) levelExitDoor.SetActive(true);

        Destroy(gameObject, 2f);
    }
}
