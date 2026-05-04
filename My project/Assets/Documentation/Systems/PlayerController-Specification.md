# Player Controller - System Specification

## System Purpose
Control all player-related behavior in a First Person Survival game, including movement, camera, interactions, and environmental responses.

## Functional Requirements

### FR-001: Player Movement
**Description**: Player can move in 3D space with multiple movement states.

**States**:
- **Idle**: Zero velocity, full control recovery
- **Walking**: Base speed (3-5 m/s), low stamina drain
- **Running**: 1.5x base speed, medium stamina drain
- **Crouching**: 0.5x base speed, no stamina drain, reduces hitbox
- **Jumping**: Vertical impulse, horizontal momentum preserved
- **Falling**: Gravity applied, air control available
- **Swimming**: Buoyancy applied, different speed curve
- **Climbing**: Vertical movement only, no gravity

**Acceptance Criteria**:
- [ ] Ground detection via raycast (0.2m distance)
- [ ] Slope limit: 45 degrees maximum
- [ ] Stamina drains when running, regenerates when walking/idle
- [ ] Crouch toggles with smooth height transition (0.5s)
- [ ] Jump only available when grounded
- [ ] Fall damage applied if falling > 5m

### FR-002: Camera System
**Description**: First-person camera with multiple view modes.

**View Modes**:
- **FirstPerson**: Standard FPS view, camera at eye level
- **ThirdPerson**: Behind-player view, for inspection
- **Scope**: Reduced FOV (20°), weapon aligned
- **Binoculars**: Reduced FOV (10°), no weapon
- **Mounted**: Fixed position (turrets, vehicles)

**Camera Features**:
- Mouse look with sensitivity curve
- View bobbing when moving
- Head bob animation
- Injury sway (if health < 30%)
- Smooth transitions between modes (0.3s)
- FOV changes with sprint (optional)

**Acceptance Criteria**:
- [ ] Mouse sensitivity configurable (X/Y independent)
- [ ] Invert Y option
- [ ] FOV range: 60° - 120°
- [ ] View mode switching < 0.5s
- [ ] Camera collision detection (walls)
- [ ] No camera clipping through geometry

### FR-003: Interaction System
**Description**: Player can interact with the world.

**Interactions**:
- **Tree Cutting**: 
  - Chop animation (1.5s)
  - Tool effectiveness (axe > knife > bare hands)
  - Tree health depletion
  - Fall direction calculation
  - Resource drop on fall

- **Combat**:
  - Melee attacks (light/heavy)
  - Ranged attacks (bow/gun)
  - Different damage types
  - Hit detection (raycast)
  - Stamina cost per attack

- **Item Pickup**:
  - Raycast detection (2m range)
  - Inventory integration
  - Prompt display ("Press E to pick up")

**Acceptance Criteria**:
- [ ] Tree cutting progresses with visual feedback
- [ ] Attack cooldown prevents spamming
- [ ] Hit markers for successful attacks
- [ ] Different tools have different speeds
- [ ] Trees fall away from player

### FR-004: Injury System
**Description**: Player can receive injuries that affect performance.

