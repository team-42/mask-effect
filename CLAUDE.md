# Mask Effect AI Agent Action Plan

## Project Overview

Mask Effect is an auto-battler where Mechs wear Masks to specify tactical roles. The core gameplay loop involves players assigning a few masks each round to reshape mech roles, targeting, and abilities. The MVP focuses on a local test mode with random AI opponents, 1 level, 3 mechs, and 3 masks, with a final score display.

## Consensus-Driven Plan Adjustments

Based on the consensus from multiple AI models, the original work breakdown has been revised to prioritize feasibility within a 48-hour game jam. The primary adjustments involve significant scope reduction, particularly cutting online multiplayer and simplifying progression systems, to focus on a polished core local AI auto-battler experience. Emphasis is placed on clear mask effects and robust player feedback.

## Detailed Step-by-Step Action Plan (MVP Focus)

### Phase 1: Core Auto-Battler Mechanics (First 12-18 Hours)

- [ ] **Technical Setup & Project Structure:**
  - [ ] Verify Unity Project & Version Control (Git) setup.
  - [ ] Integrate essential Unity Packages (e.g., TextMeshPro).
  - [ ] Ensure AI Tool Access (Claude Code/Sixth AI) is configured.
  - [x] Finalize `Assets/` directory structure as planned.
- [ ] **Basic Auto-Battler Core:**
  - [ ] Define Mech Stats & Basic Combat Rules (Max HP, Armor, Attack Damage, Attack Interval, Range, Move Speed, Evasion, Shield).
  - [ ] Implement Mech Spawning (5-10 unmasked mechs, mirrored for both sides).
  - [ ] Implement Basic Movement AI (move towards target, handle blocked tiles).
  - [ ] Implement Basic Attack & Damage System (Evasion, Armor reduction, Shield first, Min damage 1).
  - [ ] Implement Health & Death System for Mechs.
  - [ ] Implement Win/Loss Condition (Last Mech Standing, optional time limit).
- [ ] **Network Mode & Randomized AI Opponent:**
  - [ ] Implement a network mode where players can play against each other (can be independent of core game logic).
  - [ ] Develop a randomized AI opponent that assigns masks to its mechs for MVP battles.
- [ ] **Battle Arena Environment:**
  - [ ] Create Simple Battle Arena Scene (40 tiles: 15 per player, 10 neutral middle).
  - [ ] Add Basic Lighting & Camera Setup.
- [ ] **Core Mask System (Initial Implementation):**
  - [ ] Define Mask Data Structure (Scriptable Object: Name, Effect Type, Ability details).
  - [ ] Implement Basic Mask Application Logic (drag & drop, 2-3 masks per side, one per mech).
  - [ ] Create 3 Placeholder Masks & Effects (Warrior, Rogue, Angel - simplified L1 abilities).
  - [ ] Implement Status Effects (Shield, Mark, Slow, Root, Taunt) as needed for masks.
  - [ ] Implement basic targeting overrides for masks (e.g., lowest HP, highest threat).
- [ ] **Essential UI & Feedback:**
  - [ ] Design & Implement Basic HUD (Health bars, timer, mask assignment UI).
  - [ ] Implement Clear Visual Procs for mask effects (shield icons, mark icons, grapple animation, taunt indicator).
  - [ ] Implement Simple Main Menu & Game Over UI.

### Phase 2: Refinement & Polish (Next 12-18 Hours)

- [ ] **Gameplay Refinement:**
  - [ ] Implement remaining L1 Mask Abilities for Warrior, Rogue, Angel masks.
  - [ ] Balance Mech Stats and Mask Effects for engaging combat.
  - [ ] Integrate Mask Selection into Player Flow (Round Setup -> Mask Assignment -> Auto Combat -> Next Round).
- [ ] **Visual & Audio Polish (MVP Level):**
  - [ ] Create Placeholder Mech Models/Sprites (3 chassis variants).
  - [ ] Create Placeholder Mask Icons (3 icons).
  - [ ] Add Basic Sound Effects (Attacks, UI, Win/Loss).
  - [ ] Integrate Background Music.
- [ ] **AI Tool Integration (Ongoing):**
  - [ ] Utilize AI for small utility scripts.
  - [ ] Utilize AI for placeholder textures/materials.

### Phase 3: Demo & Submission (Final 12 Hours)

- [ ] **Demo Build & Testing:**
  - [ ] Perform Integration Test of Core Systems.
  - [ ] Identify & Fix Critical Bugs.
  - [ ] Create Standalone Build for Presentation.
- [ ] **Presentation Content:**
  - [ ] Prepare Short Demo Script & Talking Points.
  - [ ] Prepare Readme/Submission Documentation.
  - [ ] Upload Game & Submit Project.

## CURRENT_STATUS.md

- [x] Initial project setup and directory structure.
- [ ] Core auto-battler mechanics implemented (mech spawning, movement, basic combat).
- [ ] Basic mask system with 3 masks and simplified L1 abilities.
- [ ] Essential UI and visual feedback for gameplay.
- [ ] Game loop (round setup, mask assignment, combat, next round) functional.
- [ ] Basic art and audio placeholders integrated.
- [ ] Demo build and presentation prepared.

## Critical Risks & Mitigation

- **Scope Creep:** Strictly adhere to the MVP. Any feature not directly contributing to the core 'Mask Effect' auto-battler with local AI should be cut.
- **Time Management:** Implement rough time estimates for all tasks. Use a 'cut list' for features to drop if behind schedule.
- **Complexity of Mask Effects:** Start with simple, passive stat boosts before attempting complex battlefield manipulations. Ensure a flexible Scriptable Object architecture for masks.
- **UI/UX:** Prioritize clear and functional UI over visual polish initially. Ensure visual/audio feedback for mask effects is prominent.
