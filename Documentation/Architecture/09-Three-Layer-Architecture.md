# Three-Layer Architecture: FPSGame → SurvGame → {GameName}

## Overview
Arquitectura en 3 capas donde cada nivel agrega funcionalidad específica.

## Layer Hierarchy

```
Layer 3: {GameName}           (ej: "SurvivalProject") — es un SurvGame
    ↓ depends on
Layer 2: SurvGame              (generic survival systems) — es un FPSGame
    ↓ depends on  
Layer 1: FPSGame               (generic FPS systems) — base, sin dependencias
```

### Dependency Flow ("is-a" relationship)
```
SurvivalProject (tu juego específico)
    ↓ is a
SurvGame (inventario, salud, craft, survival)
    ↓ is a
FPSGame (movimiento FPS, cámara, combate)
```

## Folder Structure

```
Assets/
├── FPSGame/                      # Layer 1: Generic FPS Systems (BASE)
│   ├── FPSGame.asmdef            # Assembly definition (no references)
│   │
│   ├── Player/                  # namespace: FPSGame.Player
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Data/
│   │
│   ├── Weapons/                 # namespace: FPSGame.Weapons
│   ├── Camera/                  # namespace: FPSGame.Camera
│   └── Shared/                  # namespace: FPSGame.Shared
│
├── SurvGame/                      # Layer 2: Generic Survival (is-a FPSGame)
│   ├── SurvGame.asmdef           # Assembly definition
│   │   └── References: FPSGame.asmdef
│   │
│   ├── Inventory/               # namespace: SurvGame.Inventory
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Data/
│   │
│   ├── Health/                   # namespace: SurvGame.Health
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Data/
│   │
│   ├── Crafting/                # namespace: SurvGame.Crafting
│   └── Shared/                  # namespace: SurvGame.Shared
│
└── SurvivalProject/              # Layer 3: Your Specific Game
    ├── SurvivalProject.asmdef     # Assembly definition
    │   └── References: SurvGame.asmdef, FPSGame.asmdef
    │
    ├── Features/                # namespace: SurvivalProject.Features
    │   ├── StoryMissions/
    │   ├── SpecificEnemies/
    │   └── UniqueMechanics/
    │
    └── Configuration/           # Game-specific configs
```

## Assembly Definition Files

### FPSGame.asmdef (Layer 1 - Base)
```json
{
    "name": "FPSGame",
    "references": [],  // No dependencies — this is the base
    "includePlatforms": ["Editor", "Standalone"]
}
```

### SurvGame.asmdef (Layer 2 - Depends on FPSGame)
```json
{
    "name": "SurvGame",
    "references": [
        "FPSGame"  // ← A survival game IS an FPS game
    ],
    "includePlatforms": ["Editor", "Standalone"]
}
```

### SurvivalProject.asmdef (Layer 3 - Depends on both)
```json
{
    "name": "SurvivalProject",
    "references": [
        "SurvGame",      // ← Depends on Layer 2
        "FPSGame"        // ← Also uses Layer 1 directly
    ],
    "includePlatforms": ["Editor", "Standalone"]
}
```

## Namespace Convention (FLAT!)

### Layer 1: FPSGame
```csharp
namespace FPSGame.Player        // ✅ FLAT — no references to other layers
{
    public class PlayerMotor { }
    public interface IMovable { }
}

namespace FPSGame.Weapons
{
    public class WeaponSystem { }
}
```

### Layer 2: SurvGame
```csharp
namespace SurvGame.Health       // ✅ FLAT
{
    using FPSGame.Player;  // Can use Layer 1
    
    public class HealthSystem { }
    public interface IDamageable { }
}

namespace SurvGame.Inventory
{
    public class InventorySystem { }
    public interface IItemContainer { }
}
```

### Layer 3: SurvivalProject (Your Game)
```csharp
namespace SurvivalProject.Features  // ✅ FLAT
{
    using FPSGame.Player;    // Can use Layer 2
    using SurvGame.Health;   // Can use Layer 2
    
    public class StoryMissionSystem { }
}
```

## What Goes Where?

### FPSGame (Layer 1 - Generic FPS, BASE)
**First-person game mechanics that ANY FPS game needs:**
- ✅ First-person player controller
- ✅ FPS camera system (view modes)
- ✅ Shooting/aiming mechanics
- ✅ Weapon base classes
- ✅ Crosshair system
- ✅ WASD movement

**NO survival or game-specific stuff:**
- ❌ Inventory, crafting, hunger
- ❌ Specific story missions
- ❌ Game-specific UI

---

### SurvGame (Layer 2 - Generic Survival, is-a FPSGame)
**Survival mechanics that ANY survival FPS needs (on top of FPS base):**
- ✅ Inventory system
- ✅ Health/Stamina system
- ✅ Crafting system
- ✅ Hunger/Thirst system
- ✅ Item data structures
- ✅ Damage types (generic)

**NO game-specific stuff:**
- ❌ Specific story missions
- ❌ Unique enemies
- ❌ Game-specific UI

---

### SurvivalProject (Layer 3 - Your Game)
**Your specific game implementation:**
- ✅ Story missions
- ✅ Specific enemy types
- ✅ Unique game mechanics
- ✅ Game-specific configurations
- ✅ Scene configurations
- ✅ Game-specific UI theming

## Example: Player Entity Across Layers

### Layer 1: FPSGame.Player (base — no external dependencies)
```csharp
// FPSGame/Player/Domain/PlayerEntity.cs
namespace FPSGame.Player
{
    // Base FPS player — movement only, no survival mechanics
    public class PlayerEntity
    {
        public Vector3 Position { get; set; }
        public float CurrentSpeed { get; private set; }

        // FPS-specific: WASD movement
        public void Move(Vector2 input) { ... }
    }
}
```

