# Mask Effect AI Agent Action Plan

## Project Overview

Mask Effect is an auto-battler where Mechs wear Masks to specify tactical roles. The core gameplay loop involves players assigning a few masks each round to reshape mech roles, targeting, and abilities. The MVP focuses on a local test mode with random AI opponents, 1 level, 3 mechs, and 3 masks, with a final score display.

## Consensus-Driven Plan Adjustments

Based on the consensus from multiple AI models, the original work breakdown has been revised to prioritize feasibility within a 48-hour game jam. The primary adjustments involve significant scope reduction, particularly cutting online multiplayer and simplifying progression systems, to focus on a polished core local AI auto-battler experience. Emphasis is placed on clear mask effects and robust player feedback.

## Detailed Step-by-Step Action Plan (MVP Focus)

### Phase 1: Core Auto-Battler Mechanics (First 12-18 Hours)

- [x] **Technical Setup & Project Structure:**
  - [x] Verify Unity Project & Version Control (Git) setup.
  - [ ] Integrate essential Unity Packages (e.g., TextMeshPro).
  - [x] Ensure AI Tool Access (Claude Code/Sixth AI) is configured.
  - [x] Finalize `Assets/` directory structure as planned.
- [x] **Basic Auto-Battler Core:**
  - [x] Define Mech Stats & Basic Combat Rules (Max HP, Armor, Attack Damage, Attack Interval, Range, Move Speed, Evasion, Shield).
  - [x] Implement Mech Spawning (5-10 unmasked mechs, mirrored for both sides).
  - [x] Implement Basic Movement AI (move towards target, handle blocked tiles).
  - [x] Implement Basic Attack & Damage System (Evasion, Armor reduction, Shield first, Min damage 1).
  - [x] Implement Health & Death System for Mechs.
  - [x] Implement Win/Loss Condition (Last Mech Standing, optional time limit).
- [x] **Network Preparation & Randomized AI Opponent:**
  - [x] Mirror networking integrated (skeleton: NetworkIdentity on prefabs, LobbyScene with scene transition).
  - [x] Prefabs created for all network-spawnable objects (MechPrefab, TilePrefab, MaskDragProxy, MaskIndicator, ProjectilePrefab).
  - [x] Develop a randomized AI opponent that assigns masks to its mechs for MVP battles.
- [x] **Battle Arena Environment:**
  - [x] Create Simple Battle Arena Scene (40 tiles: 15 per player, 10 neutral middle).
  - [x] Add Basic Lighting & Camera Setup.
- [x] **Core Mask System (Initial Implementation):**
  - [x] Define Mask Data Structure (Scriptable Object: Name, Effect Type, Ability details).
  - [x] Implement Basic Mask Application Logic (drag & drop, 2-3 masks per side, one per mech).
  - [x] Create 3 Placeholder Masks & Effects (Warrior, Rogue, Angel - simplified L1 abilities).
  - [x] Implement Status Effects (Shield, Mark, Slow, Root, Taunt) as needed for masks.
  - [x] Implement basic targeting overrides for masks (e.g., lowest HP, highest threat).
  - [x] Configure distinct mask tint colors (Warrior=red, Rogue=green, Angel=gold) for visual identification.
  - [x] Replace mask indicator disc with glowing ground ring (custom shader with pulse animation).
  - [x] Implement random mask assignment for both teams in test/auto-play mode.
- [x] **Essential UI & Feedback:**
  - [x] Design & Implement mask assignment UI (IMGUI side panel with colored mask buttons).
  - [x] Implement LobbyScene with UIToolkit start menu and scene transition to BattleArenaScene.
  - [ ] Design & Implement Basic HUD (Health bars, timer).
  - [x] Implement Clear Visual Procs for mask effects (shield icons, mark icons, grapple animation, taunt indicator, **mech death particle effect using EnergyExplosion VFX**).
  - [ ] Implement Game Over UI.

### Phase 2: Refinement & Polish (Next 12-18 Hours)

- [ ] **Gameplay Refinement:**
  - [x] Implement remaining L1 Mask Abilities for Warrior, Rogue, Angel masks.
  - [ ] Balance Mech Stats and Mask Effects for engaging combat.
  - [x] Integrate Mask Selection into Player Flow (Round Setup -> Mask Assignment -> Auto Combat -> Next Round).
