# Design Document: Cell Game Jam

## Overview

Cell Game Jam is a Unity 2D top-down action game built with C# that uses a graph-based navigation system for intelligent enemy AI. The design prioritizes rapid development for a 36-hour game jam while maintaining clean architecture for potential post-jam expansion.

The core technical approach uses:
- **ScriptableObject-based graph system** for vessel networks (no NavMesh)
- **State machine AI** for Leukocyte behaviors (Patrol, Chase, Ambush)
- **BFS pathfinding** for simplicity and reliability
- **Rigidbody2D physics** for player movement and collision
- **Static singleton pattern** for cross-scene stat persistence
- **Modular component architecture** for rapid iteration

This design document provides specific Unity implementation details including GameObject hierarchies, component configurations, Inspector values, and C# class structures.

---

## Architecture

### High-Level System Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        Game Manager                          │
│  (Singleton, DontDestroyOnLoad, manages global state)       │
└─────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
┌───────▼────────┐   ┌────────▼────────┐   ┌───────▼────────┐
│  Stat System   │   │  Scene Manager  │   │  Graph System  │
│  (Persistent)  │   │  (Transitions)  │   │ (Per-Section)  │
└────────────────┘   └─────────────────┘   └────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
┌───────▼────────┐   ┌────────▼────────┐   ┌───────▼────────┐
│  Player Cell   │   │  Leukocyte AI   │   │  Boss Entity   │
│  (Controller)  │   │ (State Machine) │   │   (Unique)     │
└────────┬───────┘   └────────┬────────┘   └────────┬───────┘
         │                    │                      │
         └────────────────────┼──────────────────────┘
                              │
                    ┌─────────▼─────────┐
                    │  Combat System    │
                    │ (Collision-based) │
                    └───────────────────┘
```

### Scene Structure

Each section (Lung, Heart, Brain) is a separate Unity scene with the following hierarchy:

```
LungScene
├── GameManager (DontDestroyOnLoad, persists across scenes)
├── GraphData (ScriptableObject reference in scene)
├── Environment
│   ├── Background (SpriteRenderer)
│   ├── Walls (TilemapCollider2D or PolygonCollider2D)
│   └── Hazards (optional, section-specific)
├── Nodes (Empty GameObjects marking graph positions)
│   ├── Node_0 (Transform position)
│   ├── Node_1
│   └── ... (all graph nodes)
├── Player
│   ├── PlayerCell (Rigidbody2D, CircleCollider2D, PlayerController script)
│   └── PlayerSprite (SpriteRenderer child)
├── Enemies
│   ├── Leukocyte_1 (Rigidbody2D, CircleCollider2D, LeukocyteAI script)
│   ├── Leukocyte_2
│   └── ...
├── Boss (spawned at end of section)
│   └── BossEntity (specific boss script)
├── UI
│   ├── Canvas
│   │   ├── ProximityUI (displays distance to boss)
│   │   ├── HealthBar
│   │   └── StatDisplay
│   └── EventSystem
└── SceneTransition (trigger collider at boss location)
```

---

## Components and Interfaces

### 1. Graph System

The graph system uses ScriptableObjects to define vessel networks. Each section has its own GraphData asset.

#### GraphData ScriptableObject

**File:** `Assets/Scripts/Graph/GraphData.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GraphData", menuName = "CellGame/GraphData")]
public class GraphData : ScriptableObject
{
    [System.Serializable]
    public class Node
    {
        public int id;
        public Vector2 position;
        public bool isDeadEnd;
        public List<int> connectedNodeIds = new List<int>();
    }

    public List<Node> nodes = new List<Node>();
    
    // Helper method to get node by ID
    public Node GetNode(int id)
    {
        return nodes.Find(n => n.id == id);
    }
    
    // Get adjacent nodes for pathfinding
    public List<Node> GetAdjacentNodes(int nodeId)
    {
        Node node = GetNode(nodeId);
        if (node == null) return new List<Node>();
        
        List<Node> adjacent = new List<Node>();
        foreach (int connectedId in node.connectedNodeIds)
        {
            Node connectedNode = GetNode(connectedId);
            if (connectedNode != null)
                adjacent.Add(connectedNode);
        }
        return adjacent;
    }
}
```

**Unity Setup:**
1. Create ScriptableObject: Right-click in Project → Create → CellGame → GraphData
2. Name it `LungGraphData`, `HeartGraphData`, or `BrainGraphData`
3. In Inspector, add nodes manually:
   - Set `id` (0, 1, 2, ...)
   - Set `position` (world coordinates matching Node GameObjects)
   - Set `isDeadEnd` (true for terminal nodes)
   - Add `connectedNodeIds` (IDs of adjacent nodes)

#### GraphManager Component

**File:** `Assets/Scripts/Graph/GraphManager.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

public class GraphManager : MonoBehaviour
{
    public static GraphManager Instance { get; private set; }
    
    [SerializeField] private GraphData graphData;
    [SerializeField] private Transform nodesParent; // Parent GameObject containing all Node transforms
    
    private Dictionary<int, Transform> nodeTransforms = new Dictionary<int, Transform>();
    
    void Awake()
    {
        Instance = this;
        InitializeNodeTransforms();
    }
    
    private void InitializeNodeTransforms()
    {
        // Map node IDs to actual Transform positions in the scene
        if (nodesParent == null) return;
        
        for (int i = 0; i < nodesParent.childCount; i++)
        {
            Transform nodeTransform = nodesParent.GetChild(i);
            // Assume node GameObjects are named "Node_0", "Node_1", etc.
            string nodeName = nodeTransform.name;
            if (nodeName.StartsWith("Node_"))
            {
                int nodeId = int.Parse(nodeName.Substring(5));
                nodeTransforms[nodeId] = nodeTransform;
            }
        }
    }
    
    public GraphData.Node GetNode(int id)
    {
        return graphData.GetNode(id);
    }
    
    public List<GraphData.Node> GetAdjacentNodes(int nodeId)
    {
        return graphData.GetAdjacentNodes(nodeId);
    }
    
    public Transform GetNodeTransform(int nodeId)
    {
        return nodeTransforms.ContainsKey(nodeId) ? nodeTransforms[nodeId] : null;
    }
    
    public Vector2 GetNodePosition(int nodeId)
    {
        GraphData.Node node = GetNode(nodeId);
        return node != null ? node.position : Vector2.zero;
    }
    
    // Find closest node to a world position
    public int GetClosestNodeId(Vector2 worldPosition)
    {
        int closestId = -1;
        float closestDistance = float.MaxValue;
        
        foreach (var node in graphData.nodes)
        {
            float distance = Vector2.Distance(worldPosition, node.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestId = node.id;
            }
        }
        
        return closestId;
    }
}
```

**Unity Setup:**
1. Create empty GameObject named "GraphManager" in scene
2. Add `GraphManager` component
3. Assign the appropriate `GraphData` ScriptableObject to `graphData` field
4. Create empty GameObject named "Nodes" as parent
5. Create child empty GameObjects named "Node_0", "Node_1", etc.
6. Position each Node GameObject at vessel intersection points
7. Assign "Nodes" GameObject to `nodesParent` field in GraphManager

---

### 2. Pathfinding System

BFS pathfinding is chosen for simplicity and reliability. It guarantees shortest path on unweighted graphs and is easier to debug during a game jam than A*.

#### Pathfinder Class

**File:** `Assets/Scripts/Graph/Pathfinder.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

