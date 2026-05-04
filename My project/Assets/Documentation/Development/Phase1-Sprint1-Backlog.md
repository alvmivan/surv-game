# Phase 1 — Sprint 1: Core Architecture + Input

**Sprint Goal**: Tener la estructura de proyecto funcional con las tres capas, DI configurado, input system operativo, y la separación Controller/Pawn implementada con tests.

**Duración**: 1 semana  
**Story Points totales**: 26

---

## Epic 1: Project Setup (8 pts)

### Story 1.1: Crear estructura Three-Layer con Assembly Definitions
**Como** desarrollador **quiero** que el proyecto tenga la estructura de carpetas y assembly definitions correctas **para** que las dependencias entre capas estén enforceadas por el compilador.

**Points**: 5

**Tasks**:
- [ ] **T-1.1.1** Crear carpeta `Assets/SurvGame/` con `SurvGame.asmdef` (sin referencias externas)
- [ ] **T-1.1.2** Crear carpeta `Assets/FPSGame/` con `FPSGame.asmdef` (referencia a `SurvGame`)
- [ ] **T-1.1.3** Crear carpeta `Assets/SurvivalProject/` con `SurvivalProject.asmdef` (referencia a `FPSGame` y `SurvGame`)
- [ ] **T-1.1.4** Crear subcarpetas de feature en FPSGame: `Player/Domain/`, `Player/Infrastructure/`, `Player/Data/`
- [ ] **T-1.1.5** Crear subcarpetas de feature en SurvGame: `Health/Domain/`, `Health/Infrastructure/`, `Health/Data/`
- [ ] **T-1.1.6** Crear assembly definitions de tests: `SurvGame.Tests.asmdef`, `FPSGame.Tests.asmdef`
- [ ] **T-1.1.7** Verificar que SurvGame no puede importar FPSGame (test de compilación)

**Acceptance Criteria**:
- [ ] Las tres carpetas raíz existen con sus .asmdef
- [ ] FPSGame puede usar `using SurvGame.Health;` pero SurvGame NO puede usar `using FPSGame.Player;`
- [ ] Las assembly definitions de tests referencian sus layers + Unity Test Framework

**Archivos a crear**:
```
Assets/
├── SurvGame/
│   ├── SurvGame.asmdef
│   └── Health/
│       ├── Domain/
│       ├── Infrastructure/
│       └── Data/
├── FPSGame/
│   ├── FPSGame.asmdef
│   └── Player/
│       ├── Domain/
│       ├── Infrastructure/
│       └── Data/
├── SurvivalProject/
│   └── SurvivalProject.asmdef
└── Tests/
    ├── SurvGame.Tests.asmdef
    └── FPSGame.Tests.asmdef
```

---

### Story 1.2: Configurar Dependency Injection
**Como** desarrollador **quiero** que el injector esté instalado y el bootstrap configurado **para** poder registrar y resolver dependencias desde el arranque del juego.

**Points**: 3  
**Depende de**: T-1.1.1, T-1.1.2, T-1.1.3

**Tasks**:
- [ ] **T-1.2.1** Agregar `com.torque-games.injector` al `Packages/manifest.json` como Git dependency
- [ ] **T-1.2.2** Verificar que el package se importa correctamente y compila
- [ ] **T-1.2.3** Agregar referencia a `torque-games.injector` en `FPSGame.asmdef` y `SurvivalProject.asmdef`
- [ ] **T-1.2.4** Crear `SurvivalProject/Bootstrap/GameBootstrap.cs` (MonoBehaviour) con registración inicial vacía
- [ ] **T-1.2.5** Crear escena `Scenes/Bootstrap.unity` con GameBootstrap en un GameObject

**Acceptance Criteria**:
- [ ] `using Injector;` compila en FPSGame y SurvivalProject
- [ ] `Injection.Register<T>()` y `Injection.Get<T>()` funcionan en runtime
- [ ] GameBootstrap se ejecuta al iniciar la escena

**Archivos a crear**:
```
Packages/manifest.json                          ← agregar dependency
Assets/SurvivalProject/Bootstrap/
    └── GameBootstrap.cs
Assets/Scenes/
    └── Bootstrap.unity
```

---

