using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CoreUIManager : MonoBehaviour
{
    public static CoreUIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject hudPanel;       // StatsUI bu panelin i�inde olacak
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject mainMenuPanel;

    private CellInput inputActions;
    private bool isGamePaused = false;

    private void Awake()
    {
        // Singleton pattern - sadece bir instance olmalı
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        // Input actions'ı oluştur
        if (inputActions == null)
        {
            inputActions = new CellInput();
        }
    }

    private void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            inputActions.Player.Pause.performed += ctx => TogglePause();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1.0f;
        isGamePaused = false;
        HideAllPanels();

        // Ana menüdeysek MainMenu paneli, değilsek HUD açılsın
        if (scene.name == "MainMenu")
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(true);
            Cursor.visible = true; // Menüde mouse görünsün
        }
        else
        {
            if (hudPanel) hudPanel.SetActive(true);
            // Oyun içinde mouse nişan almak için gerekliyse açık kalsın,
            // değilse gizleyebilirsin: Cursor.visible = false;
        }

        // NOT: LevelExit panelleri kendi scriptleri tarafından yönetilir
        // Bu yüzden burada onları kapatmıyoruz
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(false);
        if (pauseMenu) pauseMenu.SetActive(false);
        if (gameOverMenu) gameOverMenu.SetActive(false);
    }

    private void TogglePause()
    {
        // Ana men�deysek veya Oyun bittiyse pause �al��mas�n
        if (SceneManager.GetActiveScene().name == "MainMenu" || (gameOverMenu != null && gameOverMenu.activeSelf)) return;

        isGamePaused = !isGamePaused;

        if (pauseMenu != null) pauseMenu.SetActive(isGamePaused);

        // Pause a��l�nca HUD gizlensin mi? Genelde kalabilir ama gizlemek istersen:
        // if (hudPanel != null) hudPanel.SetActive(!isGamePaused); 

        Time.timeScale = isGamePaused ? 0f : 1f;
    }

    public void Resume()
    {
        TogglePause();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("QUIT");
    }

    public void StartGame()
    {
        // Level 1'i y�kle (Build Settings'de 1. s�rada Lung olmal�)
        SceneManager.LoadScene(1);
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        isGamePaused = true;

        if (hudPanel) hudPanel.SetActive(false); // Game overda HUD gizlensin
        if (pauseMenu) pauseMenu.SetActive(false);
        if (gameOverMenu) gameOverMenu.SetActive(true);

        Cursor.visible = true; // Butonlara basmak i�in mouse a�
        Debug.Log("GAME OVER");
    }
}