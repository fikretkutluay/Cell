using System.Collections.Generic;
using UnityEngine;

public class LeukocyteAI : MonoBehaviour
{
    public enum AIState { Patrol, Chase, Ambush }

    [Header("AI Ayarlarý")]
    public AIState currentState = AIState.Patrol;
    public float moveSpeed = 10f;
    public float sightRange = 20f; 
    public float pathUpdateInterval = 0.2f; 

    [Header("Ambush Ayarlarý")]
    public bool canAmbush = true; 
    public float ambushPredictionDistance = 3f;

    [Header("Referanslar")]
    public GraphData graphData;
    public Transform player;

    private List<GraphNode> currentPath = new();
    private float lastPathUpdateTime;

    private Vector2 lastPlayerPos;

    private void Start()
    {
        if (player != null)
        {
            lastPlayerPos = player.position;
        }
    }

    private void Update()
    {
        if (player == null || graphData == null || graphData.nodes.Count == 0) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= sightRange)
        {
            if (currentState == AIState.Patrol)
            {
                if (canAmbush && Random.value > 0.5f)
                {
                    currentState = AIState.Ambush;
                }
                else
                {
                    currentState = AIState.Chase;
                }
                currentPath.Clear();
            }
        }
        else
        {
            if (currentState != AIState.Patrol)
            {
                currentState = AIState.Patrol;
                currentPath.Clear();
            }
        }

        switch (currentState)
        {
            case AIState.Patrol:
                DoPatrol();
                break;
            case AIState.Chase:
                DoChase();
                break;
            case AIState.Ambush:
                DoAmbush();
                break;
        }

        MoveAlongPath();
        lastPlayerPos = player.position;
    }

    private void DoPatrol()
    {
        if (currentPath.Count == 0)
        {
            GraphNode myNode = GetClosestNode(transform.position);
            if (myNode != null && myNode.neighbors.Count > 0)
            {
                int randomIndex = Random.Range(0, myNode.neighbors.Count);
                GraphNode randomNeighbor = myNode.neighbors[randomIndex];
                currentPath.Add(randomNeighbor);
            }
        }
    }

    private void DoChase()
    {
        if (Time.time > lastPathUpdateTime + pathUpdateInterval)
        {
            lastPathUpdateTime = Time.time;

            GraphNode myNode = GetClosestNode(transform.position);
            GraphNode playerNode = GetClosestNode(player.position);

            if (myNode != null && playerNode != null)
            {
                List<GraphNode> newPath = PathfinderAStar.FindPath(myNode, playerNode);
                if (newPath != null)
                {
                    currentPath = newPath;
                }
            }
        }
    }

    private void DoAmbush()
    {
        if (Time.time > lastPathUpdateTime + pathUpdateInterval)
        {
            lastPathUpdateTime = Time.time;

            GraphNode myNode = GetClosestNode(transform.position);

            Vector2 playerVelocity = ((Vector2)player.position - lastPlayerPos) / Time.deltaTime;

            GraphNode ambushTarget = null;

            if (playerVelocity.magnitude > 0.1f)
            {
                Vector2 predictedPos = (Vector2)player.position + (playerVelocity.normalized * ambushPredictionDistance);
                ambushTarget = GetClosestNode(predictedPos);
            }

            if (ambushTarget == null)
            {
                ambushTarget = GetClosestNode(player.position);
            }

            if (myNode != null && ambushTarget != null)
            {
                List<GraphNode> newPath = PathfinderAStar.FindPath(myNode, ambushTarget);
                if (newPath != null)
                {
                    currentPath = newPath;
                }
            }
        }
    }

    private void MoveAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        GraphNode targetNode = currentPath[0];
        transform.position = Vector2.MoveTowards(transform.position, targetNode.position, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetNode.position) < 0.1f)
        {
            currentPath.RemoveAt(0);
        }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}