using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CoreUIManager : MonoBehaviour
{
    public static CoreUIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject mainMenuPanel;


    private CellInput inputActions;
    private bool isGamePaused = false;  
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        inputActions = new CellInput();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Pause.performed += ctx => TogglePause();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        inputActions.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1.0f;
        isGamePaused = false;
        HideAllPanels();
        
        if (scene.name == "MainMenu")
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(true);
        }
        else
        {
            if(hudPanel) hudPanel.SetActive(true);
        }
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
        if (SceneManager.GetActiveScene().name == "MainMenu" || (gameOverMenu != null && gameOverMenu.activeSelf)) return;
        isGamePaused = !isGamePaused;
        if (pauseMenu != null) pauseMenu.SetActive(isGamePaused);
        if (hudPanel != null) hudPanel.SetActive(!isGamePaused);

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
        SceneManager.LoadScene(1);
    }
    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        isGamePaused = true;
        gameOverMenu.SetActive(true);
        if (hudPanel) hudPanel.SetActive(false);
        if (pauseMenu) pauseMenu.SetActive(false);
        if (mainMenuPanel) gameOverMenu.SetActive(true);

        Debug.Log("GAME OVER");
    }
}