public class Pathfinder
{
    private GraphManager graphManager;
    
    public Pathfinder(GraphManager manager)
    {
        graphManager = manager;
    }
    
    // BFS pathfinding - returns list of node IDs from start to goal
    public List<int> FindPath(int startNodeId, int goalNodeId)
    {
        if (startNodeId == goalNodeId)
            return new List<int> { startNodeId };
        
        Queue<int> frontier = new Queue<int>();
        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        
        frontier.Enqueue(startNodeId);
        cameFrom[startNodeId] = -1;
        
        while (frontier.Count > 0)
        {
            int current = frontier.Dequeue();
            
            if (current == goalNodeId)
            {
                return ReconstructPath(cameFrom, startNodeId, goalNodeId);
            }
            
            List<GraphData.Node> neighbors = graphManager.GetAdjacentNodes(current);
            foreach (var neighbor in neighbors)
            {
                if (!cameFrom.ContainsKey(neighbor.id))
                {
                    frontier.Enqueue(neighbor.id);
                    cameFrom[neighbor.id] = current;
                }
            }
        }
        
        // No path found
        return new List<int>();
    }
    
    private List<int> ReconstructPath(Dictionary<int, int> cameFrom, int start, int goal)
    {
        List<int> path = new List<int>();
        int current = goal;
        
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        
        path.Add(start);
        path.Reverse();
        return path;
    }
    
    // Calculate distance (path length) between two nodes
    public int CalculateDistance(int startNodeId, int goalNodeId)
    {
        List<int> path = FindPath(startNodeId, goalNodeId);
        return path.Count > 0 ? path.Count - 1 : -1; // -1 means no path
    }
}
```

**Usage:** Instantiate `Pathfinder` in AI scripts and pass `GraphManager.Instance`.

---

### 3. Player Controller

The player uses Rigidbody2D for physics-based movement with immediate input response.

#### PlayerController Component

**File:** `Assets/Scripts/Player/PlayerController.cs`

```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveInput;
    
    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 5f;
    
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }
    
    void Update()
    {
        // Read input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize(); // Prevent faster diagonal movement
    }
    
    void FixedUpdate()
    {
        // Apply movement with stat-modified speed
        float currentSpeed = baseMoveSpeed * StatSystem.Instance.GetSpeedMultiplier();
        rb.velocity = moveInput * currentSpeed;
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // Game over logic
        Debug.Log("Player died!");
        // TODO: Trigger respawn or game over screen
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }
}
```

**Unity Setup:**
1. Create GameObject named "Player"
2. Add `Rigidbody2D` component:
   - Body Type: Dynamic
   - Gravity Scale: 0 (top-down game)
   - Linear Drag: 0
   - Angular Drag: 0
   - Constraints: Freeze Rotation Z
3. Add `CircleCollider2D`:
   - Radius: 0.5 (adjust based on sprite size)
   - Is Trigger: false
4. Add `PlayerController` script:
   - Base Move Speed: 5
   - Max Health: 100
5. Create child GameObject "PlayerSprite" with `SpriteRenderer`:
   - Assign player cell sprite
   - Sorting Layer: "Player" (create if needed)
   - Order in Layer: 10

---

### 4. Leukocyte AI System

Leukocytes use a state machine with three states: Patrol, Chase, and Ambush.

#### LeukocyteAI Component

**File:** `Assets/Scripts/AI/LeukocyteAI.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class LeukocyteAI : MonoBehaviour
{
    public enum AIState { Patrol, Chase, Ambush }
    
    [Header("AI Configuration")]
    [SerializeField] private AIState currentState = AIState.Patrol;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sightRange = 5f;
    [SerializeField] private bool enableAmbush = false; // Enable in Heart/Brain sections
    
    [Header("Combat")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float damageInterval = 1f; // Damage cooldown
    
    [Header("Patrol")]
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float nodeReachedThreshold = 0.5f;
    
    private Rigidbody2D rb;
    private Transform player;
    private Pathfinder pathfinder;
    private GraphManager graphManager;
    
    private int currentNodeId = -1;
    private int targetNodeId = -1;
    private List<int> currentPath = new List<int>();
    private int pathIndex = 0;
    
    private float currentHealth;
    private float patrolTimer = 0f;
    private float lastDamageTime = 0f;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        graphManager = GraphManager.Instance;
        pathfinder = new Pathfinder(graphManager);
        
        // Find closest node to spawn position
        currentNodeId = graphManager.GetClosestNodeId(transform.position);
        
        if (currentState == AIState.Patrol)
        {
            ChooseRandomPatrolTarget();
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // State transitions
        if (distanceToPlayer <= sightRange && currentState == AIState.Patrol)
        {
            currentState = enableAmbush ? AIState.Ambush : AIState.Chase;
            StartChase();
        }
        else if (distanceToPlayer > sightRange * 1.5f && currentState != AIState.Patrol)
        {
            currentState = AIState.Patrol;
            ChooseRandomPatrolTarget();
        }
        
        // State behaviors
        switch (currentState)
        {
            case AIState.Patrol:
                UpdatePatrol();
                break;
            case AIState.Chase:
                UpdateChase();
                break;
            case AIState.Ambush:
                UpdateAmbush();
                break;
        }
    }
    
    void FixedUpdate()
    {
        MoveTowardsTarget();
    }
    
    private void UpdatePatrol()
    {
        if (targetNodeId == -1)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime)
            {
                ChooseRandomPatrolTarget();
                patrolTimer = 0f;
            }
        }
    }
    
    private void ChooseRandomPatrolTarget()
    {
        List<GraphData.Node> adjacentNodes = graphManager.GetAdjacentNodes(currentNodeId);
        if (adjacentNodes.Count > 0)
        {
            int randomIndex = Random.Range(0, adjacentNodes.Count);
            targetNodeId = adjacentNodes[randomIndex].id;
            currentPath = new List<int> { currentNodeId, targetNodeId };
            pathIndex = 1;
        }
    }
    
    private void StartChase()
    {
        int playerNodeId = graphManager.GetClosestNodeId(player.position);
        currentPath = pathfinder.FindPath(currentNodeId, playerNodeId);
        pathIndex = currentPath.Count > 0 ? 1 : 0;
    }
    
    private void UpdateChase()
    {
        // Recalculate path periodically
        if (Time.frameCount % 30 == 0) // Every 30 frames (~0.5 seconds)
        {
            int playerNodeId = graphManager.GetClosestNodeId(player.position);
            currentPath = pathfinder.FindPath(currentNodeId, playerNodeId);
            pathIndex = currentPath.Count > 0 ? 1 : 0;
        }
    }
    
    private void UpdateAmbush()
    {
        // Ambush: predict player's path and cut them off
        // For simplicity, find a node ahead of the player's current direction
        int playerNodeId = graphManager.GetClosestNodeId(player.position);
        List<GraphData.Node> playerAdjacentNodes = graphManager.GetAdjacentNodes(playerNodeId);
        
        if (playerAdjacentNodes.Count > 0)
        {
            // Pick a node ahead of player
            int ambushNodeId = playerAdjacentNodes[0].id;
            currentPath = pathfinder.FindPath(currentNodeId, ambushNodeId);
            pathIndex = currentPath.Count > 0 ? 1 : 0;
        }
    }
    
    private void MoveTowardsTarget()
    {
        if (currentPath.Count == 0 || pathIndex >= currentPath.Count)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        
        int nextNodeId = currentPath[pathIndex];
        Vector2 targetPosition = graphManager.GetNodePosition(nextNodeId);
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        rb.velocity = direction * moveSpeed;
        
        // Check if reached node
        if (Vector2.Distance(transform.position, targetPosition) < nodeReachedThreshold)
        {
            currentNodeId = nextNodeId;
            pathIndex++;
            
            if (pathIndex >= currentPath.Count)
            {
                // Reached end of path
                targetNodeId = -1;
                currentPath.Clear();
            }
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time - lastDamageTime >= damageInterval)
            {
                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(contactDamage);
                    lastDamageTime = Time.time;
                }
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // Award stat points to player
        StatSystem.Instance.AwardStatPoints(1);
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualize sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
```

**Unity Setup:**
1. Create GameObject named "Leukocyte"
2. Add `Rigidbody2D`:
   - Body Type: Dynamic
   - Gravity Scale: 0
   - Constraints: Freeze Rotation Z
3. Add `CircleCollider2D`:
   - Radius: 0.4
   - Is Trigger: false
4. Add `LeukocyteAI` script:
   - Current State: Patrol
   - Move Speed: 3 (Lung), 4 (Heart), 5 (Brain)
   - Sight Range: 5
   - Enable Ambush: false (Lung), true (Heart/Brain)
   - Max Health: 50
   - Contact Damage: 10
   - Damage Interval: 1
   - Patrol Wait Time: 2
   - Node Reached Threshold: 0.5
5. Tag GameObject as "Enemy"
6. Create child "LeukocyteSprite" with `SpriteRenderer`

---

### 5. Combat System

Combat is collision-based with damage cooldowns to prevent instant death.

#### CombatManager (Optional Active Attack System)

**File:** `Assets/Scripts/Combat/CombatManager.cs`

```csharp
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    
    void Awake()
    {
        Instance = this;
    }
    
    // Optional: Active attack system
    public void PlayerAttack(Vector2 attackPosition, float attackRadius, float damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, attackRadius);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                LeukocyteAI enemy = hit.GetComponent<LeukocyteAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}
