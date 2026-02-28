using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GraphManager))]
public class GraphManagerEditor : Editor
{
    public void OnSceneGUI()
    {
        GraphManager manager = (GraphManager)target;

        if (manager.graphData == null || manager.graphData.nodes == null) return;
        Event e = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector2 worldPosition = new Vector2(ray.origin.x, ray.origin.y);

        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            e.Use();
            GraphNode newNode = new GraphNode();
            newNode.position = worldPosition;
            manager.graphData.nodes.Add(newNode);

            EditorUtility.SetDirty(manager.graphData);
        }

        else if (e.type == EventType.MouseDown && e.button == 0 && !e.shift)
        {
            GraphNode clickedNode = GetClosestNode(worldPosition, manager.graphData);
            if (clickedNode != null)
            {
                manager.selectedNode = clickedNode;
                SceneView.RepaintAll();
            }
        }

        else if (e.type == EventType.MouseDown && e.button == 1)
        {
            GraphNode targetNode = GetClosestNode(worldPosition, manager.graphData);
            if (targetNode != null && manager.selectedNode != null && targetNode != manager.selectedNode)
            {
                e.Use();
                if (!manager.selectedNode.neighbors.Contains(targetNode))
                {
                    manager.selectedNode.neighbors.Add(targetNode);
                    targetNode.neighbors.Add(manager.selectedNode);

                    EditorUtility.SetDirty(manager.graphData);
                    SceneView.RepaintAll();
                }
            }
        }
    }

    private GraphNode GetClosestNode(Vector2 mousePosition, GraphData data)
    {
        GraphNode closest = null;
        float minDistance = 0.5f;

        foreach (var node in data.nodes)
        {
            float dist = Vector2.Distance(mousePosition, node.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = node;
            }
        }
        return closest;
    }
}
