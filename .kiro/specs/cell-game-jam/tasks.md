# Implementation Plan: Cell Game Jam

## Overview

This implementation plan breaks down the Cell Game Jam Unity game into discrete, incremental coding tasks. The plan prioritizes getting a playable MVP (Lung section) working first, then expanding to additional sections if time permits during the 36-hour jam.

The implementation follows this sequence:
1. Core graph and pathfinding systems
2. Player movement and basic controls
3. Enemy AI with state machine
4. Combat and stat systems
5. Boss mechanics for Lung section
6. UI and scene management
7. Optional: Heart and Brain sections

Each task builds on previous work, with checkpoints to ensure stability before moving forward.

## Tasks

- [ ] 1. Set up project structure and core graph system
  - Create Unity project with 2D template
  - Set up folder structure (Scripts/Graph, Scripts/Player, Scripts/AI, Scripts/Boss, Scripts/Combat, Scripts/Systems, Scripts/UI)
  - Create GraphData ScriptableObject class with Node structure and adjacency list
  - Create GraphManager component for runtime graph queries
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.6, 12.1, 12.2, 12.5_

- [ ] 2. Implement pathfinding system
  - [ ] 2.1 Create Pathfinder class with BFS algorithm
    - Implement FindPath() method that returns list of node IDs
    - Implement CalculateDistance() method for path length
    - Handle disconnected graph cases (return empty/null)
    - _Requirements: 3.1, 3.2, 3.3, 3.5_
  
  - [ ]* 2.2 Write property test for valid path connectivity
    - **Property 4: Valid path connectivity**
    - **Validates: Requirements 3.2**
  
  - [ ]* 2.3 Write property test for no path handling
    - **Property 5: No path handling**
    - **Validates: Requirements 3.3**
  
  - [ ]* 2.4 Write property test for distance calculation accuracy
    - **Property 6: Distance calculation accuracy**
    - **Validates: Requirements 3.5**

- [ ] 3. Create Lung section scene and graph data
  - Create LungScene with environment sprites and collision boundaries
  - Create empty GameObjects for graph nodes (Node_0, Node_1, etc.) positioned at vessel intersections
  - Create LungGraphData ScriptableObject asset
  - Populate graph data with node positions, connections, and dead end flags
  - Add GraphManager to scene and assign LungGraphData
  - _Requirements: 2.4, 2.5, 8.1, 11.2_

- [ ] 4. Implement player character controller
  - [ ] 4.1 Create PlayerController component with Rigidbody2D movement
    - Implement WASD input handling with 8-directional movement
    - Normalize diagonal movement for consistent speed
    - Apply speed stat multiplier to movement
    - Add health tracking and TakeDamage() method
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 12.3_
  
  - [ ]* 4.2 Write property test for input to velocity mapping
    - **Property 1: Input to velocity mapping**
    - **Validates: Requirements 1.1, 1.2, 1.3**
  
  - [ ] 4.3 Create Player GameObject in scene
    - Add Rigidbody2D (Dynamic, Gravity Scale: 0, Freeze Rotation Z)
    - Add CircleCollider2D (Radius: 0.5)
    - Add PlayerController script with base values
    - Add child SpriteRenderer for player sprite
    - Tag as "Player"
    - _Requirements: 1.4, 12.3, 12.4_

- [ ] 5. Implement stat system with persistence
  - [ ] 5.1 Create StatSystem singleton component
    - Implement DontDestroyOnLoad for cross-scene persistence
    - Track damage, speed, and health levels
    - Implement stat upgrade methods (UpgradeDamage, UpgradeSpeed, UpgradeHealth)
    - Implement stat multiplier calculations
    - Implement AwardStatPoints() method
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_
  
  - [ ]* 5.2 Write property test for stat persistence across scenes
    - **Property 15: Stat persistence across scenes**
    - **Validates: Requirements 6.3**
  
  - [ ]* 5.3 Write property test for stat allocation effect
    - **Property 16: Stat allocation effect**
    - **Validates: Requirements 6.4**
  
  - [ ]* 5.4 Write property test for immediate stat application
    - **Property 17: Immediate stat application**
    - **Validates: Requirements 6.5**
  
  - [ ] 5.5 Create GameManager GameObject with StatSystem
    - Add StatSystem component with base stat values
    - Configure DontDestroyOnLoad
    - _Requirements: 6.1, 6.3_

