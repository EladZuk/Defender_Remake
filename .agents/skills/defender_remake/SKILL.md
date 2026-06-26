---
name: defender-remake
description: >
  Active when working on the Defender Remake Unity project for Shenkar College.
  Provides project context, code conventions, design decisions, and Unity/GitHub
  workflow rules specific to this codebase.
---

# Defender Remake — Project Skill

## Project Identity
- **Game**: Defender (1981) reimagined — 2D rescue phase → 3D aerial combat phase
- **Engine**: Unity (URP), WebGL build target
- **Repo**: https://github.com/EladZuk/Defender_Remake
- **Developer**: Elad Zukin (solo)
- **Course**: Retro Reimagined — Shenkar College Unity Game Dev

## The One-Sentence Twist
> "The mechanical survivors rescued in the 2D phase orbit your ship as finite homing missiles in the 3D phase — the only weapon that can breach enemy shields."

## Art Direction
- Low-poly, flat-shaded geometry — NO textures on geometry
- URP Lit/Unlit materials with Emission only
- Accent color: neon cyan `#00FFE0`
- All VFX: 2.5D billboarded sprites (always face camera)
- Bloom via URP Volume system (not legacy Post-Processing package)
- 2D phase BG: 2–3 parallax layers projected on inside of a sphere
- 3D phase BG: muted dark low-poly terrain/asteroid field

## Phase Structure
| Phase | Perspective | Duration |
|-------|-------------|----------|
| Phase 1 | 2D side-scroll | ~2 min |
| Transition | Cinematic | ~10 sec |
| Phase 2 | 3D aerial arena | ~3–4 min |

## Core Systems & Design Decisions

### Laser (Both Phases)
- Visible straight projectile sprite — dumb weapon, no homing
- Overheat system: all values `[SerializeField]`
- Target: 5–8 sec to break one shield via laser (2 heat cycles)
- Shield HP (laser): 8 hits default
- Heat per shot: 20/100, fire rate: 3/sec, lockout: 1.5s, passive cooldown: 35/sec

### Drone Missiles (3D Phase Only)
- Spawned from survivorCount rescued in Phase 1
- Orbit ship as glowing 2.5D billboarded sprites
- Target lock: raycast from camera center, 15° cone → square bracket UI on target
- On fire: detach from orbit → homing via `Slerp` + `MoveTowards` → visible trail
- Impact: instant shield break (always 1-hit)
- Always-visible crosshair on screen center

### Boost (Both Phases)
- No pickup required — always available
- Boost meter: drains while active, cooldown before refill begins
- All values `[SerializeField]`: boostSpeed, boostDrain, cooldownDelay, refillRate
- UI: boost bar (same visual language as heat bar)

### Enemy AI (3D Phase)
- Chaser (easy): Slerp toward player, ram damage minor
- Flanker (hard): different angle, catches player without boost, moderate damage, fewer spawns
- NO projectile AI — enemies are physical threats only
- Both types have energy shields (laser: 8 hits, drone: 1 hit)

### Phase Transition
- Unbeatable boss kills player → `GameStateManager` loads Transition scene
- `survivorCount` passed via ScriptableObject or static
- Retro terminal text: "SURVIVOR NETWORK: X CONNECTED"
- Drones spawned in Phase 2 based on survivorCount

## Code Conventions
- Language: C# (Unity)
- All tunable values: `[SerializeField]` — never hardcode balance values
- Group Inspector fields with `[Header("Section Name")]`
- Prefer `Coroutine` for timed state changes (overheat lockout, shield window)
- Enemy tags: `"Enemy"`, `"Boss"`, `"Bot"` — use tags for collision detection
- No `FindObjectOfType` in Update — cache references in Awake/Start
- Scenes: `Phase1_2D`, `Transition`, `Phase2_3D`
- Scripts folder structure:
  ```
  Scripts/
    Player/     — PlayerController2D, PlayerController3D, DroneManager, BoostSystem, WeaponSystem
    Enemies/    — EnemyChaser, EnemyFlanker, BossController2D, EnemyShield, EnemySpawner
    Bots/       — BotPickup2D, DroneOrbit
    Systems/    — GameStateManager, MissileHoming, BillboardSprite, TargetLock
    UI/         — HeatBarUI, BoostBarUI, DroneCountUI, CrosshairUI, TransitionScreen
  ```

## GitHub Workflow
- 15+ issues already created and live on the board
- Branch per feature: `feature/issue-7-flight-controller`, etc.
- Merge to `main` only after self-review (solo dev — Review column = self-review pass)
- Commit messages: `[#7] Add Rigidbody flight controller base`
- Cut column: move issues there with one-sentence reason if descoped

## WebGL Rules
- URP shaders only (no Built-in Standard shader)
- Test WebGL build early and often — do NOT wait until demo day
- Strip unused shaders in Player Settings
- Canvas size set in Player Settings → Resolution and Presentation
- Always test in Chrome + Firefox before demo

## Asset Pipeline
- All assets: CC0, CC-BY, or equivalent license only
- Log every asset immediately in CREDITS.md (name, author, URL, license)
- Models/sprites used as-is must be visually transformed before use
- Emission color on all interactive objects must match accent color `#00FFE0`

## Critical Reminders
- Attribution failure = automatic project fail — update CREDITS.md constantly
- WebGL build must run in browser on demo day — test early
- Gameplay duration target: 5–6 minutes (2D ~2min + transition + 3D ~3–4min)
- The twist must be nameable in one sentence and demonstrable live during demo
