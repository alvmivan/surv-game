# Testable Code Guidelines

## Overview
Cómo escribir código limpio y testeable para el Player Controller, siguiendo AAA industry standards.

## Principles for Testable Code

### 1. Dependency Injection (Constructor Injection)
**Bad** (hard to test):
```csharp
public class PlayerMotor
{
    private PlayerConfig _config = Resources.Load<PlayerConfig>("PlayerConfig");
    private InputProvider _input = FindObjectOfType<InputProvider>();
    
    public void Move()
    {
        // Usa _config y _input directamente
        float speed = _config.WalkSpeed;
    }
}
```

**Good** (easy to test):
```csharp
public class PlayerMotor
{
    private readonly IPlayerConfig _config;
    private readonly IPlayerInput _input;
    
    // Dependencies inyectadas via constructor
    public PlayerMotor(IPlayerConfig config, IPlayerInput input)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _input = input ?? throw new ArgumentNullException(nameof(input));
    }
    
    public void Move()
    {
        float speed = _config.WalkSpeed;
    }
}
```

**Why**: Puedes pasar mocks en tests:
```csharp
[Test]
public void Move_UsesConfigSpeed()
{
    var mockConfig = new Mock<IPlayerConfig>();
    mockConfig.Setup(c => c.WalkSpeed).Returns(5f);
    
    var motor = new PlayerMotor(mockConfig.Object, mockInput.Object);
    // Test...
}
```

---

### 2. Pure Functions (Deterministic Output)
**Bad** (depends on Time.deltaTime):
```csharp
public float CalculateSpeed()
{
    return Input.GetAxis("Vertical") * Time.deltaTime * 10f;
}
```

**Good** (pure, testeable):
```csharp
public float CalculateSpeed(float inputMagnitude, float deltaTime, float multiplier)
{
    return inputMagnitude * deltaTime * multiplier;
}

// In Unity:
void Update()
{
    float speed = CalculateSpeed(_input.MoveInput.magnitude, Time.deltaTime, _config.SpeedMultiplier);
}
```

**Why**: Puedes testear sin Unity:
```csharp
[Test]
public void CalculateSpeed_ReturnsCorrectValue()
{
    float result = CalculateSpeed(1f, 0.016f, 10f);
    Assert.AreEqual(0.16f, result, 0.001f);
}
```

---

### 3. Interface Segregation (Small, Focused Interfaces)
**Bad** (fat interface):
```csharp
public interface IPlayerController
{
    void Move();
    void Jump();
    void Shoot();
    void OpenInventory();
    void Crouch();
    // ... 50 more methods
}
```

**Good** (segregated):
```csharp
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

public interface IAttacker
{
    void Attack(AttackType type);
}
```

**Why**: Mocks simples, tests focused:
```csharp
[Test]
public void Move_OnlyDependsOnIMovable()
{
    var mockMovable = new Mock<IMovable>();
    // Solo necesitas mockear Move(), no Shoot(), etc.
}
```

---

### 4. No Static Dependencies
**Bad** (static calls):
```csharp
public class GroundDetector
{
    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down);
    }
}
```

**Good** (inject physics wrapper):
```csharp
public interface IPhysicsService
{
    bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask);
}

public class GroundDetector
{
    private readonly IPhysicsService _physics;
    
    public GroundDetector(IPhysicsService physics)
    {
        _physics = physics;
    }
    
    public bool IsGrounded()
    {
        return _physics.Raycast(_position, Vector3.down, 0.2f, _groundLayer);
    }
}
```

**Why**: Puedes mock Raycast:
```csharp
[Test]
public void IsGrounded_ReturnsTrue_WhenRaycastHits()
{
    var mockPhysics = new Mock<IPhysicsService>();
    mockPhysics.Setup(p => p.Raycast(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), It.IsAny<int>()))
                 .Returns(true);
    
    var detector = new GroundDetector(mockPhysics.Object);
    Assert.IsTrue(detector.IsGrounded());
}
```

