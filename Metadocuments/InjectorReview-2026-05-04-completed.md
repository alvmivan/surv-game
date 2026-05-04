# Review: `alvmivan/injector` (v2)

**Repo**: https://github.com/alvmivan/injector  
**Package**: `com.torque-games.injector` v1.0.1 (UPM)  
**Licencia**: MIT  
**Reviewer**: Cascade (AI)  
**Fecha**: 2026-05-04  
**Estado**: completed — aprobado para uso en desarrollo

---

## Cambios desde la review anterior (6 commits → v2)

| Problema original | Resolución |
|---|---|
| `_extras` mutable compartido (thread-unsafe) | ✅ Extras ahora se pasan por parámetro en la cadena |
| `[Inject]` attribute sin implementar | ✅ `GetBestConstructor` prioriza `[Inject]`, fallback a greedy |
| Falla silenciosa en `Get<T>()` | ✅ Tira `InvalidOperationException`, agregado `TryGet<T>()` |
| Sin `Reset()` | ✅ Implementado en interface y facade |
| `FindObjectOfType` fallback | ✅ Eliminado |
| GPL-3.0 | ✅ Cambiado a MIT |
| Tests incompletos (6 tests) | ✅ 15 tests: singleton, create, interface, TryGet, Reset, `[Inject]`, greedy |
| Namespace inconsistente | ✅ Todo unificado en `namespace Injector` |

---

## API actual

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

## Pendientes menores (no bloqueantes para Phase 1)

- **Scoped containers**: No hay forma de crear sub-containers por escena. Para Phase 1 con una sola escena no es problema.
- **Lifecycle/Dispose**: No gestiona `IDisposable` al hacer Clear/Reset.
- **Circular dependency detection**: Podría entrar en stack overflow si A depende de B y B depende de A. No hay guard.
- **`package.json` version**: Sigue en 1.0.1, debería bumpearse a 2.0.0 por los breaking changes (Get ahora tira excepción).

---

## Veredicto

**8.5/10** — Aprobado para uso en desarrollo. Todos los problemas críticos resueltos. Liviano, propio, MIT, y cubre las necesidades de DI del survival game para Phase 1.
