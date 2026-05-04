# Documentation Review - Survival FPS Game

**Reviewer**: Cascade (AI)  
**Fecha**: 2026-05-04  
**Documentos revisados**: 9 archivos en `Documentation/Architecture`, `Documentation/Standards`, `Documentation/Systems`

---

## 1. Problemas Estructurales Graves

### 1.1 Hay DOS documentos de Coding Standards que se contradicen

Existen dos archivos llamados `Coding-Standards.md`:
- `Architecture/Coding-Standards.md` (457 líneas) — usa una estructura de carpetas plana con `Domain/`, `Application/`, `Infrastructure/`, `Data/`, `Presentation/`, `Shared/` directamente bajo `Assets/`.
- `Standards/Coding-Standards.md` (682 líneas) — usa la arquitectura de tres capas (`SurvGame/`, `FPSGame/`, `SurvivalProject/`), **pero internamente también tiene pegada una segunda estructura "Feature-based"** (líneas 54-119) que contradice la primera mitad del mismo archivo.

**Problemas concretos**:
- El archivo `Standards/Coding-Standards.md` tiene **dos footer de versión**: "Version: 2.0 (Three-Layer Architecture)" en línea 632 y "Version: 1.2 (Feature-based architecture)" en línea 679. Parece que se concatenaron dos versiones del archivo sin limpiar la anterior.
- La sección "What NOT to do" aparece **duplicada** (líneas 613-628 y líneas 637-656).
- ¿Cuál es la estructura definitiva? ¿Three-Layer o la que está en `Architecture/Coding-Standards.md`? Hay que elegir una y borrar la otra. Tener dos fuentes de verdad distintas es una receta para confusión.

**Recomendación**: Eliminar `Architecture/Coding-Standards.md` (parece la versión vieja) y limpiar `Standards/Coding-Standards.md` quitando el bloque duplicado de la estructura Feature-based (líneas 54-119) y el footer duplicado.

---

### 1.2 El Overview referencia documentos que no existen

`00-Overview.md` lista en su índice:
- `01-Controller-Pawn-Pattern.md`
- `02-Input-System.md`
- `03-State-Machine.md`
- `04-Camera-System.md`
- `05-Domain-Model.md`
- `06-Data-Flow.md`
- `07-Extensibility.md`

**Ninguno de estos archivos existe en el repositorio.** Solo existen `00-Overview.md`, `08-Phase1-Development-Plan.md`, `09-Three-Layer-Architecture.md`, `Coding-Standards.md` y `PlayerController-Design.md`.

Esto significa que el índice principal del proyecto apunta a 7 documentos fantasma. Es engañoso para cualquier persona nueva que llegue al proyecto.

**Recomendación**: O se crean esos documentos (extrayendo las secciones correspondientes de `PlayerController-Design.md`, que ya contiene toda esa información), o se actualiza el índice para reflejar la realidad.

---

### 1.3 PlayerController-Design.md viola la política de "Max 400 líneas"

El propio `00-Overview.md` establece una política de **máximo 400 líneas por archivo**. Sin embargo:
- `PlayerController-Design.md`: **529 líneas**
- `Standards/Coding-Standards.md`: **682 líneas**
- `01-Testable-Code-Guidelines.md`: **533 líneas**
- `01-Phase1-Testing-Strategy.md`: **485 líneas**
- `Architecture/Coding-Standards.md`: **457 líneas**

Es decir, **5 de 9 documentos** violan la regla que el propio proyecto define. Si la regla importa, hay que refactorizar. Si no importa, hay que quitarla.

---

## 2. Contradicciones Técnicas

### 2.1 Namespaces: ¿Flat o con subcarpetas Domain/Application/Infrastructure?

La arquitectura de tres capas dice namespaces FLAT:
```csharp
namespace SurvGame.Inventory     // ✅ FLAT
namespace FPSGame.Player         // ✅ FLAT
```

Y explícitamente prohíbe:
```csharp
namespace FPSGame.Player.Domain.Interfaces // ❌ NO!
```

**Pero** la estructura de carpetas muestra `Domain/`, `Application/`, `Infrastructure/`, `Data/` dentro de cada feature. Si los namespaces son flat pero las carpetas tienen subcarpetas de capas, ¿cómo se organiza el código? ¿Todo lo que está en `FPSGame/Player/Domain/` y `FPSGame/Player/Infrastructure/` comparte el namespace `FPSGame.Player`?