---

### 5. Constructor-Based State (No Unity Monobehaviour for Domain)
**Bad** (Monobehaviour with domain logic):
```csharp
public class PlayerMotor : MonoBehaviour
{
    public float Health = 100f; // State en Monobehaviour
    
    void Update()
    {
        if (Health <= 0) Die();
    }
}
```

**Good** (domain entity, separate):
```csharp
// Domain Layer (testeable sin Unity)
public class PlayerEntity
{
    public float Health { get; private set; }
    
    public void TakeDamage(float amount)
    {
        Health -= amount;
        if (Health <= 0) Die();
    }
}

// Infrastructure Layer (Unity specific)
public class PlayerMotor : MonoBehaviour
{
    private PlayerEntity _entity;
    
    void Update()
    {
        _entity.TakeDamage(10f);
    }
}
```

**Why**: Puedes testear PlayerEntity sin Unity:
```csharp
[Test]
public void TakeDamage_ReducesHealth()
{
    var player = new PlayerEntity(100f);
    player.TakeDamage(30f);
    Assert.AreEqual(70f, player.Health);
}
```

---

## Code Template for Testable Classes

### Domain Entity Example
```csharp
namespace FPSGame.Player
{
    /// <summary>
    /// Core player entity with state and behavior.
    /// </summary>
    public class PlayerEntity
    {
        #region Dependencies (injected)
        private readonly IPlayerStats _stats;
        private readonly IEventBus _eventBus;
        #endregion

        #region State
        public float CurrentHealth { get; private set; }
        public MovementState CurrentState { get; private set; }
        #endregion

        #region Events
        public event Action<float> OnHealthChanged;
        #endregion

        #region Constructor
        public PlayerEntity(IPlayerStats stats, IEventBus eventBus)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            CurrentHealth = _stats.MaxHealth;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies damage to the player.
        /// </summary>
        /// <param name="amount">Damage amount.</param>
        /// <returns>True if player survived.</returns>
        public bool TakeDamage(float amount)
        {
            if (amount <= 0) throw new ArgumentException("Damage must be positive", nameof(amount));
            
            CurrentHealth -= amount;
            OnHealthChanged?.Invoke(CurrentHealth);
            
            if (CurrentHealth <= 0)
            {
                Die();
                return false;
            }
            return true;
        }
        #endregion

        #region Private Methods
        private void Die()
        {
            CurrentState = MovementState.Idle; // TODO: add Dead state if needed
            _eventBus.Publish(new PlayerDiedEvent());
        }
        #endregion
    }
}
```

---

### Infrastructure Monobehaviour Example
```csharp
namespace FPSGame.Player
{
    /// <summary>
    /// Unity-specific player controller facade.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        #region Dependencies (injected via Unity Inspector or DI)
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private InputProvider _inputProvider;
        #endregion

        #region Private Fields
        private PlayerEntity _playerEntity;
        private CharacterController _characterController;
        private Vector3 _velocity;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerEntity = new PlayerEntity(_config, new UnityEventBus());
        }

        private void Update()
        {
            ApplyGravity();
            HandleMovement();
        }
        #endregion

        #region Private Methods
        private void HandleMovement()
        {
            Vector3 moveDirection = CalculateMoveDirection();
            float speed = CalculateSpeed();
            _characterController.Move(moveDirection * speed * Time.deltaTime);
        }

        private Vector3 CalculateMoveDirection()
        {
            return transform.right * _inputProvider.MoveInput.x + 
                   transform.forward * _inputProvider.MoveInput.y;
        }

        private float CalculateSpeed()
        {
            return _inputProvider.IsRunning ? _config.RunSpeed : _config.WalkSpeed;
        }

        private void ApplyGravity()
        {
            if (_characterController.isGrounded)
            {
                _velocity.y = -2f;
            }
            else
            {
                _velocity.y += Physics.gravity.y * Time.deltaTime;
            }
            _characterController.Move(_velocity * Time.deltaTime);
        }
        #endregion
    }
}
```

