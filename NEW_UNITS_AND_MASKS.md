# Mask Effect — New Units & Masks Expansion

This document extends the MVP with **2 new chassis** and **2 new masks**.

---

## New Mech Chassis

### Sniper (Glass Cannon)

- **Profile:** Immobile while firing, extreme range, high damage, very fragile
- **Role:** Backline DPS, punishes poor positioning, rewards protection
- **Best at:** Eliminating priority targets from safety, forcing enemies to close distance

| Stat | Value | Notes |
|------|-------|-------|
| Max HP | 60 | Lowest in game |
| Armor | 5 | Minimal protection |
| Attack Damage | 45 | Highest single-hit damage |
| Attack Interval | 3.0s | Slow but devastating |
| Range | 5 tiles | Longest range in game |
| Move Speed | 0.8 tiles/s | Below average |
| Evasion | 0% | No innate evasion |

**Baseline Behavior (Unmasked):**
- **AI/Targeting:** Furthest enemy in range; won't move while target exists in range
- **Special:** Must remain stationary for 1s "charge-up" before firing (interrupted if hit during charge)

**Spawn Placement:** Always backline (Row 0 for P1, Row 7 for P2)

---

### Colossus (Overwhelming Giant)

- **Profile:** Extremely slow, can attack 2 targets simultaneously, highest HP
- **Role:** Slow-moving steamroller, area denial, inevitable threat
- **Best at:** Absorbing damage, multi-target pressure, late-fight dominance

| Stat | Value | Notes |
|------|-------|-------|
| Max HP | 250 | Highest in game |
| Armor | 30 | Heavy protection |
| Attack Damage | 25 | Per target (50 total if hitting 2) |
| Attack Interval | 2.5s | Moderate |
| Range | 1 tile | Melee only |
| Move Speed | 0.25 tiles/s | Slowest in game |
| Evasion | 0% | Cannot evade |

**Baseline Behavior (Unmasked):**
- **AI/Targeting:** Two nearest enemies simultaneously
- **Special:** Attacks cleave to a second target within range (both take full damage)

**Spawn Placement:** Frontline center preferred (Row 2 Col 2 for P1)

---

## Mask Overview & Colors

| Mask | Color | Hex Code | Role |
|------|-------|----------|------|
| **Warrior** | Red | `#E63946` | Aggressive damage + pressure |
| **Rogue** | Green | `#2A9D8F` | Assassin, executes weak targets |
| **Angel** | Gold | `#F4A261` | Support, shields, healing |
| **Phantom** | Violet | `#9B5DE5` | Evasion, repositioning, hard to pin down |
| **Commander** | Light Blue | `#00B4D8` | Team buffs, auras, empowers allies |

---

## Complete Mask Combinations

### Warrior Mask (Bruiser) — 🔴 Red

**Role:** Aggressive damage + pressure

| Chassis | AI/Targeting | Ability (L1) |
|---------|--------------|--------------|
| **Scout + Warrior** | Nearest enemy; aggressive pressure | *Hit & Run* — Every 3rd attack grants +40% Attack Speed for 2s; after each attack, step back 1 tile if free |
| **Jet + Warrior** | Highest-threat backline (e.g., highest DPS) | *Dive Slash* — Every 6s: Dive the target, deal burst damage, return to original tile |
| **Tank + Warrior** | Nearest enemy; wants frontline | *Challenge* — On battle start: Taunt nearby enemies for 3s; then every 5s on hit, gain a shield (10% Max HP) |
| **Sniper + Warrior** | Highest HP enemy | *Armor Piercing Round* — Every 3rd shot ignores 50% of target's Armor; gain +10% damage for each tile of distance to target |
| **Colossus + Warrior** | Two nearest enemies | *Overwhelming Force* — Every 4th attack: Stun both targets for 1s; attacks deal +15% damage to stunned targets |

---

### Rogue Mask (Assassin) — 🟢 Green

**Role:** Picks off weak targets, executes, high priority damage

| Chassis | AI/Targeting | Ability (L1) |
|---------|--------------|--------------|
| **Scout + Rogue** | Lowest HP; backline priority | *Execute Chain* — On kill: Dash to the next target; next hit deals +25% damage |
| **Jet + Rogue** | Lowest HP backline; prioritizes marked targets | *Mark* — First hit marks target for 6s; marked targets take +15% damage from your team; Jet prioritizes marks |
| **Tank + Rogue** | Farthest support/backline (if reachable) | *Grapple* — Every 7s: Pull the farthest enemy into frontline and Root for 1s |
| **Sniper + Rogue** | Lowest HP enemy in range | *Kill Shot* — Targets below 30% HP take +75% damage; on kill, next shot has no charge-up time |
| **Colossus + Rogue** | Two lowest HP enemies | *Crushing Grip* — Every 6s: Grab one target, making it untargetable by allies but taking 50% more damage from Colossus for 2s |

