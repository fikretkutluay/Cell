using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GraphNode
{
    public Vector2 position = Vector2.zero;
    public List<int> neighborIndices = new List<int>(); // Node referansları yerine index kullan
    public bool isDeadEnd = false;

    // Runtime'da kullanılacak (kaydedilmez)
    [NonSerialized] public List<GraphNode> neighbors = new List<GraphNode>();
}
