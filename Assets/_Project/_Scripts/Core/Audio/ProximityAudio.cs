using System.Collections.Generic;
using UnityEngine;

public class ProximityAudio : MonoBehaviour
{
    public Transform player;
    public Transform boss;
    public GraphData graphData;

    public int maxHearingDistance = 200;
    public float updateInterval = 0.2f;

    private float lastUpdateTime;
    private float targetVolume = 0f;
    private float targetPitch = 1f;
    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.playOnAwake = true;

        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    private void Update()
    {
        if (player == null || boss == null || graphData == null || graphData.nodes.Count == 0) return;

        if (Time.time > lastUpdateTime + updateInterval)
        {
            lastUpdateTime = Time.time;
            CalculateProximity();
        }

        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * 3f);
        audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, Time.deltaTime * 3f);
    }

    private void CalculateProximity()
    {
        GraphNode playerNode = GetClosestNode(player.position);
        GraphNode bossNode = GetClosestNode(boss.position);

        if (playerNode == null || bossNode == null) return;

        int distance = GetBFSNodeDistance(playerNode, bossNode);

        if (distance == -1 || distance > maxHearingDistance)
        {
            targetVolume = 0f;
            targetPitch = 1f;
            return;
        }

        float volumeLevel = 1f - ((float)distance / maxHearingDistance);
        targetVolume = Mathf.Clamp01(volumeLevel);

        targetPitch = 1f + (targetVolume * 0.5f);
    }

    private GraphNode GetClosestNode(Vector2 pos)
    {
        GraphNode closest = null;
        float minDistance = float.MaxValue;

        foreach (var node in graphData.nodes)
        {
            float dist = Vector2.Distance(pos, node.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = node;
            }
        }
        return closest;
    }

    private int GetBFSNodeDistance(GraphNode startNode, GraphNode targetNode)
    {
        if (startNode == targetNode) return 0;

        Queue<GraphNode> queue = new Queue<GraphNode>();
        HashSet<GraphNode> visited = new HashSet<GraphNode>();
        Dictionary<GraphNode, int> distances = new Dictionary<GraphNode, int>();

        queue.Enqueue(startNode);
        visited.Add(startNode);
        distances[startNode] = 0;

        while (queue.Count > 0)
        {
            GraphNode currentNode = queue.Dequeue();

            if (currentNode == targetNode)
            {
                return distances[currentNode];
            }

            foreach (GraphNode neighbor in currentNode.neighbors)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    distances[neighbor] = distances[currentNode] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return -1;
    }
}
