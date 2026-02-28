using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewGraphData", menuName = "CellGame/GraphData")]
public class GraphData : ScriptableObject
{
    public List<GraphNode> nodes = new();

}
