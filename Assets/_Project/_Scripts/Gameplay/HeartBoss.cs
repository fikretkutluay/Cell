using System.Collections;
using UnityEngine;

public class HeartBoss : MonoBehaviour, IDamageable
{
    public bool IsDead { get; private set; }

    [Header("Stats")]
    [SerializeField] private float maxHealth = 750f;
    [SerializeField] private float currentHealth;

    [Header("Machine Gun Attack")]
    [SerializeField] private GameObject bloodProjectilePrefab; 
    [SerializeField] private Transform firePoint;              
    [SerializeField] private float attackDuration = 3f;        
    [SerializeField] private float fireRate = 0.1f;            
    [SerializeField] private float projectileSpeed = 12f;      
    [SerializeField] private float spreadAngle = 5f;           

    [Header("AI Settings")]
    [SerializeField] private float restTime = 2f;              
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject levelExitDoor;

    private Transform player;

    private void Start()
    {
        currentHealth = maxHealth;
        IsDead = false;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (animator) animator.speed = 1f;
        StartCoroutine(CombatLoop());
    }

    private IEnumerator CombatLoop()
    {
        while (!IsDead)
        {
            yield return new WaitForSeconds(restTime);
            if (player == null) yield break;
            yield return StartCoroutine(PerformRapidFire());
        }
    }

    private IEnumerator PerformRapidFire()
    {
        if (animator) animator.speed = 3f;

        float timer = 0f;
        while (timer < attackDuration)
        {
            if (player == null) break;

            SpawnBullet();
            yield return new WaitForSeconds(fireRate);
            timer += fireRate;
        }
        if (animator) animator.speed = 1f;
    }

    private void SpawnBullet()
    {
        if (firePoint == null || bloodProjectilePrefab == null) return;

        // Oyuncuya niþan al
        Vector2 direction = (player.position - firePoint.position).normalized;
        float randomAngle = Random.Range(-spreadAngle, spreadAngle);
        Vector2 finalDirection = Quaternion.Euler(0, 0, randomAngle) * direction;
        GameObject bullet = Instantiate(bloodProjectilePrefab, firePoint.position, Quaternion.identity);
        float rotZ = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, rotZ);
        if (bullet.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = finalDirection * projectileSpeed; 
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

        if (animator)
        {
            animator.speed = 1f; 
            animator.SetTrigger("Die");
        }

        if (levelExitDoor) levelExitDoor.SetActive(true);
        Destroy(gameObject, 2f);
    }
}