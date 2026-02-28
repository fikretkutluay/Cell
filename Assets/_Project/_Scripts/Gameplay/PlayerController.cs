using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Playing,
    Dashing,
    Dead
}

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Data & Stats")]
    [SerializeField] private PlayerStats stats;

    [Header("Combat References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera mainCam;

    [Header("Combat Settings")]
    [SerializeField] private float aoeCooldown = 5f;
    [SerializeField] private float aoeRadius = 3.5f;

    // State Machine
    private PlayerState currentState;
    // Components
    private Rigidbody2D rb;
    private CellInput inputActions;
    // Runtime Variables
    private Vector2 moveInput;
    private Vector2 mousePos;
    private float nextFireTime;
    private float nextSkillTime;

    public bool IsDead => stats.currentHealth <= 0;

    #region Unity Lifecycle
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new CellInput();

        if (mainCam == null) mainCam = Camera.main;

        currentState = PlayerState.Playing;

        if(stats != null) stats.ResetValues();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.AoESkill.performed += ctx => TryCastSkill();

        if (stats != null)
        {
            stats.OnHealthChanged += CheckDeath;
        }
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        if(stats != null)
        {
            stats.OnHealthChanged -= CheckDeath;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case PlayerState.Playing:
                HandleInputPlaying(); 
                break;
            case PlayerState.Dead:
                break;
        }
    }
    private void FixedUpdate()
    {
        switch (currentState)
        {
            case PlayerState.Playing:
                ApplyMovement();
                break;
        }
    }
    #endregion

    #region State Logic: PLAYING
    private void HandleInputPlaying()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        HandleRotation();

        if (inputActions.Player.Fire.ReadValue<float>() > 0.5f)
        {
            TryShoot();
        }
    }
    private void ApplyMovement()
    {
        if (moveInput != Vector2.zero)
        {
            rb.AddForce(moveInput.normalized * stats.currentMoveSpeed);
        }
    }
    private void HandleRotation()
    {
        mousePos = inputActions.Player.Aim.ReadValue<Vector2>();
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
        Vector2 direction = worldPos - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    #endregion

    #region Combat Mechanics
    private void TryShoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + stats.currentFireRate;
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, transform.rotation);
        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Setup(transform.right, 20f, stats.currentDamage);
        }
    }
    private void TryCastSkill()
    {
        if (currentState != PlayerState.Playing) return;
        if (Time.time < nextSkillTime) return;

        nextSkillTime = Time.time + aoeCooldown;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (var hit in hits)
        {
            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null && !hit.CompareTag("Player"))
            {
                target.TakeDamage(stats.currentDamage * 4f);
            }
        }
        Debug.Log("AoE Skill Used");
    }
    #endregion

    #region Interface Implementation (IDamageable)

    public void TakeDamage(float amount)
    {
        stats.TakeDamage(amount);

        if (stats.currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        CoreUIManager.Instance.ShowGameOver();
        currentState = PlayerState.Dead;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
    }

    private void CheckDeath(float currentHealth, float maxHealth)
    {
        if (currentHealth <= 0 && currentState != PlayerState.Dead)
        {
            Die();
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}