- [ ] 6. Checkpoint - Test core systems
  - Verify player can move in all 8 directions
  - Verify graph nodes are correctly positioned
  - Verify pathfinding returns valid paths
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 7. Implement Leukocyte AI with state machine
  - [ ] 7.1 Create LeukocyteAI component with Patrol, Chase, and Ambush states
    - Implement state transition logic based on sight range
    - Implement Patrol behavior (random node selection from adjacency list)
    - Implement Chase behavior (pathfinding to player)
    - Implement Ambush behavior (predictive interception)
    - Add health tracking and TakeDamage() method
    - Add collision damage to player
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 11.3_
  
  - [ ]* 7.2 Write property test for patrol target validity
    - **Property 7: Patrol target validity**
    - **Validates: Requirements 4.1**
  
  - [ ]* 7.3 Write property test for chase state transition
    - **Property 8: Chase state transition**
    - **Validates: Requirements 4.2**
  
  - [ ]* 7.4 Write property test for chase pathfinding usage
    - **Property 9: Chase pathfinding usage**
    - **Validates: Requirements 4.3**
  
  - [ ] 7.5 Create Leukocyte prefab
    - Add Rigidbody2D (Dynamic, Gravity Scale: 0, Freeze Rotation Z)
    - Add CircleCollider2D (Radius: 0.4)
    - Add LeukocyteAI script (Move Speed: 3, Sight Range: 5, Enable Ambush: false)
    - Add child SpriteRenderer for enemy sprite
    - Tag as "Enemy"
    - _Requirements: 4.1, 4.2, 4.3, 4.5_
  
  - [ ] 7.6 Place Leukocyte instances in Lung scene
    - Position 3-5 Leukocytes at various graph nodes
    - _Requirements: 4.5, 11.3_

- [ ] 8. Implement combat system
  - [ ] 8.1 Add collision-based combat to PlayerController and LeukocyteAI
    - Implement OnCollisionStay2D for continuous damage
    - Apply damage cooldown to prevent instant death
    - Call StatSystem.AwardStatPoints() when enemy dies
    - Trigger player death when health reaches zero
    - _Requirements: 5.1, 5.3, 5.4, 5.5_
  
  - [ ]* 8.2 Write property test for collision damage application
    - **Property 10: Collision damage application**
    - **Validates: Requirements 5.1**
  
  - [ ]* 8.3 Write property test for damage stat scaling
    - **Property 13: Damage stat scaling**
    - **Validates: Requirements 5.5**
  
  - [ ]* 8.4 Write property test for enemy death and reward
    - **Property 12: Enemy death and reward**
    - **Validates: Requirements 5.3**
  
  - [ ] 8.5 (Optional) Create CombatManager for active attacks
    - Implement PlayerAttack() method with radius detection
    - Add attack input handling to PlayerController (Space key)
    - _Requirements: 5.2_
  
  - [ ]* 8.6 (Optional) Write property test for attack range damage
    - **Property 11: Attack range damage**
    - **Validates: Requirements 5.2**

