using UnityEngine;

public class GraphManager : MonoBehaviour
{
    public GraphData graphData;
    public GraphNode selectedNode = null;

    private void OnDrawGizmos()
    {
        if(graphData == null || graphData.nodes == null) return;

        foreach(var node in graphData.nodes)
        {
            if(node == selectedNode) Gizmos.color =Color.yellow;
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
