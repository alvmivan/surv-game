# Coding Standards - Survival Game

## Folder Structure (Feature-Based)

```
Assets/
├── Documentation/
│   ├── Architecture/
│   ├── Systems/
│   └── Standards/
│
├── Features/                        # Feature-based organization
│   │
│   ├── Player/                      # Namespace: SurvGame.Player
│   │   ├── Domain/
│   │   │   ├── PlayerEntity.cs
│   │   │   ├── MovementState.cs
│   │   │   └── Interfaces/
│   │   │       ├── IPlayerInput.cs
│   │   │       ├── IMovable.cs
│   │   │       └── IState.cs
│   │   │
│   │   ├── Application/
│   │   │   ├── MovePlayerUseCase.cs
│   │   │   └── ChangeViewModeUseCase.cs
│   │   │
│   │   ├── Infrastructure/
│   │   │   ├── PlayerController.cs    # MonoBehaviour facade
│   │   │   ├── PlayerMotor.cs
│   │   │   ├── PlayerCamera.cs
│   │   │   └── InputProvider.cs
│   │   │
│   │   ├── Data/
│   │   │   ├── PlayerConfig.asset
│   │   │   └── PlayerStats.asset
│   │   │
│   │   └── Presentation/
│   │       ├── PlayerHUD/
│   │       └── Crosshair/
│   │
│   ├── Inventory/                   # Namespace: SurvGame.Inventory
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Data/
│   │
│   ├── Combat/                      # Namespace: SurvGame.Combat
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Data/
│   │
│   └── Environment/                 # Namespace: SurvGame.Environment
│       ├── Domain/
│       ├── Application/
│       ├── Infrastructure/
│       └── Data/
│
├── Shared/                          # Cross-cutting concerns
│   ├── Events/
│   │   ├── IEventBus.cs
│   │   └── GameEvent.cs
│   ├── Extensions/
│   ├── Utilities/
│   └── Attributes/
│
└── Data/                            # Global ScriptableObjects
    ├── Items/
    ├── Weapons/
    └── DamageTypes/
```

## Naming Conventions

### Namespace Convention (FLAT - NO sub-namespaces!)

```csharp
// ✅ GOOD: One namespace per feature, FLAT
namespace SurvGame.Player        // ✅ Simple!
namespace SurvGame.Inventory     // ✅ Simple!
namespace SurvGame.Combat        // ✅ Simple!

// ❌ BAD: Deep namespace hierarchy
namespace SurvGame.Player.Domain.Controllers  // ❌ Too deep!
namespace SurvGame.Player.Domain.Interfaces // ❌ NO!
```

### Feature Exposure (How features communicate)