### Layer 2: SurvGame.Health (uses FPSGame via composition)
```csharp
// SurvGame/Health/Domain/HealthEntity.cs
namespace SurvGame.Health
{
    // Standalone health system — usable by any entity via composition
    public class HealthEntity
    {
        public float MaxHealth { get; }
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        public HealthEntity(float maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        }
    }
}

// SurvGame/Player/Domain/SurvivalPlayerEntity.cs
namespace SurvGame.Player
{
    using FPSGame.Player;   // ← Using Layer 1
    using SurvGame.Health;
    
    // Composition: SurvivalPlayer HAS a PlayerEntity + HealthEntity
    public class SurvivalPlayerEntity
    {
        public PlayerEntity Movement { get; }   // ← From Layer 1
        public HealthEntity Health { get; }
        
        public SurvivalPlayerEntity(float maxHealth)
        {
            Movement = new PlayerEntity();
            Health = new HealthEntity(maxHealth);
        }
    }
}
```

### Layer 3: SurvivalProject (uses both)
```csharp
// SurvivalProject/Features/GamePlayer.cs
namespace SurvivalProject.Features
{
    using SurvGame.Player;   // ← Using Layer 2
    using SurvGame.Health;   // ← Also Layer 2
    
    // Game-specific player with story progress
    public class GamePlayer
    {
        public SurvivalPlayerEntity Player { get; }
        public int StoryProgress { get; set; }

        public GamePlayer(SurvivalPlayerEntity player)
        {
            Player = player;
        }

        // Game-specific: add effects on damage
        public void TakeDamage(float amount)
        {
            Player.Health.TakeDamage(amount);
            // Play hurt sound, show blood FX, etc.
        }
    }
}
```

> **Nota de diseño**: Se usa composición en vez de herencia para conectar capas.
> Esto sigue el principio "favor composition over inheritance" que es best practice
> en game dev con Unity (ref: https://gamedev.stackexchange.com/questions/160604).

## Feature Communication Between Layers

### Layer 2 uses Layer 1
```csharp
// SurvGame needs FPSGame.Player
namespace SurvGame.Inventory
{
    using FPSGame.Player;
    
    public class PlayerInventory
    {
        private readonly IMovable _player;  // From Layer 1
        
        public void DropItem(ItemData item)
        {
            // Drop at player position (Layer 1 functionality)
        }
    }
}
```

### Layer 3 uses both Layer 1 and 2
```csharp
namespace SurvivalProject.Features
{
    using FPSGame.Player;     // Layer 1
    using SurvGame.Crafting;  // Layer 2
    
    public class SurvivalGameplay
    {
        private readonly IMovable _player;         // From Layer 1
        private readonly ICraftingSystem _crafting; // From Layer 2
        
        public void Update()
        {
            _player.Move(...);  // FPS movement (Layer 1)
            _crafting.Craft(...);  // Survival crafting (Layer 2)
        }
    }
}
```

## Benefits of This Structure

### 1. Reusability
- **FPSGame**: Can be used for ANY FPS game (not just survival)
- **SurvGame**: Can be used for ANY survival FPS (not just this game)
- **SurvivalProject**: Your specific game

### 2. Testability
```csharp
// Test SurvGame.Health without FPS stuff
[Test]
public void HealthEntity_TakeDamage_ReducesHealth() { ... }

// Test FPSGame.Player without game-specific stuff
[Test]
public void PlayerMotor_Move_ChangesPosition() { ... }
```

### 3. Team Scalability
- Team A works on **SurvGame** (survival mechanics)
- Team B works on **FPSGame** (FPS mechanics)
- Team C works on **SurvivalProject** (game content)

## Referencing Between Layers

### ✅ GOOD: Upper layers reference lower layers
```csharp
// SurvGame (Layer 2) references FPSGame (Layer 1) ✅
using FPSGame.Player;

// SurvivalProject (Layer 3) references SurvGame (Layer 2) ✅
using SurvGame.Health;

// SurvivalProject (Layer 3) references FPSGame (Layer 1) ✅
using FPSGame.Player;
```

### ❌ BAD: Lower layers reference upper layers (Circular Dependency!)
```csharp
// FPSGame (Layer 1) tries to use SurvGame (Layer 2) ❌
// This would create a circular dependency!
using SurvGame.Health;  // ❌ NO!
```

## Assembly Definition Rules

### Rule 1: No Circular Dependencies
```
FPSGame ← SurvGame ← SurvivalProject  ✅
FPSGame → SurvGame                  ❌ (circular!)
```

### Rule 2: Lower Can't See Upper
```
FPSGame can't see SurvGame or SurvivalProject  ✅
SurvGame can see FPSGame, can't see SurvivalProject  ✅
SurvivalProject can see both  ✅
```

### Rule 3: Shared Code Goes to Lowest Possible Layer
```
Is it generic FPS? → FPSGame (Layer 1)
Is it generic survival? → SurvGame (Layer 2)
Is it game-specific? → SurvivalProject (Layer 3)
```

---

**Next**: See [08-Phase1-Development-Plan.md](08-Phase1-Development-Plan.md)  
**Related**: See [Coding-Standards.md](../Standards/Coding-Standards.md) for naming conventions

---

**Last Updated**: 2026-05-04  
**Status**: Architecture v2.1 (Three-Layer — composition over inheritance, HealthEntity init fix)