**Body Parts**:
- Head (critical: vision impairment)
- Torso (critical: health drain)
- Left Arm (critical: can't use tools/weapons)
- Right Arm (critical: can't use tools/weapons)
- Left Leg (critical: 50% speed reduction)
- Right Leg (critical: 50% speed reduction)

**Injury Severity**:
- **Minor**: 10% speed penalty, no visual effect
- **Moderate**: 25% speed penalty, slight limp
- **Severe**: 50% speed penalty, visible injury, stamina drain
- **Critical**: 75% speed penalty, severe limp, bleeding (health drain)

**Acceptance Criteria**:
- [ ] Injuries persist until healed (bed/rest)
- [ ] Multiple injuries stack effects
- [ ] Visual feedback (blood particles, limp animation)
- [ ] Audio feedback (grunts, heavy breathing)
- [ ] UI indicator for each injured body part

### FR-005: Damage System
**Description**: Player receives damage from various sources.

**Damage Types**:
- **Physical**: Reduced by armor
- **Blunt**: Causes stagger, reduced by armor
- **Slashing**: Causes bleeding, reduced by armor
- **Piercing**: Ignores some armor, reduced by armor
- **Environmental**: Fire, poison, no armor reduction
- **Fall**: Calculated from fall distance

**Damage Calculation**:
```
FinalDamage = (BaseDamage * DamageMultiplier) - ArmorReduction
ArmorReduction = ArmorValue * DamageTypeResistance
```

**Acceptance Criteria**:
- [ ] Damage numbers displayed (UI)
- [ ] Hit direction indicator (where damage came from)
- [ ] Invincibility frames (0.5s) after taking damage
- [ ] Death triggers respawn sequence
- [ ] Different death animations by damage type

### FR-006: Environment System
**Description**: Player responds to different environments.

**Environments**:
- **Ground**: Standard movement
- **Water (Shallow)**: Slowed movement (0.7x)
- **Water (Deep)**: Swimming state
- **Underwater**: Oxygen system, blurred vision
- **Climbing**: Wall/floor transition
- **Indoors**: Reverb on sounds

**Environmental Effects**:
- Temperature affects stamina regen
- Rain reduces visibility, sound dampening
- Snow reduces movement speed
- Swamp slows movement, health drain

**Acceptance Criteria**:
- [ ] Environment detection via triggers/raycasts
- [ ] Visual feedback (water splashes, snow particles)
- [ ] Audio feedback (footstep sounds change)
- [ ] UI indicators (oxygen bar, temperature gauge)

### FR-007: Stamina System
**Description**: Player has stamina that limits actions.

**Stamina Drains**:
- Running: 10 units/second
- Jumping: 20 units
- Attacking: 15 units (melee), 5 units (ranged)
- Tree Cutting: 8 units/second
- Swimming: 12 units/second

**Stamina Regen**:
- Idle: 15 units/second
- Walking: 10 units/second
- Crouching: 12 units/second
- Exhausted (0 stamina): 5 units/second (penalty)

**Acceptance Criteria**:
- [ ] Stamina bar visible in HUD
- [ ] Actions disabled when stamina < required amount
- [ ] Visual feedback when low stamina (screen edge pulse)
- [ ] Exhaustion state (can't run for 3s after reaching 0)

## Non-Functional Requirements

### NFR-001: Performance
- Target: 60 FPS on minimum spec
- Player update logic < 2ms per frame
- Maximum 3 state transitions per second
- Camera updates < 1ms per frame

### NFR-002: Extensibility
- Add new states without modifying existing code
- Add new view modes via new class + config
- Add new damage types via enum + ScriptableObject
- Add new environments via config

### NFR-003: Testability
- All domain logic unit testable
- State machine testable in isolation
- Mock interfaces for all dependencies
- Minimum 80% code coverage on domain layer

### NFR-004: Usability
- Input rebinding supported
- All actions accessible via keyboard/mouse
- Clear feedback for all actions
- No input lag > 50ms

## System Interfaces

### Public Interfaces
```csharp
// Player Controller Facade
interface IPlayerController
{
    void Move(Vector2 input);
    void Jump();
    void Crouch();
    void SwitchViewMode(ViewMode mode);
    void Attack(AttackType type);
    void Interact();
    Vector3 Position { get; }
    Quaternion Rotation { get; }
}

// Input Provider
interface IPlayerInput
{
    Vector2 MoveInput { get; }
    Vector2 LookInput { get; }
    bool JumpPressed { get; }
    bool CrouchHeld { get; }
    bool FirePressed { get; }
    bool InteractPressed { get; }
}

// Damage Receiver
interface IDamageable
{
    void TakeDamage(DamageData damage);
    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsDead { get; }
}

// Environment Detector
interface IEnvironmentDetector
{
    EnvironmentType CurrentEnvironment { get; }
    bool IsGrounded { get; }
    float WaterDepth { get; }
    float Temperature { get; }
}
```

## Data Models

### PlayerSaveData (for persistence)
```csharp
[System.Serializable]
public class PlayerSaveData
{
    public float Health;
    public float Stamina;
    public Vector3 Position;
    public Quaternion Rotation;
    public List<InjurySaveData> Injuries;
    public string CurrentEnvironment;
}

[System.Serializable]
public class InjurySaveData
{
    public BodyPartType BodyPart;
    public InjurySeverity Severity;
    public float HealingProgress;
}
```

## Dependencies

### Required Systems
- Inventory System (for item usage)
- Item System (for tools/weapons)
- UI System (for HUD)
- Audio System (for footsteps, attacks)
- Save System (for persistence)

### Required Unity Packages
- Input System 1.7+
- Cinemachine 3.0+

## Acceptance Test Scenarios

### Scenario 1: Basic Movement
1. Start game
2. Press W → Player moves forward
3. Press Shift+W → Player runs
4. Press Space → Player jumps
5. Press Ctrl → Player crouches
6. **Expected**: Smooth transitions, correct speeds

### Scenario 2: Tree Cutting
1. Equip axe
2. Approach tree
3. Press left mouse button
4. Hold button for 5 seconds
5. **Expected**: Tree falls, logs appear, stamina drains

### Scenario 3: Injury and Healing
1. Take fall damage (jump from height)
2. Leg injury applied
3. Movement speed reduced
4. Rest in bed for 5 minutes
5. **Expected**: Injury healed, speed restored

### Scenario 4: View Mode Switching
1. Press "V" key
2. Cycle through view modes
3. **Expected**: Smooth transitions, camera position updates

### Scenario 5: Underwater Survival
1. Dive into deep water
2. Oxygen bar appears
3. Oxygen depletes over time
4. Surface before empty
5. **Expected**: Breathing sound, health drain if no oxygen

## Open Questions

- [ ] Should player be able to swim backward?
- [ ] How many inventory slots for quick access?
- [ ] Should different shoes affect movement speed?
- [ ] Should weather affect stamina regen rate?
- [ ] Should injured arms affect tool effectiveness?

---

**Version**: 1.0  
**Status**: Draft  
**Priority**: High  
**Estimated Effort**: 6 weeks (full implementation)
