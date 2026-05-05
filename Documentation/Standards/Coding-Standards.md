# Coding Standards - Survival Game

## Folder Structure (Three-Layer Architecture)

Ver **[09-Three-Layer-Architecture.md](../Architecture/09-Three-Layer-Architecture.md)** para la estructura completa de carpetas, assembly definitions y reglas de dependencia.

Resumen: `FPSGame (Layer 1)` → `SurvGame (Layer 2)` → `SurvivalProject (Layer 3)`. Cada feature tiene subcarpetas `Domain/`, `Infrastructure/`, `Data/` organizacionales (el namespace NO las refleja).

## Naming Conventions

### Three-Layer Namespace Convention (FLAT!)

```csharp
// Layer 1: FPSGame (Generic FPS — base, sin dependencias)
namespace FPSGame.Player        // ✅ GOOD - Flat!
namespace FPSGame.Weapons
namespace FPSGame.Camera

// Layer 2: SurvGame (Generic Survival — is-a FPSGame)
namespace SurvGame.Inventory     // ✅ GOOD - Flat!
namespace SurvGame.Health
namespace SurvGame.Crafting

// Layer 3: SurvivalProject (Your Game — is-a SurvGame)
namespace SurvivalProject.Features   // ✅ GOOD - Flat!
namespace SurvivalProject.Missions

// ❌ BAD: Deep namespace hierarchy
namespace SurvGame.Inventory.Items.Weapons  // ❌ Too deep!
namespace FPSGame.Player.Domain.Interfaces // ❌ NO!
```

### Layer Usage Rules
```csharp
// FPSGame (Layer 1) can NOT use SurvGame or SurvivalProject
namespace FPSGame.Player
{
    // ❌ BAD: using SurvGame.Health;  // Circular dependency!
}

// SurvGame (Layer 2) CAN use FPSGame, but NOT SurvivalProject
namespace SurvGame.Health
{
    using FPSGame.Player;    // ✅ GOOD: Layer 2 uses Layer 1
    // ❌ BAD: using SurvivalProject.Features;
}

// SurvivalProject (Layer 3) can use BOTH
namespace SurvivalProject.Features
{
    using FPSGame.Player;    // ✅ GOOD: Layer 3 uses Layer 1
    using SurvGame.Health;   // ✅ GOOD: Layer 3 uses Layer 2
}
```

### Layer Exposure (How layers communicate)
```csharp
// Layer 1 (FPSGame) exposes to upper layers:
namespace FPSGame.Player
{
    public interface IMovable { ... }
    public class PlayerMotor { ... }
}

// Layer 2 (SurvGame) uses Layer 1:
namespace SurvGame.Health
{
    using FPSGame.Player;  // Layer 2 CAN use Layer 1
    
    public class HealthComponent { }
}

// Layer 3 imports both:
using FPSGame.Player;      // Layer 1
using SurvGame.Health;     // Layer 2
```

### Interfaces
```csharp
namespace SurvGame.Player
{
    // Prefix with 'I', describe capability
    public interface IPlayerInput 
    { 
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
    }
    
    public interface IDamageable 
    { 
        void TakeDamage(DamageData damage);
    }
    
    public interface IMovable 
    { 
        void Move(Vector2 direction);
    }
}
```

### Classes
```csharp
namespace SurvGame.Player
{
    // No prefix, describe what it is
    public class PlayerEntity { ... }
    public class PlayerMotor { ... }
    public abstract class BaseState { ... }
    public class RunningState : BaseState { ... }
}
```

### ScriptableObjects
```csharp
namespace SurvGame.Player
{
    [CreateAssetMenu(menuName = "SurvGame/Player Config")]
    public class PlayerConfig : ScriptableObject 
    { 
        public float WalkSpeed = 5f;
        public float RunSpeedMultiplier = 1.6f;
        
        // Valores de diseño viven en SO, no como magic numbers en código
    }
}
```

### Events
```csharp
namespace SurvGame.Player
{
    // Suffix with 'Event', past tense verb
    public class PlayerDamagedEvent 
    { 
        public DamageData Damage { get; }
        public float RemainingHealth { get; }
    }
    
    public class MovementStateChangedEvent 
    { 
        public MovementState OldState { get; }
        public MovementState NewState { get; }
    }
}
```

### Enums
```csharp
namespace SurvGame.Player
{
    public enum MovementState 
    { 
        Idle, Walking, Running, Crouching, Jumping, Falling 
    }
    
    public enum DamageType 
    { 
        Physical, Blunt, Slashing, Piercing, Environmental, Fall 
    }
}
```

## Code Style

### File Structure
```csharp
using System;
using UnityEngine;  // Unity imports
using SurvGame.Shared;  // Shared imports (flat!)

namespace SurvGame.Player  // ONE namespace, flat!
{
    /// <summary>
    /// Core player entity containing state and behavior.
    /// </summary>
    public class PlayerEntity : IDamageable, IHealable
    {
        // Dependencies (injected)
        readonly IPlayerStats _stats;
        readonly IEventBus _eventBus;

        // State
        public float CurrentHealth { get; set; }
        public MovementState CurrentMovementState { get; set; }

        // Events
        public event Action<PlayerDamagedEvent> OnDamaged;

        // Constructor
        public PlayerEntity(IPlayerStats stats, IEventBus eventBus)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            CurrentHealth = stats.MaxHealth;
        }

        // Public Methods
        public void TakeDamage(DamageData damage)
        {
            if (damage == null) throw new ArgumentNullException(nameof(damage));
            
            float reducedDamage = CalculateReducedDamage(damage);
            CurrentHealth -= reducedDamage;
            
            var eventData = new PlayerDamagedEvent(damage, CurrentHealth);
            _eventBus.Publish(eventData);
            OnDamaged?.Invoke(eventData);
        }

        // Private Methods
        float CalculateReducedDamage(DamageData damage)
        {
            const float fullResistance = 1f;
            return damage.Amount * (fullResistance - _stats.GetResistance(damage.Type));
        }
    }
}
```

