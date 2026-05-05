# Patterns & Examples — Three-Layer Architecture

Ejemplos concretos de código para las convenciones definidas en [Coding-Standards.md](Coding-Standards.md).

---

## ScriptableObject Patterns

### Layer 2: SurvGame Configuration
```csharp
namespace SurvGame.Health
{
    // Menu path indicates layer: "SurvGame/..."
    [CreateAssetMenu(menuName = "SurvGame/Health Config")]
    public class HealthConfig : ScriptableObject
    {
        public float MaxHealth = 100f;
        public float RegenRate = 1f;
    }
}
```

### Layer 1: FPSGame Configuration
```csharp
namespace FPSGame.Player
{
    // Menu path indicates layer: "FPSGame/..."
    [CreateAssetMenu(menuName = "FPSGame/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float BaseSpeed = 5f;
        public float RunSpeedMultiplier = 1.6f;
    }
}
```

### Layer 3: SurvivalProject Configuration
```csharp
namespace SurvivalProject.Configuration
{
    using FPSGame.Player;
    using SurvGame.Health;
    
    // Menu path indicates layer: "SurvivalProject/..."
    [CreateAssetMenu(menuName = "SurvivalProject/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("References")]
        public PlayerConfig PlayerConfig;   // From Layer 1
        public HealthConfig HealthConfig;     // From Layer 2
        
        [Header("Game-Specific")]
        public string GameTitle = "My Survival Game";
    }
}
```

---

## Testing Patterns

### Testing Layer 2 (SurvGame - Depends on FPSGame)
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

---

## XML Documentation (With Layer References)
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

---

## Layer Communication (Exposure Pattern)

### How Layers Expose Functionality
```csharp
// Layer 1: FPSGame exposes to upper layers
namespace FPSGame.Player
{
    public interface IMovable { ... }
    public class PlayerEntity { ... }
}

// Layer 2: SurvGame uses Layer 1, exposes to Layer 3
namespace SurvGame.Health
{
    using FPSGame.Player;
    
    public interface IDamageable { ... }
    public class HealthEntity { ... }
}

// Layer 3: Your game uses both
namespace SurvivalProject.Features
{
    using FPSGame.Player;
    using SurvGame.Health;
    
    public class GameManager
    {
        private readonly IMovable _player;
        private readonly IDamageable _health;
        
        public GameManager(IMovable player, IDamageable health)
        {
            _player = player;  // From Layer 1
            _health = health;  // From Layer 2
        }
    }
}
```

### What NOT to do
```csharp
// ❌ BAD: Layer 1 trying to use Layer 2 (circular dependency!)
namespace FPSGame.Player
{
    using SurvGame.Health;  // ❌ NO! Can't reference upper layer!
}

// ❌ BAD: Deep namespace hierarchy
namespace FPSGame.Player.Domain.Entities { }
namespace SurvGame.Inventory.Items.Weapons { }

// ✅ GOOD: Flat namespaces
namespace FPSGame.Player { }
namespace SurvGame.Inventory { }
```

---

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

**Relacionado**: [Coding-Standards.md](Coding-Standards.md) | [02-DI-Guidelines.md](02-DI-Guidelines.md)  
**Version**: 1.0  
**Last Updated**: 2026-05-04
