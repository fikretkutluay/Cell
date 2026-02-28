using System.Collections.Generic;
using UnityEngine;

public class PathfinderAStar : MonoBehaviour
{
    public static List<GraphNode> FindPath(GraphNode startNode, GraphNode targetNode)
    {
        if (startNode == null || targetNode == null) return null;

        List<GraphNode> openSet = new List<GraphNode>();
        HashSet<GraphNode> closedSet = new HashSet<GraphNode>();

        Dictionary<GraphNode, float> gCost = new Dictionary<GraphNode, float>();
        Dictionary<GraphNode, float> fCost = new Dictionary<GraphNode, float>();
        Dictionary<GraphNode, GraphNode> cameFrom = new Dictionary<GraphNode, GraphNode>();

        openSet.Add(startNode);
        gCost[startNode] = 0;
        fCost[startNode] = Vector2.Distance(startNode.position, targetNode.position);

        while (openSet.Count > 0)
        {
            GraphNode currentNode = openSet[0];
            for (int i = 0; i < openSet.Count; i++)
            {
                if (fCost.ContainsKey(openSet[i]) && fCost.ContainsKey(currentNode))
                {
                    if (fCost[openSet[i]] < fCost[currentNode] || (fCost[openSet[i]] == fCost[currentNode] && gCost[openSet[i]] < gCost[currentNode]))
                    {
                        currentNode = openSet[i];
                    }
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode, cameFrom);
            }

            foreach (GraphNode neighbor in currentNode.neighbors)
            {
                if (neighbor == null || closedSet.Contains(neighbor)) continue;

                float tentativeGCost = gCost[currentNode] + Vector2.Distance(currentNode.position, neighbor.position);
                float currentNeighborGCost = gCost.ContainsKey(neighbor) ? gCost[neighbor] : float.MaxValue;

                if (!openSet.Contains(neighbor) || tentativeGCost < currentNeighborGCost)
                {
                    cameFrom[neighbor] = currentNode;
                    gCost[neighbor] = tentativeGCost;
                    fCost[neighbor] = tentativeGCost + Vector2.Distance(neighbor.position, targetNode.position);
                }

                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);
            }
        }

        return null;

    }

    private static List<GraphNode> RetracePath(GraphNode startNode, GraphNode endNode, Dictionary<GraphNode, GraphNode> cameFrom)
    {
        List<GraphNode> path = new List<GraphNode>();
        GraphNode currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            if (cameFrom.ContainsKey(currentNode))
                currentNode = cameFrom[currentNode];
            else
                break;
        }
        path.Reverse();
        return path;
    }
}
