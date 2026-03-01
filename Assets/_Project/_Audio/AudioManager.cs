using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    // Singleton yapýsý: Her yerden bu scripte kolayca ulaþmamýzý saðlar
    public static AudioManager Instance;

    [Header("Sahne Müziði")]
    public AudioClip backgroundMusic;

    [Header("Ses Ayarlarý")]
    [Range(0f, 1f)] public float normalVolume = 0.5f; // Oyun içi normal ses
    [Range(0f, 1f)] public float lowVolume = 0.15f;   // Daktilo çalarkenki kýsýk ses
    public float fadeSpeed = 2f; // Sesin kýsýlma ve geri açýlma hýzý (Yumuþak geçiþ için)

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Sahnede zaten bir AudioManager varsa, yenisini yok et (Müziðin üst üste binmesini engeller)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahneler deðiþse de beni silme!
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = normalVolume;

        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }
    }

    // Daktilo scripti bunu çaðýrýp sesi yumuþakça kýsacak
    public void SetLowVolume()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeVolume(lowVolume));
    }

    // Yazý bitince bunu çaðýrýp sesi yumuþakça geri açacak
    public void SetNormalVolume()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeVolume(normalVolume));
    }

    // Sesi aniden kesmek yerine sinematik bir þekilde azaltýp/arttýran sistem
    private IEnumerator FadeVolume(float targetVolume)
    {
        while (Mathf.Abs(audioSource.volume - targetVolume) > 0.01f)
        {
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }
}