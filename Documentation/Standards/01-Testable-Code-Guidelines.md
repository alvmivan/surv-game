# Testable Code Guidelines

## Overview
Cómo escribir código limpio y testeable para el Player Controller, siguiendo AAA industry standards.

## Principles for Testable Code

### 1. Dependency Injection (Constructor Injection)
**Bad** (hard to test):
```csharp
public class PlayerMotor
{
     PlayerConfig _config = Resources.Load<PlayerConfig>("PlayerConfig");
     InputProvider _input = FindObjectOfType<InputProvider>();
    
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
     readonly IPlayerConfig _config;
     readonly IPlayerInput _input;
    
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
     readonly IPhysicsService _physics;
    
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

**Good** (domain entity, separate — immutable where possible):
```csharp
// Domain Layer (testeable sin Unity)
// NOTE: Properties don't serialize in Unity by default.
// Use fields for Unity-serializable state, or reconstruct from save data.
public class PlayerEntity
{
    // For Unity serialization: use field + readonly if immutable
    [SerializeField]  float _health;
    
    // Prefer exposing as property for encapsulation
    public float Health => _health;
    public bool IsAlive => _health > 0;
    
    // Constructor for initial state (immutable pattern)
    public PlayerEntity(float maxHealth)
    {
        _health = maxHealth;
    }
    
    // Returns new instance instead of mutating (immutable approach)
    public PlayerEntity TakeDamage(float amount)
    {
        return new PlayerEntity(Mathf.Max(0, _health - amount));
    }
    
    // Or if mutation is required, keep it encapsulated:
    public void TakeDamage(float amount) => _health = Mathf.Max(0, _health - amount);
}

// Infrastructure Layer (Unity specific)
public class PlayerMotor : MonoBehaviour
{
    [SerializeField]  float _initialHealth = 100f;
     PlayerEntity _entity;
    
    void Awake()
    {
        _entity = new PlayerEntity(_initialHealth);
    }
}
```

**Why**: Puedes testear PlayerEntity sin Unity. Inmutabilidad evita side-effects:
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

## Code Templates

Ver **[04-Code-Templates.md](04-Code-Templates.md)** para templates completos de:
- Domain Entity (PlayerEntity con DI, events — sin regions)
- Infrastructure MonoBehaviour (PlayerMotor con CharacterController)
- State Machine testing (MockState, tests Given-When-Then)

---

## Testing Checklist

### Before Writing Code
- [ ] Can I instantiate this class without Unity? (Domain layer)
- [ ] Are all dependencies injected via constructor?
- [ ] Are there no static method calls (Physics, Input, UnityEngine.Random)?
- [ ] Is the class focused on one responsibility?

### Writing Tests
- [ ] Test naming: `Given_Precondition_When_Action_Then_ExpectedResult()`
- [ ] Given-When-Then pattern (Given context, When action, Then result)
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

**Next**: See [Coding-Standards.md](Coding-Standards.md) for naming conventions.  
**Related**: See [Phase1-Sprint1-Backlog.md](../Development/Phase1-Sprint1-Backlog.md) for implementation order.

---

**Last Updated**: 2026-05-04  
**Version**: 1.1 (Fixed namespaces to match project conventions)  
**Status**: Active Reference
