using UnityEngine;

/// <summary>
/// Beyin Boss'unun projectile'ı
/// Player'a hasar verir, duvarlara çarpınca yok olur
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BrainProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Rigidbody2D ayarlarını garanti et
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Start()
    {
        // Belirli süre sonra yok et
        Destroy(gameObject, lifetime);
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Duvara çarptı mı?
        if (collision.CompareTag("Wall"))
        {
            Debug.Log("Brain Projectile hit wall!");
            Destroy(gameObject);
            return;
        }

        // Player'a çarptı mı?
        if (collision.CompareTag("Player"))
        {
            IDamageable player = collision.GetComponent<IDamageable>();
            if (player != null && !player.IsDead)
            {
                player.TakeDamage(damage);
                Debug.Log($"Brain Projectile hit player for {damage} damage!");
            }
            Destroy(gameObject);
        }
    }
}
