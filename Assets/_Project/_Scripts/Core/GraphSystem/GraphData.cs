using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGraphData", menuName = "CellGame/GraphData")]
public class GraphData : ScriptableObject
{
    public List<GraphNode> nodes = new List<GraphNode>();

    /// <summary>
    /// Index'lerden neighbor referanslarını yeniden oluştur
    /// Her scene load'da veya GraphData kullanılmadan önce çağrılmalı
    /// </summary>
    public void RebuildNeighborReferences()
    {
        foreach (var node in nodes)
        {
            node.neighbors.Clear();
            foreach (int index in node.neighborIndices)
            {
                if (index >= 0 && index < nodes.Count)
                {
                    node.neighbors.Add(nodes[index]);
                }
            }
        }
    }

    private void OnEnable()
    {
        // Asset yüklendiğinde otomatik olarak neighbor'ları yeniden oluştur
        RebuildNeighborReferences();
    }
}
