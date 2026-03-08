<h1 align="center">🛗 Elevator Simulator</h1>

<p align="center">
  <b>A Unity 2D simulation of a multi-elevator dispatch system with SCAN algorithm, door animation, and player carry mechanic.</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Engine-Unity%206-blue?logo=unity" alt="Engine"/>
  <img src="https://img.shields.io/badge/Language-C%23-239120?logo=csharp" alt="Language"/>
  <img src="https://img.shields.io/badge/Platform-Windows-lightgrey" alt="Platform"/>
  <img src="https://img.shields.io/badge/Status-In%20Development-yellow" alt="Status"/>
</p>

---

## 📖 Description

- **Genre:** 2D Simulation / Puzzle
- **Theme:** Modern building / Urban
- **Gameplay:** Player navigates a multi-floor building using 3 elevators. Press hall call buttons on each floor to summon the nearest elevator, then use car call buttons inside the elevator to select a destination floor.
- **Target Audience:** Assignment / Academic

---

## 🛠️ Tech Stack / Tools

| Category        | Tool / Technology          |
| --------------- | -------------------------- |
| Game Engine     | Unity 6 (2D URP)           |
| Language        | C#                         |
| IDE / Editor    | Visual Studio Code         |
| Art Tool        | Aseprite                   |
| UI Framework    | Unity UI (Canvas + TextMeshPro) |
| Input System    | Unity Input System (PlayerInputActions) |
| Version Control | Git                        |

---


---

## 🏗️ Scene Hierarchy (Per Elevator)

```
Elevator_A                  ← ElevatorController, ElevatorDoor
  ├── DoorSprite            ← Animator, SpriteRenderer
  ├── InteriorZone          ← ElevatorInteriorZone, Collider2D (Is Trigger ✓)
  └── Canvas (World Space)
        ├── CarCallPanel    ← assigned to ElevatorDoor._carCallPanel
        │     ├── Button_G  ← ElevatorCarCallButton (floorIndex = 0)
        │     ├── Button_1  ← ElevatorCarCallButton (floorIndex = 1)
        │     ├── Button_2  ← ElevatorCarCallButton (floorIndex = 2)
        │     └── Button_3  ← ElevatorCarCallButton (floorIndex = 3)
        └── DirectionText   ← ElevatorDisplay
```

---

## 🔘 Floor Call Setup (Per Floor)

```
Floor_G_UI
  └── CallButton   ← FloorCallButton (floorIndex = 0)

Floor_1_UI
  └── CallButton   ← FloorCallButton (floorIndex = 1)
```

---

## 🧍 Player Requirements

- Tag: `Player`
- `Rigidbody2D` — Dynamic, freeze Z rotation
- `PlayerController` — from `Platformer.Controller` namespace
- Collider must overlap `InteriorZone` trigger to be detected

### Assets — By Type with Grouped Utilities

```
📦 project-root/
├── 📂 Assets/
│   ├── 📂 _ThirdParty/             # External libraries (DOTween, TextMeshPro, etc.)
│   │   └── 📂 Plugins/             #   Asset Store plugins, SDKs, native plugins
│   ├── 📂 _Shared/                 # Cross-feature shared assets
│   │   ├── 📂 Fonts/               #   Font files (.ttf, .otf)
│   │   └── 📂 Shaders/             #   Custom shaders & shader graphs
│   ├── 📂 Animations/              # Animation clips & controllers
│   ├── 📂 Art/
│   │   ├── 📂 Materials/           #   Materials & shaders
│   │   ├── 📂 Models/              #   3D models (.fbx, .obj)
│   │   ├── 📂 Sprites/             #   2D sprites & sprite sheets
│   │   ├── 📂 Textures/            #   Textures, UI graphics, icons
│   │   └── 📂 VFX/                 #   Particle systems, visual effects
│   ├── 📂 Audio/
│   │   ├── 📂 Music/               #   Background music tracks
│   │   ├── 📂 SFX/                 #   Sound effects
│   │   └── 📂 Ambience/            #   Ambient/environment sounds
│   ├── 📂 Prefabs/                 # Reusable game objects
│   │   ├── 📂 Characters/          #   Player, enemies, NPCs
│   │   ├── 📂 Environment/         #   Props, obstacles, platforms
│   │   ├── 📂 Projectiles/         #   Bullets, arrows, spells
│   │   ├── 📂 UI/                  #   UI prefabs (panels, popups)
│   │   └── 📂 VFX/                 #   Particle prefabs (hit, explosion)
│   ├── 📂 Scenes/                  # All game scenes
│   │   ├── 📂 Levels/              #   Gameplay levels
│   │   ├── 📂 UI/                  #   Menu scenes (MainMenu, Loading)
│   │   └── 📂 Test/                #   Sandbox / test scenes (exclude from build)
│   ├── 📂 Scripts/                 # ← See Script Structure below
│   ├── 📂 Resources/               # Assets loaded via Resources.Load() (use sparingly!)
│   │   └── 📂 Data/                #   ScriptableObjects, configs, DataAssets
│   └── 📂 StreamingAssets/         # Files copied as-is to build (JSON, CSV, video)
├── 📂 Docs/                        # Design documents, GDD
├── 📂 Packages/                    # Unity Package Manager overrides
├── 📄 .gitignore
├── 📄 LICENSE
└── 📄 README.md
```