---

### Angel Mask (Support) — 🟡 Gold

**Role:** Shields, mitigation, stabilization, healing

| Chassis | AI/Targeting | Ability (L1) |
|---------|--------------|--------------|
| **Scout + Angel** | Protect lowest-HP ally; stays mobile | *Guardian Leap* — Every 6s: Leap to lowest-HP ally; grant a shield (20% Max HP) for 3s |
| **Jet + Angel** | Support from anywhere; positioning-independent | *Sky Barrier* — Every 6s: Shield 2 allies for 3s (optional: cleanse 1 debuff if implemented) |
| **Tank + Angel** | Hold frontline; protect nearby allies | *Sanctuary* — Adjacent allies take -10% damage; every 8s emit a small heal pulse |
| **Sniper + Angel** | Lowest HP ally | *Overwatch Protocol* — Every 5s: Shield an ally for 15% of Sniper's Attack Damage; if shielded ally is attacked, Sniper instantly fires at the attacker (no charge-up) |
| **Colossus + Angel** | Two nearest allies (defensive stance) | *Living Fortress* — Passive: Allies directly behind Colossus (same column, higher row) take -25% damage; every 10s, Colossus grants himself and adjacent allies a shield (10% Max HP) |

---

### Phantom Mask (Evasion / Hit-and-Fade) — 🟣 Violet

**Role:** Hard to pin down, punishes attackers, repositioning specialist

| Chassis | AI/Targeting | Ability (L1) |
|---------|--------------|--------------|
| **Scout + Phantom** | Isolated enemies (no adjacent allies) | *Phase Dash* — After each attack: 40% chance to become untargetable for 1s |
| **Jet + Phantom** | Random backline enemy | *Cloak* — Every 8s: Become invisible for 2s; next attack deals +30% damage |
| **Tank + Phantom** | Nearest enemy | *Mirage* — Every 10s: Create an illusion with 1 HP that draws aggro for 3s |
| **Sniper + Phantom** | Furthest enemy | *Revenge Blink* — When hit: Teleport to a tile at maximum range from any enemy; next shot deals +50% damage. Cooldown: 10s |
| **Colossus + Phantom** | Two nearest enemies | *Displacement Field* — Passive: Attackers have 25% miss chance against Colossus |

---

### Commander Mask (Aura / Team Buff) — 🔵 Light Blue

**Role:** Empowers nearby allies, provides strategic buffs, team-oriented play

| Chassis | AI/Targeting | Ability (L1) |
|---------|--------------|--------------|
| **Scout + Commander** | Protect highest-DPS ally | *Rally Cry* — Every 6s: Allies within 2 tiles gain +15% Move Speed for 3s |
| **Jet + Commander** | Support from above; positioning-independent | *Air Superiority* — Passive: Allies in the same column as Jet take -10% damage |
| **Tank + Commander** | Hold frontline; nearest enemy | *Iron Will* — Passive: Adjacent allies are immune to Slow and Root |
| **Sniper + Commander** | Furthest enemy | *Spotter* — Every 5s: Mark a target for 4s; all allies have +20% damage against marked target |
| **Colossus + Commander** | Two nearest enemies | *Titan's Presence* — Passive: Allies within 3 tiles regenerate 2% Max HP per second |

---

## Complete Mech + Mask Matrix (5×5)

|  | **Warrior** 🔴 | **Rogue** 🟢 | **Angel** 🟡 | **Phantom** 🟣 | **Commander** 🔵 |
|---|---|---|---|---|---|
| **Scout** | Hit & Run | Execute Chain | Guardian Leap | Phase Dash | Rally Cry |
| **Jet** | Dive Slash | Mark | Sky Barrier | Cloak | Air Superiority |
| **Tank** | Challenge | Grapple | Sanctuary | Mirage | Iron Will |
| **Sniper** | Armor Piercing Round | Kill Shot | Overwatch Protocol | Revenge Blink | Spotter |
| **Colossus** | Overwhelming Force | Crushing Grip | Living Fortress | Displacement Field | Titan's Presence |

**Total Combinations:** 25 unique Chassis + Mask pairings