### Member Order (Organizational — no C# regions needed)
Organize class members in this order, using comments to separate sections:

1. Dependencies (injected)
2. Constants/Statics
3. Serialized Fields (Unity only)
4. State/Properties
5. Events
6. Constructor/Initialize
7. Unity Methods (Awake, Start, Update - Unity only)
8. Public Methods
9. Protected Methods
10. Private Methods

### Access Modifiers
- **No usar `private`**: es implícito en C#. Solo declarar `public`, `protected`, o `internal` cuando sea necesario.
- `readonly` en campos que no cambian después del constructor.

```csharp
// ✅ GOOD: private es implícito
readonly IPlayerStats _stats;
float _currentHealth;

// ❌ BAD: redundante
private readonly IPlayerStats _stats;
private float _currentHealth;
```

### Magic Numbers
- **Usar `const` para cualquier valor que no sea 0 o 1** (o valores obvios como -1 para "no encontrado").
- Los valores de diseño viven en ScriptableObjects, no hardcodeados.

```csharp
// ✅ GOOD: const para valores con significado
const float fullResistance = 1f;
const int maxRetries = 3;
const float groundCheckDistance = 0.2f;

// ✅ OK: 0 y 1 son auto-explicativos en contexto
if (health <= 0) Die();
for (int i = 0; i < items.Count; i++) { }

// ❌ BAD: magic numbers sin explicación
if (health <= 0.2f) PlayLowHealthSound();  // ¿Qué significa 0.2?
_characterController.Move(_velocity * 0.02f);  // ¿Por qué 0.02?
```

## SOLID Implementation Examples

### Single Responsibility (Across Layers)
```csharp
// Layer 1: FPSGame (base, movement only)
namespace FPSGame.Player
{
    // Each class has ONE job
    public class PlayerMotor : IMovable { }      // Only movement
    public class PlayerCamera : ICameraController { } // Only camera
    public class InputProvider : IPlayerInput { }   // Only input
}

// Layer 2: SurvGame (health on top of FPS)
namespace SurvGame.Health
{
    public class HealthEntity { }  // Only health
}

// Layer 3: SurvivalProject (uses both)
namespace SurvivalProject.Features
{
    public class StoryMission { }  // Only story
}
```

### Open/Closed
```csharp
namespace SurvGame.Player
{
    // GOOD: Extend via new classes, don't modify old ones
    public abstract class MovementState 
    { 
        public abstract void Update(float deltaTime); 
    }
    
    public class RunningState : MovementState { }
    public class NewState : MovementState { }  // Just add class
}
```

### Liskov Substitution
```csharp
namespace SurvGame.Player
{
    // All states interchangeable
    IMovementState state = new RunningState();
    state.Update(deltaTime);  // Works with any state
    
    state = new CrouchingState();
    state.Update(deltaTime);  // Still works
}
```

### Interface Segregation
```csharp
namespace SurvGame.Player
{
    // GOOD: Small, focused interfaces
    public interface IMovable 
    { 
        void Move(Vector2 direction);
        float CurrentSpeed { get; }
    }
    
    public interface IJumpable 
    { 
        void Jump();
        bool IsGrounded { get; }
    }
}
```

### Dependency Inversion
```csharp
namespace SurvGame.Player
{
    // GOOD: Depends on abstraction
    public class PlayerMotor
    {
        readonly IPlayerInput _input;
        
        public PlayerMotor(IPlayerInput input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }
    }
}
```

## Dependency Injection

El proyecto usa [`com.torque-games.injector`](https://github.com/alvmivan/injector) para DI. Ver **[02-DI-Guidelines.md](02-DI-Guidelines.md)** para API completa, ejemplos y reglas de uso.

## ScriptableObject, Testing, Layer Communication & Performance Patterns

Ver **[03-Patterns-And-Examples.md](03-Patterns-And-Examples.md)** para ejemplos completos de:
- ScriptableObject configs por capa
- Testing patterns por capa
- XML documentation con layer references
- Layer communication (exposure pattern)
- What NOT to do (antipatterns)
- Performance guidelines (Memory, CPU, GPU)

## Git Commit Standards

### Commit Messages (With Layer Prefix)
```
feat(SurvGame-Health): add injury system with body part tracking
feat(FPSGame-Player): implement coyote time for jumping
fix(FPSGame-Camera): correct FOV transition when switching view modes
feat(SurvivalProject): add story mission system
docs(Architecture): update three-layer architecture document
test(FPSGame-Player): add unit tests for movement
```

### Branch Naming (With Layer Prefix)
```
feature/survgame-inventory-system
feature/fpsgame-player-movement
feature/fpsgame-camera-system
feature/survivalproject-story-missions
bugfix/fpsgame-player-coyote-time
```

---

**Version**: 2.2 (Extracted patterns to 03-Patterns-And-Examples.md)  
**Last Updated**: 2026-05-04  
**Compliance**: SOLID, Clean Architecture, DDD, Three-Layer
