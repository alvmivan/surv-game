# Player Controller - Design Document

## Overview
First Person Survival game player controller designed for large-scale development using DDD, SOLID principles, and moderate Clean Architecture.

Based on AAA industry practices from Unreal Engine, Unity DOTS, and survival games like V Rising, Hardspace: Shipbreaker, and The Last of Us.

## Architecture Principles

### AAA Industry Patterns (Unreal Engine, Unity DOTS)

#### Controller/Pawn Separation (Unreal Engine Pattern)
```
PlayerController (persistent, manages input, score, state)
    ↓ possesses
PlayerPawn (transient, can be replaced on death/respawn)
    ├── PlayerMotor (movement logic)
    ├── PlayerCamera (view handling)
    └── PlayerHealth (health/damage)
```
**Benefits**: Controller persists through death/respawn cycles, enables AI/Player swapping, supports multiple characters.

#### 3Cs Architecture (Controls, Camera, Character)
- **Controls**: Input processing, response curves, device abstraction
- **Camera**: View modes, collision, effects
- **Character**: Physical representation, animation, collision

#### Fixed Timestep Updates
```
Player movement: Unity FixedUpdate at 50Hz (default Time.fixedDeltaTime = 0.02s)
Non-critical systems (AI, environment): 15-30Hz custom tick (optional optimization)
Input sampling: Update() (every frame) — stored and consumed in FixedUpdate
Benefit: Deterministic physics, framerate-independent movement
```

### DDD (Domain-Driven Design)
- **Domain Layer**: Core business logic and rules (frame-rate independent)
- **Data Layer**: ScriptableObjects as ubiquitous language carriers
- **Infrastructure Layer**: Unity-specific implementations
- **Presentation Layer**: User input and visual feedback

### SOLID Principles Applied
- **S**: Single Responsibility - Each class has one reason to change
- **O**: Open/Closed - Open for extension, closed for modification
- **L**: Liskov Substitution - Derived classes replace base classes
- **I**: Interface Segregation - Client-specific interfaces
- **D**: Dependency Inversion - Depend on abstractions, not concretions

## Domain Model

### Core Entities

#### PlayerController (Persistent)
```
PlayerController (survives death/respawn)
├── Score/Stats (persistent across lives)
├── InputMapping (device-specific configurations)
├── UnlockedAbilities (persistent unlocks)
└── PossessedPawn (reference to current PlayerPawn)
```

#### PlayerPawn (Transient - can be destroyed/respawned)
```
PlayerPawn (the physical representation)
├── PlayerMotor (movement logic)
├── PlayerCamera (view handling)
├── PlayerHealth (health/damage system)
├── PlayerStamina (stamina system)
├── PlayerInventory (inventory reference)
├── InjurySystem (body parts, severity, effect multipliers)
└── EnvironmentDetector (current environment state)
```

#### PlayerStats (ScriptableObject)
```yaml
- MoveSpeed: float (base speed)
- RunSpeedMultiplier: float
- CrouchSpeedMultiplier: float
- JumpForce: float
- StaminaDrainRate: float
- HealthRegenRate: float
- InjurySpeedPenalty: float (per body part)
```

### Value Objects

#### DamageData
```
DamageData
├── Amount: float
├── Type: DamageType (Physical, Blunt, Slashing, Piercing, Environmental, Fall)
├── Direction: Vector3
├── HitPoint: Vector3
└── BodyPart: BodyPartType (Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg)
```

#### InjuryData (ScriptableObject)
```
InjuryData
├── BodyPart: BodyPartType
├── Severity: InjurySeverity (Minor, Moderate, Severe, Critical)
├── SpeedMultiplier: float
├── StaminaMultiplier: float
├── DamageMultiplier: float
└── VisualEffect: GameObject (optional)
```

## ScriptableObject Architecture

### Configuration Assets

#### PlayerConfig
- Base stats and multipliers
- Physics parameters
- Camera settings per state

#### MovementConfig
- Speed curves per terrain type
- Acceleration/Deceleration values
- Jump height and gravity modifiers

#### DamageTypeConfig
- Resistance calculations
- Visual feedback mappings
- Sound effect mappings

#### EnvironmentConfig
- Water physics parameters
- Climbing settings
- Terrain interaction values

## Component Architecture

### AAA-Style Separation (Controller/Pawn Pattern)

