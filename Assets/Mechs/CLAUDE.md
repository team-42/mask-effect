# Mechs Component Sub-tasks

## Overview

This document outlines the specific mech-related tasks for the 'Mask Effect' project, extracted from the main project action plan. This includes mech assets, spawning, movement, and health management.

## Sub-tasks

- [x] Create Placeholder Mech Models/Sprites (3 chassis variants: Scout, Jet, Tank).
- [x] Implement Mech Spawning logic for 5-10 unmasked mechs, mirrored for both sides.
- [x] Implement Basic Movement AI for mechs (move towards target, handle blocked tiles).
- [x] Implement Health & Death System for mechs.
- [x] Integrate Mech Stats (defined in Data) into mech prefabs/scripts.
- [x] Implement two-tone mech visuals (bottom half=team color, top half=mask color via EquipMask).
- [x] Add BoxCollider to root mech GameObject for raycast-based interaction (drag & mask assignment).
- [x] Set mech layer to "Mech" for layer-filtered raycasting.
- [x] Implement advanced pathfinding for Mechs using A* algorithm.
- [x] Integrate collision boxes and pathing logic for Mechs.
- [x] Implement damage types and resistances for Mechs and Masks.
- [x] Implement ranged combat for Jet mechs using projectiles.

## CURRENT_STATUS.md

- [x] Placeholder mech models/sprites created.
- [x] 3D models replace primitive shapes for all 3 chassis (Scout, Jet, Tank).
- [x] All mech types saved as prefabs with NetworkIdentity for Mirror.
- [x] Mech spawning logic implemented (prefab-based instantiation).
- [x] Basic mech movement AI implemented.
- [x] Advanced A* pathfinding implemented.
- [x] Mech health and death system implemented.
- [x] Mech stats integrated.
- [x] Two-tone mech visuals working (bottom=team color, top=mask tint color).
- [x] Mech root colliders and "Mech" layer added for click/drag interaction.
- [x] Damage types and resistances implemented.
- [x] Ranged combat for Jet mechs using projectile prefabs.