---

## Suggested Base Stats Comparison

| Chassis | HP | Armor | Damage | Interval | Range | Speed | Role |
|---------|-----|-------|--------|----------|-------|-------|------|
| Scout | 80 | 10 | 15 | 1.0s | 1 | 1.5 | Mobile Skirmisher |
| Jet | 70 | 5 | 20 | 1.5s | 3 | 1.2 | Ranged Harasser |
| Tank | 150 | 25 | 20 | 2.0s | 1 | 0.5 | Frontline Anchor |
| **Sniper** | 60 | 5 | 45 | 3.0s | 5 | 0.8 | Glass Cannon |
| **Colossus** | 250 | 30 | 25×2 | 2.5s | 1 | 0.25 | Overwhelming Giant |

---

## Visual Design Notes

### Mask Color Application
- **Upper body / Head / Shoulders** of the mech should be tinted with the mask color
- **Lower body / Legs / Base** remains team color (Blue for player, Orange for enemy)
- **Glowing ring** beneath the mech matches mask color
- **Particle effects** for abilities should use the mask's color

### Color Palette Reference
```
Warrior:    #E63946 (Crimson Red)
Rogue:      #2A9D8F (Emerald Green)  
Angel:      #F4A261 (Warm Gold)
Phantom:    #9B5DE5 (Violet Purple)
Commander:  #00B4D8 (Cyan / Light Blue)

Team Blue:  #4361EE (Player team base)
Team Orange:#FF6B35 (Enemy team base)
```

---

## Implementation Checklist

### New Chassis
- [ ] Sniper prefab + stats
- [ ] Sniper charge-up mechanic (1s before firing)
- [ ] Sniper charge interrupt on hit
- [ ] Colossus prefab + stats
- [ ] Colossus dual-target attack logic
- [ ] Spawn placement rules for new chassis

### Warrior Mask (Sniper + Colossus)
- [ ] Armor Piercing Round: 50% armor ignore + distance bonus
- [ ] Overwhelming Force: Stun on 4th attack + bonus damage

### Rogue Mask (Sniper + Colossus)
- [ ] Kill Shot: Execute threshold + instant follow-up
- [ ] Crushing Grip: Grab mechanic + damage amplification

### Angel Mask (Sniper + Colossus)
- [ ] Overwatch Protocol: Shield ally + reactive shot
- [ ] Living Fortress: Positional damage reduction + AoE shield

### Phantom Mask
- [ ] Phase Dash: 40% untargetable proc
- [ ] Cloak: Invisibility + damage bonus
- [ ] Mirage: Illusion spawn + aggro redirect
- [ ] Revenge Blink: Teleport on hit + damage buff + cooldown tracking
- [ ] Displacement Field: Miss chance calculation

### Commander Mask
- [ ] Rally Cry: AoE speed buff
- [ ] Air Superiority: Column-based damage reduction
- [ ] Iron Will: CC immunity aura
- [ ] Spotter: Mark application + team damage bonus
- [ ] Titan's Presence: AoE regeneration tick

### Visual Implementation
- [ ] Mask color materials (5 colors)
- [ ] Team color materials (2 colors)
- [ ] Mask color ring VFX prefabs
- [ ] Ability particle effects per mask color

---

## Edge Cases

| Situation | Resolution |
|-----------|------------|
| Revenge Blink: No valid tile at max range | Teleport to furthest available empty tile |
| Revenge Blink: Sniper hit during cooldown | No teleport, no damage bonus |
| Cloak: Jet attacked while invisible | Attack breaks invisibility after damage applies |
| Mirage: Illusion killed | Disappears, no death effects |
| Spotter Mark + Rogue Mark on same target | Both apply: +20% (Spotter) + 15% (Rogue) = +35% total |
| Titan's Presence + Sanctuary overlap | Both apply: Regen + Damage Reduction stack |
| Colossus: Only 1 enemy in range | Attacks that single enemy (no cleave) |
| Dual attack: One target dies mid-attack | Second attack still hits if target valid |
| Overwatch Protocol: Sniper currently charging | Reactive shot cancels charge, fires at attacker instead |
| Kill Shot: Target healed above 30% mid-attack | Normal damage applies (threshold checked at attack start) |
| Crushing Grip: Target dies during grip | Grip ends, Colossus can select new targets |
| Living Fortress: No allies behind Colossus | Shield still triggers for adjacent allies |
| Armor Piercing Round: Target has 0 armor | Distance bonus still applies |
