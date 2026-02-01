# Networking Component Sub-tasks

## Overview

This document outlines networking-related tasks for the 'Mask Effect' project. A network mode where players can play against each other is included in Phase 1 of the action plan, implemented independently of the core game logic. Mirror has been selected as the networking solution (replacing the initial NGO consideration).

## Sub-tasks

- [x] Select & Integrate Networking Solution (Mirror).
- [x] Implement Basic Lobby & Room Creation (LobbyScene with UIToolkit, host/join buttons).
- [x] Add NetworkIdentity to all mech and projectile prefabs.
- [x] Implement LobbyScene → BattleArenaScene scene transition via Mirror.
- [x] Synchronize Mech Spawning Across Network. (Initial steps for projectile spawning implemented)
- [x] Synchronize Mech Movement & Actions. (Initial steps for projectile movement and mech stats implemented)
- [ ] Synchronize Health & Combat Events. (Damage and destruction are server-authoritative for projectiles)
- [x] Remove legacy NGO references (DefaultNetworkPrefabs.asset deleted).
- [x] Make `Projectile.cs` a `NetworkBehaviour`.
- [x] Implement `[SyncVar]` for `Projectile` properties and resolve `MechController` references on clients.
- [x] Update `MechController.TryAttack` to spawn networked projectiles.
- [x] Make `MechController.cs` a `NetworkBehaviour` and add `[SyncVar]` to relevant properties.
- [x] Add `NetworkTransform` to `ProjectilePrefab` (manual step required in Unity Editor).
- [x] Implement `CmdRepositionMech` in BattleManager for client mech repositioning during mask assignment phase (server-validated team/zone/occupancy, synced via NetworkTransform).

## CURRENT_STATUS.md

- [x] Networking solution integrated (Mirror).
- [x] Basic lobby and room creation implemented (LobbyScene with UIToolkit, host/join buttons).
- [x] NetworkIdentity added to all mech and projectile prefabs.
- [x] Scene transition from LobbyScene → BattleArenaScene working via Mirror.
- [x] Mech spawning synchronized across network. (Initial steps for projectile spawning implemented)
- [x] Mech movement and actions synchronized. (Initial steps for projectile movement and mech stats implemented)
- [ ] Health and combat events synchronized. (Damage and destruction are server-authoritative for projectiles)
- [x] Legacy NGO setup removed.
- [x] `Projectile.cs` now inherits from `NetworkBehaviour`.
- [x] `Projectile.cs` uses `[SyncVar]` for `damage`, `damageType`, `attackerNetId`, and `targetNetId`.
- [x] `MechController.TryAttack` now uses `NetworkServer.Spawn` for projectiles and passes `netId`s.
- [x] `MechController.cs` now inherits from `NetworkBehaviour` and uses `[SyncVar]` for `mechId`, `team`, `maxHP`, `currentHP`, `armor`, `attackDamage`, `attackInterval`, `range`, `moveSpeed`, `evasion`, `currentDamageType`, `currentResistanceType`, `currentResistanceValue`, `isAlive`, `currentTargetNetId`, `targetingMode`, `attackCooldown`, and `retargetTimer`.
- [x] `NetworkTransform` component needs to be manually added to `Assets/Prefabs/ProjectilePrefab.prefab` in the Unity Editor.
- [x] `CmdRepositionMech` command added to BattleManager — enables client mech drag-and-drop repositioning in multiplayer mask assignment phase.
