# UI Component Sub-tasks

## Overview

This document outlines the specific UI asset and prefab-related tasks for the 'Mask Effect' project, extracted from the main project action plan. This includes designing and creating the visual elements for the HUD, mask effects, and scene-specific UI.

## Sub-tasks

- [x] Design and implement MaskPanelUI (IMGUI left-side panel with colored mask buttons, counter, instructions).
- [x] Implement LobbyUIController with UIToolkit (host/join buttons, status text) for LobbyScene.
- [x] Implement MaskPanelUI Update-based polling for reliable cross-scene initialization.
- [ ] Design and create prefabs for Basic HUD (Health bars, timer).
- [ ] Design and create visual elements/prefabs for Clear Visual Procs for mask effects (shield icons, mark icons, grapple animation, taunt indicator).
- [ ] Design and create prefabs for Simple Main Menu UI.
- [ ] Design and create prefabs for Game Over UI.

## CURRENT_STATUS.md

- [x] MaskPanelUI implemented (IMGUI, left-side panel, colored mask buttons).
- [x] LobbyUIController implemented (UIToolkit, host/join flow).
- [x] MaskPanelUI robust across scene transitions (Update polling + event subscription).
- [ ] Basic HUD prefabs created (Health bars, timer).
- [ ] Mask effect visual proc prefabs created (shield icons, mark icons, grapple animation, taunt indicator).
- [ ] Main Menu UI prefabs created.
- [ ] Game Over UI prefabs created.
- [x] Implemented "Exit Game" button in LobbyUIController.
