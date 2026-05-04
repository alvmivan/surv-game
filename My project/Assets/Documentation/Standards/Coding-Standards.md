# Coding Standards - Survival Game

## Folder Structure (Three-Layer Architecture)

```
Assets/
├── Documentation/
│   ├── Architecture/
│   ├── Systems/
│   └── Standards/
│
├── SurvGame/                        # Layer 1: Generic Survival Systems
│   ├── SurvGame.asmdef              # No dependencies
│   │
│   ├── Inventory/                   # namespace: SurvGame.Inventory
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Data/
│   │
│   ├── Health/                      # namespace: SurvGame.Health
│   ├── Crafting/                    # namespace: SurvGame.Crafting
│   └── Shared/                      # namespace: SurvGame.Shared
│
├── FPSGame/                         # Layer 2: Generic FPS Systems
│   ├── FPSGame.asmdef               # Depends on: SurvGame.asmdef
│   │
│   ├── Player/                      # namespace: FPSGame.Player
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Data/
│   │
│   ├── Weapons/                     # namespace: FPSGame.Weapons
│   ├── Camera/                      # namespace: FPSGame.Camera
│   └── Shared/                      # namespace: FPSGame.Shared
│
└── SurvivalProject/                 # Layer 3: Your Specific Game
    ├── SurvivalProject.asmdef        # Depends on: FPSGame.asmdef, SurvGame.asmdef
    │
    ├── Features/                    # namespace: SurvivalProject.Features
    ├── Missions/
    └── Configuration/
```

### Layer Dependencies
```
SurvivalProject (Layer 3)
    ↓ depends on
FPSGame (Layer 2) ← Generic FPS systems
    ↓ depends on  
SurvGame (Layer 1) ← Generic survival systems
```

## Naming Conventions

### Three-Layer Namespace Convention (FLAT!)

```csharp
// Layer 1: SurvGame (Generic Survival)
namespace SurvGame.Inventory     // ✅ GOOD - Flat!
namespace SurvGame.Health
namespace SurvGame.Crafting

// Layer 2: FPSGame (Generic FPS)
namespace FPSGame.Player        // ✅ GOOD - Flat!
namespace FPSGame.Weapons
namespace FPSGame.Camera

// Layer 3: SurvivalProject (Your Game)
namespace SurvivalProject.Features   // ✅ GOOD - Flat!
namespace SurvivalProject.Missions

// ❌ BAD: Deep namespace hierarchy
namespace SurvGame.Inventory.Items.Weapons  // ❌ Too deep!
namespace FPSGame.Player.Domain.Interfaces // ❌ NO!
```

### Layer Usage Rules
```csharp
// SurvGame can NOT use FPSGame or SurvivalProject
namespace SurvGame.Inventory
{
    // ❌ BAD: using FPSGame.Player;  // Circular dependency!
}

// FPSGame CAN use SurvGame, but NOT SurvivalProject
namespace FPSGame.Player
{
    using SurvGame.Health;  // ✅ GOOD
    // ❌ BAD: using SurvivalProject.Features;
}

// SurvivalProject can use BOTH
namespace SurvivalProject.Features
{
    using FPSGame.Player;    // ✅ GOOD
    using SurvGame.Health;   // ✅ GOOD
}
```

### Layer Exposure (How layers communicate)
```csharp
// Layer 1 exposes to upper layers:
namespace SurvGame.Inventory
{
    public interface IItemContainer { ... }
    public class InventorySystem { ... }
    public class ItemAddedEvent { ... }
}

// Layer 2 uses Layer 1, exposes to Layer 3:
namespace FPSGame.Player
{
    using SurvGame.Inventory;  // Can use Layer 1
    
    public interface IPlayerInput { ... }
    public interface IMovable { ... }
    public class PlayerController : MonoBehaviour { ... }
}

// Layer 3 imports both:
using SurvGame.Inventory;  // Layer 1
using FPSGame.Player;      // Layer 2
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
    [CreateAssetMenu(menuName = "Player/Player Config")]
    public class PlayerConfig : ScriptableObject 
    { 
        public float WalkSpeed = 5f;
        public float RunSpeedMultiplier = 1.6f;
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
        #region Dependencies (injected)
        private readonly IPlayerStats _stats;
        private readonly IEventBus _eventBus;
        #endregion

        #region State
        public float CurrentHealth { get; private set; }
        public MovementState CurrentMovementState { get; private set; }
        #endregion

        #region Events
        public event Action<PlayerDamagedEvent> OnDamaged;
        #endregion

        #region Constructor
        public PlayerEntity(IPlayerStats stats, IEventBus eventBus)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            CurrentHealth = stats.MaxHealth;
        }
        #endregion

        #region Public Methods
        public void TakeDamage(DamageData damage)
        {
            if (damage == null) throw new ArgumentNullException(nameof(damage));
            
            float reducedDamage = CalculateReducedDamage(damage);
            CurrentHealth -= reducedDamage;
            
            var eventData = new PlayerDamagedEvent(damage, CurrentHealth);
            _eventBus.Publish(eventData);
            OnDamaged?.Invoke(eventData);
        }
        #endregion

        #region Private Methods
        private float CalculateReducedDamage(DamageData damage)
        {
            return damage.Amount * (1 - _stats.GetResistance(damage.Type));
        }
        #endregion
    }
}
```

### Region Order
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

