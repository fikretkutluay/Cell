# Requirements Document: Cell Game Jam

## Introduction

Cell Game Jam is a Unity 2D top-down pixel art action game where the player controls a diseased cell navigating through the human body while being hunted by the immune system. The game features three distinct organ sections (Lung, Heart, Brain), each with unique environmental hazards and boss fights. The core gameplay loop involves strategic navigation through a node-based vessel network, combat with leukocytes (white blood cells), and progression through stat upgrades.

## Glossary

- **Player_Cell**: The diseased cell controlled by the player
- **Leukocyte**: Enemy white blood cell that hunts the player
- **Graph_System**: Node-edge data structure representing vessel pathways
- **Section**: A distinct organ level (Lung, Heart, or Brain)
- **Boss**: End-of-section enemy with unique mechanics
- **Stat_System**: Character progression system tracking damage, speed, and health
- **Proximity_UI**: Visual indicator showing distance to boss
- **Dead_End**: Terminal node in the graph with no forward progression
- **Pathfinding_Engine**: BFS or A* algorithm for navigation
- **Combat_System**: Collision or active attack mechanics
- **Scene_Manager**: System handling transitions between sections

## Requirements

### Requirement 1: Character Movement and Control

**User Story:** As a player, I want to control the diseased cell with smooth top-down movement, so that I can navigate through the vessel network.

#### Acceptance Criteria

1. WHEN the player presses WASD keys or uses a joystick, THE Player_Cell SHALL move in the corresponding direction with immediate response
2. THE Player_Cell SHALL support 8-directional movement (cardinal and diagonal directions)
3. WHEN the Player_Cell moves, THE movement speed SHALL be determined by the current speed stat value
4. THE Player_Cell SHALL use Unity's Rigidbody2D physics for collision detection
5. WHEN the Player_Cell collides with environment boundaries, THE Player_Cell SHALL stop movement in that direction while allowing movement in other directions

### Requirement 2: Graph-Based Navigation System

**User Story:** As a developer, I want a node-edge graph system for vessel pathways, so that enemies can navigate intelligently and the level structure is maintainable.

#### Acceptance Criteria

1. THE Graph_System SHALL store nodes as Transform positions in world space
2. THE Graph_System SHALL store edges as an adjacency list data structure
3. WHEN a node is marked as a dead end, THE Graph_System SHALL set the isDeadEnd flag to true
4. THE Graph_System SHALL be implemented using ScriptableObject-based GraphData assets
5. THE Graph_System SHALL provide a separate GraphData asset for each section (Lung, Heart, Brain)
6. WHEN querying node connections, THE Graph_System SHALL return all adjacent nodes for a given node
7. THE Graph_System SHALL support runtime queries for pathfinding algorithms

### Requirement 3: Pathfinding Implementation

**User Story:** As a developer, I want BFS or A* pathfinding on the graph, so that enemies can find optimal paths to the player.

#### Acceptance Criteria

1. THE Pathfinding_Engine SHALL implement either BFS or A* algorithm on the node-edge graph
2. WHEN calculating a path, THE Pathfinding_Engine SHALL return a list of nodes from start to goal
3. WHEN no path exists, THE Pathfinding_Engine SHALL return an empty path or null
4. THE Pathfinding_Engine SHALL calculate paths using the graph structure without NavMesh
5. WHEN calculating boss distance, THE Pathfinding_Engine SHALL use BFS to determine the shortest path length

### Requirement 4: Leukocyte AI Behavior

**User Story:** As a player, I want enemies with varied AI behaviors, so that the game provides strategic challenge and unpredictability.

#### Acceptance Criteria

1. WHEN a Leukocyte is in Patrol mode, THE Leukocyte SHALL move randomly between connected nodes
2. WHEN the Player_Cell enters a Leukocyte's sight range, THE Leukocyte SHALL switch to Chase mode
3. WHEN a Leukocyte is in Chase mode, THE Leukocyte SHALL use pathfinding to pursue the Player_Cell
4. WHERE Ambush mode is enabled, WHEN the Player_Cell is detected, THE Leukocyte SHALL attempt to cut off the player by advancing from alternate paths
5. WHILE in Section 1 (Lung), THE Leukocyte SHALL only use Patrol and Chase modes
6. WHILE in Section 2 (Heart), THE Leukocyte SHALL activate Ambush mode and increase spawn count
7. WHILE in Section 3 (Brain), THE Leukocyte SHALL use aggressive pathfinding with group coordination tactics

### Requirement 5: Combat System

**User Story:** As a player, I want to fight leukocytes through collision or active attacks, so that I can defend myself and earn progression rewards.

#### Acceptance Criteria

1. WHEN the Player_Cell collides with a Leukocyte, THE Combat_System SHALL apply damage to both entities
2. WHERE active attack is implemented, WHEN the player triggers an attack, THE Combat_System SHALL deal damage to leukocytes in range
3. WHEN a Leukocyte's health reaches zero, THE Combat_System SHALL destroy the Leukocyte and award stat points to the player
4. WHEN the Player_Cell's health reaches zero, THE Combat_System SHALL trigger game over or respawn logic
5. THE Combat_System SHALL calculate damage based on the player's current damage stat value

