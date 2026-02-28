using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxEffect : MonoBehaviour
{
    [Header("Referanslar")]
    public Camera cam;

    [Header("Parallax Ayarý")]
    [Tooltip("0 = Kameraya tam yapýþýr (birlikte gider). 1 = Dünyada sabit çakýlý kalýr.")]
    [Range(0f, 1f)]
    public float parallaxMultiplier = 0.0f; // TEST ÝÇÝN BUNU 0 YAPIN

    [Header("Boyutlandýrma")]
    public float scaleMultiplier = 1.5f;

    // Mutlak pozisyonlarý tutacaðýmýz deðiþkenler
    private Vector2 startPos;
    private Vector2 camStartPos;
    private float startZ;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Z eksenini (derinliði) kaybetmeden objeyi kameranýn tam ortasýna ýþýnla
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);

        ScaleToCameraSize();

        // Baþlangýç noktalarýný hafýzaya MÜHÜRLE
        startPos = transform.position;
        camStartPos = cam.transform.position;
        startZ = transform.position.z;
    }

    // Cinemachine güncellemelerinden sonra çalýþmasý için LateUpdate
    void LateUpdate()
    {
        // Kameranýn oyunun baþýndan beri TOPLAM ne kadar saptýðýný bul (Mutlak Matematik)
        Vector2 travel = (Vector2)cam.transform.position - camStartPos;

        // Çarpan 0 ise travel'ýn tamamýný ekler (Kameraya yapýþýr). 
        // Çarpan 0.5 ise travel'ýn yarýsýný ekler (Tatlý parallax derinliði verir).
        float newX = startPos.x + (travel.x * (1f - parallaxMultiplier));
        float newY = startPos.y + (travel.y * (1f - parallaxMultiplier));

        // Objeyi yeni konumuna taþý
        transform.position = new Vector3(newX, newY, startZ);
    }

    private void ScaleToCameraSize()
    {
        if (spriteRenderer.sprite == null) return;

        float cameraHeight = cam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * cam.aspect;

        float spriteHeight = spriteRenderer.sprite.bounds.size.y;
        float spriteWidth = spriteRenderer.sprite.bounds.size.x;

        transform.localScale = new Vector3((cameraWidth / spriteWidth) * scaleMultiplier, (cameraHeight / spriteHeight) * scaleMultiplier, 1f);
    }
}