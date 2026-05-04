# Phase 1 — Sprint 2: State Machine + Movement + Polish

**Sprint Goal**: Tener un personaje jugable con movimiento completo (walk, run, crouch, jump) usando state machine, coyote time, jump buffering, y tests ≥80% en domain.

**Duración**: 1 semana  
**Story Points totales**: 29  
**Depende de**: Sprint 1 completado

---

## Epic 5: State Machine (8 pts)

### Story 5.1: Implementar State Machine base
**Como** desarrollador **quiero** una state machine genérica y extensible **para** manejar los estados de movimiento del jugador de forma limpia.

**Points**: 3  
**Depende de**: Sprint 1 completado (interfaces, Controller/Pawn)

**Tasks**:
- [ ] **T-5.1.1** Crear `FPSGame/Player/Domain/StateMachine.cs` — clase pura. Mantiene estado actual, maneja transiciones, llama `Enter()`/`Exit()`/`Update()`/`FixedUpdate()`. Recibe `float deltaTime` por parámetro (no usa `Time.deltaTime`).
- [ ] **T-5.1.2** Crear `FPSGame/Player/Domain/BaseMovementState.cs` — clase abstracta implementando `IState`. Recibe `PlayerConfig` y `IPlayerInput` por constructor.
- [ ] **T-5.1.3** Implementar `IdleState` — speed = 0, transiciona a Walking cuando `MoveInput.magnitude > 0.1f`
- [ ] **T-5.1.4** Implementar `WalkingState` — speed = `WalkSpeed` (5 m/s), transiciona a Idle cuando input = 0, a Running cuando RunHeld
- [ ] **T-5.1.5** Edit Mode tests:
  - `StateMachine_StartsInInitialState`
  - `StateMachine_TransitionsCallEnterAndExit`
  - `IdleState_TransitionsToWalking_WhenInputDetected`
  - `WalkingState_TransitionsToIdle_WhenNoInput`
  - `WalkingState_TransitionsToRunning_WhenRunHeld`

**Acceptance Criteria**:
- [ ] StateMachine es pura (sin Unity API)
- [ ] Todos los estados reciben deltaTime por parámetro
- [ ] 5 tests pasan en Edit Mode
- [ ] StateMachine es fácil de extender (agregar estado = crear clase + registrar transición)

**Archivos a crear**:
```
Assets/FPSGame/Player/Domain/
├── StateMachine.cs
├── BaseMovementState.cs
├── States/
│   ├── IdleState.cs
│   └── WalkingState.cs
Assets/Tests/Editor/
└── StateMachineTests.cs
```

---

### Story 5.2: Implementar estados avanzados
**Como** jugador **quiero** poder correr, agacharme y saltar **para** tener un movimiento FPS completo.

**Points**: 5  
**Depende de**: Story 5.1

**Tasks**:
- [ ] **T-5.2.1** Implementar `RunningState` — speed = `WalkSpeed × RunSpeedMultiplier` (8 m/s), stamina drain, transiciona a Walking cuando suelta Run o sin stamina
- [ ] **T-5.2.2** Implementar `CrouchingState` — speed = `WalkSpeed × CrouchSpeedMultiplier` (2.5 m/s), reduce hitbox, toggle
- [ ] **T-5.2.3** Implementar `JumpingState` — aplica JumpForce (5 m/s), transiciona a Falling al alcanzar peak
- [ ] **T-5.2.4** Implementar `FallingState` — gravity (-9.81 m/s²), transiciona a Idle/Walking al tocar ground
- [ ] **T-5.2.5** Edit Mode tests:
  - `RunningState_UsesCorrectSpeed`
  - `RunningState_TransitionsToWalking_WhenRunReleased`
  - `CrouchingState_UsesHalfSpeed`
  - `JumpingState_TransitionsToFalling_AtPeak`
  - `FallingState_TransitionsToGrounded_OnLand`

**Acceptance Criteria**:
- [ ] Los 6 estados (Idle, Walking, Running, Crouching, Jumping, Falling) funcionan
- [ ] Speeds coinciden con los valores canónicos (5, 8, 2.5 m/s)
- [ ] 5 tests nuevos pasan
- [ ] Total: ≥10 tests de state machine

**Archivos a crear**:
```
Assets/FPSGame/Player/Domain/States/
├── RunningState.cs
├── CrouchingState.cs
├── JumpingState.cs
└── FallingState.cs
```

---

## Epic 6: Movement Implementation (13 pts)

### Story 6.1: Implementar PlayerMotor con CharacterController
**Como** jugador **quiero** que mi personaje se mueva físicamente en el mundo **para** explorar el entorno.

**Points**: 5  
**Depende de**: Story 5.2

**Tasks**:
- [ ] **T-6.1.1** Crear `FPSGame/Player/Infrastructure/PlayerMotor.cs` (MonoBehaviour). Usa `CharacterController.Move()`. Recibe velocidad del state machine. Ejecuta movement en `FixedUpdate()`.
- [ ] **T-6.1.2** Implementar ground detection: raycast hacia abajo (0.2m), slope detection (45° limit)
- [ ] **T-6.1.3** Implementar gravedad: acumular velocidad vertical, aplicar en `FixedUpdate()`
- [ ] **T-6.1.4** Crear `FPSGame/Player/Data/PlayerConfig.cs` (ScriptableObject) con todos los valores canónicos:
  - `WalkSpeed = 5f`
  - `RunSpeedMultiplier = 1.6f`
  - `CrouchSpeedMultiplier = 0.5f`
  - `JumpForce = 5f`
  - `Gravity = -9.81f`
  - `GroundCheckDistance = 0.2f`
  - `SlopeLimit = 45f`
  - `CoyoteTime = 0.1f`
  - `JumpBufferTime = 0.1f`