- [ ] 9. Checkpoint - Test combat and AI
  - Verify enemies patrol between nodes
  - Verify enemies chase player when in sight range
  - Verify collision damage works for both player and enemies
  - Verify stat points are awarded on enemy death
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 10. Implement Lung boss mechanics
  - [ ] 10.1 Create BossBase abstract class
    - Implement health tracking and TakeDamage() method
    - Implement Die() method that awards bonus stat points
    - Implement abstract methods for boss-specific behavior
    - _Requirements: 9.4, 9.5_
  
  - [ ] 10.2 Create LungBoss component with breathing rhythm mechanic
    - Implement breathing cycle timer (inhale/exhale)
    - Implement arena wall movement (expand/contract)
    - Implement basic chase behavior toward player
    - Implement collision damage to player
    - Tag GameObject as "Boss"
    - _Requirements: 9.1, 9.4, 9.5, 11.5_
  
  - [ ]* 10.3 Write property test for Lung boss arena oscillation
    - **Property 19: Lung boss arena oscillation**
    - **Validates: Requirements 9.1**
  
  - [ ] 10.4 Create Lung boss arena in scene
    - Create ArenaCenter empty GameObject
    - Create 4 wall GameObjects with BoxCollider2D
    - Create LungBoss GameObject with components
    - Configure LungBoss script (Max Health: 500, Move Speed: 2, Breath Cycle: 6)
    - Assign arena walls to LungBoss script
    - _Requirements: 9.1, 9.5_

- [ ] 11. Implement Proximity UI system
  - [ ] 11.1 Create ProximityUI component
    - Calculate distance using pathfinding between player and boss nodes
    - Update distance display every 0.5 seconds
    - Display distance as text (TextMeshPro)
    - Optional: Display signal bar that fills as player gets closer
    - _Requirements: 7.1, 7.2, 7.3, 11.6_
  
  - [ ]* 11.2 Write property test for proximity UI accuracy
    - **Property 18: Proximity UI accuracy**
    - **Validates: Requirements 7.1, 7.2**
  
  - [ ] 11.3 Create UI Canvas in scene
    - Add Canvas (Screen Space - Overlay)
    - Add TextMeshPro for distance display (top-right corner)
    - Add Image for signal bar (optional, horizontal fill)
    - Add ProximityUI component and assign references
    - _Requirements: 7.1, 7.5, 11.6_

- [ ] 12. Implement scene management and transitions
  - [ ] 12.1 Create SceneTransitionManager singleton
    - Implement DontDestroyOnLoad for persistence
    - Implement LoadNextSection() with AsyncOperation
    - Implement RestartCurrentSection() for game over
    - Define scene sequence (Lung → Heart → Brain → Victory)
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5_
  
  - [ ] 12.2 Create SceneTransitionTrigger component
    - Implement trigger collider at boss location
    - Require boss defeat before allowing transition
    - Call SceneTransitionManager.LoadNextSection() on player trigger
    - _Requirements: 10.1, 10.3_
  
  - [ ] 12.3 Add SceneTransitionManager to GameManager
    - Configure scene names (LungScene, HeartScene, BrainScene, VictoryScene)
    - _Requirements: 10.1, 10.3, 12.6_
  
  - [ ] 12.4 Add scene transition trigger to Lung scene
    - Place BoxCollider2D (Is Trigger: true) at boss location
    - Add SceneTransitionTrigger component
    - _Requirements: 10.1_
  
  - [ ] 12.5 Add all scenes to Build Settings
    - Create placeholder scenes for Heart, Brain, Victory
    - Add scenes to Build Settings in correct order
    - _Requirements: 10.3, 12.6_

- [ ] 13. Add UI for health and stats
  - Create health bar UI (Slider component)
  - Create stat display UI (TextMeshPro showing current levels)
  - Update health bar in PlayerController.Update()
  - Update stat display when stats change
  - _Requirements: 6.4, 11.6_

