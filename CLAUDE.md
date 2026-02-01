# Mask Effect AI Agent Action Plan

## Technical Environment

- **Unity Version:** 6.3 LTS (6000.3.6f1)
- **Render Pipeline:** URP (Universal Render Pipeline)
- **Networking:** Mirror

## Prefab Convention

All project-owned `.prefab` files MUST be placed in `Assets/Prefabs/` (flat, no subfolders).
Third-party prefabs (e.g. Mirror, Particle Pack) are exempt from this rule.
Never create prefab files in other directories.

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
  - [x] Mirror networking fully functional (Host/Join flow, server-authoritative spawning, mask assignment, mech repositioning, NetworkTransform sync, dual-mode NetworkHelper).
  - [x] Prefabs created for all network-spawnable objects (MechPrefab, TilePrefab, MaskDragProxy, MaskIndicator, ProjectilePrefab).
  - [x] Develop a randomized AI opponent that assigns masks to its mechs for MVP battles.
- [x] **Battle Arena Environment:**
  - [x] Create Simple Battle Arena Scene (28x12 grid: 7 deep per player, 14 neutral middle).
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
  - [x] Implement Game Over UI (IMGUI overlay with round stats, "Naechste Runde" and "Zurueck zur Lobby" buttons).

### Phase 2: Refinement & Polish (Next 12-18 Hours)

- [x] **Bug Fixes & Stability:**
  - [x] Fix SP server binding to all interfaces — restricted to localhost before StartHost().
  - [x] Fix server persisting after lobby return — StopHost() called in GameOverUI before scene transition.
  - [x] Fix multiplayer not working — added Host/Join UI flow with IP input, StartClient() for joining.
  - [x] Fix jets not flying — added hover logic + HoverBob in SetupVisuals() (was lost on SyncVar rebuild).
  - [x] Fix projectiles as white cubes — yellow URP material, reduced scale.
  - [x] Fix MaskRing shader edgeFade bug — inverted smoothstep was zeroing out the entire ring.
  - [x] Fix projectiles instantly disappearing — `_target` was null on first Update() because SP host took multiplayer path where OnStartClient netId resolution raced with Update; Initialize() now sets direct refs immediately.
  - [x] All chassis (Scout, Tank, Jet) now fire projectiles — Scout/Tank were melee-only, added isRanged + projectilePrefab + increased range.
  - [x] Fix client unlimited mask placement — added per-side mask count guard in `CmdAssignMask()` server validation.
  - [x] Fix AI pre-assigning enemy masks in MP round 1 — replaced `IsMultiplayerMatch` (connection count) with `autoCreatePlayer` check that works before client connects.
  - [x] Fix client missing Game Over UI — added `OnStartClient()` override in BattleManager to create `GameOverUI` on clients.
  - [x] Fix win/loss perspective for client — `GameOverUI` now uses `MyTeam` property so client sees "SIEG!" when their team wins.
  - [x] Fix missing mask assignment UI in MP — `BattleManager.EnsureUIComponents()` dynamically creates `MaskAssignmentManager`, `MaskPanelUI`, and `GameOverUI` if not present in scene (multiplayer scene was missing these).
  - [x] Fix client unable to reposition mechs in MP — mech dragging was local-only; added `CmdRepositionMech` command in BattleManager with server-side validation (team, zone, occupancy); `MaskAssignmentManager.EndMechDrag()` now routes through command in multiplayer; NetworkTransform syncs position to all clients.
- [ ] **Gameplay Refinement:**
  - [x] Implement remaining L1 Mask Abilities for Warrior, Rogue, Angel masks.
  - [ ] Balance Mech Stats and Mask Effects for engaging combat.
  - [x] Integrate Mask Selection into Player Flow (Round Setup -> Mask Assignment -> Auto Combat -> Next Round).
- [ ] **Visual & Audio Polish (MVP Level):**
  - [x] Replace primitive mech shapes with 3D models (Scout, Jet, Tank loaded from Resources/Models/).
  - [x] MaskRing shader rewritten for URP (HLSLPROGRAM, SRP Batcher compatible, additive glow).
  - [ ] Create Placeholder Mask Icons (3 icons).
  - [x] Add chassis-specific laser SFX (Scout, Jet, Tank sounds in `Resources/MechSounds/`, played via `MechController.PlayLaserSound()` on each ranged attack).
  - [ ] Add remaining Sound Effects (UI, Win/Loss).
  - [x] Integrate Background Music (persistent MusicManager singleton with DontDestroyOnLoad + own AudioListener).
  - [x] Procedural Mars environment: `MaskEffect/MarsSurface` ground shader (voronoi cracks, FBM sand/rock, distance fog) + `MaskEffect/SpaceSkybox` skybox retuned to dusty Mars atmosphere. Applied to all 3 arena scenes.
