using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("�retim Ayarlar�")]
    public GameObject enemyPrefab; // Akyuvar Prefab'�m�z
    public int enemyCount = 50;     // Haritada ka� tane d��man olacak?

    [Header("Referanslar")]
    public GraphData graphData;    // D���mlerin yerini bilmek i�in
    public Transform player;       // D��manlara oyuncuyu hedef g�stermek i�in

    [Header("G�venlik")]
    [Tooltip("D��manlar oyuncunun en az bu kadar uza��nda do�mal� ki an�nda �lme")]
    public float safeDistanceFromPlayer = 10f;

    private void Start()
    {
        // Oyun ba�lad��� anda d��manlar� haritaya da��t
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (graphData == null || graphData.nodes.Count == 0 || enemyPrefab == null)
        {
            Debug.LogWarning("Spawner eksik referans veya bo� GraphData y�z�nden �al��amad�!");
            return;
        }

        int spawned = 0;
        int attempts = 0; // Sonsuz d�ng�ye girmemek i�in g�venlik

        while (spawned < enemyCount && attempts < 1000)
        {
            attempts++;

            // GraphData i�inden rastgele bir d���m (Node) se�
            int randomIndex = Random.Range(0, graphData.nodes.Count);
            GraphNode randomNode = graphData.nodes[randomIndex];

            // Se�ilen d���m oyuncuya �ok mu yak�n? �yleyse ba�ka bir tane se� (continue)
            if (player != null && Vector2.Distance(randomNode.position, player.position) < safeDistanceFromPlayer)
            {
                continue;
            }

            // G�venli bir d���m bulundu! Oraya d��man� (Akyuvar) yarat
            GameObject newEnemy = Instantiate(enemyPrefab, randomNode.position, Quaternion.identity);

            // Yarat�lan bu yeni d��man�n beynine (AI) player referans�n� y�kle
            // NOT: graphData art�k otomatik olarak GraphManager'dan al�n�yor
            LeukocyteAI aiScript = newEnemy.GetComponent<LeukocyteAI>();
            if (aiScript != null)
            {
                // graphData art�k otomatik bulunuyor, manuel atamaya gerek yok
                aiScript.player = player;

                // B�l�m 1 i�in Ambush kapal� kalabilir, ilerleyen b�l�mlerde a��labilir.
            }

            spawned++;
        }

        Debug.Log($"[Spawner] Haritaya ba�ar�yla {spawned} adet Akyuvar yerle�tirildi!");
    }
}