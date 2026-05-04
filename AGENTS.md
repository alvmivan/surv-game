# AGENTS.md — Instrucciones para agentes de IA

Este archivo define las reglas que cualquier agente de IA (Windsurf/Cascade, OpenCode, Cursor, Copilot, Codex, etc.) debe seguir al trabajar en este repositorio.

---

## Proyecto

First Person Survival game en **Unity 6000.2.6f2+**. Arquitectura en tres capas reutilizables: `SurvGame` (survival genérico) → `FPSGame` (FPS genérico) → `SurvivalProject` (el juego concreto). Usa SOLID, Clean Architecture interna por feature, y patrones AAA (Controller/Pawn, State Machine, Strategy para view modes).

---

## Estructura del repo

```
/                                   ← Raíz del repo
├── AGENTS.md                       ← Este archivo
├── DocumentationReview.md          ← Review de documentación (no tocar sin permiso)
├── README.md
└── My project/                     ← Proyecto Unity
    └── Assets/
        ├── Documentation/          ← Toda la documentación del juego
        │   ├── INDEX.md            ← ÍNDICE MAESTRO (fuente de verdad para navegación)
        │   ├── Architecture/       ← Cómo está construido
        │   ├── Standards/          ← Cómo escribir código
        │   └── Systems/            ← Qué hace el juego
        ├── SurvGame/               ← Layer 1: survival genérico (sin dependencias)
        ├── FPSGame/                ← Layer 2: FPS genérico (depende de SurvGame)
        └── SurvivalProject/        ← Layer 3: el juego concreto (depende de ambas)
```

---

## Reglas para documentación

### Fuente de verdad

- **`Documentation/INDEX.md`** es el índice maestro. Si creás o eliminás un doc, actualizá el INDEX.
- **`Standards/Coding-Standards.md`** es el documento canónico de coding standards. No debe existir otro archivo con el mismo propósito.
- **`Architecture/PlayerController-Design.md`** es el documento central de diseño del player controller.

### Al crear o editar documentación

1. **No duplicar información.** Si algo ya está definido en otro doc, linkeá en vez de copiar. La duplicación genera contradicciones cuando se actualiza solo una copia.
2. **Un solo valor para cada parámetro.** Si un valor de diseño (ej: `RunSpeedMultiplier`) aparece en múltiples docs, debe ser idéntico en todos. Si necesitás cambiarlo, buscá y actualizá **todas** las ocurrencias.
3. **No concatenar versiones.** Cuando actualizás un doc, reemplazá el contenido viejo. No pegues la versión nueva al final del archivo.
4. **Un solo footer de versión por archivo.** Cada doc termina con exactamente un bloque de versión/fecha/status.
5. **No referenciar archivos que no existen.** Antes de linkear a otro doc, verificá que el archivo destino existe. Si no existe, no lo linkees.
6. **Máximo 400 líneas por archivo.** Si un doc excede este límite, partilo en archivos más chicos y linkeá desde el original. Esta regla está definida en el proyecto y debe respetarse.
7. **Mantener el INDEX actualizado.** Al crear, renombrar o eliminar un doc, actualizá `Documentation/INDEX.md` para reflejarlo.

### Citas y fuentes

Cuando un agente de IA investiga información técnica online para tomar decisiones de diseño o arquitectura, **debe citar las URLs de las fuentes** en el documento o commit message donde aplique esa información. No se necesita formato APA, pero sí:
- La URL completa.
- Una breve descripción de qué se tomó de esa fuente.
- Ubicación: en una sección `## Referencias` al final del doc, o inline si es una decisión puntual.

Esto aplica para decisiones de arquitectura, valores de diseño basados en investigación, patrones tomados de otros engines o juegos, y cualquier afirmación técnica no trivial. **No confiamos a ciegas en la documentación generada por IA.**

### Convenciones de documentos

- **Ubicación**: Docs de arquitectura en `Architecture/`, estándares de código en `Standards/`, especificaciones de sistemas en `Systems/`.
- **Naming**: `NN-Topic-Description.md` para docs numerados. Nombres descriptivos para el resto.
- **Links**: Usar rutas relativas entre docs dentro de `Documentation/`.
- **Idioma**: Se acepta español o inglés, pero cada documento debe ser consistente internamente.

---

## Reglas para código C#

### Arquitectura de tres capas

```
SurvGame (Layer 1)    ← NO puede referenciar FPSGame ni SurvivalProject
    ↑
FPSGame (Layer 2)     ← Puede referenciar SurvGame. NO puede referenciar SurvivalProject
    ↑
SurvivalProject (Layer 3) ← Puede referenciar ambas
```