```csharp
// SurvGame.Player exposes to other features via:
namespace SurvGame.Player
{
    // Public interfaces in Domain/Interfaces
    public interface IPlayerInput { ... }
    public interface IMovable { ... }
    public interface IPlayerController { ... }
    
    // Public events
    public class PlayerMovedEvent { ... }
    public class PlayerDamagedEvent { ... }
    
    // Public facade class
    public class PlayerController : MonoBehaviour { ... }
}

// Other features IMPORT the namespace:
using SurvGame.Player;  // ✅ Simple import

// In Combat feature:
namespace SurvGame.Combat
{
    public class CombatSystem
    {
        private readonly IMovable _player;  // From SurvGame.Player
        
        public CombatSystem(IMovable player)
        {
            _player = player;
        }
    }
}
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
        public float RunSpeedMultiplier = 1.5f;
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

### Single Responsibility
```csharp
namespace SurvGame.Player
{
    // Each class has ONE job
    public class PlayerMotor : IMovable { }
    public class PlayerCamera : ICameraController { }
    public class InputProvider : IPlayerInput { }
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

## ScriptableObject Patterns

### Configuration Pattern
```csharp
namespace SurvGame.Player
{
    [CreateAssetMenu(menuName = "Player/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        [Range(1f, 10f)] public float BaseSpeed = 5f;
        [Range(1f, 20f)] public float RunSpeedMultiplier = 1.5f;
        
        [Header("Health")]
        public float MaxHealth = 100f;
        public float RegenRate = 1f;
        
        // Validation
        private void OnValidate()
        {
            BaseSpeed = Mathf.Max(0.1f, BaseSpeed);
            MaxHealth = Mathf.Max(1f, MaxHealth);
        }
    }
}
```

## Testing Patterns

### Domain Logic Tests
```csharp
namespace SurvGame.Player.Tests
{
    [TestFixture]
    public class PlayerEntityTests
    {
        private PlayerEntity _player;
        private MockPlayerStats _stats;
        private MockEventBus _eventBus;
        
        [SetUp]
        public void Setup()
        {
            _stats = new MockPlayerStats { MaxHealth = 100f };
            _eventBus = new MockEventBus();
            _player = new PlayerEntity(_stats, _eventBus);
        }
        
        [Test]
        public void TakeDamage_ReducesHealth()
        {
            // Arrange
            var damage = new DamageData { Amount = 30f, Type = DamageType.Physical };
            
            // Act
            _player.TakeDamage(damage);
            
            // Assert
            Assert.AreEqual(70f, _player.CurrentHealth);
        }
    }
}
```

## Documentation Standards

### XML Documentation
```csharp
namespace SurvGame.Player
{
    /// <summary>
    /// Applies damage to the player, considering resistances and injuries.
    /// </summary>
    /// <param name="damage">The damage data containing amount, type, and hit location.</param>
    /// <returns>True if player survived, false if health reached zero.</returns>
    /// <remarks>
    /// This method publishes a PlayerDamagedEvent that can be listened to by UI,
    /// sound systems, and visual effects.
    /// </remarks>
    public bool TakeDamage(DamageData damage)
    {
        // Implementation
    }
}
```

## Git Commit Standards

### Commit Messages
```
feat(Player): add injury system with body part tracking
fix(Player): correct FOV transition when switching view modes
refactor(Player): extract movement logic from PlayerController
docs(Architecture): update state machine design document
test(Player): add unit tests for damage calculation
```

### Branch Naming (Feature-based)
```
feature/player-core-movement
feature/player-camera-system
feature/inventory-basic
bugfix/player-camera-transition
refactor/player-extract-domain
```

## Feature Communication (Exposure Pattern)

### How Features Expose Functionality
```csharp
// ✅ GOOD: Feature exposes via public interfaces/classes in namespace
namespace SurvGame.Player
{
    // Exposed to other features
    public interface IPlayerInput { ... }
    public interface IMovable { ... }
    public class PlayerController : MonoBehaviour { ... }
    
    // Internal only (implicit)
    internal class PlayerMotor { ... }  // Not for other features
}

// ✅ GOOD: Other feature uses it
using SurvGame.Player;  // Simple import

namespace SurvGame.Combat
{
    public class CombatSystem
    {
        private readonly IMovable _player;  // From Player feature
        
        public CombatSystem(IMovable player)
        {
            _player = player;
        }
    }
}
```

### What NOT to do
```csharp
// ❌ BAD: Deep namespace hierarchy
using SurvGame.Player.Domain.Entities;    // NO!
using SurvGame.Player.Infrastructure.Controllers;  // NO!

// ❌ BAD: Feature folder called "Controllers"
Features/
├── Player/
│   ├── Controllers/      // ❌ NO!
│   ├── Models/          // ❌ NO!
│   └── Views/          // ❌ NO!

// ✅ GOOD: Feature folder is just the feature name
Features/
├── Player/              // ✅ GOOD
│   ├── Domain/
│   ├── Infrastructure/
│   └── Data/
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

**Version**: 1.2 (Feature-based architecture)  
**Last Updated**: 2026-05-04  
**Compliance**: SOLID, Clean Architecture, DDD, Feature-Based