```
PlayerController (Persistent GameObject)
├── InputRouter (manages input contexts)
│   ├── GameplayInputContext
│   ├── UIInputContext
│   └── CinematicInputContext
│
├── PlayerInputHandler
│   └── Interfaces: IPlayerInput, IDeviceAdapter
│
└── Score/Stats (persistent data)

PlayerPawn (Transient - respawns)
├── PlayerMotor
│   ├── IMovable
│   ├── IJumpable
│   ├── ICrouchable
│   └── FixedTimestepUpdater (50Hz via FixedUpdate)
│
├── PlayerCamera
│   ├── ICameraController
│   ├── IViewModeSwitcher
│   ├── IViewBobber (head bob, injury sway)
│   └── ViewModes: [FirstPerson, ThirdPerson, Scope, Binoculars]
│
├── PlayerHealth
│   ├── IDamageable
│   ├── IHealable
│   └── IInjurable
│
├── PlayerStamina
│   └── IStaminaController
│
├── PlayerInteraction
│   ├── IInteractable
│   ├── ITreeCuttable
│   └── IAttacker
│
└── PlayerEnvironment
    ├── IEnvironmentDetector
    └── IWeatherAffected
```

### Input System Architecture (AAA Pattern)

```
InputLayer (Hardware)
    ↓
DeviceAdapter (abstracts keyboard/mouse/gamepad)
    ↓
InputRouter (context switching: Gameplay/UI/Cinematic)
    ↓
InputProcessor (response curves, dead zones, filtering)
    ↓
CommandGenerator (translates input to semantic actions)
    ↓
PlayerController (executes commands via Pawns)
```

**Key Features**:
- **Device Adapters**: Normalize input across keyboard, mouse, gamepad, touch
- **Context Layers**: Gameplay, UI, Cinematic, PhotoMode (only active layer processes input)
- **Response Curves**: Customizable input sensitivity (precision at low input, speed at high input)
- **Input Buffering**: Coyote time (jump within 100ms of leaving ledge)
- **Aim Assist**: Friction and magnetism for gamepad aiming

## Camera System Design (3Cs - Camera)

### View Modes (Strategy Pattern)
```csharp
interface IViewMode
{
    void Enter();
    void Exit();
    void Update();
    float FieldOfView { get; }
    Vector3 Offset { get; }
    float SensitivityMultiplier { get; }
}

// Implementations:
- FirstPersonView : IViewMode (standard FPS, eye level)
- ThirdPersonView : IViewMode (inspection, rear view)
- ScopeView : IViewMode (reduced FOV 20°, weapon aligned)
- BinocularView : IViewMode (FOV 10°, no weapon)
- MountedView : IViewMode (turrets, vehicles)
- PhotoModeView : IViewMode (free camera, freeze time)
```

### Camera Feel Systems (AAA Techniques)
- **Head Bob**: Vertical/horizontal oscillation based on movement speed
- **View Punch**: Recoil, impact feedback (instant offset, smooth return)
- **Injury Sway**: Unstable camera when health < 30% or injured
- **Landing Rumble**: Camera shake on hard landings
- **Breathing**: Subtle idle oscillation (more pronounced when injured)
- **Collision**: Camera pushes away from walls (sphere cast)
- **Smoothing**: Lerp between states (0.3s transition time)

### Camera States
- **Normal**: Standard FPS view
- **Aiming**: Reduced FOV, weapon alignment
- **Injured**: Sway, blur, reduced stability
- **Underwater**: Distortion, bubbles
- **Environmental**: Rain droplets, dust particles

## State Machine Design (with AAA Enhancements)

### Movement State Machine (Fixed Timestep: 50Hz via FixedUpdate)
```
BaseState (executes at fixed frequency)
├── GroundedState
│   ├── IdleState (energy recovery)
│   ├── WalkingState (base speed)
│   ├── RunningState (stamina drain, 1.6x speed)
│   └── CrouchingState (0.5x speed, smaller hitbox)
├── AirborneState
│   ├── JumpingState (ascending, control available)
│   ├── FallingState (descending, air control)
│   └── LedgeGrabbingState (mantle/climb)
├── SwimmingState
│   ├── TreadingWaterState (head above water)
│   ├── SwimmingState (arms only, slow)
│   └── UnderwaterState (oxygen drain, blurred vision)
├── InteractionState
│   ├── TreeCuttingState (tool-based, progress tracking)
│   ├── AttackingState (melee/ranged cooldowns)
│   └── ClimbingState (vertical only, no gravity)
└── VehicleState (mounted turrets, vehicles)
```

### State Transitions with AAA Features
- **Coyote Time**: 100ms grace period after leaving ground (can still jump)
- **Jump Buffering**: Queue jump input 100ms before landing (executes on landing)
- **Auto-Step**: Automatically vault over small obstacles (height < 0.5m)
- **Slope Limit**: Maximum 45° incline (slide if steeper)
- **Conditions**: Evaluated via `ITransitionCondition`
- **State Interface**: Each state implements `IState` with Enter/Exit/Update
- **Communication**: States communicate via `IStateEventBus`

## Interaction System

### Tree Cutting
```
ITreeCutter
├── Chop(Durability, ToolType)
├── GetProgress(): float
└── OnTreeFallen event

TreeData (ScriptableObject)
├── Health: float
├── ResourceYield: ItemData[]
├── FallDirection: calculated
└── ChopTime: float
```

