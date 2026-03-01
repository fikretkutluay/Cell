using UnityEngine;

/// <summary>
/// Her sahnede bir GraphManager olmalı
/// Enemy'ler bu singleton'dan GraphData'yı otomatik alır
/// </summary>
public class GraphManager : MonoBehaviour
{
    public static GraphManager Instance { get; private set; }

    [Header("Graph Data")]
    public GraphData graphData;
    
    [Header("Editor Only")]
    public GraphNode selectedNode = null;

    private void Awake()
    {
        // Singleton pattern - her sahnede sadece bir GraphManager
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple GraphManagers found in scene! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // GraphData kontrolü ve neighbor referanslarını yeniden oluştur
        if (graphData == null)
        {
            Debug.LogError("GraphManager: GraphData is not assigned!");
        }
        else
        {
            graphData.RebuildNeighborReferences();
            Debug.Log($"GraphManager initialized with {graphData.nodes.Count} nodes");
        }
    }

    private void OnDestroy()
    {
        // Scene değiştiğinde Instance'ı temizle
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnDrawGizmos()
    {
        if(graphData == null || graphData.nodes == null) return;

        foreach(var node in graphData.nodes)
        {
            if(node == selectedNode) Gizmos.color = Color.yellow;
            else if (node.isDeadEnd) Gizmos.color = Color.red;
            else Gizmos.color = Color.green;

            Gizmos.DrawSphere(node.position, .2f);

            foreach(var neighbour in node.neighbors)
            {
                Gizmos.DrawLine(node.position, neighbour.position);
            }
        }
    }
}
