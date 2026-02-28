using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private Rigidbody2D rb;

    public void Setup(Vector2 direction, float speed, float damageValue)
    {
        rb = GetComponent<Rigidbody2D>();
        this.damage = damageValue;

        rb.gravityScale = 0f;

        rb.linearVelocity = direction.normalized * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, 3f);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null && !collision.CompareTag("Player"))
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }

    }
}