- [ ] **T-6.1.5** Crear `PlayerConfig.asset` en `FPSGame/Player/Data/` con los valores por defecto
- [ ] **T-6.1.6** Conectar state machine → PlayerMotor → CharacterController en PlayerPawn
- [ ] **T-6.1.7** Registrar dependencias en GameBootstrap

**Acceptance Criteria**:
- [ ] WASD mueve el personaje a 5 m/s
- [ ] Shift + WASD mueve a 8 m/s
- [ ] Ctrl togglea crouch a 2.5 m/s
- [ ] Space salta (solo cuando grounded)
- [ ] Gravedad funciona (caída, aterrizaje)
- [ ] No se puede subir slopes > 45°
- [ ] Input se lee en Update(), movement se aplica en FixedUpdate()

---

### Story 6.2: Coyote Time + Jump Buffering
**Como** jugador **quiero** poder saltar un poco después de dejar el borde y que mi input de salto se recuerde antes de aterrizar **para** que el movimiento se sienta responsivo.

**Points**: 3  
**Depende de**: Story 6.1

**Tasks**:
- [ ] **T-6.2.1** Implementar coyote time (100ms): si el jugador deja de estar grounded, tiene 100ms para saltar. El timer se pasa como parámetro (no `Time.deltaTime` directo en domain).
- [ ] **T-6.2.2** Implementar jump buffer (100ms): si el jugador presiona saltar antes de aterrizar, el salto se ejecuta al tocar el suelo.
- [ ] **T-6.2.3** Edit Mode tests:
  - `CoyoteTime_AllowsJump_WithinGracePeriod`
  - `CoyoteTime_DeniesJump_AfterGracePeriod`
  - `JumpBuffer_ExecutesJump_OnLanding`
  - `JumpBuffer_Expires_AfterBufferTime`

**Acceptance Criteria**:
- [ ] Puede saltar hasta 100ms después de dejar un borde
- [ ] Presionar saltar 100ms antes de aterrizar ejecuta el salto al aterrizar
- [ ] 4 tests pasan
- [ ] Valores configurables via PlayerConfig

---

### Story 6.3: Polish de movimiento
**Como** jugador **quiero** que el movimiento sea suave y sin glitches **para** que se sienta bien jugar.

**Points**: 5  
**Depende de**: Story 6.2

**Tasks**:
- [ ] **T-6.3.1** Implementar aceleración/deceleración suave (no instant 0→5 m/s). Usar lerp o acceleration curve.
- [ ] **T-6.3.2** Fix wall sliding: el personaje no debe quedarse pegado a paredes al moverse en diagonal
- [ ] **T-6.3.3** Fix corner catching: el personaje no debe trabarse en esquinas de geometría
- [ ] **T-6.3.4** Implementar transiciones suaves de velocidad entre estados (walk→run, run→crouch)
- [ ] **T-6.3.5** Playtest: verificar game feel completo con la playtest checklist:
  - [ ] Movimiento se siente responsivo (< 50ms de latencia percibida)
  - [ ] No hay jitter contra paredes
  - [ ] Las transiciones de velocidad son suaves
  - [ ] El salto se siente natural
  - [ ] Coyote time y jump buffer funcionan intuitivamente
  - [ ] No hay penetración de geometry

**Acceptance Criteria**:
- [ ] Movimiento suave sin acceleration instantánea
- [ ] Sin bugs de wall sliding o corner catching
- [ ] Playtest checklist completada

---

## Sprint 2 — Definition of Done

- [ ] State Machine funcional con 6 estados (Idle, Walking, Running, Crouching, Jumping, Falling)
- [ ] PlayerMotor mueve al personaje con CharacterController
- [ ] Todos los valores canónicos en PlayerConfig.asset (5 m/s, 1.6x, 0.5x, etc.)
- [ ] Coyote time (100ms) y jump buffer (100ms) implementados
- [ ] Ground detection via raycast
- [ ] Slope limit 45°
- [ ] Movimiento suave (acceleration, no jitter)
- [ ] ≥20 tests passing (acumulado de ambos sprints)
- [ ] ≥80% cobertura en domain layer
- [ ] Input en Update(), movement en FixedUpdate()
- [ ] Playtest checklist completada
- [ ] Código con XML comments
- [ ] Commits con formato `feat(fpsgame-player): ...`

---

## Phase 1 — Definition of Done (ambos sprints)

Cuando ambos sprints están completos, Phase 1 está terminada:
- [ ] Sprint 1 DoD completado
- [ ] Sprint 2 DoD completado
- [ ] Juego playable: caminar, correr, agacharse, saltar en una escena de test
- [ ] Documentación actualizada (INDEX.md, backlogs marcados)
- [ ] Branch mergeada a main

**Siguiente fase**: Phase 2 — Camera System (ver INDEX.md para roadmap)

---

**Sprint anterior**: [Phase1-Sprint1-Backlog.md](Phase1-Sprint1-Backlog.md)  
**Arquitectura**: [09-Three-Layer-Architecture.md](../Architecture/09-Three-Layer-Architecture.md)  
**Valores canónicos**: Ver tabla en [AGENTS.md](../../../../AGENTS.md)  
**Testing strategy**: [01-Phase1-Testing-Strategy.md](../Systems/01-Phase1-Testing-Strategy.md)