### Requirement 6: Stat System and Progression

**User Story:** As a player, I want to earn and upgrade stats by defeating enemies, so that I can become stronger and progress through harder sections.

#### Acceptance Criteria

1. THE Stat_System SHALL track three core stats: damage, speed, and health
2. WHEN a Leukocyte is defeated, THE Stat_System SHALL award stat points to the player
3. THE Stat_System SHALL persist stat values across section transitions
4. WHEN stat points are earned, THE Stat_System SHALL provide a mechanism for the player to allocate points to damage, speed, or health
5. WHEN a stat is upgraded, THE Stat_System SHALL immediately apply the new value to the Player_Cell

### Requirement 7: Proximity UI System

**User Story:** As a player, I want to see how far I am from the boss, so that I can make informed navigation decisions.

#### Acceptance Criteria

1. THE Proximity_UI SHALL display the distance to the boss using a visual indicator (heart icon or signal bar)
2. WHEN the player moves, THE Proximity_UI SHALL update the distance calculation in real-time
3. THE Proximity_UI SHALL calculate distance using BFS path length on the graph
4. WHEN the player is at a dead end, THE Proximity_UI SHALL reflect the increased distance accurately
5. THE Proximity_UI SHALL be visible and readable against the game's pixel art aesthetic

### Requirement 8: Section Design and Environment

**User Story:** As a player, I want distinct visual and mechanical themes for each organ section, so that the game feels varied and immersive.

#### Acceptance Criteria

1. WHEN the player is in Section 1 (Lung), THE environment SHALL use pink and red-orange pixel art palette with branched bronchi and alveoli vessels
2. WHEN the player is in Section 2 (Heart), THE environment SHALL use dark red and burgundy palette with heart chambers and blood vessel networks
3. WHEN the player is in Section 3 (Brain), THE environment SHALL use purple and blue-gray palette with neuron-like complex vessel systems
4. WHERE Section 2 is implemented, THE environment SHALL include blood flow direction changes in corridors as a hazard
5. THE environment SHALL maintain visual consistency with 2D pixel art style across all sections

### Requirement 9: Boss Fight Mechanics

**User Story:** As a player, I want challenging boss fights at the end of each section, so that I experience climactic encounters that test my skills.

#### Acceptance Criteria

1. WHEN the player reaches the end of Section 1 (Lung), THE Boss SHALL implement a breathing rhythm mechanic that expands and contracts the arena
2. WHERE Section 2 is implemented, WHEN the player fights the Heart boss, THE Boss SHALL use regular beat patterns with attack waves
3. WHERE Section 3 is implemented, WHEN the player fights the Brain boss, THE Boss SHALL implement multi-phase combat with different attack patterns
4. WHEN a Boss's health reaches zero, THE Boss SHALL trigger section completion and transition to the next section or end game
5. THE Boss SHALL have significantly more health than regular Leukocytes

### Requirement 10: Scene Management and Transitions

**User Story:** As a player, I want smooth transitions between sections, so that my progression feels seamless and my stats are preserved.

#### Acceptance Criteria

1. WHEN a section is completed, THE Scene_Manager SHALL load the next section scene
2. WHEN transitioning between sections, THE Scene_Manager SHALL preserve the player's stat values
3. THE Scene_Manager SHALL handle transitions from Lung → Heart → Brain in sequence
4. WHERE the final boss is defeated, THE Scene_Manager SHALL trigger the game completion sequence
5. THE Scene_Manager SHALL provide loading feedback during scene transitions

### Requirement 11: MVP Core Features

**User Story:** As a developer, I want to prioritize core features for the 36-hour game jam, so that a playable game is delivered within the time constraint.

#### Acceptance Criteria

1. THE game SHALL include working character movement with WASD or joystick control
2. THE game SHALL include at least one complete section (Lung recommended)
3. THE game SHALL include Leukocyte Chase AI behavior
4. THE game SHALL include at least 1-2 functional stats (damage and/or speed)
5. THE game SHALL include one boss fight with unique mechanics
6. THE game SHALL include Proximity UI showing distance to boss
7. THE game SHALL include pixel art environment and character sprites (static sprites acceptable)
8. WHERE time permits, THE game SHALL add Section 2 (Heart) and Section 3 (Brain)

### Requirement 12: Unity Integration and Technical Architecture

**User Story:** As a developer, I want clean Unity C# architecture, so that the codebase is maintainable and extensible during the jam.

#### Acceptance Criteria

1. THE game SHALL be implemented in Unity using C# scripts
2. THE Graph_System SHALL use ScriptableObject assets for graph data storage
3. THE game SHALL use Unity's Rigidbody2D for Player_Cell physics
4. THE game SHALL use Unity's Sprite Renderer for 2D pixel art rendering
5. THE game SHALL organize scripts into logical namespaces or folders (e.g., AI, Graph, Combat, UI)
6. THE game SHALL use Unity's scene system for section management
7. WHERE animations are implemented, THE game SHALL use Unity's Animator component
