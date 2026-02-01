# Mask Effect Game Architecture Overview

## 1. Project Structure and Core Systems

The 'Mask Effect' project is structured to organize game assets and scripts logically, primarily within the `Assets/` directory. Key subdirectories correspond to major game components:

*   **`Assets/Audio/`**: Manages sound effects and background music.
*   **`Assets/Data/`**: Stores ScriptableObjects for game data, such as `ChassisData` (Mech stats) and `MaskData` (Mask definitions and abilities).
*   **`Assets/Gameplay/`**: Contains core gameplay logic, including `BattleManager` for combat flow, AI, and game state management.
*   **`Assets/Input/`**: Handles player input configurations, primarily through `InputSystem_Actions.inputactions`.
*   **`Assets/Masks/`**: Manages Mask-related assets, prefabs, and scripts.
*   **`Assets/Mechs/`**: Manages Mech-related assets, prefabs, and scripts, including movement and health.
*   **`Assets/Networking/`**: Contains Mirror networking-related scripts and configurations.
*   **`Assets/Scenes/`**: Stores Unity scene files, such as `LobbyScene` and `BattleArenaScene`.
*   **`Assets/Scripts/Core/`**: Houses fundamental utility scripts, interfaces (e.g., `IBattleGrid`), status effect systems, and targeting logic.
*   **`Assets/Scripts/UI/`**: Contains scripts for user interface elements, including `LobbyUIController` and `MaskPanelUI`.
*   **`Assets/UI/`**: Stores UI assets, such as UXML files for UIToolkit and IMGUI panel settings.
*   **`Assets/VFX/`**: Manages visual effects for mask abilities and other in-game events.
*   **`Assets/YughuesFreeMetalMaterials/`**: Third-party metal material pack (3 materials with diffuse/normal/specular textures) used for mech visuals.

## 2. Key Data Structures and Their Utilization

The project heavily utilizes **ScriptableObjects** for defining game data, promoting a data-driven design:

*   **`ChassisData.cs`**: Defines the base statistics for different Mech chassis types (e.g., Max HP, Armor, Attack Damage, Attack Interval, Range, Move Speed, Evasion, Shield). These are instantiated as assets and assigned to Mech prefabs.
*   **`MaskData.cs`**: Defines the properties and abilities of each Mask (Name, Effect Type, Ability details, tint color). These are also instantiated as assets and applied to Mechs during gameplay.
*   **`MaskAbilityData.cs`**: Likely an abstract base class or interface for specific mask abilities, allowing for varied effects to be defined and attached to `MaskData` assets.

This approach allows for easy balancing and modification of game elements without requiring code changes.

## 3. Overall Game Flow

The game flow generally follows these stages:

1.  **Lobby Scene (`LobbyScene.unity`)**: Players can host or join a game using the `LobbyUIController` (UIToolkit).
2.  **Scene Transition**: Upon starting a game, the scene transitions to `BattleArenaScene.unity`.
3.  **Round Setup / Mask Assignment (`BattleArenaScene.unity`)**:
    *   Mechs are spawned for both player and AI teams.
    *   The `MaskPanelUI` (IMGUI) allows players to drag and drop masks onto their mechs.
    *   AI opponents randomly assign masks to their mechs.
    *   Players can reposition their mechs on player-zone tiles.
    *   Combat auto-starts once all player masks are placed.
4.  **Auto Combat**: Mechs engage in combat based on their stats, assigned masks, and AI logic (`BattleManager.cs`, Mech movement/attack scripts).
5.  **Win/Loss Condition**: The game determines a winner based on the last mech standing or a time limit.
6.  **Next Round / Game Over**: The game either proceeds to the next round or displays a Game Over UI.

## 4. Mirror Networking Integration

Mirror has been integrated as the networking solution.

*   **`CustomNetworkManager.cs`**: Extends Mirror's `NetworkManager` to handle custom network events, scene transitions, and player management.
*   **NetworkIdentity**: All network-spawnable prefabs (e.g., `MechPrefab`, `TilePrefab`, `MaskDragProxy`, `MaskIndicator`, `ProjectilePrefab`, `Player`, `GameController`) have a `NetworkIdentity` component, indicating their readiness for network synchronization.
*   **Lobby and Scene Transitions**: Basic lobby functionality and scene transitions between `LobbyScene` and `BattleArenaScene` are implemented and work over the network.

