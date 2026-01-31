# Gameplay Component Sub-tasks

## Overview

This document outlines the specific gameplay-related tasks for the 'Mask Effect' project, extracted from the main project action plan. This covers core auto-battler mechanics, mask application, and combat logic.

## Sub-tasks

- [x] Implement Mech Spawning (5-10 unmasked mechs, mirrored for both sides).
- [x] Implement Basic Movement AI (move towards target, handle blocked tiles).
- [x] Implement Basic Attack & Damage System (Evasion, Armor reduction, Shield first, Min damage 1).
- [x] Implement Health & Death System for Mechs.
- [x] Implement Win/Loss Condition (Last Mech Standing, optional time limit).
- [x] Develop a randomized AI opponent that assigns masks to its mechs for MVP battles.
- [x] Implement random mask assignment for player team in test/auto-play mode (ForceStartCombat).
- [x] Implement Basic Mask Application Logic (drag & drop, 2-3 masks per side, one per mech).
- [x] Implement Status Effects (Shield, Mark, Slow, Root, Taunt) as needed for masks.
- [x] Implement basic targeting overrides for masks (e.g., lowest HP, highest threat).
- [x] Implement remaining L1 Mask Abilities for Warrior, Rogue, Angel masks.
- [x] Implement MaskAssignmentManager (mech dragging on player-zone tiles + mask carry-and-drop onto mechs).
- [x] Implement MaskPanelUI (IMGUI left-side panel with colored mask buttons, counter, instructions).
- [x] BattleManager flow: enemy masks pre-assigned at round start, player assigns via UI, combat auto-starts when all masks placed.
- [x] Integrate Mask Selection into Player Flow (Round Setup -> Mask Assignment -> Auto Combat -> Next Round).
- [ ] Balance Mech Stats and Mask Effects for engaging combat.

## CURRENT_STATUS.md

- [x] Mech spawning implemented.
- [x] Basic movement AI implemented.
- [x] Basic attack and damage system implemented.
- [x] Health and death system implemented.
- [x] Win/loss condition implemented.
- [x] Random mask assignment for both teams in test mode implemented.
- [x] Basic mask application logic (drag & drop) implemented.
- [x] Status effects implemented.
- [x] Basic targeting overrides implemented.
- [x] Remaining L1 Mask Abilities implemented.
- [x] MaskAssignmentManager: mech dragging + mask carry-and-drop interaction.
- [x] MaskPanelUI: IMGUI left-side panel with colored mask buttons.
- [x] BattleManager: autoStartCombat toggle, PlayerAssignMask(), enemy masks pre-assigned at round start.
- [x] Mask selection integrated into player flow.
- [ ] Mech stats and mask effects balanced.
