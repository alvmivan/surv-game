# Coding Standards - Survival Game

## Folder Structure (Clean Architecture + Unity)

```
Assets/
├── Documentation/
│   ├── Architecture/
│   ├── Systems/
│   └── Decisions/
│
├── Domain/                          # Core business logic (no Unity dependencies)
│   ├── Entities/
│   │   ├── Player/
│   │   │   ├── PlayerEntity.cs
│   │   │   ├── PlayerStats.cs
│   │   │   └── MovementState.cs
│   │   ├── Health/
│   │   ├── Inventory/
│   │   └── Environment/
│   │
│   ├── Interfaces/
│   │   ├── IPlayerInput.cs
│   │   ├── IMovable.cs
│   │   ├── IDamageable.cs
│   │   └── IState.cs
│   │
│   ├── ValueObjects/
│   │   ├── DamageData.cs
│   │   ├── InjuryData.cs
│   │   └── MovementData.cs
│   │
│   └── Events/
│       ├── IDomainEvent.cs
│       └── EventBus.cs
│
├── Application/                      # Use cases (orchestration)
│   ├── UseCases/
│   │   ├── MovePlayerUseCase.cs
│   │   ├── AttackUseCase.cs
│   │   └── ChangeViewModeUseCase.cs
│   │
│   └── DTOs/
│       ├── PlayerInputDTO.cs
│       └── DamageResultDTO.cs
│
├── Infrastructure/                   # Unity-specific implementations
│   ├── Player/
│   │   ├── PlayerController.cs       # MonoBehaviour facade
│   │   ├── PlayerMotor.cs
│   │   ├── PlayerCamera.cs
│   │   └── PlayerInputHandler.cs
│   │
│   ├── StateMachine/
│   │   ├── StateMachine.cs
│   │   ├── BaseState.cs
│   │   └── States/
│   │
│   ├── Camera/
│   │   ├── ViewModes/
│   │   └── CameraEffects/
│   │
│   └── Interaction/
│       ├── TreeCutter.cs
│       └── CombatSystem.cs
│
├── Data/                            # ScriptableObjects
│   ├── Players/
│   │   ├── PlayerConfig.asset
│   │   └── PlayerStats.asset
│   │
│   ├── Items/
│   ├── Environments/
│   ├── Weapons/
│   └── DamageTypes/
│
├── Presentation/                     # UI and Visuals
│   ├── UI/
│   │   ├── PlayerHUD/
│   │   └── Crosshair/
│   │
│   └── Views/
│       ├── FirstPersonView.cs
│       └── ThirdPersonView.cs
│
└── Shared/                          # Cross-cutting concerns
    ├── Extensions/
    ├── Utilities/
    └── Attributes/
```

## Naming Conventions

### Interfaces
```csharp
// Prefix with 'I', describe capability
IPlayerInput
IDamageable
IMovable
IState<TContext>
```

### Abstract Classes
```csharp
// Describe what it is, no prefix
PlayerEntity
BaseState
MovementState
```

### ScriptableObjects
```csharp
// Suffix with data type
PlayerConfig : ScriptableObject
WeaponData : ScriptableObject
DamageTypeData : ScriptableObject
```

### Events
```csharp
// Suffix with 'Event', past tense verb
PlayerDamagedEvent
MovementStateChangedEvent
ViewModeSwitchedEvent
```

### Enums
```csharp
// Singular for single value, plural for flags
MovementState { Idle, Walking, Running }
DamageType { Physical, Blunt, Slashing }
BodyPartType { Head, Torso, LeftArm }
```

## Code Style

### File Structure
```csharp
using System;
using UnityEngine;  // Unity imports
using MyGame.Domain.Interfaces;  // Project imports (alphabetical)

namespace MyGame.Domain.Entities.Player
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
            // Implementation
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
// BAD: One class does movement, camera, and input
class PlayerController { }

// GOOD: Separate responsibilities
class PlayerMotor : IMovable { }
class PlayerCamera : ICameraController { }
class PlayerInputHandler : IPlayerInput { }
```

### Open/Closed
```csharp
// BAD: Modifying existing code to add states
if (state == "running") { }
else if (state == "newState") { }  // Adding new code

// GOOD: Extend via new classes
abstract class MovementState { abstract void Update(); }
class RunningState : MovementState { }
class NewState : MovementState { }  // Just add new class
```

### Liskov Substitution
```csharp
// All states interchangeable
IMovementState state = new RunningState();
state.Update();  // Works with any state

// Can replace with any derived state
state = new CrouchingState();
state.Update();  // Still works
```

### Interface Segregation
```csharp
// BAD: Fat interface
interface IPlayerActions
{
    void Move();
    void Jump();
    void CutTree();
    void Attack();
    void Swim();  // Not all players can swim
}

// GOOD: Segregated interfaces
interface IMovable { void Move(); }
interface IJumpable { void Jump(); }
interface IInteractable { void Interact(); }
interface ISwimmable { void Swim(); }
```

### Dependency Inversion
```csharp
// BAD: Depends on concrete class
class PlayerController
{
    private PlayerInput _input = new PlayerInput();
}

// GOOD: Depends on abstraction
class PlayerController
{
    private readonly IPlayerInput _input;
    
    public PlayerController(IPlayerInput input)
    {
        _input = input;
    }
}
```

## ScriptableObject Patterns

### Configuration Pattern
```csharp
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
```

### Event Pattern
```csharp
[CreateAssetMenu(menuName = "Events/Player Damaged Event")]
public class PlayerDamagedEvent : GameEvent<DamageData> { }

// Usage in Inspector: Any MonoBehaviour can listen
// No compile-time dependencies
```

### Factory Pattern
```csharp
[CreateAssetMenu(menuName = "Factories/State Factory")]
public class StateFactory : ScriptableObject
{
    [System.Serializable]
    public class StateEntry
    {
        public MovementStateType Type;
        public BaseState StatePrefab;  // Actually a prototype
    }
    
    public List<StateEntry> States = new();
    
    public BaseState CreateState(MovementStateType type)
    {
        var entry = States.FirstOrDefault(s => s.Type == type);
        return entry?.StatePrefab.Clone();  // Prototype pattern
    }
}
```

## Testing Patterns

### Domain Logic Tests
```csharp
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
```

## Documentation Standards

### XML Documentation
```csharp
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
```

### Architecture Decision Records (ADR)
Location: `Documentation/Decisions/`
Format: `YYYY-MM-DD-Title.md`

```markdown
# ADR 001: Use State Machine for Player Movement

## Status
Accepted

## Context
Player movement has multiple states (idle, walk, run, jump, crouch) with complex transitions.

## Decision
Implement a state machine pattern with ScriptableObject-based state prototypes.

## Consequences
- Easy to add new states without modifying existing code
- States can be configured in Unity Inspector
- Slight learning curve for new team members
```

## Git Commit Standards

### Commit Messages
```
feat(player): add injury system with body part tracking
fix(camera): correct FOV transition when switching view modes
refactor(domain): extract movement logic from PlayerController
docs(architecture): update state machine design document
test(player): add unit tests for damage calculation
```

### Branch Naming
```
feature/player-injury-system
bugfix/camera-viewmode-transition
refactor/extract-domain-entities
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

**Version**: 1.0  
**Last Updated**: 2026-05-04  
**Compliance**: SOLID, Clean Architecture, DDD
