# Dependency Injection con `com.torque-games.injector`

El proyecto usa [alvmivan/injector](https://github.com/alvmivan/injector) como contenedor DI liviano.  
**Licencia**: MIT | **Package**: `com.torque-games.injector` | **Instalación**: UPM Git dependency

---

## Instalación

En `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.torque-games.injector": "https://github.com/alvmivan/injector.git"
  }
}
```

---

## API

```csharp
using Injector;

// Registrar
Injection.Register<IFoo, Foo>();       // Interface → Implementación
Injection.Register<Foo>();             // Tipo concreto
Injection.Register<Foo>(instance);     // Instancia existente

// Resolver
var foo = Injection.Get<IFoo>();       // Singleton (cached) — throws si no registrado
var ok = Injection.TryGet<IFoo>(out var foo); // Safe resolution
var fresh = Injection.Create<IFoo>();  // Nueva instancia (no cached)

// Con dependencias extra
var x = Injection.GetWithExtraDependencies<IFoo>(extra1, extra2);

// Limpiar
Injection.Clear<IFoo>();               // Elimina un registro
Injection.Reset();                     // Limpia todo
```

---

## Ejemplos en el proyecto

### Registrar dependencias (bootstrap)
```csharp
using Injector;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        Injection.Register<IPlayerInput, InputProvider>();
        Injection.Register<IMovable, PlayerMotor>();
        Injection.Register<IEventBus, SimpleEventBus>();
        
        // Registrar instancia existente (ej: ScriptableObject)
        Injection.Register<PlayerConfig>(playerConfig);
    }
}
```

### Resolver dependencias (domain layer)
```csharp
using Injector;

// El injector resuelve constructor dependencies recursivamente
var motor = Injection.Get<IMovable>();       // Singleton (cached)
var newMotor = Injection.Create<IMovable>(); // Siempre nueva instancia

// Safe resolution
if (Injection.TryGet<IEventBus>(out var bus))
{
    bus.Publish(new PlayerSpawnedEvent());
}
```

### En tests: Reset entre tests
```csharp
using Injector;

[SetUp]
public void Setup()
{
    Injection.Reset();  // Limpia todas las registraciones
    Injection.Register<IPlayerInput, MockPlayerInput>();
    Injection.Register<IEventBus, MockEventBus>();
}

[Test]
public void Player_TakesDamage_ReducesHealth()
{
    var player = Injection.Create<PlayerEntity>();
    player.Health.TakeDamage(30f);
    Assert.AreEqual(70f, player.Health.CurrentHealth);
}
```

### `[Inject]` para elegir constructor
```csharp
using Injector;

public class PlayerEntity
{
    // Sin [Inject]: el injector elige el constructor con más parámetros resolvibles (greedy)
    public PlayerEntity(IPlayerInput input, IEventBus bus) { ... }
    
    [Inject]  // Con [Inject]: el injector SIEMPRE usa este, ignorando greedy
    public PlayerEntity(IPlayerInput input) { ... }
}
```

---

## Reglas de uso

| Capa | Cómo inyectar |
|------|--------------|
| **Domain** (clases puras) | Constructor injection — el injector resuelve automáticamente |
| **Infrastructure** (MonoBehaviours) | `[SerializeField]` o resolver en `Awake()` con `Injection.Get<T>()` |
| **Tests** | `Injection.Reset()` en `[SetUp]`, luego registrar mocks |

### No hacer
- **No llamar `Injection.Get<T>()` dentro de clases domain** — las dependencias llegan por constructor
- **No registrar MonoBehaviours** con `Register<T>()` — usar `[SerializeField]` o buscar con `GetComponent`
- **No olvidar `Reset()` en tests** — el container es estático, los tests se contaminan sin reset

---

## Cómo funciona internamente

1. `Register<TInterface, TImpl>()` guarda el mapping tipo → implementación
2. `Get<T>()` busca en cache → si no hay, busca el mejor constructor de la implementación
3. **Best constructor**: primero busca uno con `[Inject]`, si no, el que tenga más parámetros resolvibles
4. Resuelve cada parámetro recursivamente (llama `Resolve()` para cada uno)
5. Crea la instancia y la guarda en cache (singleton per container)
6. `Create<T>()` es igual pero **no guarda en cache** — siempre nueva instancia

---

**Relacionado**: Ver [Coding-Standards.md](Coding-Standards.md) para SOLID examples y Dependency Inversion  
**Repo**: https://github.com/alvmivan/injector

---

**Version**: 1.0  
**Last Updated**: 2026-05-04  
**Status**: Active Reference
