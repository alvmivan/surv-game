# Code Templates for Testable Classes

Templates y ejemplos completos que acompañan las [01-Testable-Code-Guidelines.md](01-Testable-Code-Guidelines.md).

---

## Domain Entity Example
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

## Infrastructure MonoBehaviour Example
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

## State Machine Testing Example

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

### Tests
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

**Relacionado**: [01-Testable-Code-Guidelines.md](01-Testable-Code-Guidelines.md) | [Coding-Standards.md](Coding-Standards.md)  
**Version**: 1.0  
**Last Updated**: 2026-05-04
