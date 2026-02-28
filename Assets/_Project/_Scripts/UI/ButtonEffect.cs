using UnityEngine;
using UnityEngine.EventSystems; // Bu kütüphane þart (Hover/Click algýlamak için)
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))] // Otomatik AudioSource ekler
public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [Range(0f, 1f)][SerializeField] private float volume = 0.5f;

    [Header("Visual Settings")]
    [SerializeField] private float hoverScale = 1.1f; // Üzerine gelince %10 büyü
    [SerializeField] private float clickScale = 0.95f; // Týklayýnca %5 küçül
    [SerializeField] private float animationSpeed = 10f; // Büyüme hýzý

    private Vector3 originalScale;
    private Vector3 targetScale;
    private AudioSource source;
    private Button button;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        button = GetComponent<Button>();

        // AudioSource ayarlarý
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D Ses olsun

        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        // Buton interactable deðilse (Disabled) efekt çalýþmasýn
        if (button != null && !button.interactable) return;

        // Yumuþak geçiþ (Lerp) ile scale deðiþtir
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
    }

    // --- MOUSE ÜZERÝNE GELDÝÐÝNDE ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        targetScale = originalScale * hoverScale; // Hedef: Büyü
        PlaySound(hoverSound);
    }

    // --- MOUSE ÇIKTIÐINDA ---
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale; // Hedef: Normale dön
    }

    // --- TIKLANDIÐINDA (BASILDIÐINDA) ---
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        targetScale = originalScale * clickScale; // Hedef: Küçül (Basýlma hissi)
        PlaySound(clickSound);
    }

    // --- TIKLAMA BIRAKILDIÐINDA ---
    public void OnPointerUp(PointerEventData eventData)
    {
        // Mouse hala üzerindeyse hover boyutuna, deðilse normale dön
        targetScale = originalScale * hoverScale;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && source != null)
        {
            // PlayOneShot sayesinde sesler üst üste binebilir (Hýzlý geçiþlerde kesilmez)
            source.PlayOneShot(clip, volume);
        }
    }

    // Obje kapanýrsa scale'i sýfýrla ki bir dahaki açýlýþta dev gibi kalmasýn
    private void OnDisable()
    {
        transform.localScale = originalScale;
    }
}