```

**Unity Setup:**
1. Create empty GameObject "CombatManager"
2. Add `CombatManager` script
3. For active attacks, add input handling in `PlayerController`:

```csharp
// In PlayerController.Update()
if (Input.GetKeyDown(KeyCode.Space))
{
    float attackDamage = 20f * StatSystem.Instance.GetDamageMultiplier();
    CombatManager.Instance.PlayerAttack(transform.position, 1.5f, attackDamage);
}
```

---

### 6. Stat System

The stat system persists across scenes using a singleton pattern with DontDestroyOnLoad.

#### StatSystem Component

**File:** `Assets/Scripts/Systems/StatSystem.cs`

```csharp
using UnityEngine;

public class StatSystem : MonoBehaviour
{
    public static StatSystem Instance { get; private set; }
    
    [Header("Base Stats")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseSpeed = 1f;
    [SerializeField] private float baseHealth = 100f;
    
    [Header("Current Stats")]
    private int damageLevel = 0;
    private int speedLevel = 0;
    private int healthLevel = 0;
    
    private int availableStatPoints = 0;
    
    [Header("Stat Scaling")]
    [SerializeField] private float damagePerLevel = 5f;
    [SerializeField] private float speedPerLevel = 0.2f;
    [SerializeField] private float healthPerLevel = 20f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AwardStatPoints(int points)
    {
        availableStatPoints += points;
        Debug.Log($"Awarded {points} stat points. Total: {availableStatPoints}");
    }
    
    public void UpgradeDamage()
    {
        if (availableStatPoints > 0)
        {
            damageLevel++;
            availableStatPoints--;
        }
    }
    
    public void UpgradeSpeed()
    {
        if (availableStatPoints > 0)
        {
            speedLevel++;
            availableStatPoints--;
        }
    }
    
    public void UpgradeHealth()
    {
        if (availableStatPoints > 0)
        {
            healthLevel++;
            availableStatPoints--;
        }
    }
    
    public float GetDamageMultiplier()
    {
        return 1f + (damageLevel * damagePerLevel / baseDamage);
    }
    
    public float GetSpeedMultiplier()
    {
        return 1f + (speedLevel * speedPerLevel / baseSpeed);
    }
    
    public float GetMaxHealth()
    {
        return baseHealth + (healthLevel * healthPerLevel);
    }
    
    public int GetAvailableStatPoints()
    {
        return availableStatPoints;
    }
    
    public int GetDamageLevel() => damageLevel;
    public int GetSpeedLevel() => speedLevel;
    public int GetHealthLevel() => healthLevel;
}
```

**Unity Setup:**
1. Create empty GameObject "GameManager" in first scene (Lung)
2. Add `StatSystem` script:
   - Base Damage: 10
   - Base Speed: 1
   - Base Health: 100
   - Damage Per Level: 5
   - Speed Per Level: 0.2
   - Health Per Level: 20
3. This GameObject will persist across all scenes

---

### 7. Proximity UI System

Displays distance to boss using BFS path calculation.

#### ProximityUI Component

**File:** `Assets/Scripts/UI/ProximityUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProximityUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform boss;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private Image signalBar; // Optional: visual bar
    
    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.5f; // Update every 0.5 seconds
    
    private Pathfinder pathfinder;
    private GraphManager graphManager;
    private float updateTimer = 0f;
    
    void Start()
    {
        graphManager = GraphManager.Instance;
        pathfinder = new Pathfinder(graphManager);
        
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (boss == null)
            boss = GameObject.FindGameObjectWithTag("Boss")?.transform;
    }
    
    void Update()
    {
        updateTimer += Time.deltaTime;
        
        if (updateTimer >= updateInterval)
        {
            UpdateDistance();
            updateTimer = 0f;
        }
    }
    
    private void UpdateDistance()
    {
        if (player == null || boss == null || graphManager == null)
            return;
        
        int playerNodeId = graphManager.GetClosestNodeId(player.position);
        int bossNodeId = graphManager.GetClosestNodeId(boss.position);
        
        int distance = pathfinder.CalculateDistance(playerNodeId, bossNodeId);
        
        if (distance >= 0)
        {
            distanceText.text = $"Boss Distance: {distance}";
            
            // Optional: Update signal bar (inverse - closer = fuller bar)
            if (signalBar != null)
            {
                float maxDistance = 20f; // Adjust based on level size
                float fillAmount = 1f - Mathf.Clamp01(distance / maxDistance);
                signalBar.fillAmount = fillAmount;
            }
        }
        else
        {
            distanceText.text = "Boss: No Path";
        }
    }
}
```

**Unity Setup:**
1. In Canvas, create UI → Text - TextMeshPro named "DistanceText"
2. Position in top-right corner
3. Create UI → Image named "SignalBar" (optional)
4. Set Image Type to "Filled", Fill Method to "Horizontal"
5. Create empty GameObject "ProximityUI" in Canvas
6. Add `ProximityUI` script:
   - Assign Player transform
   - Assign Boss transform (or leave null, will find by tag)
   - Assign DistanceText
   - Assign SignalBar (optional)
   - Update Interval: 0.5

---

## Data Models

### GraphData Structure

```csharp
// Node representation
public class Node
{
    public int id;                      // Unique identifier
    public Vector2 position;            // World space coordinates
    public bool isDeadEnd;              // Terminal node flag
    public List<int> connectedNodeIds; // Adjacency list
}