### Combat System
```
IAttackable
├── MeleeAttack(WeaponData)
├── RangedAttack(ProjectileData)
└── HeavyAttack(WeaponData)

IDamageReceiver
├── ReceiveDamage(DamageData)
├── ApplyInjury(InjuryData)
└── CalculateDamageReduction(DamageType): float
```

## Environment Integration

### Environment Detector
```
IEnvironmentDetector
├── CurrentEnvironment: EnvironmentType
├── IsGrounded: bool
├── WaterDepth: float
├── Temperature: float
└── WeatherEffect: WeatherData

EnvironmentType
- Forest
- Desert
- Snow
- Swamp
- Indoors
- Underwater
```

## Data Flow (Clean Architecture + AAA Patterns)

```
┌─────────────────────────────────────┐
│   Presentation Layer (Unity)         │
│   - Input System (Device Adapters)   │
│   - Camera Rendering (Cinemachine)   │
│   - UI Feedback (HUD, Damage Numbers)│
└──────────────┬──────────────────────┘
               │ Input Commands
┌──────────────▼──────────────────────┐
│   Application Layer (Use Cases)      │
│   - MovePlayerUseCase (with coyote) │
│   - AttackUseCase (with buffering)   │
│   - ChangeViewModeUseCase           │
│   - TakeDamageUseCase (knockback)   │
└──────────────┬──────────────────────┘
               │ Domain Events
┌──────────────▼──────────────────────┐
│   Domain Layer (Entities)           │
│   - PlayerPawn entity              │
│   - Fixed-timestep logic (50Hz)    │
│   - Movement rules (deterministic)  │
│   - Damage calculations             │
│   - State transitions               │
└──────────────┬──────────────────────┘
               │ Config References
┌──────────────▼──────────────────────┐
│   Data Layer (ScriptableObjects)     │
│   - PlayerConfig (base stats)       │
│   - InputConfig (curves, deadzones) │
│   - WeaponData                      │
│   - EnvironmentData                 │
│   - DamageTypeData                  │
└─────────────────────────────────────┘
```

## Extensibility, Events, Roadmap & Dependencies

Ver **[PlayerController-Extensibility.md](PlayerController-Extensibility.md)** para:
- Extensibility points (agregar estados, view modes, damage types, environments, input devices, multiplayer)
- Events and messaging (domain events, event bus)
- Testing strategy summary
- Performance considerations (Data-Oriented Design, DOTS)
- Roadmap de fases (Phase 1-6)
- Unity packages y project dependencies

## References

### AAA Games Studied
- **Unreal Engine 5**: Controller/Pawn separation, CharacterMovementComponent
- **V Rising**: DOTS for survival game (1000+ entities)
- **Hardspace: Shipbreaker**: DOTS for physics-heavy gameplay
- **The Last of Us**: Injury systems, character feel
- **Unity DOTS Sample**: ECS character controller patterns

### Key Insights Applied
1. **Fixed timestep** (50Hz via Unity FixedUpdate) for deterministic movement
2. **Input abstraction** for device-agnostic controls
3. **Coyote time + buffering** for forgiving platforming
4. **Controller/Pawn split** for persistence through death
5. **3Cs separation** for maintainable code
6. **DOTS optional** for performance-critical scenarios

### URLs de referencia
- **Gaffer on Games — "Fix Your Timestep!"**: https://gafferongames.com/post/fix_your_timestep/ — Patrón de fixed timestep con accumulator. Nuestro approach usa el FixedUpdate de Unity (50Hz) que implementa este patrón internamente.
- **John Austin — "Fix your (Unity) Timestep!"**: https://johnaustin.io/articles/2019/fix-your-unity-timestep — Análisis detallado de cómo Unity implementa FixedUpdate a 50Hz y Update a 60Hz. Confirma que input debe leerse en Update() y consumirse en FixedUpdate().
- **Unity Manual — Fixed Updates**: https://docs.unity3d.com/6000.3/Documentation/Manual/fixed-updates.html — Documentación oficial de Unity sobre FixedUpdate y Time.fixedDeltaTime.
- **GameDeveloper — CharacterController vs Rigidbody**: https://www.gamedeveloper.com/programming/unity-character-controller-vs-rigidbody — Comparación de approaches para movimiento en Unity. CharacterController es preferido para FPS por no depender del physics tick para movimiento básico.
- **Unreal Engine — Pawn documentation**: https://dev.epicgames.com/documentation/en-us/unreal-engine/pawn-in-unreal-engine — Referencia del patrón Controller/Pawn de Unreal en el que se basa nuestra separación.
- **Unreal Engine — Parrot sample (Pawn + CharacterMovement)**: https://dev.epicgames.com/documentation/en-us/unreal-engine/parrot-pawn-player-controller-and-character-movement-in-unreal-engine — Ejemplo oficial de cómo Unreal separa Controller, Pawn y CharacterMovementComponent.

---

**Version**: 1.2 | **Updated**: 2026-05-04 | **Status**: Draft - Ready for Review
