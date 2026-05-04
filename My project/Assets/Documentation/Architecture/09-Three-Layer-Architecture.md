# Three-Layer Architecture: SurvGame → FPSGame → {GameName}

## Overview
Arquitectura en 3 capas donde cada nivel agrega funcionalidad específica.

## Layer Hierarchy

```
Layer 3: {GameName}           (ej: "SurvivalProject")
    ↓ depends on
Layer 2: FPSGame               (generic FPS systems)
    ↓ depends on  
Layer 1: SurvGame              (generic survival systems)
```

### Dependency Flow
```
SurvivalProject (tu juego específico)
    ↓ uses
FPSGame (movimiento FPS, cámara, combate)
    ↓ uses
SurvGame (inventario, salud, craft, survival)
```

## Folder Structure

```
Assets/
├── SurvGame/                      # Layer 1: Generic Survival Systems
│   ├── SurvGame.asmdef           # Assembly definition
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
├── FPSGame/                      # Layer 2: Generic FPS Systems
│   ├── FPSGame.asmdef            # Assembly definition
│   │   └── References: SurvGame.asmdef
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
└── SurvivalProject/              # Layer 3: Your Specific Game
    ├── SurvivalProject.asmdef     # Assembly definition
    │   └── References: FPSGame.asmdef, SurvGame.asmdef
    │
    ├── Features/                # namespace: SurvivalProject.Features
    │   ├── StoryMissions/
    │   ├── SpecificEnemies/
    │   └── UniqueMechanics/
    │
    └── Configuration/           # Game-specific configs
```

## Assembly Definition Files

### SurvGame.asmdef (Layer 1 - Base)
```json
{
    "name": "SurvGame",
    "references": [],  // No dependencies
    "includePlatforms": ["Editor", "Standalone"]
}
```

### FPSGame.asmdef (Layer 2 - Depends on SurvGame)
```json
{
    "name": "FPSGame",
    "references": [
        "SurvGame"  // ← Depends on Layer 1
    ],
    "includePlatforms": ["Editor", "Standalone"]
}
```

### SurvivalProject.asmdef (Layer 3 - Depends on both)
```json
{
    "name": "SurvivalProject",
    "references": [
        "FPSGame",      // ← Depends on Layer 2
        "SurvGame"       // ← Also uses Layer 1 directly
    ],
    "includePlatforms": ["Editor", "Standalone"]
}
```

## Namespace Convention (FLAT!)

### Layer 1: SurvGame
```csharp
namespace SurvGame.Inventory     // ✅ FLAT
{
    public class InventorySystem { }
    public interface IItemContainer { }
}

namespace SurvGame.Health
{
    public class HealthSystem { }
    public interface IDamageable { }
}
```

### Layer 2: FPSGame
```csharp
namespace FPSGame.Player        // ✅ FLAT
{
    using SurvGame.Health;  // Can use Layer 1
    
    public class PlayerMotor { }
    public interface IMovable { }
}

namespace FPSGame.Weapons
{
    using SurvGame.Inventory;  // Can use Layer 1
    
    public class WeaponSystem { }
}
```

### Layer 3: SurvivalProject (Your Game)
```csharp
namespace SurvivalProject.Features  // ✅ FLAT
{
    using FPSGame.Player;    // Can use Layer 2
    using SurvGame.Health;   // Can use Layer 1
    
    public class StoryMissionSystem { }
}
```

## What Goes Where?

### SurvGame (Layer 1 - Generic Survival)
**Survival mechanics that ANY survival game needs:**
- ✅ Inventory system
- ✅ Health/Stamina system
- ✅ Crafting system
- ✅ Hunger/Thirst system
- ✅ Item data structures
- ✅ Damage types (generic)

**NO FPS-specific stuff:**
- ❌ First-person camera
- ❌ Mouse look
- ❌ Shooting mechanics
- ❌ Movement acceleration curves

---

### FPSGame (Layer 2 - Generic FPS)
**First-person game mechanics that ANY FPS game needs:**
- ✅ First-person player controller
- ✅ FPS camera system (view modes)
- ✅ Shooting/aiming mechanics
- ✅ Weapon base classes
- ✅ Crosshair system
- ✅ WASD movement

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