- [ ] **Visual & Audio Polish (MVP Level):**
  - [x] Replace primitive mech shapes with 3D models (Scout, Jet, Tank loaded from Resources/Models/).
  - [ ] Create Placeholder Mask Icons (3 icons).
  - [ ] Add Basic Sound Effects (Attacks, UI, Win/Loss).
  - [ ] Integrate Background Music.
- [x] **Render Pipeline & Material Compatibility:**
  - [x] Identified and resolved pink material issues on tiles, projectiles, and masks after importing the Particle Pack.
  - [x] Created new URP-compatible materials (`TileMaterial.mat`, `ProjectileMaterial_URP.mat`, `MaskMaterial_URP.mat`) and assigned them to respective prefabs (`TilePrefab`, `ProjectilePrefab`, `MaskDragProxy`, `MaskIndicator`).
  - [x] Ensured all new materials use the `Universal Render Pipeline/Lit` shader.
- [ ] **AI Tool Integration (Ongoing):**
  - [ ] Utilize AI for small utility scripts.
  - [ ] Utilize AI for placeholder textures/materials.

### Phase 3: Demo & Submission (Final 12 Hours)

- [ ] **Demo Build & Testing:**
  - [ ] Perform Integration Test of Core Systems.
  - [ ] Identify & Fix Critical Bugs.
  - [ ] Create Standalone Build for Presentation.
- [ ] **Presentation Content:**
  - [ ] Prepare Short Demo Script & Talking Points.
  - [ ] Prepare Readme/Submission Documentation.
  - [ ] Upload Game & Submit Project.

## CURRENT_STATUS.md

- [x] Initial project setup and directory structure.
- [x] Core auto-battler mechanics implemented (mech spawning, movement, basic combat).
- [x] Basic mask system with 3 masks and simplified L1 abilities.
- [x] Mask tint colors configured; glowing ground ring indicator replaces old head disc (custom MaskRing shader with pulse animation).
- [x] Essential UI and visual feedback for gameplay.
- [x] Game loop (round setup, mask assignment, combat, next round) functional.
- [x] BattleArenaScene: visual grid (20x10, 3 colored zones), drag-and-drop mask assignment via IMGUI side panel, mech repositioning, enemy masks pre-assigned randomly, combat auto-starts when all player masks placed.
- [x] Player interaction: click mask in left-side panel then click player mech to assign (ground ring appears in mask color); drag player mechs to reposition on player-zone tiles; camera controls (WASD, right-click rotate, scroll zoom).
- [x] 3D mech models replace primitives (Scout, Jet, Tank loaded from Resources/Models/).
- [x] Prefab system: MechPrefab, TilePrefab, MaskDragProxy, MaskIndicator, ProjectilePrefab with NetworkIdentity for Mirror readiness.
- [x] LobbyScene with UIToolkit start menu; scene transition to BattleArenaScene works.
- [x] Mirror networking skeleton integrated (NetworkIdentity on spawnable prefabs, Player + GameController prefabs).
- [x] Camera adjusted for full battlefield view (position 0/25/-12, 60deg pitch).
- [x] Jet chassis flies: elevated hover with bobbing animation, direct movement ignoring ground obstacles, projectiles spawn/target at visual height.
- [x] Death particle effects (EnergyExplosion VFX) on mech death via deathEffectPrefab.
- [x] URP-compatible materials for tiles, projectiles, and masks (TileMaterial, ProjectileMaterial_URP, MaskMaterial_URP).
- [x] UnityTechnologies ParticlePack integrated for VFX assets.
- [ ] Basic art and audio placeholders integrated.
- [ ] Demo build and presentation prepared.

## Critical Risks & Mitigation

- **Scope Creep:** Strictly adhere to the MVP. Any feature not directly contributing to the core 'Mask Effect' auto-battler with local AI should be cut.
- **Time Management:** Implement rough time estimates for all tasks. Use a 'cut list' for features to drop if behind schedule.
- **Complexity of Mask Effects:** Start with simple, passive stat boosts before attempting complex battlefield manipulations. Ensure a flexible Scriptable Object architecture for masks.
- **UI/UX:** Prioritize clear and functional UI over visual polish initially. Ensure visual/audio feedback for mask effects is prominent.
