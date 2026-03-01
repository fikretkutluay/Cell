using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Leukocyte (Akyuvar) AI - Patrol, Chase ve Ambush modları
/// GraphData'yı otomatik olarak GraphManager'dan alır
/// </summary>
public class LeukocyteAI : MonoBehaviour
{
    public enum AIState { Patrol, Chase, Ambush }

    [Header("AI Ayarları")]
    public AIState currentState = AIState.Patrol;
    public float moveSpeed = 5f;
    public float sightRange = 20f;
    public float pathUpdateInterval = 0.2f;

    [Header("Ambush Ayarları")]
    public bool canAmbush = true;
    public float ambushPredictionDistance = 3f;

    [Header("Referanslar")]
    public Transform player;

    [HideInInspector] public List<GraphNode> currentPath = new List<GraphNode>();
    private GraphData graphData; // Otomatik bulunacak
    private float lastPathUpdateTime;
    private Vector2 lastPlayerPos;

    private void Start()
    {
        // GraphData'yı otomatik bul
        if (GraphManager.Instance != null)
        {
            graphData = GraphManager.Instance.graphData;
            if (graphData == null)
            {
                Debug.LogError("LeukocyteAI: GraphManager.graphData is null!");
            }
        }
        else
        {
            Debug.LogError("LeukocyteAI: GraphManager not found in scene! Enemy AI will not work.");
        }

        // Player'ı otomatik bul (eğer atanmamışsa)
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("LeukocyteAI: Player not found! Make sure Player has 'Player' tag.");
            }
        }

        if (player != null)
        {
            lastPlayerPos = player.position;
        }
    }

    private void Update()
    {
        if (player == null || graphData == null || graphData.nodes.Count == 0) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // State geçişleri
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
                lastPathUpdateTime = 0;
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

        // State logic - sadece gerektiğinde path hesapla
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
                if (myNode == playerNode)
                {
                    if (!IsWallBetweenPositions(transform.position, player.position))
                    {
                        currentPath.Clear();
                    }
                    else
                    {
                        currentPath = new List<GraphNode> { myNode };
                    }
                    return;
                }

                List<GraphNode> newPath = PathfinderAStar.FindPath(myNode, playerNode);
                if (newPath != null && newPath.Count > 0)
                {
                    currentPath = newPath;
                }
                else
                {
                    currentPath.Clear();
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

            Vector2 playerVelocity = Vector2.zero;
            if (Time.deltaTime > 0f) playerVelocity = ((Vector2)player.position - lastPlayerPos) / Time.deltaTime;
            playerVelocity = Vector2.ClampMagnitude(playerVelocity, 10f);

            GraphNode ambushTarget = null;

            if (playerVelocity.magnitude > 0.1f)
            {
                Vector2 predictedPos = (Vector2)player.position + (playerVelocity.normalized * ambushPredictionDistance);
                ambushTarget = GetClosestNode(predictedPos);
            }

            if (ambushTarget == null) ambushTarget = GetClosestNode(player.position);

            if (myNode != null && ambushTarget != null)
            {
                if (myNode == ambushTarget)
                {
                    if (!IsWallBetweenPositions(transform.position, player.position))
                    {
                        currentPath.Clear();
                    }
                    else
                    {
                        currentPath = new List<GraphNode> { myNode };
                    }
                    return;
                }

                List<GraphNode> newPath = PathfinderAStar.FindPath(myNode, ambushTarget);
                if (newPath != null && newPath.Count > 0)
                {
                    currentPath = newPath;
                }
                else
                {
                    currentPath.Clear();
                    currentState = AIState.Chase;
                }
            }
        }
    }

    private void MoveAlongPath()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            GraphNode targetNode = currentPath[0];
            transform.position = Vector2.MoveTowards(transform.position, targetNode.position, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetNode.position) < 0.4f)
            {
                currentPath.RemoveAt(0);
            }
        }
        else
        {
            if (currentState == AIState.Chase || currentState == AIState.Ambush)
            {
                if (!IsWallBetweenPositions(transform.position, player.position))
                {
                    transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                }
            }
        }
    }

    private bool IsWallBetweenPositions(Vector2 start, Vector2 end)
    {
        Vector2 dir = end - start;
        float dist = dir.magnitude;

        RaycastHit2D[] hits = Physics2D.RaycastAll(start, dir.normalized, dist);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }

    private GraphNode GetClosestNode(Vector2 pos)
    {
        if (graphData == null || graphData.nodes == null) return null;

        GraphNode closest = null;
        float minDistance = float.MaxValue;

        foreach (var node in graphData.nodes)
        {
            float dist = Vector2.Distance(pos, node.position);

            if (dist < minDistance)
            {
                if (!IsWallBetweenPositions(pos, node.position))
                {
                    minDistance = dist;
                    closest = node;
                }
            }
        }

        if (closest == null)
        {
            minDistance = float.MaxValue;
            foreach (var node in graphData.nodes)
            {
                float dist = Vector2.Distance(pos, node.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = node;
                }
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
