# Networking Component Sub-tasks

## Overview

This document outlines networking-related tasks for the 'Mask Effect' project. A network mode where players can play against each other is included in Phase 1 of the action plan, implemented independently of the core game logic. Mirror has been selected as the networking solution (replacing the initial NGO consideration).

## Sub-tasks

- [x] Select & Integrate Networking Solution (Mirror).
- [x] Implement Basic Lobby & Room Creation (LobbyScene with UIToolkit, host/join buttons).
- [x] Add NetworkIdentity to all mech and projectile prefabs.
- [x] Implement LobbyScene → BattleArenaScene scene transition via Mirror.
- [ ] Synchronize Mech Spawning Across Network.
- [ ] Synchronize Mech Movement & Actions.
- [ ] Synchronize Health & Combat Events.
- [x] Remove legacy NGO references (DefaultNetworkPrefabs.asset deleted).

## CURRENT_STATUS.md

- [x] Networking solution integrated (Mirror).
- [x] Basic lobby and room creation implemented (LobbyScene with UIToolkit, host/join buttons).
- [x] NetworkIdentity added to all mech and projectile prefabs.
- [x] Scene transition from LobbyScene → BattleArenaScene working via Mirror.
- [ ] Mech spawning synchronized across network.
- [ ] Mech movement and actions synchronized.
- [ ] Health and combat events synchronized.
- [x] Legacy NGO setup removed.
