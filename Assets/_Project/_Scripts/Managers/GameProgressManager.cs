using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Oyuncu statlarını sahneler arası korur
/// DontDestroyOnLoad ile singleton pattern kullanır
/// </summary>
public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [Header("Player Stats Reference")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Saved Progress")]
    private float savedMaxHealth;
    private float savedCurrentHealth;
    private float savedMoveSpeed;
    private float savedDamage;
    private float savedFireRate;
    private bool hasProgress = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Scene değişikliklerini dinle
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Mevcut player statlarını kaydet
    /// </summary>
    public void SaveProgress()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("GameProgressManager: PlayerStats reference is null!");
            return;
        }

        savedMaxHealth = playerStats.maxHealth;
        savedCurrentHealth = playerStats.currentHealth;
        savedMoveSpeed = playerStats.currentMoveSpeed;
        savedDamage = playerStats.currentDamage;
        savedFireRate = playerStats.currentFireRate;
        hasProgress = true;

        Debug.Log($"Progress saved: HP={savedCurrentHealth}/{savedMaxHealth}, Damage={savedDamage}, Speed={savedMoveSpeed}");
    }

    /// <summary>
    /// Kaydedilmiş statları geri yükle
    /// </summary>
    public void LoadProgress()
    {
        if (!hasProgress)
        {
            Debug.Log("No saved progress found. Using default stats.");
            return;
        }

        if (playerStats == null)
        {
            Debug.LogWarning("GameProgressManager: PlayerStats reference is null!");
            return;
        }

        playerStats.maxHealth = savedMaxHealth;
        playerStats.currentHealth = savedCurrentHealth;
        playerStats.currentMoveSpeed = savedMoveSpeed;
        playerStats.currentDamage = savedDamage;
        playerStats.currentFireRate = savedFireRate;

        // UI'ı güncelle
        playerStats.NotifyStatsChanged();

        Debug.Log($"Progress loaded: HP={savedCurrentHealth}/{savedMaxHealth}, Damage={savedDamage}, Speed={savedMoveSpeed}");
    }

    /// <summary>
    /// Tüm progress'i sıfırla (oyun baştan başladığında)
    /// </summary>
    public void ResetProgress()
    {
        hasProgress = false;
        if (playerStats != null)
        {
            playerStats.ResetValues();
        }
        Debug.Log("Progress reset to default values.");
    }

    /// <summary>
    /// Yeni sahne yüklendiğinde çağrılır
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // PlayerStats referansını yeniden bul (ScriptableObject olduğu için gerekli değil ama güvenlik için)
        if (playerStats == null)
        {
            // Resources klasöründen veya başka bir yerden bulabilirsin
            Debug.LogWarning("GameProgressManager: PlayerStats reference lost after scene load!");
        }

        // Eğer kaydedilmiş progress varsa yükle
        if (hasProgress)
        {
            LoadProgress();
        }
    }

    /// <summary>
    /// Bir sonraki sahneye geç ve progress'i kaydet
    /// </summary>
    public void LoadNextScene(string sceneName)
    {
        SaveProgress();
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Bir sonraki sahneye geç (build index ile)
    /// </summary>
    public void LoadNextScene(int sceneIndex)
    {
        SaveProgress();
        SceneManager.LoadScene(sceneIndex);
    }
}
