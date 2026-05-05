# Code Templates for Testable Classes

Templates y ejemplos completos que acompañan las [01-Testable-Code-Guidelines.md](01-Testable-Code-Guidelines.md).

---

## Domain Entity Example
```csharp
namespace FPSGame.Player
{
    /// <summary>
    /// Core player entity with state and behavior.
    /// Dependencies injected via constructor.
    /// </summary>
    public class PlayerEntity
    {
        // Dependencies (injected)
         readonly IPlayerStats _stats;
         readonly IEventBus _eventBus;

        // State
        public float CurrentHealth { get; set; }
        public MovementState CurrentState { get; set; }

        // Events
        public event Action<float> OnHealthChanged;

        // Constructor
        public PlayerEntity(IPlayerStats stats, IEventBus eventBus)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            CurrentHealth = _stats.MaxHealth;
        }

        // Public Methods
        
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

        // Private Methods
        
         void Die()
        {
            CurrentState = MovementState.Idle; // TODO: add Dead state if needed
            _eventBus.Publish(new PlayerDiedEvent());
        }
    }
}
```

---

## Infrastructure MonoBehaviour Example
```csharp
namespace FPSGame.Player
{
    /// <summary>
    /// Unity-specific player controller facade.
    /// Dependencies injected via Inspector or DI container.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        // Dependencies (injected via Unity Inspector or DI)
        [SerializeField] PlayerConfig _config;
        [SerializeField] InputProvider _inputProvider;

        // Private Fields
         PlayerEntity _playerEntity;
         CharacterController _characterController;
         Vector3 _velocity;

        // Unity Lifecycle
        
         void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerEntity = new PlayerEntity(_config, new UnityEventBus());
        }

         void Update()
        {
            ApplyGravity();
            HandleMovement();
        }

        // Private Methods
        
         void HandleMovement()
        {
            Vector3 moveDirection = CalculateMoveDirection();
            float speed = CalculateSpeed();
            _characterController.Move(moveDirection * speed * Time.deltaTime);
        }

         Vector3 CalculateMoveDirection()
        {
            return transform.right * _inputProvider.MoveInput.x + 
                   transform.forward * _inputProvider.MoveInput.y;
        }

         float CalculateSpeed()
        {
            return _inputProvider.IsRunning ? _config.RunSpeed : _config.WalkSpeed;
        }

         void ApplyGravity()
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
    }
}
```

---

## State Machine Testing Example

### State Machine (Domain Layer)
```csharp
public class StateMachine
{
    public IState CurrentState { get; set; }
    
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
    public bool EnterCalled { get; set; }
    public bool ExitCalled { get; set; }
    public bool UpdateCalled { get; set; }
    
    public void Enter() => EnterCalled = true;
    public void Exit() => ExitCalled = true;
    public void Update(float deltaTime) => UpdateCalled = true;
}
```

### Tests (Given-When-Then pattern)
```csharp
[Test]
public void Given_ActiveState_When_ChangeState_Then_CallsExitOnOldState()
{
    // Given
    var machine = new StateMachine();
    var oldState = new MockState();
    var newState = new MockState();
    machine.ChangeState(oldState);
    
    // When
    machine.ChangeState(newState);
    
    // Then
    Assert.IsTrue(oldState.ExitCalled);
}

[Test]
public void Given_EmptyMachine_When_ChangeState_Then_CallsEnterOnNewState()
{
    // Given
    var machine = new StateMachine();
    var newState = new MockState();
    
    // When
    machine.ChangeState(newState);
    
    // Then
    Assert.IsTrue(newState.EnterCalled);
}
```

---

**Relacionado**: [01-Testable-Code-Guidelines.md](01-Testable-Code-Guidelines.md) | [Coding-Standards.md](Coding-Standards.md)  
**Version**: 1.0  
**Last Updated**: 2026-05-04