**Current Status of Networking Synchronization:**
While the framework is in place, the `Networking/CLAUDE.md` indicates that synchronization of core gameplay elements like Mech spawning, movement, actions, health, and combat events is *not yet implemented*. The MVP focuses on a local AI auto-battler, so the current networking integration serves as a skeleton for future multiplayer expansion.

## 5. Adherence to Unity Best Practices

The project demonstrates adherence to several Unity best practices:

*   **ScriptableObjects for Data**: Effectively used for `ChassisData` and `MaskData`, promoting modularity and ease of content creation/balancing.
*   **Prefabs**: Extensively used for game objects like Mechs, Tiles, Masks, and Projectiles, facilitating reusability and consistent instantiation.
*   **Layer-based Interaction**: Mechs are assigned to a "Mech" layer for efficient raycast-based interaction.
*   **Input System**: Utilizes Unity's Input System for player controls.

## 6. Unity and Mirror Versions

*   **Unity Version**: 6000.3.6f1
*   **Mirror Version**: 96.0.1

## 7. Current Development Status (MVP Focus)

The project has made significant progress towards the MVP, focusing on a polished core local AI auto-battler experience. Many core mechanics, data structures, and UI elements are implemented. Remaining tasks primarily involve balancing, visual/audio polish, and completing UI elements. The networking aspect is currently a foundational setup for future expansion, with core gameplay synchronization pending.

---

# Cleanup and Refactoring Suggestions (Phase 2 - Step 1)

## Networking Integration Review

**Current State:**
The project has Mirror integrated, and basic network functionality like scene transitions and `NetworkIdentity` on prefabs is in place. However, the `Networking/CLAUDE.md` explicitly states that synchronization of "Mech Spawning Across Network," "Mech Movement & Actions," and "Health & Combat Events" is *not yet implemented*. The root `CLAUDE.md` also emphasizes an MVP focus on a "local test mode with random AI opponents."

**Suggestions:**

1.  **Clarify Networking Scope for MVP:** Given the MVP's focus on local AI, it's crucial to explicitly define whether the current networking skeleton is purely for future-proofing or if any minimal network synchronization is intended for the MVP.
    *   **Recommendation:** If multiplayer is strictly out of scope for the MVP, consider temporarily disabling or clearly marking network-related code that is not actively used to avoid confusion and potential bugs. Ensure that the local AI auto-battler functions entirely independently of any network manager presence.
2.  **Implement Essential Synchronization (if MVP scope changes):** If the MVP scope *does* expand to include basic multiplayer, the following are critical next steps:
    *   **Networked Mech Spawning:** Implement `NetworkServer.Spawn()` for mechs and projectiles within `CustomNetworkManager` or a dedicated `NetworkSpawner` script.
    *   **Networked Movement:** Utilize `NetworkTransform` or custom `SyncVar`s and `[Command]` methods for reliable mech movement synchronization.
    *   **Networked Combat:** Implement `[Command]` and `[ClientRpc]` attributes for damage calculation, health updates, and status effect application to ensure all clients have a consistent view of combat.
3.  **Separate Local vs. Networked Logic:** For scripts that handle both local AI and potentially networked player input/actions (e.g., `BattleManager`, Mech control scripts), clearly separate the logic. Use `isLocalPlayer` and `isServer` checks to ensure the correct code paths are executed.
4.  **Review `CustomNetworkManager`:** Ensure `CustomNetworkManager.cs` is lean and focused. If it's accumulating too much game-specific logic, consider offloading responsibilities to other dedicated network components (e.g., a `NetworkGameManager` for game state synchronization).
5.  **Consistency in Prefab NetworkIdentity:** Double-check that *all* prefabs intended to be spawned or managed over the network have a `NetworkIdentity` component and are registered in the `NetworkManager`'s spawnable prefabs list.