Esto puede funcionar, pero genera confusión: archivos con responsabilidades muy distintas (una interfaz de dominio pura y un MonoBehaviour) comparten namespace. Debería explicarse explícitamente que las carpetas son solo organizacionales y que el namespace no las refleja. Actualmente no está claro.

### 2.2 ¿Dónde vive la State Machine?

- `08-Phase1-Development-Plan.md` dice crear `StateMachine.cs` y `BaseMovementState.cs` en `Domain/States/`.
- `Architecture/Coding-Standards.md` los ubica en `Infrastructure/StateMachine/`.
- `09-Three-Layer-Architecture.md` no menciona dónde va la state machine.
- `Standards/Coding-Standards.md` la pone en `SurvGame.Player` como `abstract class MovementState`.

¿Es la state machine domain o infrastructure? Si es lógica pura (lo cual tiene sentido para testearla), debería ser domain. Pero entonces el plan de Phase 1 contradice al Coding Standards de Architecture. **Hay que unificar.**

### 2.3 Walk Speed: ¿3-5 m/s o 5 m/s?

- `PlayerController-Specification.md` dice Walking es "Base speed (3-5 m/s)" — un rango.
- `08-Phase1-Development-Plan.md` dice WalkSpeed es exactamente 5 m/s.
- `PlayerConfig` en el Coding Standards dice `BaseSpeed = 5f`.

El rango "3-5 m/s" de la spec no es un valor, es un rango de diseño. Pero no se explica bajo qué condiciones varía. ¿Depende del terreno? ¿De injuries? Si es siempre 5, la spec debería decir 5.

### 2.4 RunSpeedMultiplier: ¿1.5x o 1.6x?

- `PlayerController-Design.md` línea 248: Running = "1.5x speed"
- `08-Phase1-Development-Plan.md` línea 175: RunSpeed = "8 m/s (1.6x multiplier)"
- `PlayerController-Specification.md` línea 15: Running = "1.5x base speed"
- `Standards/Coding-Standards.md` línea 466: `RunSpeedMultiplier = 1.5f`
- `Architecture/Coding-Standards.md` línea 294: `RunSpeedMultiplier = 1.5f`

**El plan de desarrollo dice 1.6x, todo lo demás dice 1.5x.** Si la velocidad base es 5 m/s y el run speed es 8 m/s, el multiplicador es 1.6x. Hay que decidir cuál es el valor correcto y unificarlo.

### 2.5 Crouch: ¿Hold o Toggle?

- `08-Phase1-Development-Plan.md` línea 70: "Crouch: Button (Ctrl, X button) - with **Toggle**"
- `08-Phase1-Development-Plan.md` línea 184: "Ctrl **toggles** crouch with speed change"
- `PlayerController-Specification.md` línea 25: "Crouch **toggles** with smooth height transition"

Pero el input de Run dice "with **Hold**" (línea 69). ¿No debería Crouch también soportar Hold como alternativa? Muchos FPS modernos ofrecen ambas opciones (Toggle/Hold) configurables por el usuario. Esto no está contemplado en ningún documento.

---

## 3. Preguntas de Diseño Sin Responder

### 3.1 Fixed Timestep 15-30Hz: ¿Cómo se implementa exactamente?

`PlayerController-Design.md` menciona repetidamente "Fixed Timestep: 15-30Hz" para la lógica de movimiento, citando el patrón de Guerrilla Games. Pero:

- Unity ya tiene `FixedUpdate` que corre a 50Hz por defecto. ¿Se va a cambiar `Time.fixedDeltaTime`? ¿Se va a implementar un accumulator custom?
- 15-30Hz es **muy bajo** para un FPS. La mayoría de los juegos AAA corren la física del jugador a 60Hz o más para evitar que el movimiento se sienta "pegajoso". 15Hz significaría actualizar el movimiento ~cada 66ms, lo cual se siente horrible en un FPS.
- ¿No contradice esto el requisito NFR-004 de "No input lag > 50ms"? Si la lógica corre a 15Hz, el peor caso de latencia del game logic es 66ms, ya excediendo los 50ms permitidos.

**Recomendación**: Clarificar si esto aplica solo a NPCs/AI (donde sí tiene sentido) o también al jugador. Para el jugador, sugeriría mínimo 60Hz o usar el Update de Unity con deltaTime.

### 3.2 ¿Dónde está el DI container?

