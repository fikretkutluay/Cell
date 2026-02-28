using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Üretim Ayarlarý")]
    public GameObject enemyPrefab; // Akyuvar Prefab'ýmýz
    public int enemyCount = 50;     // Haritada kaç tane düþman olacak?

    [Header("Referanslar")]
    public GraphData graphData;    // Düðümlerin yerini bilmek için
    public Transform player;       // Düþmanlara oyuncuyu hedef göstermek için

    [Header("Güvenlik")]
    [Tooltip("Düþmanlar oyuncunun en az bu kadar uzaðýnda doðmalý ki anýnda ölme")]
    public float safeDistanceFromPlayer = 10f;

    private void Start()
    {
        // Oyun baþladýðý anda düþmanlarý haritaya daðýt
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (graphData == null || graphData.nodes.Count == 0 || enemyPrefab == null)
        {
            Debug.LogWarning("Spawner eksik referans veya boþ GraphData yüzünden çalýþamadý!");
            return;
        }

        int spawned = 0;
        int attempts = 0; // Sonsuz döngüye girmemek için güvenlik

        while (spawned < enemyCount && attempts < 1000)
        {
            attempts++;

            // GraphData içinden rastgele bir düðüm (Node) seç
            int randomIndex = Random.Range(0, graphData.nodes.Count);
            GraphNode randomNode = graphData.nodes[randomIndex];

            // Seçilen düðüm oyuncuya çok mu yakýn? Öyleyse baþka bir tane seç (continue)
            if (player != null && Vector2.Distance(randomNode.position, player.position) < safeDistanceFromPlayer)
            {
                continue;
            }

            // Güvenli bir düðüm bulundu! Oraya düþmaný (Akyuvar) yarat
            GameObject newEnemy = Instantiate(enemyPrefab, randomNode.position, Quaternion.identity);

            // Yaratýlan bu yeni düþmanýn beynine (AI) gerekli verileri yükle
            LeukocyteAI aiScript = newEnemy.GetComponent<LeukocyteAI>();
            if (aiScript != null)
            {
                aiScript.graphData = graphData;
                aiScript.player = player;

                // Bölüm 1 için Ambush kapalý kalabilir, ilerleyen bölümlerde açýlabilir.
            }

            spawned++;
        }

        Debug.Log($"[Spawner] Haritaya baþarýyla {spawned} adet Akyuvar yerleþtirildi!");
    }
}