## SOLID Implementation Examples

### Single Responsibility (Across Layers)
```csharp
// Layer 1: SurvGame
namespace SurvGame.Health
{
    public class HealthEntity { }  // Only health
}

// Layer 2: FPSGame (uses Layer 1)
namespace FPSGame.Player
{
    // Each class has ONE job
    public class PlayerMotor : IMovable { }      // Only movement
    public class PlayerCamera : ICameraController { } // Only camera
    public class InputProvider : IPlayerInput { }   // Only input
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
        private readonly IPlayerInput _input;
        
        public PlayerMotor(IPlayerInput input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }
    }
}
```

## Dependency Injection

El proyecto usa [`com.torque-games.injector`](https://github.com/alvmivan/injector) para DI. Ver **[02-DI-Guidelines.md](02-DI-Guidelines.md)** para API completa, ejemplos y reglas de uso.

## ScriptableObject Patterns

### Layer 1: SurvGame Configuration
```csharp
namespace SurvGame.Health
{
    [CreateAssetMenu(menuName = "Survival/Health Config")]
    public class HealthConfig : ScriptableObject
    {
        public float MaxHealth = 100f;
        public float RegenRate = 1f;
    }
}
```

### Layer 2: FPSGame Configuration (uses Layer 1)
```csharp
namespace FPSGame.Player
{
    using SurvGame.Health;
    
    [CreateAssetMenu(menuName = "FPS/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float BaseSpeed = 5f;
        public float RunSpeedMultiplier = 1.6f;
        
        [Header("References")]
        public HealthConfig HealthConfig;  // From Layer 1
    }
}
```

## Testing Patterns

### Testing Layer 1 (SurvGame - No Dependencies)
```csharp
namespace SurvGame.Health.Tests
{
    [TestFixture]
    public class HealthEntityTests
    {
        private HealthEntity _health;
        
        [SetUp]
        public void Setup()
        {
            _health = new HealthEntity(100f);
        }
        
        [Test]
        public void TakeDamage_ReducesHealth()
        {
            _health.TakeDamage(30f);
            Assert.AreEqual(70f, _health.CurrentHealth);
        }
    }
}
```

### Testing Layer 2 (FPSGame - Depends on Layer 1)
```csharp
namespace FPSGame.Player.Tests
{
    using SurvGame.Health;
    
    [TestFixture]
    public class PlayerEntityTests
    {
        private PlayerEntity _player;
        
        [SetUp]
        public void Setup()
        {
            _player = new PlayerEntity();
        }
        
        [Test]
        public void Move_ChangesPosition()
        {
            _player.Move(new Vector2(1, 0));
            Assert.Greater(_player.Position.z, 0);
        }
    }
}
```

## Documentation Standards

### XML Documentation (With Layer References)
```csharp
namespace FPSGame.Player
{
    using SurvGame.Health;
    
    /// <summary>
    /// Core player entity for FPS games.
    /// Uses <see cref="HealthEntity"/> (Layer 1) via composition.
    /// </summary>
    public class PlayerEntity
    {
        /// <summary>Health component from Layer 1.</summary>
        public HealthEntity Health { get; }

        /// <summary>
        /// Moves the player in the specified direction.
        /// </summary>
        /// <param name="direction">Movement direction (X: left/right, Y: forward/back).</param>
        public void Move(Vector2 direction)
        {
            // Implementation
        }
    }
}
```

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

## Layer Communication (Exposure Pattern)

### How Layers Expose Functionality
```csharp
// Layer 1: SurvGame exposes to upper layers
namespace SurvGame.Health
{
    public interface IDamageable { ... }
    public class HealthEntity { ... }
}

// Layer 2: FPSGame uses Layer 1, exposes to Layer 3
namespace FPSGame.Player
{
    using SurvGame.Health;
    
    public interface IMovable { ... }
    public class PlayerEntity { ... }  // Uses HealthEntity via composition
}

// Layer 3: Your game uses both
namespace SurvivalProject.Features
{
    using FPSGame.Player;
    using SurvGame.Health;
    
    public class GameManager
    {
        private readonly IMovable _player;
        
        public GameManager(IMovable player)
        {
            _player = player;  // From Layer 2
        }
    }
}
```

### What NOT to do
```csharp
// ❌ BAD: Layer 1 trying to use Layer 2 (circular dependency!)
namespace SurvGame.Health
{
    using FPSGame.Player;  // ❌ NO! Can't reference upper layer!
}

// ❌ BAD: Deep namespace hierarchy
namespace FPSGame.Player.Domain.Entities { }
namespace SurvGame.Inventory.Items.Weapons { }

// ✅ GOOD: Flat namespaces
namespace FPSGame.Player { }
namespace SurvGame.Inventory { }
```

## Performance Guidelines

### Memory
- Use object pooling for frequently created objects
- Cache component references in Awake/Start
- Use `ScriptableObject` references instead of copying data
- Null-check events before invoking

### CPU
- Avoid GetComponent in Update
- Use coroutines or async for expensive operations
- State machine only updates active state
- Cache LayerMask values

### GPU
- Use LOD for player models in third-person view
- Optimize camera effects based on view mode
- Batch similar materials

---

**Version**: 2.1 (Three-Layer Architecture — cleaned duplicates)  
**Last Updated**: 2026-05-04  
**Compliance**: SOLID, Clean Architecture, DDD, Three-Layer
