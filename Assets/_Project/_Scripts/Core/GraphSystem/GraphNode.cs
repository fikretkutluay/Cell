using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class GraphNode
{
    public Vector2 position = Vector2.zero;
    [SerializeReference] public List<GraphNode> neighbors = new();
    public bool isDeadEnd = false;
}