// Example graph for Lung section (simplified)
LungGraphData:
  nodes:
    - id: 0, position: (0, 0), isDeadEnd: false, connectedNodeIds: [1, 2]
    - id: 1, position: (5, 0), isDeadEnd: false, connectedNodeIds: [0, 3]
    - id: 2, position: (0, 5), isDeadEnd: true, connectedNodeIds: [0]
    - id: 3, position: (10, 0), isDeadEnd: false, connectedNodeIds: [1, 4]
    - id: 4, position: (10, 5), isDeadEnd: false, connectedNodeIds: [3] // Boss node
```

### Stat Data Structure

```csharp
// Persistent stat data (stored in StatSystem singleton)
public class StatData
{
    public int damageLevel;
    public int speedLevel;
    public int healthLevel;
    public int availableStatPoints;
}

// Calculated values
public float GetDamageMultiplier() => 1f + (damageLevel * damagePerLevel / baseDamage);
public float GetSpeedMultiplier() => 1f + (speedLevel * speedPerLevel / baseSpeed);
public float GetMaxHealth() => baseHealth + (healthLevel * healthPerLevel);
```

### AI State Data

```csharp
// Leukocyte AI state
public enum AIState
{
    Patrol,  // Random movement between nodes
    Chase,   // Direct pursuit using pathfinding
    Ambush   // Predictive interception
}

// AI runtime data
public class AIData
{
    public AIState currentState;
    public int currentNodeId;
    public int targetNodeId;
    public List<int> currentPath;
    public int pathIndex;
    public float patrolTimer;
}
```

### Boss Data Structure

```csharp
// Boss configuration (per section)
public class BossData
{
    public string bossName;
    public float maxHealth;
    public float moveSpeed;
    public BossType type; // Lung, Heart, Brain
    public List<AttackPattern> attackPatterns;
}

public enum BossType
{
    Lung,   // Breathing rhythm mechanic
    Heart,  // Beat pattern attacks
    Brain   // Multi-phase combat
}
```

---

## Boss Mechanics

### Boss Base Class

**File:** `Assets/Scripts/Boss/BossBase.cs`

```csharp
using UnityEngine;
using System.Collections;

public abstract class BossBase : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] protected float maxHealth = 500f;
    [SerializeField] protected float moveSpeed = 2f;
    
    protected float currentHealth;
    protected Transform player;
    protected bool isFighting = false;
    
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        gameObject.tag = "Boss";
    }
    
    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        // Award bonus stat points
        StatSystem.Instance.AwardStatPoints(5);
        
        // Trigger section completion
        SceneTransitionManager.Instance.LoadNextSection();
        
        Destroy(gameObject);
    }
    
    // Abstract methods for boss-specific behavior
    protected abstract void UpdateBossAI();
    protected abstract void ExecuteAttackPattern();
}
```

### Lung Boss (Breathing Rhythm)

**File:** `Assets/Scripts/Boss/LungBoss.cs`

```csharp
using UnityEngine;
using System.Collections;

public class LungBoss : BossBase
{
    [Header("Lung Boss Mechanics")]
    [SerializeField] private float breathCycleDuration = 6f; // 3s inhale, 3s exhale
    [SerializeField] private Transform arenaCenter;
    [SerializeField] private float arenaExpandRadius = 10f;
    [SerializeField] private float arenaContractRadius = 5f;
    
    private float breathTimer = 0f;
    private bool isInhaling = true;
    
    [Header("Arena Walls")]
    [SerializeField] private Transform[] arenaWalls; // 4 walls that move
    
    void Update()
    {
        if (!isFighting) return;
        
        UpdateBossAI();
    }
    
    protected override void UpdateBossAI()
    {
        // Update breathing cycle
        breathTimer += Time.deltaTime;
        
        if (breathTimer >= breathCycleDuration / 2f)
        {
            isInhaling = !isInhaling;
            breathTimer = 0f;
        }
        
        // Animate arena expansion/contraction
        float targetRadius = isInhaling ? arenaContractRadius : arenaExpandRadius;
        float currentRadius = Vector2.Distance(arenaWalls[0].position, arenaCenter.position);
        float newRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * 2f);
        
        // Move walls (simplified - assumes 4 walls: top, bottom, left, right)
        if (arenaWalls.Length >= 4)
        {
            arenaWalls[0].position = arenaCenter.position + Vector3.up * newRadius;    // Top
            arenaWalls[1].position = arenaCenter.position + Vector3.down * newRadius;  // Bottom
            arenaWalls[2].position = arenaCenter.position + Vector3.left * newRadius;  // Left
            arenaWalls[3].position = arenaCenter.position + Vector3.right * newRadius; // Right
        }
        
        // Chase player
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        }
        
        ExecuteAttackPattern();
    }
    
    protected override void ExecuteAttackPattern()
    {
        // Simple contact damage (handled by collision)
        // Could add projectile attacks here
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(15f * Time.deltaTime); // Continuous damage
            }
        }
    }
    
    public void StartFight()
    {
        isFighting = true;
    }
}
```

**Unity Setup:**
1. Create GameObject "LungBoss"
2. Add `Rigidbody2D` (Gravity Scale: 0, Freeze Rotation Z)
3. Add `CircleCollider2D` (Radius: 1.5)
4. Add `LungBoss` script:
   - Max Health: 500
   - Move Speed: 2
   - Breath Cycle Duration: 6
   - Arena Expand Radius: 10
   - Arena Contract Radius: 5
5. Create empty GameObject "ArenaCenter" at boss arena center
6. Create 4 GameObjects with BoxCollider2D as arena walls
7. Assign ArenaCenter and walls to script
8. Add trigger collider to start fight when player enters

### Heart Boss (Beat Pattern)

**File:** `Assets/Scripts/Boss/HeartBoss.cs`

```csharp
using UnityEngine;
using System.Collections;

public class HeartBoss : BossBase
{
    [Header("Heart Boss Mechanics")]
    [SerializeField] private float beatInterval = 2f; // Attack every 2 seconds
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectilesPerBeat = 8;
    
    private float beatTimer = 0f;
    
    void Update()
    {
        if (!isFighting) return;
        
        UpdateBossAI();
    }
    
    protected override void UpdateBossAI()
    {
        beatTimer += Time.deltaTime;
        
        if (beatTimer >= beatInterval)
        {
            ExecuteAttackPattern();
            beatTimer = 0f;
        }
    }
    
    protected override void ExecuteAttackPattern()
    {
        // Radial projectile burst
        float angleStep = 360f / projectilesPerBeat;
        
        for (int i = 0; i < projectilesPerBeat; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
            
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.Initialize(direction, 5f, 20f); // speed, damage
            }
        }
    }
    
    public void StartFight()
    {
        isFighting = true;
    }
}
```

**Unity Setup:**
1. Create GameObject "HeartBoss"
2. Add `Rigidbody2D` (Kinematic, Gravity Scale: 0)
3. Add `CircleCollider2D` (Radius: 2)
4. Add `HeartBoss` script:
   - Max Health: 600
   - Beat Interval: 2
   - Projectiles Per Beat: 8
5. Create projectile prefab (see Projectile class below)

### Brain Boss (Multi-Phase)

**File:** `Assets/Scripts/Boss/BrainBoss.cs`

```csharp
using UnityEngine;
using System.Collections;

