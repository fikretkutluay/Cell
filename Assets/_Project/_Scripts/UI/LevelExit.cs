using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("UI Referansları")]
    [Tooltip("İçine girdiğimizde açılacak olan Next Level paneli")]
    public GameObject nextLevelPanel;

    // Karakter içerideyken panelin defalarca açılmaya çalışmasını engeller
    private bool isLevelCompleted = false;

    private void Start()
    {
        // Eğer panel atanmamışsa otomatik bulmaya çalış
        if (nextLevelPanel == null)
        {
            // Önce normal Canvas'larda ara
            Canvas[] canvases = FindObjectsOfType<Canvas>(true); // true = inactive objeleri de ara
            foreach (Canvas canvas in canvases)
            {
                Transform found = canvas.transform.Find("NextLevelPanel");
                if (found == null) found = canvas.transform.Find("LevelCompletePanel");
                if (found == null) found = canvas.transform.Find("Next Level Panel");
                
                if (found != null)
                {
                    nextLevelPanel = found.gameObject;
                    Debug.Log($"LevelExit: Auto-found panel '{nextLevelPanel.name}' in canvas '{canvas.name}'");
                    break;
                }
            }

            // Eğer bulamadıysa DontDestroyOnLoad objelerinde ara
            if (nextLevelPanel == null)
            {
                GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "NextLevelPanel" || obj.name == "LevelCompletePanel")
                    {
                        nextLevelPanel = obj;
                        Debug.Log($"LevelExit: Found panel '{nextLevelPanel.name}' in DontDestroyOnLoad");
                        break;
                    }
                }
            }

            // Son çare: CoreUISystem altında ara
            if (nextLevelPanel == null)
            {
                GameObject coreUI = GameObject.Find("CoreUISystem");
                if (coreUI != null)
                {
                    Transform found = coreUI.transform.Find("NextLevelPanel");
                    if (found != null)
                    {
                        nextLevelPanel = found.gameObject;
                        Debug.Log($"LevelExit: Found panel in CoreUISystem");
                    }
                }
            }
        }

        // Oyun başladığında panelin kapalı olduğundan emin ol
        if (nextLevelPanel != null)
        {
            nextLevelPanel.SetActive(false);
            Debug.Log($"LevelExit: Panel '{nextLevelPanel.name}' found and set to inactive in scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        }
        else
        {
            Debug.LogError($"LevelExit: nextLevelPanel is NULL in scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}! Please assign it in Inspector or ensure NextLevelPanel exists in CoreUISystem.");
        }
    }

    // Karakter (Player) Collider'ın İÇİNE GİRDİĞİNDE çalışır
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"LevelExit: OnTriggerEnter2D called with {collision.gameObject.name}, tag: {collision.tag}");
        
        if (collision.CompareTag("Player") && !isLevelCompleted)
        {
            Debug.Log("LevelExit: Player detected! Showing panel...");
            ShowNextLevelPanel();
        }
    }

    // Karakter (Player) Collider'ın İÇİNDEN ÇIKTIĞINDA çalışır
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isLevelCompleted)
        {
            HideNextLevelPanel();
        }
    }

    private void ShowNextLevelPanel()
    {
        isLevelCompleted = true; // İçeride olduğunu belirt

        if (nextLevelPanel != null)
        {
            nextLevelPanel.SetActive(true); // Paneli ekranda göster
            Debug.Log("LevelExit: Panel activated successfully!");
        }
        else
        {
            Debug.LogError("LevelExit: Cannot show panel - nextLevelPanel is NULL!");
        }
    }

    private void HideNextLevelPanel()
    {
        isLevelCompleted = false; // Çıktığı için durumu sıfırla (tekrar girebilsin)

        if (nextLevelPanel != null)
        {
            nextLevelPanel.SetActive(false); // Paneli ekrandan gizle
        }
    }

    // Bu metodu butonun OnClick Event'ine bağlıyoruz
    // GÜNCELLEME: Artık statları kaydediyor
    public void GoToNextLevel()
    {
        // Statları kaydet
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.SaveProgress();
            Debug.Log("Stats saved before loading next level!");
        }
        else
        {
            Debug.LogWarning("GameProgressManager not found! Stats will not be saved.");
        }

        // Şu anki sahnenin indeksini al ve 1 fazlasını yükle (Bir sonraki Level)
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
    }

    // Alternatif: Direkt sahne geçişi (panel olmadan)
    public void GoToNextLevelDirect()
    {
        if (GameProgressManager.Instance != null)
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            GameProgressManager.Instance.LoadNextScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("GameProgressManager not found! Loading scene without saving stats.");
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}