- [x] **Render Pipeline & Material Compatibility:**
  - [x] Identified and resolved pink material issues on tiles, projectiles, and masks after importing the Particle Pack.
  - [x] Created new URP-compatible materials (`TileMaterial.mat`, `ProjectileMaterial_URP.mat`, `MaskMaterial_URP.mat`) and assigned them to respective prefabs (`TilePrefab`, `ProjectilePrefab`, `MaskDragProxy`, `MaskIndicator`).
  - [x] Ensured all new materials use the `Universal Render Pipeline/Lit` shader.
  - [x] MaskRing material uses custom `MaskEffect/MaskRing` shader (URP HLSL, additive blending).
  - [x] Bulk-converted all Mirror example materials and project materials to URP.
  - [x] Imported `YughuesFreeMetalMaterials` asset pack (3 metal materials with diffuse/normal/specular textures).
  - [x] Updated model import settings (`.obj.meta`) for Scout, Jet, Tank models.
  - [x] Changed enemy team color from orange to yellow for better visual distinction.
  - [x] Procedural Mars surface shader (`Assets/Shaders/MarsSurface.shader`) with voronoi rock cracks, FBM sand/rock terrain, and distance fade. Skybox retuned to Mars atmosphere (dusty orange). Ground plane (150x150) placed via `Assets/Editor/SetSkybox.cs` menu tool. Directional light warm-tinted (1, 0.85, 0.7) at intensity 1.5.

### Phase 3: Demo & Submission (Final 12 Hours)

- [ ] **Demo Build & Testing:**
  - [ ] Perform Integration Test of Core Systems.
  - [ ] Identify & Fix Critical Bugs.
  - [ ] Create Standalone Build for Presentation.
- [ ] **Presentation Content:**
  - [ ] Prepare Short Demo Script & Talking Points.
  - [ ] Prepare Readme/Submission Documentation.
  - [ ] Upload Game & Submit Project.

## Current Status

- [x] Core auto-battler mechanics (mech spawning, movement, combat, death).
- [x] 5 masks (Warrior, Rogue, Angel, Phantom, Commander) with full per-chassis abilities.
- [x] 5 chassis types (Scout, Jet, Tank, Sniper, Colossus) with unique combat mechanics.
- [x] Sniper charge-up mechanic (1s charge before firing, interrupted on hit, reset on kill via KillShot).
- [x] Colossus dual-target cleave (melee hits 2 enemies in range).
- [x] 25 mask-chassis abilities (5 masks x 5 chassis), each with ScriptableObject data assets.
- [x] 4 new status effects: Stun (blocks actions), Untargetable (skipped by targeting), Invisible (skipped by targeting), MissChance (additive evasion).
- [x] Per-chassis spawn preferences: Sniper → Backline, Colossus → FrontlineCenter, others → Random.
- [x] masksPerSide increased from 2 to 3 for more strategic depth.
- [x] Glowing ground ring under masked mechs (MaskRing shader, URP HLSL, additive glow + pulse).
- [x] 3D mech models (Scout, Jet, Tank, Sniper, Colossus) with team colors and mask tint on top half.
- [x] All ranged chassis fire projectiles (Scout range 3, Tank range 2.5, Jet range 4, Sniper range 5).
- [x] Jet chassis hovers with bobbing animation; projectiles target VisualCenter height.
- [x] Game loop: round setup → mask assignment → auto combat → round end → next round.
- [x] BattleArenaScene: 28x12 grid, 3 zones (player 7, neutral 14, enemy 7), IMGUI mask panel, drag-to-reposition, camera controls (edge panning removed, arena bounds X:-20..20 Z:-15..15, zoom clamped).
- [x] LobbyScene: UIToolkit menu with Singleplayer, Host Game, Join (IP field + button).
- [x] Singleplayer: localhost-only Mirror host, autoCreatePlayer=false, StopHost() on lobby return.
- [x] Multiplayer: Host/Join flow via Mirror (StartHost / StartClient with IP).
- [x] Multiplayer: Server-side per-side mask limit enforced in `CmdAssignMask()`. Client mask panel with slot tracking and "(vergeben)" feedback. `EnsureUIComponents()` dynamically creates `MaskAssignmentManager`, `MaskPanelUI`, `GameOverUI` in MP scene.
- [x] Multiplayer: AI enemy mask pre-assignment uses `autoCreatePlayer` flag (not connection count) to reliably detect MP mode before client connects.
- [x] Multiplayer: Client mech repositioning via `CmdRepositionMech` command (server-validated team/zone/occupancy, NetworkTransform syncs position).
- [x] Game Over UI: IMGUI overlay with stats, next round, and lobby return (with proper network cleanup). Perspective-correct for client (`MyTeam` property: client sees "SIEG!" when Enemy team wins).
- [x] NetworkHelper dual-mode system (IsOffline, IsServerOrOffline, SmartDestroy, SpawnOrIgnore).
- [x] URP-compatible materials throughout (tiles, projectiles, masks, ring shader, all Mirror examples).
- [x] YughuesFreeMetalMaterials asset pack integrated (metal textures for mech visuals).
- [x] Enemy team color updated to yellow (was orange) for clearer team distinction.
- [x] Death particle effects (EnergyExplosion VFX).
- [x] MusicManager singleton (DontDestroyOnLoad) with own AudioListener, 2-track playlist loop, clip-null recovery via Resources.Load fallback. Audio files in `Assets/Resources/Audio/`.
- [x] Background music integrated (2 tracks, persistent MusicManager with own AudioListener, auto-advances playlist, survives all scene transitions).
- [x] Chassis-specific laser attack SFX (`scout_laser.wav`, `jet_laser.wav`, `tank_laser.wav` in `Assets/Resources/MechSounds/`). Loaded per chassis type via `Resources.Load`, played with `AudioSource.PlayOneShot` in `MechController.TryAttack()`.
- [x] Procedural Mars environment: `MaskEffect/MarsSurface` shader (`Assets/Shaders/MarsSurface.shader`, `Assets/Materials/MarsGround.mat`) with voronoi rock cracks, FBM sand/rock/dust terrain variation, bump mapping, distance fade to dusty horizon. 150x150 ground plane placed beneath grid. Skybox (`MaskEffect/SpaceSkybox`) retuned to Mars atmosphere (dusty orange tones, faint stars). Warm directional light (1, 0.85, 0.7) at 1.5 intensity. Dusty orange fog. Applied to all 3 arena scenes.
- [ ] Remaining sound effects (UI, Win/Loss).
- [ ] Demo build and presentation.