public class BrainBoss : BossBase
{
    public enum BossPhase { Phase1, Phase2, Phase3 }
    
    [Header("Brain Boss Mechanics")]
    [SerializeField] private BossPhase currentPhase = BossPhase.Phase1;
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private float phase2HealthThreshold = 0.66f;
    [SerializeField] private float phase3HealthThreshold = 0.33f;
    
    private bool hasEnteredPhase2 = false;
    private bool hasEnteredPhase3 = false;
    
    void Update()
    {
        if (!isFighting) return;
        
        CheckPhaseTransition();
        UpdateBossAI();
    }
    
    private void CheckPhaseTransition()
    {
        float healthPercent = currentHealth / maxHealth;
        
        if (healthPercent <= phase3HealthThreshold && !hasEnteredPhase3)
        {
            EnterPhase3();
        }
        else if (healthPercent <= phase2HealthThreshold && !hasEnteredPhase2)
        {
            EnterPhase2();
        }
    }
    
    private void EnterPhase2()
    {
        currentPhase = BossPhase.Phase2;
        hasEnteredPhase2 = true;
        moveSpeed *= 1.5f;
        SpawnMinions(2);
    }
    
    private void EnterPhase3()
    {
        currentPhase = BossPhase.Phase3;
        hasEnteredPhase3 = true;
        moveSpeed *= 1.5f;
        SpawnMinions(4);
    }
    
    private void SpawnMinions(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 spawnOffset = Random.insideUnitCircle * 3f;
            Instantiate(minionPrefab, (Vector2)transform.position + spawnOffset, Quaternion.identity);
        }
    }
    
    protected override void UpdateBossAI()
    {
        // Chase player with phase-dependent speed
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        }
        
        ExecuteAttackPattern();
    }
    
    protected override void ExecuteAttackPattern()
    {
        // Phase-specific attacks
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                // Basic chase
                break;
            case BossPhase.Phase2:
                // Faster chase + minions
                break;
            case BossPhase.Phase3:
                // Aggressive chase + more minions
                break;
        }
    }
    
    public void StartFight()
    {
        isFighting = true;
    }
}
```

**Unity Setup:**
1. Create GameObject "BrainBoss"
2. Add `Rigidbody2D` (Gravity Scale: 0, Freeze Rotation Z)
3. Add `CircleCollider2D` (Radius: 2)
4. Add `BrainBoss` script:
   - Max Health: 800
   - Move Speed: 1.5
   - Phase 2 Health Threshold: 0.66
   - Phase 3 Health Threshold: 0.33
5. Create minion prefab (use LeukocyteAI with higher stats)

### Projectile Class (for Heart Boss)

**File:** `Assets/Scripts/Combat/Projectile.cs`

```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float damage;
    private Rigidbody2D rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }
    
    public void Initialize(Vector2 dir, float spd, float dmg)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        rb.velocity = direction * speed;
        
        // Auto-destroy after 5 seconds
        Destroy(gameObject, 5f);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
```

**Unity Setup:**
1. Create GameObject "Projectile"
2. Add `Rigidbody2D` (Gravity Scale: 0)
3. Add `CircleCollider2D` (Is Trigger: true, Radius: 0.2)
4. Add `Projectile` script
5. Add SpriteRenderer with projectile sprite
6. Save as prefab in Assets/Prefabs/

---

## Scene Management and Persistence

### SceneTransitionManager

**File:** `Assets/Scripts/Systems/SceneTransitionManager.cs`

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    
    [Header("Scene Names")]
    [SerializeField] private string lungSceneName = "LungScene";
    [SerializeField] private string heartSceneName = "HeartScene";
    [SerializeField] private string brainSceneName = "BrainScene";
    [SerializeField] private string victorySceneName = "VictoryScene";
    
    private int currentSectionIndex = 0;
    private string[] sectionScenes;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        sectionScenes = new string[] { lungSceneName, heartSceneName, brainSceneName };
    }
    
    public void LoadNextSection()
    {
        currentSectionIndex++;
        
        if (currentSectionIndex < sectionScenes.Length)
        {
            StartCoroutine(LoadSceneAsync(sectionScenes[currentSectionIndex]));
        }
        else
        {
            // Game complete
            StartCoroutine(LoadSceneAsync(victorySceneName));
        }
    }
    
    public void RestartCurrentSection()
    {
        StartCoroutine(LoadSceneAsync(sectionScenes[currentSectionIndex]));
    }
    
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Optional: Show loading screen
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            // Update loading bar if needed
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            yield return null;
        }
    }
}
```

**Unity Setup:**
1. Add `SceneTransitionManager` to GameManager GameObject
2. Set scene names:
   - Lung Scene Name: "LungScene"
   - Heart Scene Name: "HeartScene"
   - Brain Scene Name: "BrainScene"
   - Victory Scene Name: "VictoryScene"
3. Add all scenes to Build Settings (File → Build Settings → Add Open Scenes)

### Scene Transition Trigger

**File:** `Assets/Scripts/Systems/SceneTransitionTrigger.cs`

```csharp
using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private bool requiresBossDefeat = true;
    
    private bool bossDefeated = false;
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!requiresBossDefeat || bossDefeated)
            {
                SceneTransitionManager.Instance.LoadNextSection();
            }
            else
            {
                Debug.Log("Defeat the boss first!");
            }
        }
    }
    
    public void OnBossDefeated()
    {
        bossDefeated = true;
    }
}
```

**Unity Setup:**
1. Create GameObject at section exit point
2. Add `BoxCollider2D` (Is Trigger: true)
3. Add `SceneTransitionTrigger` script
4. In boss Die() method, call: `FindObjectOfType<SceneTransitionTrigger>()?.OnBossDefeated();`

---

## Unity Inspector Setup Guide

### Complete GameObject Hierarchy for Lung Section

```
LungScene
├── GameManager (DontDestroyOnLoad)
│   ├── StatSystem
│   ├── SceneTransitionManager
│   └── CombatManager
│
├── GraphManager
│   └── GraphData: LungGraphData (ScriptableObject)
│
├── Nodes
│   ├── Node_0 (Transform: 0, 0, 0)
│   ├── Node_1 (Transform: 5, 0, 0)
│   ├── Node_2 (Transform: 0, 5, 0)
│   └── ... (all graph nodes)
│
├── Environment
│   ├── Background (SpriteRenderer, Sorting Layer: Background, Order: 0)
│   ├── Walls (TilemapCollider2D or PolygonCollider2D)
│   └── Decorations (SpriteRenderer, Sorting Layer: Background, Order: 1)
│
├── Player
│   ├── Rigidbody2D: Dynamic, Gravity Scale: 0, Freeze Rotation Z
│   ├── CircleCollider2D: Radius: 0.5
│   ├── PlayerController: Base Move Speed: 5, Max Health: 100
│   └── PlayerSprite (child)
│       └── SpriteRenderer: Sorting Layer: Player, Order: 10
│
├── Enemies
│   ├── Leukocyte_1
│   │   ├── Rigidbody2D: Dynamic, Gravity Scale: 0, Freeze Rotation Z
│   │   ├── CircleCollider2D: Radius: 0.4
│   │   ├── LeukocyteAI: Move Speed: 3, Sight Range: 5, Enable Ambush: false
│   │   └── LeukocyteSprite (child)
│   └── ... (more leukocytes)
│
├── Boss
│   └── LungBoss
│       ├── Rigidbody2D: Dynamic, Gravity Scale: 0, Freeze Rotation Z
│       ├── CircleCollider2D: Radius: 1.5
│       ├── LungBoss: Max Health: 500, Move Speed: 2, Breath Cycle: 6
│       └── BossSprite (child)
│
├── Arena
│   ├── ArenaCenter (empty GameObject at center)
│   ├── WallTop (BoxCollider2D)
│   ├── WallBottom (BoxCollider2D)
│   ├── WallLeft (BoxCollider2D)
│   └── WallRight (BoxCollider2D)
│
├── UI
│   ├── Canvas (Screen Space - Overlay)
│   │   ├── ProximityUI
│   │   │   ├── DistanceText (TextMeshPro)
│   │   │   └── SignalBar (Image, Fill Type: Horizontal)
│   │   ├── HealthBar (Slider)
│   │   └── StatDisplay (TextMeshPro)
│   └── EventSystem
│
└── SceneTransition
    ├── BoxCollider2D: Is Trigger: true
    └── SceneTransitionTrigger: Requires Boss Defeat: true
```

