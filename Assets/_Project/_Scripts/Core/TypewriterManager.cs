using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class TypewriterManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    [Tooltip("Yavaþça belirecek olan baþlýk yazýsý (Örn: BÖLÜM 1)")]
    public TextMeshProUGUI titleTextComponent;
    [Tooltip("Daktilo efektiyle yazýlacak olan ana hikaye")]
    public TextMeshProUGUI storyTextComponent;

    [Header("Yazý Ayarlarý")]
    public string titleString; // Baþlýkta ne yazsýn?
    [TextArea(5, 10)]
    public string storyText;   // Hikayede ne yazsýn?

    [Header("Zamanlama Ayarlarý")]
    public float titleFadeDuration = 2f;    // Baþlýðýn yavaþça belirmesi kaç saniye sürsün?
    public float delayBeforeTyping = 3f;    // Baþlýk belirdikten sonra daktilo için kaç saniye beklensin?
    public float typingSpeed = 0.05f;       // Harflerin yazýlma hýzý
    public float delayBeforeTransition = 2f;// Yazý bitince diðer sahneye geçmeden önce ne kadar beklensin?

    [Header("Sahne Geçiþi")]
    public string nextSceneName;

    [Header("Ses Efekti")]
    public AudioClip typingSound;

    private AudioSource audioSource;
    private bool isSequenceRunning = true;
    private bool transitionStarted = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 1. HAZIRLIK: Ekraný temizle ve baþlýðýn görünürlüðünü (Alpha) 0 yap
        storyTextComponent.text = "";
        if (titleTextComponent != null)
        {
            titleTextComponent.text = titleString;
            Color c = titleTextComponent.color;
            c.a = 0f; // Tamamen saydam
            titleTextComponent.color = c;
        }

        // SÝNEMATÝK DOKUNUÞ: Yazý sekansý baþlýyor, müziði kýs!
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetLowVolume();
        }

        // Sekansý (Baþlýk -> Bekleme -> Hikaye) baþlat
        StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        // Oyuncu týklarsa veya boþluða basarsa (Geçiþ / Skip sistemi)
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isSequenceRunning)
            {
                // Sekans devam ediyorsa her þeyi durdur ve yazýlarý anýnda ekrana bas!
                StopAllCoroutines();

                if (titleTextComponent != null)
                {
                    Color c = titleTextComponent.color;
                    c.a = 1f; // Baþlýðý anýnda %100 görünür yap
                    titleTextComponent.color = c;
                }

                storyTextComponent.text = storyText;
                isSequenceRunning = false;
                EndSequence();
            }
            else if (!transitionStarted)
            {
                // Her þey zaten ekrandaysa, beklemeden hedef sahneye geç
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    // Ana Sinematik Sýralama (Sequence)
    private IEnumerator PlaySequence()
    {
        isSequenceRunning = true;

        // AÞAMA 1: BAÞLIÐI YAVAÞÇA GÖSTER (FADE IN)
        if (titleTextComponent != null)
        {
            float timer = 0f;
            Color c = titleTextComponent.color;
            while (timer < titleFadeDuration)
            {
                timer += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, timer / titleFadeDuration); // Saydamlýðý yavaþça artýr
                titleTextComponent.color = c;
                yield return null;
            }
            c.a = 1f;
            titleTextComponent.color = c;
        }

        // AÞAMA 2: BELÝRLENEN SÜRE KADAR SESSÝZLÝK (örn: 3 saniye)
        yield return new WaitForSeconds(delayBeforeTyping);

        // AÞAMA 3: HÝKAYEYÝ DAKTÝLO EFEKTÝYLE YAZ
        foreach (char c in storyText.ToCharArray())
        {
            storyTextComponent.text += c;

            // Boþluk karakteri deðilse daktilo sesini çal
            if (c != ' ' && typingSound != null)
            {
                audioSource.PlayOneShot(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isSequenceRunning = false;
        EndSequence();
    }

    private void EndSequence()
    {
        // Yazý sekansý tamamen bitti, müziðin sesini eski haline getir!
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetNormalVolume();
        }

        if (!transitionStarted)
        {
            StartCoroutine(WaitAndTransition());
        }
    }

    private IEnumerator WaitAndTransition()
    {
        transitionStarted = true;
        yield return new WaitForSeconds(delayBeforeTransition);
        SceneManager.LoadScene(nextSceneName);
    }
}