- [ ] 14. Checkpoint - Test complete Lung section
  - Play through entire Lung section from start to boss
  - Verify all systems work together correctly
  - Verify scene transition triggers after boss defeat
  - Verify stats persist to next scene
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 15. (Optional) Implement Heart section
  - [ ] 15.1 Create HeartScene with environment
    - Use dark red and burgundy color palette
    - Create heart chamber and blood vessel environment
    - _Requirements: 8.2_
  
  - [ ] 15.2 Create HeartGraphData and populate nodes
    - Create more complex graph than Lung section
    - _Requirements: 2.5_
  
  - [ ] 15.3 Configure Leukocytes for Heart section
    - Increase move speed to 4
    - Enable Ambush mode (Enable Ambush: true)
    - Increase spawn count (6-8 enemies)
    - _Requirements: 4.6_
  
  - [ ] 15.4 Create Projectile component for Heart boss
    - Implement Initialize() method with direction and speed
    - Implement collision damage to player
    - Auto-destroy after 5 seconds
    - _Requirements: 9.2_
  
  - [ ] 15.5 Create HeartBoss component with beat pattern attacks
    - Implement beat timer for regular attack intervals
    - Implement radial projectile burst (8 projectiles)
    - Instantiate projectiles at beat intervals
    - _Requirements: 9.2, 11.8_
  
  - [ ]* 15.6 Write property test for Heart boss beat timing
    - **Property 20: Heart boss beat timing**
    - **Validates: Requirements 9.2**
  
  - [ ] 15.7 Create Heart boss arena and GameObject
    - Create HeartBoss GameObject with components
    - Configure HeartBoss script (Max Health: 600, Beat Interval: 2)
    - Create Projectile prefab
    - _Requirements: 9.2, 9.5_

- [ ] 16. (Optional) Implement Brain section
  - [ ] 16.1 Create BrainScene with environment
    - Use purple and blue-gray color palette
    - Create neuron-like complex vessel system
    - _Requirements: 8.3_
  
  - [ ] 16.2 Create BrainGraphData with complex node network
    - Create most complex graph of all sections
    - _Requirements: 2.5_
  
  - [ ] 16.3 Configure Leukocytes for Brain section
    - Increase move speed to 5
    - Enable Ambush mode
    - Increase spawn count (8-10 enemies)
    - _Requirements: 4.7_
  
  - [ ] 16.4 Create BrainBoss component with multi-phase combat
    - Implement phase transition at 66% and 33% health
    - Increase move speed in each phase
    - Spawn minions at phase transitions
    - _Requirements: 9.3, 11.8_
  
  - [ ]* 16.5 Write property test for Brain boss phase transitions
    - **Property 21: Brain boss phase transitions**
    - **Validates: Requirements 9.3**
  
  - [ ] 16.6 Create Brain boss arena and GameObject
    - Create BrainBoss GameObject with components
    - Configure BrainBoss script (Max Health: 800, Phase thresholds: 0.66, 0.33)
    - Create minion prefab (enhanced Leukocyte)
    - _Requirements: 9.3, 9.5_

- [ ] 17. (Optional) Add stat upgrade UI
  - Create stat upgrade panel with buttons for each stat
  - Display available stat points
  - Wire buttons to StatSystem upgrade methods
  - Show panel when stat points are earned
  - _Requirements: 6.4_

- [ ] 18. (Optional) Polish and juice
  - Add particle effects for combat hits
  - Add screen shake for boss attacks
  - Add sound effects for movement, combat, and UI
  - Add background music for each section
  - Add death animation for enemies
  - Add victory screen with stats summary
  - _Requirements: 11.7_

- [ ] 19. Final checkpoint - Complete game testing
  - Play through all implemented sections
  - Verify all boss fights are balanced
  - Verify no game-breaking bugs
  - Test edge cases (dead ends, no path scenarios)
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional property-based tests that can be skipped for faster MVP delivery
- Core MVP (tasks 1-14) focuses on completing Lung section with all essential features
- Optional tasks (15-18) add Heart and Brain sections plus polish
- Each checkpoint ensures stability before moving to next major system
- Property tests validate universal correctness across randomized inputs
- Unit tests (not explicitly listed) should be added for specific edge cases as needed
- Prioritize getting Lung section fully playable before expanding to other sections