### Inspector Values Summary

**Player:**
- Rigidbody2D: Dynamic, Gravity: 0, Freeze Rotation Z
- CircleCollider2D: Radius: 0.5
- PlayerController: Base Move Speed: 5, Max Health: 100

**Leukocyte (Lung):**
- Move Speed: 3
- Sight Range: 5
- Enable Ambush: false
- Max Health: 50
- Contact Damage: 10

**Leukocyte (Heart):**
- Move Speed: 4
- Sight Range: 6
- Enable Ambush: true
- Max Health: 60
- Contact Damage: 12

**Leukocyte (Brain):**
- Move Speed: 5
- Sight Range: 7
- Enable Ambush: true
- Max Health: 70
- Contact Damage: 15

**Lung Boss:**
- Max Health: 500
- Move Speed: 2
- Breath Cycle Duration: 6
- Arena Expand Radius: 10
- Arena Contract Radius: 5

**Heart Boss:**
- Max Health: 600
- Move Speed: 0 (stationary)
- Beat Interval: 2
- Projectiles Per Beat: 8

**Brain Boss:**
- Max Health: 800
- Move Speed: 1.5
- Phase 2 Threshold: 0.66
- Phase 3 Threshold: 0.33

**StatSystem:**
- Base Damage: 10
- Base Speed: 1
- Base Health: 100
- Damage Per Level: 5
- Speed Per Level: 0.2
- Health Per Level: 20

---

## Correctness Properties

A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.

### Property Reflection

After analyzing all acceptance criteria, I identified the following testable properties. During reflection, I found several areas of redundancy:

- Properties 1.1 and 1.2 (input response and 8-directional movement) can be combined into a single comprehensive property about input-to-velocity mapping
- Property 7.3 (UI distance calculation using BFS) is redundant with 3.5 (pathfinding distance calculation) since the UI uses the pathfinding system
- Property 10.2 (stat persistence during scene transitions) is identical to 6.3
- Several MVP requirements (11.x) duplicate earlier functional requirements

After consolidation, the following properties provide unique validation value:

### Movement Properties

**Property 1: Input to velocity mapping**
*For any* valid input combination (WASD or joystick), the Player_Cell's velocity should be normalized and scaled by the current speed stat, supporting all 8 directions (cardinal and diagonal).
**Validates: Requirements 1.1, 1.2, 1.3**

### Graph System Properties

**Property 2: Dead end flag consistency**
*For any* node marked as a dead end, querying the node's isDeadEnd flag should return true.
**Validates: Requirements 2.3**

**Property 3: Adjacency list correctness**
*For any* node with defined connections, querying adjacent nodes should return exactly the set of connected nodes specified in the adjacency list.
**Validates: Requirements 2.6**

### Pathfinding Properties

**Property 4: Valid path connectivity**
*For any* start and goal nodes with a path between them, the pathfinding result should be a list of nodes where each consecutive pair is connected in the graph.
**Validates: Requirements 3.2**

**Property 5: No path handling**
*For any* two nodes with no connecting path in the graph, the pathfinding result should be empty or null.
**Validates: Requirements 3.3**

**Property 6: Distance calculation accuracy**
*For any* two connected nodes, the calculated distance should equal the length of the shortest path between them (BFS path length).
**Validates: Requirements 3.5, 7.1, 7.2**

### AI Behavior Properties

**Property 7: Patrol target validity**
*For any* Leukocyte in Patrol mode selecting a target node, the target node should be in the adjacency list of the current node.
**Validates: Requirements 4.1**

**Property 8: Chase state transition**
*For any* Leukocyte and Player_Cell position, when the distance is less than or equal to sight range, the Leukocyte state should transition to Chase (or Ambush if enabled).
**Validates: Requirements 4.2**

**Property 9: Chase pathfinding usage**
*For any* Leukocyte in Chase mode, the current path should be a valid path from the Leukocyte's node to the Player's node.
**Validates: Requirements 4.3**

### Combat Properties

**Property 10: Collision damage application**
*For any* collision between Player_Cell and Leukocyte, both entities should receive damage according to their respective damage values.
**Validates: Requirements 5.1**

**Property 11: Attack range damage**
*For any* active attack with position and radius, all Leukocytes within the attack radius should receive damage.
**Validates: Requirements 5.2**

**Property 12: Enemy death and reward**
*For any* Leukocyte, when its health reaches zero, the Leukocyte should be destroyed and stat points should be awarded to the player.
**Validates: Requirements 5.3**

**Property 13: Damage stat scaling**
*For any* damage stat value, the actual damage dealt should scale proportionally with the stat multiplier.
**Validates: Requirements 5.5**

### Stat System Properties

**Property 14: Stat point award**
*For any* enemy defeat, the player's available stat points should increase by the reward amount.
**Validates: Requirements 6.2**

**Property 15: Stat persistence across scenes**
*For any* stat values before a scene transition, after loading the new scene, the stat values should remain unchanged.
**Validates: Requirements 6.3, 10.2**

**Property 16: Stat allocation effect**
*For any* stat upgrade (damage, speed, or health), the corresponding stat level should increase and the available points should decrease.
**Validates: Requirements 6.4**

**Property 17: Immediate stat application**
*For any* stat upgrade, the player's effective stat multiplier or max health should immediately reflect the new value.
**Validates: Requirements 6.5**

### UI Properties

**Property 18: Proximity UI accuracy**
*For any* player position and boss position, the displayed distance should equal the BFS path length between their closest nodes.
**Validates: Requirements 7.1, 7.2**

### Boss Mechanics Properties

**Property 19: Lung boss arena oscillation**
*For any* time during the Lung boss fight, the arena walls should oscillate between expanded and contracted states with the specified cycle duration.
**Validates: Requirements 9.1**

**Property 20: Heart boss beat timing**
*For any* sequence of Heart boss attacks, the time interval between consecutive attacks should be consistent with the beat interval.
**Validates: Requirements 9.2**

**Property 21: Brain boss phase transitions**
*For any* Brain boss health value, when health drops below phase thresholds (66% and 33%), the boss should transition to the next phase exactly once.
**Validates: Requirements 9.3**

---

## Error Handling

### Player Death

When player health reaches zero:
1. Trigger death animation (if implemented)
2. Display game over UI with options:
   - Restart current section
   - Return to main menu
