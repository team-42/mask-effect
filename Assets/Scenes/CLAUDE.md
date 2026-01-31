# Scenes Component Sub-tasks

## Overview

This document outlines the specific scene-related tasks for the 'Mask Effect' project, extracted from the main project action plan. This includes creating the battle arena, lobby, main menu, and game over scenes.

## Sub-tasks

- [x] Create Simple Battle Arena Scene (40 tiles: 15 per player, 10 neutral middle).
- [x] Add Basic Lighting & Camera Setup to the Battle Arena Scene.
- [x] Combine BattleArenaScene with BattleScene: visual grid (20x10, 3 colored zones), mech spawning, mask assignment panel, camera controls.
- [x] BattleArenaScene = Planning phase (reposition mechs, assign masks). BattleScene = Combat rendering (auto-play mode).
- [x] Create LobbyScene with UIToolkit UI for Mirror host/join flow.
- [x] Implement scene transition from LobbyScene → BattleArenaScene.
- [x] Fix camera positioning for full battlefield view (0,25,-12 at 60° pitch).
- [ ] Create Main Menu Scene.
- [ ] Create Game Over Scene.

## CURRENT_STATUS.md

- [x] Battle Arena Scene created.
- [x] Basic lighting and camera setup added to Battle Arena.
- [x] BattleArenaScene upgraded: BattleSystem GO with SimpleFlatGrid (visual tiles), MechSpawner, BattleManager, MaskAssignmentManager, MaskPanelUI (left-side). Camera with PlayerCameraControl. Mask assignment updates mech top-half tint color.
- [x] LobbyScene created with UIToolkit UI (host/join buttons, status text).
- [x] Scene transition from LobbyScene → BattleArenaScene working.
- [x] Camera repositioned for optimal battlefield view.
- [ ] Main Menu Scene created (UIToolkit).
- [ ] Game Over Scene created (UIToolkit).