## Mask-Chassis Ability Matrix (5x5)

|              | **Scout**       | **Jet**          | **Tank**         | **Sniper**           | **Colossus**          |
|--------------|-----------------|------------------|------------------|----------------------|-----------------------|
| **Warrior**  | HitAndRun       | DiveSlash        | Challenge        | ArmorPiercingRound   | OverwhelmingForce     |
| **Rogue**    | ExecuteChain    | Mark             | Grapple          | KillShot             | CrushingGrip          |
| **Angel**    | GuardianLeap    | SkyBarrier       | Sanctuary        | OverwatchProtocol    | LivingFortress        |
| **Phantom**  | PhaseDash       | Cloak            | Mirage           | RevengeBlink         | DisplacementField     |
| **Commander**| RallyCry        | AirSuperiority   | IronWill         | Spotter              | TitanPresence         |

## Mask Color Palette

| Mask      | Color       | Hex     | RGB                |
|-----------|-------------|---------|--------------------|
| Warrior   | Red         | —       | Unity Color.red    |
| Rogue     | Green       | —       | Unity Color.green  |
| Angel     | Gold        | —       | Unity Color(1,0.84,0) |
| Phantom   | Violet      | #9B5DE5 | (0.608, 0.365, 0.898) |
| Commander | Light Blue  | #00B4D8 | (0, 0.706, 0.847)    |

## Chassis Stats

| Chassis  | HP  | Armor | Dmg | Interval | Range | Speed | Ranged | Special               |
|----------|-----|-------|-----|----------|-------|-------|--------|-----------------------|
| Scout    | 100 | 10    | 10  | 1.0s     | 3     | 2.0   | Yes    | —                     |
| Jet      | 80  | 5     | 15  | 1.2s     | 4     | 3.0   | Yes    | Flies, hover bob      |
| Tank     | 150 | 20    | 12  | 1.5s     | 2.5   | 1.0   | Yes    | —                     |
| Sniper   | 60  | 5     | 45  | 3.0s     | 5     | 0.8   | Yes    | 1s charge-up, backline|
| Colossus | 250 | 30    | 25  | 2.5s     | 1     | 0.5   | No     | Cleave 2 targets, front|

## Critical Risks & Mitigation

- **Scope Creep:** Strictly adhere to the MVP. Any feature not directly contributing to the core 'Mask Effect' auto-battler with local AI should be cut.
- **Time Management:** Implement rough time estimates for all tasks. Use a 'cut list' for features to drop if behind schedule.
- **Complexity of Mask Effects:** Start with simple, passive stat boosts before attempting complex battlefield manipulations. Ensure a flexible Scriptable Object architecture for masks.
- **UI/UX:** Prioritize clear and functional UI over visual polish initially. Ensure visual/audio feedback for mask effects is prominent.