3. Reset player stats to section start values (optional)
4. Call `SceneTransitionManager.Instance.RestartCurrentSection()`

**Implementation:**
```csharp
// In PlayerController.Die()
private void Die()
{
    Debug.Log("Player died!");
    Time.timeScale = 0f; // Pause game
    // Show game over UI
    GameOverUI.Instance.Show();
}
```

### Pathfinding Failures

When no path exists between nodes:
1. Return empty list or null from `FindPath()`
2. AI should fall back to direct movement toward player
3. Log warning for debugging

**Implementation:**
```csharp
// In LeukocyteAI.UpdateChase()
currentPath = pathfinder.FindPath(currentNodeId, playerNodeId);
if (currentPath.Count == 0)
{
    Debug.LogWarning($"No path found from {currentNodeId} to {playerNodeId}");
    // Fallback: move directly toward player
    Vector2 direction = (player.position - transform.position).normalized;
    rb.velocity = direction * moveSpeed;
}
```

### Graph Data Validation

When loading GraphData ScriptableObject:
1. Validate all node IDs are unique
2. Validate all connected node IDs reference existing nodes
3. Validate no self-loops (node connected to itself)
4. Log errors for invalid configurations

**Implementation:**
```csharp
// In GraphManager.Awake()
private void ValidateGraphData()
{
    HashSet<int> nodeIds = new HashSet<int>();
    
    foreach (var node in graphData.nodes)
    {
        if (!nodeIds.Add(node.id))
        {
            Debug.LogError($"Duplicate node ID: {node.id}");
        }
        
        foreach (int connectedId in node.connectedNodeIds)
        {
            if (connectedId == node.id)
            {
                Debug.LogError($"Self-loop detected at node {node.id}");
            }
            
            if (graphData.GetNode(connectedId) == null)
            {
                Debug.LogError($"Node {node.id} references non-existent node {connectedId}");
            }
        }
    }
}
```

### Scene Transition Failures

When scene loading fails:
1. Catch AsyncOperation errors
2. Display error message to player
3. Offer option to retry or return to main menu
4. Log detailed error information

**Implementation:**
```csharp
// In SceneTransitionManager.LoadSceneAsync()
private IEnumerator LoadSceneAsync(string sceneName)
{
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
    
    if (asyncLoad == null)
    {
        Debug.LogError($"Failed to load scene: {sceneName}");
        // Show error UI
        yield break;
    }
    
    while (!asyncLoad.isDone)
    {
        yield return null;
    }
}
```

### Collision Detection Edge Cases

Handle cases where collisions occur at high speeds:
1. Use Continuous collision detection for fast-moving objects
2. Set Rigidbody2D Collision Detection to "Continuous"
3. Ensure colliders are appropriately sized

**Unity Setup:**
- Player Rigidbody2D: Collision Detection: Continuous
- Enemy Rigidbody2D: Collision Detection: Continuous
- Projectile Rigidbody2D: Collision Detection: Continuous

### Stat Overflow Protection

Prevent stat values from exceeding reasonable limits:
1. Cap maximum stat levels (e.g., 50 levels per stat)
2. Validate stat point calculations
3. Prevent negative stat values

**Implementation:**
```csharp
// In StatSystem
private const int MAX_STAT_LEVEL = 50;

public void UpgradeDamage()
{
    if (availableStatPoints > 0 && damageLevel < MAX_STAT_LEVEL)
    {
        damageLevel++;
        availableStatPoints--;
    }
    else if (damageLevel >= MAX_STAT_LEVEL)
    {
        Debug.LogWarning("Damage stat is already at maximum level");
    }
}
```

---

## Testing Strategy

### Dual Testing Approach

The testing strategy uses both unit tests and property-based tests to ensure comprehensive coverage:

- **Unit tests**: Verify specific examples, edge cases, and error conditions
- **Property-based tests**: Verify universal properties across randomized inputs

Both approaches are complementary and necessary. Unit tests catch concrete bugs in specific scenarios, while property-based tests verify general correctness across a wide input space.

### Property-Based Testing Configuration

**Library Selection:** Use a C# property-based testing library compatible with Unity:
- **Recommended**: FsCheck (via NuGet) or custom generator implementation
- **Alternative**: Implement simple random test generators using Unity's `Random` class

**Test Configuration:**
- Minimum 100 iterations per property test (due to randomization)
- Each property test must reference its design document property
- Tag format: `// Feature: cell-game-jam, Property {number}: {property_text}`

**Example Property Test Structure:**

```csharp
using NUnit.Framework;
using UnityEngine;

public class PathfindingPropertyTests
{
    // Feature: cell-game-jam, Property 4: Valid path connectivity
    [Test]
    public void Property_ValidPathConnectivity_AllPathsAreConnected()
    {
        // Arrange
        GraphData testGraph = CreateTestGraph();
        GraphManager graphManager = CreateGraphManager(testGraph);
        Pathfinder pathfinder = new Pathfinder(graphManager);
        
        // Act & Assert - Run 100 iterations
        for (int i = 0; i < 100; i++)
        {
            int startNode = Random.Range(0, testGraph.nodes.Count);
            int goalNode = Random.Range(0, testGraph.nodes.Count);
            
            List<int> path = pathfinder.FindPath(startNode, goalNode);
            
            if (path.Count > 0)
            {
                // Verify each consecutive pair is connected
                for (int j = 0; j < path.Count - 1; j++)
                {
                    var currentNode = graphManager.GetNode(path[j]);
                    Assert.IsTrue(
                        currentNode.connectedNodeIds.Contains(path[j + 1]),
                        $"Path contains disconnected nodes: {path[j]} -> {path[j + 1]}"
                    );
                }
            }
        }
    }
    
    // Feature: cell-game-jam, Property 5: No path handling
    [Test]
    public void Property_NoPathHandling_DisconnectedNodesReturnEmpty()
    {
        // Arrange
        GraphData testGraph = CreateDisconnectedGraph();
        GraphManager graphManager = CreateGraphManager(testGraph);
        Pathfinder pathfinder = new Pathfinder(graphManager);
        
        // Act & Assert - Run 100 iterations
        for (int i = 0; i < 100; i++)
        {
            // Pick nodes from different disconnected components
            int startNode = 0; // Component A
            int goalNode = 5;  // Component B (disconnected)
            
            List<int> path = pathfinder.FindPath(startNode, goalNode);
            
            Assert.IsTrue(
                path == null || path.Count == 0,
                "Pathfinding should return empty for disconnected nodes"
            );
        }
    }
}
```

### Unit Testing Focus Areas

Unit tests should focus on:

1. **Specific Examples**:
   - Player death at exactly 0 health
   - Boss phase transition at exact threshold values
   - Scene transition sequence (Lung → Heart → Brain)

2. **Edge Cases**:
   - Dead end nodes in distance calculation
   - Empty graph handling
   - Single-node graphs
   - Player at same position as boss

3. **Integration Points**:
   - StatSystem persistence across scene loads
   - Combat damage application between player and enemies
   - UI updates when game state changes

4. **Error Conditions**:
   - Invalid node IDs in pathfinding
   - Null references in AI behavior
   - Scene loading failures

**Example Unit Tests:**

