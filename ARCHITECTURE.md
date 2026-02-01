# Mask Effect Game Architecture Overview

## 1. Project Structure and Core Systems

The 'Mask Effect' project is structured to organize game assets and scripts logically, primarily within the `Assets/` directory. Key subdirectories correspond to major game components:

* **`Assets/Prefabs/`**: **Central prefab directory.** ALL project-owned prefabs live here (flat, no subfolders). Third-party prefabs (Mirror, etc.) remain in their own directories.
* **`Assets/Audio/`**: Manages sound effects and background music.
* **`Assets/Resources/Data/`**: Stores ScriptableObjects for game data, such as `ChassisData` (Mech stats) and `MaskData` (Mask definitions and abilities).
* **`Assets/Gameplay/`**: Contains core gameplay logic, including `BattleManager` for combat flow, AI, and game state management.
* **`Assets/Input/`**: Handles player input configurations, primarily through `InputSystem_Actions.inputactions`.
* **`Assets/Masks/`**: Manages Mask-related assets, prefabs, and scripts.
* **`Assets/Mechs/`**: Manages Mech-related assets, prefabs, and scripts, including movement and health.
* **`Assets/Networking/`**: Contains Mirror networking-related scripts and configurations.
* **`Assets/Scenes/`**: Stores Unity scene files, such as `LobbyScene` and `BattleArenaScene`.
* **`Assets/Scripts/Core/`**: Houses fundamental utility scripts, interfaces (e.g., `IBattleGrid`), status effect systems, and targeting logic.
* **`Assets/Scripts/UI/`**: Contains scripts for user interface elements, including `LobbyUIController` and `MaskPanelUI`.
* **`Assets/Resources/Images/`**: Stores image assets.
* **`Assets/Resources/Materials/`**: Stores material assets.
* **`Assets/UI/`**: Stores UI assets, such as UXML files for UIToolkit and IMGUI panel settings.
* **`Assets/VFX/`**: Manages visual effects for mask abilities and other in-game events.
* **`Assets/YughuesFreeMetalMaterials/`**: Third-party metal material pack (3 materials with diffuse/normal/specular textures) used for mech visuals.

## 2. Key Data Structures and Their Utilization

The project heavily utilizes **ScriptableObjects** for defining game data, promoting a data-driven design:

* **`ChassisData.cs`**: Defines the base statistics for different Mech chassis types (e.g., Max HP, Armor, Attack Damage, Attack Interval, Range, Move Speed, Evasion, Shield). These are instantiated as assets and assigned to Mech prefabs.
* **`MaskData.cs`**: Defines the properties and abilities of each Mask (Name, Effect Type, Ability details, tint color). These are also instantiated as assets and applied to Mechs during gameplay.
* **`MaskAbilityData.cs`**: Likely an abstract base class or interface for specific mask abilities, allowing for varied effects to be defined and attached to `MaskData` assets.

This approach allows for easy balancing and modification of game elements without requiring code changes.

## 3. Overall Game Flow

The game flow generally follows these stages:

1. **Lobby Scene (`LobbyScene.unity`)**: Players can host or join a game using the `LobbyUIController` (UIToolkit).
2. **Scene Transition**: Upon starting a game, the scene transitions to `BattleArenaScene.unity`.
3. **Round Setup / Mask Assignment (`BattleArenaScene.unity`)**:
    * Mechs are spawned for both player and AI teams.
    * The `MaskPanelUI` (IMGUI) allows players to drag and drop masks onto their mechs.
    * AI opponents randomly assign masks to their mechs.
    * Players can reposition their mechs on player-zone tiles.
    * Combat auto-starts once all player masks are placed.
4. **Auto Combat**: Mechs engage in combat based on their stats, assigned masks, and AI logic (`BattleManager.cs`, Mech movement/attack scripts).
5. **Win/Loss Condition**: The game determines a winner based on the last mech standing or a time limit.
6. **Next Round / Game Over**: The game either proceeds to the next round or displays a Game Over UI.

