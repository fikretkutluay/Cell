using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// EventSystem'in sahneler arası tek instance olmasını sağlar
/// Yeni sahnede EventSystem varsa onu yok eder
/// </summary>
public class EventSystemManager : MonoBehaviour
{
    private void Awake()
    {
        // Sahnede birden fazla EventSystem var mı kontrol et
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        
        if (eventSystems.Length > 1)
        {
            // Bu EventSystem DontDestroyOnLoad değilse yok et
            if (transform.parent == null && gameObject.scene.name != "DontDestroyOnLoad")
            {
                Debug.Log($"Destroying duplicate EventSystem in scene: {gameObject.scene.name}");
                Destroy(gameObject);
            }
        }
        else
        {
            // İlk EventSystem - DontDestroyOnLoad yap
            DontDestroyOnLoad(gameObject);
        }
    }
}