```csharp
using NUnit.Framework;
using UnityEngine;

public class CombatSystemTests
{
    [Test]
    public void PlayerDeath_HealthZero_TriggersGameOver()
    {
        // Arrange
        GameObject playerObj = new GameObject();
        PlayerController player = playerObj.AddComponent<PlayerController>();
        
        // Act
        player.TakeDamage(player.GetMaxHealth());
        
        // Assert
        Assert.AreEqual(0, player.GetCurrentHealth());
        // Verify game over state (would need GameOverUI mock)
    }
    
    [Test]
    public void BossPhaseTransition_At66Percent_EntersPhase2()
    {
        // Arrange
        GameObject bossObj = new GameObject();
        BrainBoss boss = bossObj.AddComponent<BrainBoss>();
        float initialHealth = 800f;
        
        // Act
        boss.TakeDamage(initialHealth * 0.34f); // Reduce to 66%
        
        // Assert
        // Verify phase is Phase2 (would need public getter)
    }
    
    [Test]
    public void SceneTransition_LungToHeart_LoadsCorrectScene()
    {
        // Arrange
        SceneTransitionManager manager = new GameObject().AddComponent<SceneTransitionManager>();
        
        // Act
        manager.LoadNextSection();
        
        // Assert
        // Verify HeartScene is loaded (would need scene loading mock)
    }
}
```

### Test Organization

Organize tests into separate files by system:

```
Assets/Tests/
├── GraphSystemTests.cs
│   ├── Unit tests for GraphData validation
│   └── Property tests for adjacency queries
├── PathfindingTests.cs
│   ├── Property tests for path validity
│   └── Unit tests for edge cases
├── AIBehaviorTests.cs
│   ├── Property tests for state transitions
│   └── Unit tests for specific behaviors
├── CombatSystemTests.cs
│   ├── Property tests for damage calculations
│   └── Unit tests for death conditions
├── StatSystemTests.cs
│   ├── Property tests for stat scaling
│   └── Unit tests for persistence
└── UITests.cs
    ├── Property tests for distance display
    └── Unit tests for UI updates
```

### Testing During Game Jam

Given the 36-hour time constraint, prioritize testing:

1. **Critical Path (Must Test)**:
   - Pathfinding correctness (Properties 4, 5, 6)
   - Stat persistence (Property 15)
   - Combat damage (Properties 10, 13)

2. **High Value (Should Test)**:
   - AI state transitions (Properties 7, 8, 9)
   - Stat allocation (Properties 16, 17)

3. **Nice to Have (Optional)**:
   - Boss mechanics (Properties 19, 20, 21)
   - UI accuracy (Property 18)

### Manual Testing Checklist

In addition to automated tests, manually verify:

- [ ] Player can move in all 8 directions smoothly
- [ ] Enemies chase player when in sight range
- [ ] Collision damage works for both player and enemies
- [ ] Stats persist when transitioning between sections
- [ ] Boss fights are challenging but fair
- [ ] UI displays correct information
- [ ] Game over and victory screens work
- [ ] No game-breaking bugs or crashes

---

## Implementation Notes

### Development Priority for 36-Hour Jam

**Day 1 (Hours 0-12): Core Systems**
1. Graph system and ScriptableObject setup (2 hours)
2. Player controller with movement (2 hours)
3. Basic pathfinding (BFS) (2 hours)
4. Leukocyte AI with Patrol and Chase (3 hours)
5. Collision-based combat (2 hours)
6. Basic stat system (1 hour)

**Day 1 (Hours 12-24): First Playable**
1. Lung section environment art (4 hours)
2. Proximity UI (2 hours)
3. Lung boss mechanics (3 hours)
4. Scene transition system (2 hours)
5. Testing and bug fixes (1 hour)

**Day 2 (Hours 24-36): Polish and Expansion**
1. Heart section (if time permits) (4 hours)
2. Stat upgrade UI (2 hours)
3. Sound effects and music (2 hours)
4. Additional polish and balancing (3 hours)
5. Final testing and bug fixes (1 hour)

### Code Organization

```
Assets/
├── Scripts/
│   ├── Graph/
│   │   ├── GraphData.cs
│   │   ├── GraphManager.cs
│   │   └── Pathfinder.cs
│   ├── Player/
│   │   └── PlayerController.cs
│   ├── AI/
│   │   └── LeukocyteAI.cs
│   ├── Boss/
│   │   ├── BossBase.cs
│   │   ├── LungBoss.cs
│   │   ├── HeartBoss.cs
│   │   └── BrainBoss.cs
│   ├── Combat/
│   │   ├── CombatManager.cs
│   │   └── Projectile.cs
│   ├── Systems/
│   │   ├── StatSystem.cs
│   │   └── SceneTransitionManager.cs
│   └── UI/
│       ├── ProximityUI.cs
│       ├── HealthBar.cs
│       └── StatDisplay.cs
├── Prefabs/
│   ├── Player.prefab
│   ├── Leukocyte.prefab
│   ├── Projectile.prefab
│   └── Bosses/
│       ├── LungBoss.prefab
│       ├── HeartBoss.prefab
│       └── BrainBoss.prefab
├── ScriptableObjects/
│   ├── GraphData/
│   │   ├── LungGraphData.asset
│   │   ├── HeartGraphData.asset
│   │   └── BrainGraphData.asset
│   └── Stats/
│       └── DefaultStats.asset
├── Scenes/
│   ├── MainMenu.unity
│   ├── LungScene.unity
│   ├── HeartScene.unity
│   ├── BrainScene.unity
│   └── VictoryScene.unity
├── Sprites/
│   ├── Player/
│   ├── Enemies/
│   ├── Bosses/
│   └── Environment/
└── Tests/
    ├── GraphSystemTests.cs
    ├── PathfindingTests.cs
    ├── AIBehaviorTests.cs
    ├── CombatSystemTests.cs
    ├── StatSystemTests.cs
    └── UITests.cs
```

### Performance Considerations

1. **Pathfinding Optimization**:
   - Cache paths for a few frames instead of recalculating every frame
   - Limit pathfinding updates to every 0.5 seconds for enemies
   - Use simple BFS instead of A* for faster computation

2. **Object Pooling**:
   - Pool projectiles for Heart boss to avoid instantiation overhead
   - Pool particle effects if implemented

3. **Graph Queries**:
   - Use Dictionary for O(1) node lookups by ID
   - Cache adjacent node lists in GraphManager

4. **UI Updates**:
   - Update Proximity UI every 0.5 seconds instead of every frame
   - Use TextMeshPro for better text rendering performance

### Debugging Tools

Add debug visualizations in Scene view:

```csharp
// In GraphManager
void OnDrawGizmos()
{
    if (graphData == null) return;
    
    // Draw nodes
    Gizmos.color = Color.green;
    foreach (var node in graphData.nodes)
    {
        Gizmos.DrawSphere(node.position, 0.3f);
        
        if (node.isDeadEnd)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(node.position, 0.5f);
            Gizmos.color = Color.green;
        }
    }
    
    // Draw edges
    Gizmos.color = Color.yellow;
    foreach (var node in graphData.nodes)
    {
        foreach (int connectedId in node.connectedNodeIds)
        {
            var connectedNode = graphData.GetNode(connectedId);
            if (connectedNode != null)
            {
                Gizmos.DrawLine(node.position, connectedNode.position);
            }
        }
    }
}
```

This design document provides a complete technical blueprint for implementing Cell Game Jam in Unity with C#. All systems are designed for rapid development within a 36-hour game jam while maintaining clean architecture for potential post-jam expansion.