## Epic 2: Domain Interfaces (5 pts)

### Story 2.1: Definir interfaces core del Player
**Como** desarrollador **quiero** tener las interfaces de dominio definidas **para** poder implementar contra abstracciones y testear con mocks.

**Points**: 3  
**Depende de**: Story 1.1

**Tasks**:
- [ ] **T-2.1.1** Crear `FPSGame/Player/Domain/IPlayerInput.cs` — `Vector2 MoveInput`, `Vector2 LookInput`, `bool JumpPressed`, `bool RunHeld`, `bool CrouchPressed`
- [ ] **T-2.1.2** Crear `FPSGame/Player/Domain/IMovable.cs` — `void Move(Vector3 direction, float speed)`, `bool IsGrounded`
- [ ] **T-2.1.3** Crear `FPSGame/Player/Domain/IJumpable.cs` — `void Jump(float force)`, `bool CanJump`
- [ ] **T-2.1.4** Crear `FPSGame/Player/Domain/IState.cs` — `void Enter()`, `void Exit()`, `void Update(float deltaTime)`, `void FixedUpdate(float fixedDeltaTime)`
- [ ] **T-2.1.5** Crear `FPSGame/Player/Domain/MovementState.cs` (enum) — `Idle, Walking, Running, Crouching, Jumping, Falling`

**Acceptance Criteria**:
- [ ] Todas las interfaces están en `namespace FPSGame.Player`
- [ ] Ninguna interfaz depende de Unity API (puras)
- [ ] `MovementState` enum tiene los 6 estados de Phase 1

**Archivos a crear**:
```
Assets/FPSGame/Player/Domain/
├── IPlayerInput.cs
├── IMovable.cs
├── IJumpable.cs
├── IState.cs
└── MovementState.cs
```

---

### Story 2.2: Definir interfaces de Health (Layer 1)
**Como** desarrollador **quiero** tener el sistema de salud base en SurvGame **para** que FPSGame pueda usarlo por composición.

**Points**: 2  
**Depende de**: Story 1.1

**Tasks**:
- [ ] **T-2.2.1** Crear `SurvGame/Health/Domain/HealthEntity.cs` con constructor, `TakeDamage(float)`, `Heal(float)`, `CurrentHealth`, `MaxHealth`, `IsDead`
- [ ] **T-2.2.2** Crear `SurvGame/Health/Domain/IDamageable.cs` — `void TakeDamage(float amount)`
- [ ] **T-2.2.3** Crear `SurvGame/Health/Data/HealthConfig.cs` (ScriptableObject) — `MaxHealth`, `RegenRate`
- [ ] **T-2.2.4** Escribir Edit Mode tests para `HealthEntity`: `TakeDamage_ReducesHealth`, `Heal_IncreasesHealth`, `TakeDamage_ClampsToZero`, `IsDead_WhenHealthZero`

**Acceptance Criteria**:
- [ ] `HealthEntity` es una clase pura (sin Unity API)
- [ ] `namespace SurvGame.Health`
- [ ] 4 tests pasan en Edit Mode
- [ ] `HealthConfig` es un ScriptableObject creatable desde el menú

---

## Epic 3: Input System (5 pts)

### Story 3.1: Implementar Input System con Unity Input System
**Como** jugador **quiero** poder mover el personaje con WASD y mirar con el mouse **para** tener un control FPS básico.

**Points**: 5  
**Depende de**: Story 2.1

**Tasks**:
- [ ] **T-3.1.1** Crear `Assets/FPSGame/Player/Infrastructure/PlayerInputActions.inputactions` con Action Map "Gameplay": Move (Vector2), Look (Vector2), Jump (Button), Run (Button/Hold), Crouch (Button/Toggle)
- [ ] **T-3.1.2** Crear `Assets/FPSGame/Player/Infrastructure/InputProvider.cs` implementando `IPlayerInput`. Leer input en `Update()`, exponer via propiedades
- [ ] **T-3.1.3** Registrar `IPlayerInput → InputProvider` en `GameBootstrap.cs`
- [ ] **T-3.1.4** Crear escena de test con un cubo que se mueva según input (verificación visual)
- [ ] **T-3.1.5** Escribir Play Mode test: input normalizado (magnitude ≤ 1)