### Layer 1: SurvGame.Health
```csharp
// SurvGame/Health/Domain/HealthEntity.cs
namespace SurvGame.Health
{
    public class HealthEntity
    {
        public float CurrentHealth { get; private set; }
        
        public void TakeDamage(float amount)
        {
            CurrentHealth -= amount;
        }
    }
}
```

### Layer 2: FPSGame.Player (uses SurvGame)
```csharp
// FPSGame/Player/Domain/PlayerEntity.cs
namespace FPSGame.Player
{
    using SurvGame.Health;  // ← Using Layer 1
    
    public class PlayerEntity : HealthEntity  // ← Inherits from Layer 1
    {
        public Vector3 Position { get; set; }
        
        // FPS-specific: WASD movement
        public void Move(Vector2 input) { ... }
    }
}
```

### Layer 3: SurvivalProject (uses both)
```csharp
// SurvivalProject/Features/SurvivalPlayer.cs
namespace SurvivalProject.Features
{
    using FPSGame.Player;   // ← Using Layer 2
    using SurvGame.Health;  // ← Can also use Layer 1
    
    public class SurvivalPlayer : FPSGame.Player.PlayerEntity
    {
        // Game-specific: Add story progress
        public int StoryProgress { get; set; }
        
        // Override with game-specific logic
        public void TakeDamage(float amount)
        {
            base.TakeDamage(amount);
            // Add game-specific: play hurt sound, show blood FX
        }
    }
}
```

## Feature Communication Between Layers

### Layer 2 uses Layer 1
```csharp
// FPSGame.Player needs SurvGame.Inventory
namespace FPSGame.Player
{
    using SurvGame.Inventory;
    
    public class PlayerInteraction
    {
        private readonly IItemContainer _inventory;  // From Layer 1
        
        public void PickupItem(ItemData item)
        {
            _inventory.AddItem(item);  // Layer 1 functionality
        }
    }
}
```

### Layer 3 uses both Layer 1 and 2
```csharp
namespace SurvivalProject.Features
{
    using FPSGame.Player;
    using SurvGame.Crafting;  // Layer 1
    
    public class SurvivalGameplay
    {
        private readonly IMovable _player;         // From Layer 2
        private readonly ICraftingSystem _crafting; // From Layer 1
        
        public void Update()
        {
            _player.Move(...);  // FPS movement
            _crafting.Craft(...);  // Survival crafting
        }
    }
}
```

## Benefits of This Structure

### 1. Reusability
- **SurvGame**: Can be used for ANY survival game (2D, 3D, RTS, etc.)
- **FPSGame**: Can be used for ANY FPS game (not just survival)
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
// FPSGame (Layer 2) references SurvGame (Layer 1) ✅
using SurvGame.Inventory;

// SurvivalProject (Layer 3) references FPSGame (Layer 2) ✅
using FPSGame.Player;

// SurvivalProject (Layer 3) references SurvGame (Layer 1) ✅
using SurvGame.Health;
```

### ❌ BAD: Lower layers reference upper layers (Circular Dependency!)
```csharp
// SurvGame (Layer 1) tries to use FPSGame (Layer 2) ❌
// This would create a circular dependency!
using FPSGame.Player;  // ❌ NO!
```

## Assembly Definition Rules

### Rule 1: No Circular Dependencies
```
SurvGame ← FPSGame ← SurvivalProject  ✅
SurvGame → FPSGame                  ❌ (circular!)
```

### Rule 2: Lower Can't See Upper
```
SurvGame can't see FPSGame or SurvivalProject  ✅
FPSGame can see SurvGame, can't see SurvivalProject  ✅
SurvivalProject can see both  ✅
```

### Rule 3: Shared Code Goes to Lowest Possible Layer
```
Is it generic survival? → SurvGame
Is it generic FPS? → FPSGame
Is it game-specific? → SurvivalProject
```

---

**Next**: See [08-Phase1-Development-Plan.md](08-Phase1-Development-Plan.md)  
**Related**: See [Coding-Standards.md](../Standards/Coding-Standards.md) for naming conventions

---

**Last Updated**: 2026-05-04  
**Status**: Architecture v2.0 (Three-Layer)
