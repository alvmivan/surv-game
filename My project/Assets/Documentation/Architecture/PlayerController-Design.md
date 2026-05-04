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

#### Fixed Timestep Updates (Guerrilla Games pattern)
```
Target: 15-30Hz for game logic (not render framerate)
Benefit: Deterministic behavior across all platforms
Implementation: Custom update loop or fixed delta time
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
│   └── FixedTimestepUpdater (15-30Hz)
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

### Movement State Machine (Fixed Timestep: 15-30Hz)
```
BaseState (executes at fixed frequency)
├── GroundedState
│   ├── IdleState (energy recovery)
│   ├── WalkingState (base speed)
│   ├── RunningState (stamina drain, 1.5x speed)
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
│   - Fixed-timestep logic (15-30Hz) │
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

### Performance Options (Unity DOTS)
For large-scale scenarios (1000+ entities):
- Use **ECS (Entity Component System)** for NPCs
- **CharacterControllerComponent** with Job System
- **Burst Compiler** for SIMD optimization
- Keep Player as MonoBehaviour (needs rich interaction)
- Convert NPCs to Entities for performance

## Extensibility Points (AAA-Style)

### Adding New States
1. Create new class implementing `IState`
2. Define transition conditions (with coyote/buffer support)
3. Register in `StateFactory` (ScriptableObject)
4. No modification to existing states needed
5. **Optional**: Add to `InputContext` if new inputs required

### Adding New View Modes
1. Implement `IViewMode` (with sensitivity multiplier)
2. Add to `ViewModeConfig` ScriptableObject
3. Inject via `IViewModeProvider`
4. Configure camera collision settings per mode

### Adding New Damage Types
1. Add enum value to `DamageType`
2. Create `DamageTypeData` ScriptableObject
3. Configure resistances in `PlayerConfig`
4. Add visual/sound feedback in `DamageFeedbackConfig`

### Adding New Environments
1. Add enum value to `EnvironmentType`
2. Create `EnvironmentData` ScriptableObject
3. Configure in `EnvironmentSystem`
4. Add surface detection for footstep sounds

### Adding New Input Devices
1. Create `IDeviceAdapter` implementation
2. Map device inputs to normalized actions (Vector2 move/look, buttons)
3. Configure response curves per device
4. Register in `InputRouter`

### Adding Multiplayer Support
1. `PlayerController` already persistent (good for networking)
2. Add `NetworkPlayerController` (syncs input to server)
3. Use `CharacterMovementComponent` pattern (server-authoritative)
4. Client-side prediction with server reconciliation

## Events and Messaging

### Domain Events
```csharp
- PlayerDamagedEvent(DamageData, remainingHealth)
- PlayerHealedEvent(float amount, totalHealth)
- MovementStateChangedEvent(MovementState old, MovementState new)
- ViewModeChangedEvent(IViewMode old, IViewMode new)
- EnvironmentChangedEvent(EnvironmentType new)
- InventoryChangedEvent(ItemData, int amount)
```

### Event Bus (ScriptableObject)
- `GameEvent<T>` : generic event asset
- `EventBus` : centralized or distributed
- Listeners subscribe via `IEventListener<T>`

## Testing Strategy

### Unit Tests
- Domain logic (damage calculation, state transitions)
- Data validation (ScriptableObject integrity)

### Integration Tests
- Player movement in different environments
- Camera mode switching
- Interaction systems

### Play Mode Tests
- Full player controller scenarios
- Performance benchmarks

## Performance Considerations

### Data-Oriented Design
- Use `ScriptableObject` references, not copies
- Cache component references
- Object pooling for projectiles/effects

### Update Optimization
- State machine only updates active state
- Camera updates only when view mode changes
- Environment checks on trigger/interval, not per frame

## Next Steps (AAA Production Pipeline)

1. **Phase 1**: Core architecture + movement (2 weeks)
   - PlayerController/Pawn separation
   - Input system with device adapters
   - Fixed-timestep movement state machine
   - Ground detection + coyote time
   - **Playtest**: Movement feel, responsiveness

2. **Phase 2**: Camera system (1 week)
   - View mode framework (Strategy pattern)
   - First person implementation
   - Camera collision + smoothing
   - Head bob + injury sway
   - **Playtest**: Camera comfort, motion sickness

3. **Phase 3**: Interaction systems (2 weeks)
   - Tree cutting (with tool effectiveness)
   - Combat foundation (melee/ranged)
   - Item pickup + inventory integration
   - **Playtest**: Combat feel, attack timing

4. **Phase 4**: Survival systems (2 weeks)
   - Health/stamina/injury systems
   - Environment detection
   - Weather integration
   - **Playtest**: Survival balance, difficulty

5. **Phase 5**: Polish + feel (1 week)
   - Response curves + aim assist
   - Input buffering + coyote time tuning
   - Camera effects (landing rumble, damage punch)
   - Footstep sounds per surface
   - **Playtest**: Overall game feel, accessibility

6. **Phase 6** (Optional): Multiplayer prep
   - Network architecture planning
   - Server-authoritative movement
   - Client-side prediction

## Dependencies

### Unity Packages
- Input System (1.7+) - Device abstraction, context layers
- Cinemachine (3.0+) - Camera modes, collision, noise
- State Machine (custom or Asset Store)
- **Optional for Performance**: Entities, Jobs, Burst (for 1000+ NPCs)

### Project Dependencies
- Inventory System (to be designed)
- Item System (ScriptableObject-based)
- UI System (for player HUD)
- Audio System (footsteps, combat, environment)
- Save System (persist PlayerController data)

## References

### AAA Games Studied
- **Unreal Engine 5**: Controller/Pawn separation, CharacterMovementComponent
- **V Rising**: DOTS for survival game (1000+ entities)
- **Hardspace: Shipbreaker**: DOTS for physics-heavy gameplay
- **The Last of Us**: Injury systems, character feel
- **Unity DOTS Sample**: ECS character controller patterns

### Key Insights Applied
1. **Fixed timestep** (15-30Hz) for deterministic movement
2. **Input abstraction** for device-agnostic controls
3. **Coyote time + buffering** for forgiving platforming
4. **Controller/Pawn split** for persistence through death
5. **3Cs separation** for maintainable code
6. **DOTS optional** for performance-critical scenarios

---

**Document Version**: 1.0  
**Last Updated**: 2026-05-04  
**Author**: Architecture Team  
**Status**: Draft - Ready for Review