Toda la documentación enfatiza Dependency Injection vía constructor. Pero no se menciona **cómo** se resuelven las dependencias en Unity:
- ¿Se usa un DI framework (Zenject/VContainer)?
- ¿Se resuelven manualmente en `Awake()`?
- ¿Se usa un Service Locator?

El ejemplo en `01-Testable-Code-Guidelines.md` muestra `new PlayerEntity(_config, new UnityEventBus())` en `Awake()`, lo cual es DI manual. Pero a medida que el proyecto crezca, esto se vuelve inmanejable. Debería haber una decisión documentada sobre esto.

### 3.3 La capa Application/UseCases: ¿realmente se necesita?

Se mencionan `MovePlayerUseCase.cs`, `AttackUseCase.cs`, `ChangeViewModeUseCase.cs`. Pero:
- En un juego, el "caso de uso" de mover al jugador es simplemente `motor.Move(input)`. Crear una clase UseCase que solo delega a otra clase es over-engineering.
- Clean Architecture tiene sentido en aplicaciones enterprise con múltiples entry points (API, CLI, UI). En un juego, la entrada siempre es el game loop.
- Ningún ejemplo de código en toda la documentación muestra cómo se usa realmente un UseCase en el flujo del juego.

**Pregunta**: ¿Se va a implementar realmente la capa Application, o se puede simplificar a Domain + Infrastructure + Data?

### 3.4 Controller/Pawn: ¿Qué pasa con DontDestroyOnLoad?

Se dice que `PlayerController` "persists through death/respawn cycles" y "survives scene reload". Esto implica `DontDestroyOnLoad`. Pero:
- ¿Cómo se maneja cuando se vuelve al menú principal? ¿Se destruye manualmente?
- ¿Qué pasa si hay múltiples scene loads? ¿Se duplica el controller?
- No hay mención de un singleton pattern o bootstrap scene.

### 3.5 EventBus: ¿ScriptableObject o C# puro?

En distintos documentos aparece:
- `EventBus.cs` como clase C# en `Domain/Events/`
- `GameEvent<T> : ScriptableObject` como asset de Unity
- `IEventBus` interfaz con `Publish()`

¿Son dos sistemas de eventos paralelos? ¿El domain usa uno y la infraestructura otro? Esto debería estar claro porque tener dos sistemas de eventos es una fuente segura de bugs (eventos que se publican en uno pero se escuchan en otro).

---

## 4. Cosas Incompletas

### 4.1 No hay documento de arquitectura para el inventario, crafting, ni health

El documento de tres capas (`09-Three-Layer-Architecture.md`) nombra `SurvGame.Inventory`, `SurvGame.Health`, `SurvGame.Crafting` como sistemas de Layer 1. Pero no hay ningún documento que especifique cómo funcionan. La `PlayerController-Specification.md` lista FR-004 (Injury), FR-005 (Damage), FR-006 (Environment), FR-007 (Stamina) pero todo como texto descriptivo sin diseño detallado.

### 4.2 No hay ADRs (Architecture Decision Records)

`Architecture/Coding-Standards.md` define un formato para ADRs en `Documentation/Decisions/`. Esa carpeta no existe y no hay ningún ADR. Las decisiones importantes (¿por qué Controller/Pawn? ¿por qué tres capas? ¿por qué namespaces flat?) deberían estar documentadas como ADRs.

### 4.3 Falta un diagrama de dependencias entre clases

Hay muchos diagramas ASCII de carpetas y capas, pero ningún diagrama de cómo las clases se relacionan entre sí en runtime. Un diagrama de secuencia mostrando el flujo Input -> Controller -> Pawn -> Motor -> StateMachine sería muy valioso.

### 4.4 No hay mención de cómo manejar el respawn

Se dice que el Pawn es "transient, can be destroyed/respawned", pero no hay especificación de:
- ¿Dónde respawnea?
- ¿Se pierde inventario?
- ¿Hay penalización por muerte?
- ¿Hay pantalla de muerte?

---

## 5. Errores Menores

### 5.1 Typo en test
`01-Phase1-Testing-Strategy.md` línea 247: `Player_Jumpes_WhenSpacePressed` — debería ser `Player_Jumps_WhenSpacePressed`.

### 5.2 Herencia cuestionable de HealthEntity
`09-Three-Layer-Architecture.md` muestra `PlayerEntity : HealthEntity`. Esto es herencia para compartir funcionalidad, lo cual es exactamente el anti-pattern que la documentación intenta evitar con Interface Segregation. ¿No sería mejor que `PlayerEntity` tenga un `HealthEntity` por composición en vez de heredar de él?

