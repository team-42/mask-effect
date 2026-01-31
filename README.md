# Mask Effect

A mask-driven auto-battler prototype (Global Game Jam 2026).  
Both teams start with the same **unmasked** mechs — strategy comes from **assigning a few masks each round** to reshape roles, targeting, and abilities.

---

## Core Idea

- There are **3 mech chassis**: **Scout**, **Jet**, **Tank**.
- At the start of each round, the game **randomizes the mech lineup**: **5–10 unmasked mechs** are spawned and **mirrored** to both sides for a fair round.
- Each round, the player can assign only **2–3 masks** (one mask per mech).
- A **mask defines a role** (behavior + targeting + one signature ability).
- **Same mechs, different fights** — the “Mask Effect” is how roles emerge from mask assignments.

---

## Board / Tiles (MVP)

- Total board: **40 tiles**
  - **15 tiles per player side** (30 total)
  - **10 neutral tiles in the middle**: **2 lanes × 5 tiles** (a buffer zone between teams)
- Mechs spawn on their side’s tiles; the opposing side is a **mirrored** copy of the same unmasked lineup.

> Exact coordinate system is implementation-defined; the important part is symmetry + limited placement space.

---

## Gameplay Loop (MVP)

1. **Round Setup (Randomized + Mirrored)**
   - Generate **5–10 unmasked mechs** and place them on one side.
   - Mirror the same lineup/positions to the other side.
2. **Mask Assignment**
   - Each player places **2–3 masks** onto their mechs (**one mask per mech**).
3. **Auto Combat**
   - The fight runs automatically using basic AI + mask abilities.
4. **Next Round**
   - Repeat with a new randomized mirrored mech setup.

---

## Mech Chassis

### Scout (Small + Fast)

- High mobility / skirmisher profile
- Best at: repositioning, chasing weak targets, “hit-and-run” patterns

### Jet (Flying)

- Ranged pressure / backline harassment
- Best at: ignoring frontline blocking rules (target selection), hitting backline safely

### Tank (Big + Slow)

- Frontline anchor / space control
- Best at: soaking damage, holding the line, setting up control effects (via masks)

> Note: For MVP, chassis should stay simple. The identity comes primarily from masks.

---

## Masks (MVP Set)

### Warrior Mask (Bruiser)

Role: aggressive damage + pressure

- **Scout Mech + Warrior Mask**
  - **AI/Targeting:** nearest enemy; aggressive pressure  
  - **Ability (L1):** *Hit & Run* — every 3rd attack grants **+40% Attack Speed** for 2s; after each attack, **step back 1 tile** if free
- **Jet Mech + Warrior Mask**
  - **AI/Targeting:** highest-threat backline (e.g., highest DPS)  
  - **Ability (L1):** *Dive Slash* — every 6s, dive the target, deal burst damage, return to original tile
- **Tank Mech + Warrior Mask**
  - **AI/Targeting:** nearest enemy; wants frontline  
  - **Ability (L1):** *Challenge* — on battle start, taunt nearby enemies for 3s; then every 5s on hit, gain a small shield (10% Max HP)

### Rogue Mask (Assassin)

Role: picks off weak targets / executes  
*(Named “Rogue” to avoid confusion with the Scout mech chassis.)*

- **Scout Mech + Rogue Mask**
  - **AI/Targeting:** lowest HP; backline priority  
  - **Ability (L1):** *Execute Chain* — on kill, dash to the next target; next hit deals **+25% damage**
- **Jet Mech + Rogue Mask**
  - **AI/Targeting:** lowest HP backline; prioritizes marked targets  
  - **Ability (L1):** *Mark* — first hit marks target for 6s; marked targets take **+15% damage** from your team; Jet prioritizes marks
- **Tank Mech + Rogue Mask**
  - **AI/Targeting:** farthest support/backline (if reachable)  
  - **Ability (L1):** *Grapple* — every 7s, pull the farthest enemy into frontline and **Root** for 1s

### Angel Mask (Support)

Role: shields / mitigation / stabilization

- **Scout Mech + Angel Mask**
  - **AI/Targeting:** protect lowest-HP ally; stays mobile  
  - **Ability (L1):** *Guardian Leap* — every 6s, leap to lowest-HP ally; grant a shield (20% Max HP) for 3s
- **Jet Mech + Angel Mask**
  - **AI/Targeting:** support from anywhere; positioning-independent  
  - **Ability (L1):** *Sky Barrier* — every 6s, shield 2 allies for 3s (optional: cleanse 1 debuff if implemented)
- **Tank Mech + Angel Mask**
  - **AI/Targeting:** hold frontline; protect nearby allies  
  - **Ability (L1):** *Sanctuary* — adjacent allies take **-10% damage**; every 8s emit a small heal pulse

---

## Basic Combat Rules (MVP)

### Stats (core)

- **Max HP**, **Armor**, **Attack Damage**
- **Attack Interval** (seconds between basic attacks)
- **Range** (tiles)
- **Move Speed** (tiles/sec)
- **Evasion** (chance to dodge basic attacks)
- **Shield** (temporary HP layer)

### Hit & Damage

- Basic attacks roll against **Evasion**
  - On dodge: no damage (and for MVP: no on-hit effects)
- Armor reduces damage using a simple diminishing rule, e.g.:
  - `damageAfterArmor = floor(rawDamage * 100 / (100 + armor))`
- Damage applies to **Shield first**, then HP  
- Minimum damage on hit: **1**

### Targeting (baseline)

- Default: **nearest enemy**
- Tie-breakers: lowest HP, then stable deterministic ID
- Some masks override targeting (e.g., lowest HP, highest threat, backline priority)

### Movement

- If target is out of range: move toward it
- Tiles are occupied; if blocked, try adjacent alternatives; otherwise wait

### Status Effects (needed for current masks)

- **Shield**: absorbs damage before HP
- **Mark**: target takes increased damage (+15%)
- **Slow**: increases attack interval / reduces attack speed
- **Root**: cannot move
- **Taunt**: forced targeting toward the taunter

### Round End

- Round ends when one team has **no living mechs**  
- Optional: time limit (e.g., 45s) → remaining team HP decides

---

## Controls / UI (placeholder)

- Drag & drop masks onto mechs (**2–3 masks per side per round**, one mask per mech)
- Clear visual procs: shield icons, mark icons, grapple animation, taunt indicator

---

## Scope Notes (Jam Reality)

- Keep **numbers small** and **rules visible** (clarity > complexity).
- MVP has **only Level 1 masks** (no leveling, no item stacks).
- Add features only after unmasked combat feels solid and deterministic.

---

## Credits

Built for Global Game Jam 2026 by the team behind **Mask Effect**.

---

## License

MIT