## 4. Mirror Networking Integration

Mirror is fully integrated and multiplayer is functional.

* **`CustomNetworkManager.cs`**: Extends Mirror's `NetworkManager` to handle custom network events, scene transitions, and player management.
* **`NetworkHelper`**: Dual-mode utility providing `IsOffline`, `IsServerOrOffline`, `SmartDestroy`, and `SpawnOrIgnore` methods so gameplay code works seamlessly in both singleplayer and multiplayer.
* **NetworkIdentity**: All network-spawnable prefabs (`MechPrefab`, `TilePrefab`, `MaskDragProxy`, `MaskIndicator`, `ProjectilePrefab`, `Player`, `GameController`) have `NetworkIdentity` and are registered in the `NetworkManager`'s spawn list.
* **Lobby**: `LobbyScene` provides Singleplayer (localhost-only host, `autoCreatePlayer=false`), Host Game, and Join Game (IP input + `StartClient()`) flows via UIToolkit.
* **Mech Spawning & Movement**: Mechs are spawned server-side via `NetworkServer.Spawn()`. `NetworkTransformReliable` synchronizes positions to all clients.
* **Mask Assignment**: Server-side per-side mask limit enforced in `CmdAssignMask()`. Client mask panel tracks slots and shows feedback. `EnsureUIComponents()` dynamically creates `MaskAssignmentManager`, `MaskPanelUI`, and `GameOverUI` in the multiplayer scene if not present.
* **Mech Repositioning**: Clients reposition mechs via `CmdRepositionMech` command with server-side validation (team, zone, occupancy).
* **AI in Multiplayer**: AI enemy mask pre-assignment uses `autoCreatePlayer` flag (not connection count) to reliably detect multiplayer mode before the client connects.
* **Game Over**: `GameOverUI` uses `MyTeam` property so the client sees the correct win/loss perspective. `StopHost()` is called before returning to the lobby.

## 5. Adherence to Unity Best Practices

The project demonstrates adherence to several Unity best practices:

* **ScriptableObjects for Data**: Effectively used for `ChassisData` and `MaskData`, promoting modularity and ease of content creation/balancing.
* **Prefabs**: Extensively used for game objects like Mechs, Tiles, Masks, and Projectiles, facilitating reusability and consistent instantiation.
* **Layer-based Interaction**: Mechs are assigned to a "Mech" layer for efficient raycast-based interaction.
* **Input System**: Utilizes Unity's Input System for player controls.

## 6. Unity and Mirror Versions

* **Unity Version**: 6000.3.6f1
* **Mirror Version**: 96.0.1

## 7. Current Development Status (MVP Focus)

The project has made significant progress towards the MVP. Core auto-battler mechanics, data structures, UI elements, and multiplayer networking are all functional. Both singleplayer (local AI opponent) and multiplayer (Host/Join via Mirror) modes work. Remaining tasks primarily involve balancing, visual/audio polish, and completing UI elements.

---

# Cleanup and Refactoring Suggestions (Phase 2 - Step 1)

## Networking Status

Multiplayer is functional. Singleplayer uses a localhost-only Mirror host with `autoCreatePlayer=false`. Multiplayer uses the Host/Join flow via `StartHost()` / `StartClient()` with IP input. The `NetworkHelper` dual-mode utility abstracts singleplayer vs. multiplayer differences so gameplay code does not need to branch explicitly.

Key networking features implemented:
* Server-authoritative mech spawning, mask assignment (with per-side limits), and mech repositioning.
* `NetworkTransformReliable` for position synchronization.
* `SyncVar`-based chassis data path synchronization for client-side model reconstruction.
* Dynamic UI component creation (`EnsureUIComponents()`) for multiplayer scenes.
* Correct client-side Game Over perspective via `MyTeam` property.
* Clean network teardown (`StopHost()`) on lobby return.