Además, `SurvivalPlayer.TakeDamage()` hace `base.TakeDamage(amount)` pero el método no es `virtual` en `HealthEntity`, así que ese código no compilaría. Y si `PlayerEntity` ya overridea `TakeDamage`, `SurvivalPlayer` estaría ocultando el método (warning CS0114).

### 5.3 HealthEntity no inicializa CurrentHealth
En `09-Three-Layer-Architecture.md`, `HealthEntity` tiene `CurrentHealth { get; private set; }` pero el constructor no se muestra y no hay inicialización. Un `TakeDamage(30f)` sobre una salud de 0 la dejaría en -30.

### 5.4 MovementState enum: ¿Dead?
`01-Testable-Code-Guidelines.md` usa `MovementState.Dead` en el método `Die()`, pero el enum `MovementState` definido en `Standards/Coding-Standards.md` es `{ Idle, Walking, Running, Crouching, Jumping, Falling }` — no incluye `Dead`.

### 5.5 Namespace inconsistente en ejemplos
- `01-Testable-Code-Guidelines.md` usa `namespace MyGame.Domain.Entities` (deep).
- `Standards/Coding-Standards.md` dice que eso es un anti-pattern (`❌ NO!`).

Los propios ejemplos de la documentación violan las reglas que la documentación define.

### 5.6 InputProvider tests imposibles
`01-Phase1-Testing-Strategy.md` sección 1.3 muestra tests para `InputProvider` que hacen `new InputProvider()` y luego acceden a `MoveInput`. Pero `InputProvider` envuelve el Input System de Unity, que no funciona fuera de Play Mode. Estos tests no pueden ser Edit Mode tests como el archivo sugiere. Deberían ser Play Mode tests, o se necesita un wrapper/mock.

---

## 6. Observaciones Positivas

- **La separación Controller/Pawn es una buena decisión.** Es un patrón probado que facilita respawn, AI takeover y testing.
- **Los namespaces flat son una buena idea** para un proyecto de este tamaño. Evitan la explosión de imports.
- **La documentación de testing es la más madura** de todos los documentos. La pirámide de testing, los benchmarks de performance y la playtest checklist son prácticos y útiles.
- **Los coding standards son detallados y con buenos ejemplos** de SOLID. Aunque estén duplicados, el contenido es sólido.
- **ScriptableObjects para configuración** es el approach correcto en Unity. Evita hardcoding y permite tuning sin recompilar.
- **Coyote time y jump buffering** desde la fase 1 muestra que hay atención al game feel desde el principio.

---

## 7. Resumen de Acciones Recomendadas

| Prioridad | Acción |
|-----------|--------|
| **ALTA** | Unificar los dos `Coding-Standards.md` en uno solo |
| **ALTA** | Eliminar el bloque duplicado/concatenado en `Standards/Coding-Standards.md` |
| **ALTA** | Actualizar `00-Overview.md` para reflejar los archivos que realmente existen |
| **ALTA** | Decidir RunSpeedMultiplier: 1.5x o 1.6x |
| **MEDIA** | Clarificar si el fixed timestep 15-30Hz aplica al jugador o solo a NPCs |
| **MEDIA** | Documentar la estrategia de DI (manual, Zenject, VContainer) |
| **MEDIA** | Decidir si la capa Application/UseCases se implementa o se elimina |
| **MEDIA** | Refactorizar documentos para cumplir el límite de 400 líneas (o eliminar la regla) |
| **MEDIA** | Usar composición en vez de herencia para HealthEntity -> PlayerEntity |
| **BAJA** | Crear los ADRs prometidos |
| **BAJA** | Agregar diagrama de secuencia del flujo de input |
| **BAJA** | Definir mecánicas de respawn |
| **BAJA** | Corregir typo "Jumpes" -> "Jumps" |
| **BAJA** | Agregar `Dead` al enum `MovementState` |
| **BAJA** | Corregir namespace `MyGame.Domain.Entities` en Testable-Code-Guidelines para que sea flat |

---

**Conclusión**: La documentación muestra una visión ambiciosa y bien informada por patrones de la industria AAA. El problema principal es que parece haber evolucionado por iteraciones (de una arquitectura plana a tres capas) y quedaron restos de versiones anteriores mezclados. La prioridad debería ser **limpiar las contradicciones** antes de empezar a codear, porque si no, cada desarrollador va a interpretar la arquitectura de forma distinta.