### Namespaces FLAT

```csharp
// ✅ CORRECTO
namespace SurvGame.Health
namespace FPSGame.Player
namespace SurvivalProject.Features

// ❌ INCORRECTO — No usar sub-namespaces por capa interna
namespace FPSGame.Player.Domain.Interfaces
namespace SurvGame.Inventory.Items.Weapons
```

Las subcarpetas `Domain/`, `Application/`, `Infrastructure/`, `Data/` dentro de cada feature son **organizacionales** (para ordenar archivos en disco). El namespace no las refleja.

### Estructura de carpetas por feature

```
SurvGame/
└── Health/                  ← namespace: SurvGame.Health
    ├── Domain/              ← Lógica pura, sin dependencias de Unity
    ├── Infrastructure/      ← MonoBehaviours, Unity API
    └── Data/                ← ScriptableObjects
```

### Code style

- **Region order**: Dependencies → Constants → Serialized Fields → State/Properties → Events → Constructor → Unity Methods → Public → Protected → Private.
- **Interfaces**: prefijo `I`, describen capacidad (`IMovable`, `IDamageable`).
- **Events**: sufijo `Event`, verbo en pasado (`PlayerDamagedEvent`).
- **ScriptableObjects**: sufijo `Config` o `Data` (`PlayerConfig`, `WeaponData`).
- **Enums**: singular para valor único, plural para flags.
- **DI**: Constructor injection para domain. `[SerializeField]` o `Awake()` para MonoBehaviours.
- **No statics en domain**: Inyectar wrappers (`IPhysicsService`) en vez de llamar `Physics.Raycast` directamente.

### Valores de diseño canónicos (Phase 1)

Estos son los valores definitivos. Si los cambiás, actualizá **todos** los docs donde aparecen:

| Parámetro | Valor | Docs donde aparece |
|-----------|-------|-------------------|
| WalkSpeed | 5 m/s | Phase1-Dev-Plan, PlayerController-Spec, Coding-Standards |
| RunSpeedMultiplier | 1.6x (8 m/s) | Phase1-Dev-Plan, PlayerController-Design, PlayerController-Spec |
| CrouchSpeedMultiplier | 0.5x (2.5 m/s) | Phase1-Dev-Plan, PlayerController-Spec |
| JumpForce | 5 m/s | Phase1-Dev-Plan |
| Gravity | -9.81 m/s² | Phase1-Dev-Plan |
| CoyoteTime | 100ms | Phase1-Dev-Plan, PlayerController-Design |
| JumpBuffer | 100ms | Phase1-Dev-Plan, PlayerController-Design |
| SlopeLimit | 45° | Phase1-Dev-Plan, PlayerController-Design |
| GroundCheckDistance | 0.2m | Phase1-Dev-Plan, PlayerController-Spec |

---

## Testing

- **Framework**: Unity Test Framework (Edit Mode + Play Mode).
- **Naming**: `MethodName_ExpectedBehavior_WhenCondition()`.
- **Pattern**: Arrange-Act-Assert.
- **Domain tests**: Edit Mode (no Unity runtime). Usar mocks para dependencias.
- **Infrastructure tests**: Play Mode si necesitan Unity runtime.
- **Cobertura objetivo**: ≥80% en domain layer.

### Comandos

```bash
# Edit Mode tests
unity -runTests -projectPath "My project" -testPlatform EditMode -testResults results.xml

# Play Mode tests
unity -runTests -projectPath "My project" -testPlatform PlayMode -testResults results.xml
```

---

## Git

### Commits
```
feat(SurvGame-Health): add injury system
feat(FPSGame-Player): implement coyote time
fix(FPSGame-Camera): correct FOV transition
test(FPSGame-Player): add movement unit tests
docs(Architecture): update three-layer doc
```

### Branches
```
feature/survgame-inventory-system
feature/fpsgame-player-movement
bugfix/fpsgame-player-coyote-time
```

---

## Qué NO hacer

- **No crear archivos .md sueltos fuera de `Documentation/`** (excepto este AGENTS.md, README.md, y DocumentationReview.md en la raíz).
- **No duplicar Coding Standards.** Solo debe existir `Standards/Coding-Standards.md`.
- **No usar `namespace MyGame.*`** en ejemplos. Usar los namespaces reales del proyecto (`SurvGame.*`, `FPSGame.*`, `SurvivalProject.*`).
- **No hardcodear valores de diseño en código.** Usar `ScriptableObject` configs.
- **No poner lógica de dominio en MonoBehaviours.** Extraer a clases puras testeables.
- **No crear dependencias circulares entre capas.** Layer 1 nunca importa Layer 2 o 3.
