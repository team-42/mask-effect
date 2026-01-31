# User Story: Networked Combat with Prefab Mechs

## User Story

As both the host and client, I want to see the same combat scene unfolding with properly instantiated and networked mech prefabs.
The host shall be able to control the blue mechs and masks. The client shall be able to see the changes by the host and also the following battle between the mechs.

## Acceptance Criteria

1.  The host can control the blue faction's mechs and assign masks to them.
2.  Both the host and the client see the same game state, including mech positions, health, equipped masks, and combat animations.
3.  The client cannot change the game state directly; all actions must be initiated via commands to the server.
4.  Mechs are instantiated from a Unity prefab that includes visual components (e.g., "BottomHalf" and "TopHalf" GameObjects with renderers).
5.  All relevant documentation has been updated.

## Implementation Plan

This plan outlines the steps to refactor mech instantiation to use prefabs and re-implement networked combat.

### Phase 1: Mech Prefab Creation and Integration

1.  **Create a comprehensive Mech Prefab:**
    *   Create a new Unity prefab (e.g., `Assets/Prefabs/MechPrefab.prefab`).
    *   Attach the `NetworkIdentity`, `MechController`, `StatusEffectHandler`, and `MechMovement` components to the root of this prefab.
    *   Add visual child GameObjects named "BottomHalf" and "TopHalf" (e.g., Cubes) to the prefab. Ensure they have `Mesh Renderer` components and default materials. Adjust their transforms for proper visual representation.
    *   Register this `MechPrefab` in the `CustomNetworkManager`'s "Spawnable Prefabs" list.

2.  **Modify `MechSpawner.cs` to use the prefab:**
    *   Add a `[SerializeField] private GameObject mechPrefab;` field to `MechSpawner`.
    *   Update the `CreateMech` method to `Instantiate` the `mechPrefab` instead of dynamically creating GameObjects.
    *   Ensure `NetworkServer.Spawn(go)` is called after instantiation to network the mech.
    *   Adjust the logic for applying team colors and setting the mech layer to work with the prefab's structure.
    *   Update `ClearAllMechs` to use `NetworkServer.Destroy(allMechs[i].gameObject)`.

### Phase 2: Networked Combat Re-implementation

1.  **Make `BattleManager` Network-Aware:**
    *   Convert `BattleManager` from `MonoBehaviour` to `NetworkBehaviour`.
    *   Synchronize critical battle state variables (e.g., `currentState`, `roundNumber`, `roundTimer`, `playerMasksAssigned`) using `[SyncVar]`.
    *   Mark core game logic methods (`StartNewRound`, `AssignMaskToMech`, `PlayerAssignMask`, `StartCombat`, `EndRound`, `AIAssignMasks`, `RandomAssignMasks`, `SetState`, `SkipToNextRound`, `ForceStartCombat`) with `[Server]`.
    *   Remove the `TickCombat` method call from `BattleManager`'s `Update` as `MechController` will handle its own updates.

2.  **Implement Faction Control and Input:**
    *   In `CustomNetworkManager.cs`, assign `Team.Player` (blue faction) to the first connected player (host) and `Team.Enemy` (red faction) to subsequent players (clients) in `OnServerAddPlayer`.
    *   In `PlayerNetworkBehaviour.cs`, add a `[SyncVar]` for `playerFaction`.
    *   Implement a `[Server]` method `SetPlayerFaction` in `PlayerNetworkBehaviour` to set the faction.
    *   Add a `[Command]` method `CmdAssignMask` to `PlayerNetworkBehaviour`. This command will be sent from the client to the server to request mask assignments. The server will validate if the requesting player's faction matches the target mech's faction before executing the action via `BattleManager.Instance.PlayerAssignMask`.

3.  **Synchronize Combat Simulation and Mech State:**
    *   Convert `MechController` from `MonoBehaviour` to `NetworkBehaviour`.
    *   Synchronize key `MechController` properties (`mechId`, `team`, `maxHP`, `currentHP`, `armor`, `attackDamage`, `attackInterval`, `range`, `moveSpeed`, `evasion`, `isAlive`) using `[SyncVar]`.
    *   Synchronize `equippedMask` by its ID using `[SyncVar(hook = nameof(OnMaskEquippedChanged))]`. Implement `OnMaskEquippedChanged` to update the `equippedMask` reference on clients.
    *   Implement a `[ClientRpc]` method `RpcApplyMaskColor` in `MechController` to apply mask colors to the visual child objects on all clients.
    *   Mark combat-related methods (`TakeDamage`, `Die`, `TryAttack`, `OnBattleStart`) with `[Server]`.
    *   Ensure `MechController`'s `Update` method only runs on the server (`if (!isServer) return;`).

### Phase 3: Documentation Update

1.  **Update `Assets/Networking/CLAUDE.md`:**
    *   Reflect the changes made to `MechSpawner` regarding prefab instantiation and networking.
    *   Update the status of all relevant sub-tasks to `[x]` as they are completed.
    *   Ensure the documentation accurately describes the networked combat system, faction control, and mech synchronization.