---
## 📏 Coding Standards

### Naming Conventions

| Type           | Convention        | Example                     |
| -------------- | ----------------- | --------------------------- |
| Class          | PascalCase        | `PlayerController`          |
| Method         | PascalCase        | `TakeDamage()`              |
| Variable       | camelCase         | `moveSpeed`                 |
| Private Field  | _camelCase        | `_currentHealth`            |
| Constant       | UPPER_SNAKE_CASE  | `MAX_HEALTH`                |
| Enum           | PascalCase        | `GameState.Playing`         |
| Interface      | I + PascalCase    | `IDamageable`               |
| ScriptableObj  | SO_ + PascalCase  | `SO_WeaponData`             |

### Code Structure (per script)

```csharp
// 1. Using statements
// 2. Namespace (MyGame.FeatureName)
// 3. Class declaration
//    3a. [SerializeField] private fields     ← Inspector-exposed
//    3b. Private fields                      ← Internal state
//    3c. Events (Action, UnityEvent)
//    3d. Properties
//    3e. Unity lifecycle (Awake → OnEnable → Start → Update → LateUpdate → OnDisable)
//    3f. Public methods
//    3g. Private methods
//    3h. Editor-only (#if UNITY_EDITOR)
```

## 🌿 Branching & Workflow

### Branch Strategy

```
main ────────────────────────────────────── (stable release)
  │
  ├── develop ───────────────────────────── (integration branch)
  │     │
  │     ├── feature/player-movement ─────── (new feature)
  │     ├── feature/enemy-ai ────────────── (new feature)
  │     ├── fix/camera-bug ──────────────── (bug fix)
  │     └── art/new-tileset ─────────────── (art update)
  │
  └── release/v0.2.0 ───────────────────── (release candidate)
```

### Branch Naming

| Type      | Format                     | Example                      |
| --------- | -------------------------- | ---------------------------- |
| Feature   | `feature/description`      | `feature/inventory-system`   |
| Bug Fix   | `fix/description`          | `fix/player-fall-through`    |
| Art       | `art/description`          | `art/new-enemy-sprites`      |
| Audio     | `audio/description`        | `audio/boss-theme`           |
| Refactor  | `refactor/description`     | `refactor/input-system`      |
| Docs      | `docs/description`         | `docs/update-readme`         |


### Commit Convention
```
<type>: <short description>
```

| Prefix Type | Usage                                                |
| ----------- | ---------------------------------------------------- |
| `Add:`      | New feature or content                               |
| `Fix:`      | Bug fix                                              |
| `Update:`   | Improvement or refactor                              |
| `Remove:`   | Removed feature or file                              |
| `Docs:`     | Documentation changes                                |
| `Art:`      | Art/asset changes                                    |
| `Audio:`    | Audio-related changes                                |
| `Refactor:` | Code restructuring without new features or bug fixes |
| `Test:`     | Adding or modifying tests                            |

**Examples:**
```
Add: basic enemy patrol behavior
Fix: player clipping through walls on slopes
Art: new idle animation for main character (8 frames)
Refactor: extract health system into reusable component
```


## 🚀 Getting Started

### Setup

```bash
# 1. Clone the repository
git clone https://github.com/username/elevator-simulator.git

# 2. Open with Unity Hub → Add project from disk
# 3. Open Scenes/Gameplay.unity
# 4. Press Play
```

### First Time Scene Setup

1. Create `ElevatorData` ScriptableObject → assign to all `ElevatorController`
2. Set `_groundFloorWorldY` on each `ElevatorController` to match Ground floor Y in scene
3. Add all 3 `ElevatorController` references to `ElevatorManager._elevators`
4. Attach `ElevatorInteriorZone` to each `InteriorZone` child (ensure Collider2D Is Trigger ✓)
5. Assign `CarCallPanel` to `ElevatorDoor._carCallPanel` on each elevator

---

## 📜 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

<p align="center">
  Made with ❤️ and ☕ — Elevator Simulator Assignment
</p>