---

## Testing Checklist

### Before Writing Code
- [ ] Can I instantiate this class without Unity? (Domain layer)
- [ ] Are all dependencies injected via constructor?
- [ ] Are there no static method calls (Physics, Input, UnityEngine.Random)?
- [ ] Is the class focused on one responsibility?

### Writing Tests
- [ ] Test naming: `MethodName_ExpectedBehavior_WhenCondition()`
- [ ] Arrange-Act-Assert pattern
- [ ] One assertion per test (or group of related assertions)
- [ ] Mocks only for dependencies, not internals

### Code Review
- [ ] No Unity API calls in Domain layer
- [ ] Interfaces are small and focused
- [ ] Constructors validate parameters
- [ ] Methods have XML documentation

---

## Common Pitfalls

### Pitfall 1: Testing Monobehaviours
**Problem**: Can't instantiate Monobehaviour without Unity.
**Solution**: Extract domain logic to separate class, test that.

### Pitfall 2: Time.deltaTime in Domain
**Problem**: Tests run at different framerates.
**Solution**: Pass deltaTime as parameter to pure functions.

### Pitfall 3: Random in Domain
**Problem**: Tests non-deterministic.
**Solution**: Inject IRandomService, mock in tests.

### Pitfall 4: Too Many Dependencies
**Problem**: Constructor with 10+ parameters.
**Solution**: Group related dependencies into Facade interfaces.

---

## Tools for Testing

### Unity Test Framework
```bash
# Edit Mode tests (no Unity runtime needed)
Window → General → Test Runner → EditMode

# Play Mode tests (Unity runtime needed)
Window → General → Test Runner → PlayMode
```

### NSubstitute or Moq (for mocking)
```csharp
// Usando Moq
var mockInput = new Mock<IPlayerInput>();
mockInput.Setup(i => i.MoveInput).Returns(new Vector2(1, 0));

// Usando NSubstitute
var mockInput = Substitute.For<IPlayerInput>();
mockInput.MoveInput.Returns(new Vector2(1, 0));
```

---

## Example: Testing State Machine

### State Machine (Domain Layer)
```csharp
public class StateMachine
{
    public IState CurrentState { get; private set; }
    
    public void ChangeState(IState newState)
    {
        if (newState == null) throw new ArgumentNullException(nameof(newState));
        
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
    
    public void Update(float deltaTime)
    {
        CurrentState?.Update(deltaTime);
    }
}
```

### Mock State for Testing
```csharp
public class MockState : IState
{
    public bool EnterCalled { get; private set; }
    public bool ExitCalled { get; private set; }
    public bool UpdateCalled { get; private set; }
    
    public void Enter() => EnterCalled = true;
    public void Exit() => ExitCalled = true;
    public void Update(float deltaTime) => UpdateCalled = true;
}
```

### Test
```csharp
[Test]
public void ChangeState_CallsExitOnOldState()
{
    // Arrange
    var machine = new StateMachine();
    var oldState = new MockState();
    var newState = new MockState();
    machine.ChangeState(oldState);
    
    // Act
    machine.ChangeState(newState);
    
    // Assert
    Assert.IsTrue(oldState.ExitCalled);
}

[Test]
public void ChangeState_CallsEnterOnNewState()
{
    // Arrange
    var machine = new StateMachine();
    var newState = new MockState();
    
    // Act
    machine.ChangeState(newState);
    
    // Assert
    Assert.IsTrue(newState.EnterCalled);
}
```

---

**Next**: See [Coding-Standards.md](Coding-Standards.md) for naming conventions.  
**Related**: See [08-Phase1-Development-Plan.md](../Architecture/08-Phase1-Development-Plan.md) for implementation order.

---

**Last Updated**: 2026-05-04  
**Version**: 1.1 (Fixed namespaces to match project conventions)  
**Status**: Active Reference