**Acceptance Criteria**:
- [ ] WASD genera Vector2 normalizado
- [ ] Mouse delta genera Vector2 de look
- [ ] Space detecta JumpPressed en el frame correcto
- [ ] Shift detecta RunHeld mientras está presionado
- [ ] Ctrl togglea CrouchPressed
- [ ] Input debug log muestra valores correctos

**Archivos a crear**:
```
Assets/FPSGame/Player/Infrastructure/
├── PlayerInputActions.inputactions
└── InputProvider.cs
```

---

## Epic 4: Controller/Pawn (8 pts)

### Story 4.1: Implementar separación Controller/Pawn
**Como** desarrollador **quiero** que el PlayerController persista entre muerte/respawn y el Pawn sea destruible **para** mantener score/stats entre vidas.

**Points**: 5  
**Depende de**: Story 3.1

**Tasks**:
- [ ] **T-4.1.1** Crear `FPSGame/Player/Infrastructure/PlayerController.cs` (MonoBehaviour, `DontDestroyOnLoad`). Tiene referencia a InputProvider y al Pawn actual.
- [ ] **T-4.1.2** Crear `FPSGame/Player/Infrastructure/PlayerPawn.cs` (MonoBehaviour). Requiere `CharacterController`. Es poseído por el Controller.
- [ ] **T-4.1.3** Implementar mecanismo `Possess(PlayerPawn)` / `Unpossess()` en PlayerController
- [ ] **T-4.1.4** Crear prefab `PlayerPawn.prefab` con CharacterController, placeholder visual (cápsula)
- [ ] **T-4.1.5** Registrar PlayerController y PlayerPawn en GameBootstrap

**Acceptance Criteria**:
- [ ] PlayerController sobrevive scene reload
- [ ] PlayerPawn puede ser destruido y re-creado
- [ ] Possess/Unpossess funciona sin errores
- [ ] Input fluye de Controller → Pawn correctamente

**Jerarquía de escena**:
```
Scene:
├── [Bootstrap] GameBootstrap
├── [DontDestroyOnLoad] PlayerController
│   └── InputProvider
└── PlayerPawn (instanciable)
    ├── CharacterController
    ├── Camera (child GO)
    └── PlayerPawn.cs
```

---

### Story 4.2: Tests de Controller/Pawn
**Como** desarrollador **quiero** tests que validen la separación Controller/Pawn **para** evitar regressions.

**Points**: 3  
**Depende de**: Story 4.1

**Tasks**:
- [ ] **T-4.2.1** Edit Mode test: `PlayerController_Possess_SetsPawnReference`
- [ ] **T-4.2.2** Edit Mode test: `PlayerController_Unpossess_ClearsPawnReference`
- [ ] **T-4.2.3** Play Mode test: `PlayerController_SurvivesSceneReload`
- [ ] **T-4.2.4** Play Mode test: `PlayerPawn_CanBeDestroyedAndRecreated`

**Acceptance Criteria**:
- [ ] 4 tests pasan
- [ ] Tests usan `Injection.Reset()` en SetUp

---

## Sprint 1 — Definition of Done

- [ ] Estructura Three-Layer compilando sin errores
- [ ] Injector instalado y funcionando
- [ ] Interfaces de domain definidas (FPSGame.Player, SurvGame.Health)
- [ ] HealthEntity con 4 tests passing
- [ ] Input System configurado y respondiendo a WASD/Mouse/Space/Shift/Ctrl
- [ ] Controller/Pawn separados con possess/unpossess
- [ ] Controller persiste entre scene reloads
- [ ] Al menos 8 tests passing (4 health + 4 controller/pawn)
- [ ] Código documentado con XML comments
- [ ] Commits con formato `feat(fpsgame-player): ...`

---

**Sprint siguiente**: [Phase1-Sprint2-Backlog.md](Phase1-Sprint2-Backlog.md) — State Machine + Movement + Polish  
**Arquitectura de referencia**: [09-Three-Layer-Architecture.md](../Architecture/09-Three-Layer-Architecture.md)  
**DI reference**: [02-DI-Guidelines.md](../Standards/02-DI-Guidelines.md)
