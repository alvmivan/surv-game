# Phase 1: Core Architecture + Movement - Development Plan

## Overview
**Duration**: 2 weeks  
**Goal**: Implement foundational player controller architecture with responsive movement system.

## Prerequisites
- [ ] Unity 6000.2.6f2 or newer
- [ ] Input System package (1.7+)
- [ ] Cinemachine package (3.0+)
- [ ] Git repository initialized

## Week 1: Core Architecture

### Day 1-2: Project Structure
**Tasks**:
1. Create folder structure (Domain, Application, Infrastructure, Data, Presentation)
2. Set up Assembly Definitions (.asmdef) for each layer
3. Create base interfaces in Domain layer

**Files to Create**:
```
Assets/
├── Domain/
│   ├── Interfaces/
│   │   ├── IPlayerInput.cs
│   │   ├── IMovable.cs
│   │   ├── IJumpable.cs
│   │   └── IState.cs
│   └── Entities/
│       └── MovementState.cs (enum)
├── Infrastructure/
│   └── Player/
│       └── (empty, for Day 3)
└── Data/
    └── Players/
        └── PlayerConfig.asset (via ScriptableObject)
```

**Acceptance Criteria**:
- [ ] Folder structure matches architecture diagram
- [ ] Assembly definitions allow compilation
- [ ] Interfaces are empty (no implementation yet)

---

### Day 3-4: Input System
**Tasks**:
1. Create `PlayerInputActions.inputactions` asset
2. Configure Action Maps: Gameplay, UI, Cinematic
3. Set up bindings: WASD, Mouse Delta, Space, Shift, Ctrl
4. Implement `InputProvider.cs` with `IPlayerInput`

**Files to Create**:
```
Infrastructure/
└── Player/
    ├── InputProvider.cs
    └── InputRouter.cs (optional for now)
```

**Input Actions Structure**:
```yaml
ActionMaps:
  - Gameplay (default active)
    - Move: Vector2 (WASD, Left Stick)
    - Look: Vector2 (Mouse Delta, Right Stick)
    - Jump: Button (Space, A button)
    - Run: Button (Shift, B button) - with Hold
    - Crouch: Button (Ctrl, X button) - with Toggle
  - UI (disabled)
  - Cinematic (disabled)
```

**Acceptance Criteria**:
- [ ] Input Actions asset created and configured
- [ ] WASD moves character in test scene
- [ ] Mouse look rotates camera
- [ ] Input debug logs show correct values

---

### Day 5: Controller/Pawn Separation
**Tasks**:
1. Create `PlayerController.cs` (persistent, empty GameObject)
2. Create `PlayerPawn.cs` (transient, with visuals)
3. Implement possession mechanism
4. Set up communication between Controller ↔ Pawn

**Files to Create**:
```
Infrastructure/
└── Player/
    ├── PlayerController.cs
    ├── PlayerPawn.cs
    └── PawnPossessor.cs (handles possess/unpossess)
```

**Hierarchy**:
```
Scene:
├── PlayerController (GameObject, persists)
│   └── InputProvider.cs
└── PlayerPawn (GameObject, can be destroyed)
    ├── Camera (child)
    ├── CharacterController (component)
    └── PlayerPawn.cs
```

**Acceptance Criteria**:
- [ ] PlayerController survives scene reload
- [ ] PlayerPawn can be possessed/unpossessed
- [ ] Multiple pawns can be created/destroyed

---

## Week 2: Movement System

### Day 6-7: State Machine
**Tasks**:
1. Implement `StateMachine.cs` base class
2. Create `BaseMovementState.cs` abstract class
3. Implement first state: `IdleState`
4. Implement `WalkingState`

**Files to Create**:
```
Domain/
└── States/
    ├── StateMachine.cs
    ├── BaseMovementState.cs
    └── States/
        ├── IdleState.cs
        └── WalkingState.cs
```

**State Interface**:
```csharp
public interface IState
{
    void Enter();
    void Exit();
    void Update(float deltaTime);
}
```

**Acceptance Criteria**:
- [ ] State machine can switch states
- [ ] Idle → Walking transition works (input > 0)
- [ ] Walking → Idle transition works (input = 0)
- [ ] States call Enter/Exit/Update correctly

---

### Day 8-9: Movement Implementation
**Tasks**:
1. Implement `PlayerMotor.cs` with CharacterController
2. Add `RunningState` and `CrouchingState`
3. Implement ground detection (raycast)
4. Add gravity and jumping

**Files to Update**:
```
Infrastructure/Player/
├── PlayerMotor.cs (NEW - handles movement)
└── States/
    ├── RunningState.cs
    ├── CrouchingState.cs
    └── JumpingState.cs (basic)
```

**Movement Parameters** (in PlayerConfig.asset):
- WalkSpeed: 5 m/s
- RunSpeed: 8 m/s (1.6x multiplier)
- CrouchSpeed: 2.5 m/s (0.5x multiplier)
- JumpForce: 5 m/s
- Gravity: -9.81 m/s²
- GroundCheckDistance: 0.2m
- SlopeLimit: 45°

**Acceptance Criteria**:
- [ ] WASD moves character at walk speed
- [ ] Shift + WASD runs at run speed
- [ ] Ctrl toggles crouch with speed change
- [ ] Space jumps (only when grounded)
- [ ] Character slides on slopes > 45°
- [ ] Gravity works (falling, landing)

---

### Day 10: Coyote Time + Polish
**Tasks**:
1. Implement coyote time (100ms grace period after leaving ground)
2. Add jump input buffering (queue jump before landing)
3. Smooth speed transitions (acceleration/deceleration)
4. Fix edge cases (wall sliding, corner catching)

**Coyote Time Logic**:
```csharp
private float _coyoteTimer = 0f;
private bool CanJump => IsGrounded || _coyoteTimer > 0f;

void Update()
{
    if (!IsGrounded)
    {
        _coyoteTimer -= Time.deltaTime;
    }
    else
    {
        _coyoteTimer = COYOTE_TIME; // 0.1f
    }
}
```

**Acceptance Criteria**:
- [ ] Can jump 100ms after walking off ledge
- [ ] Jump input 100ms before landing executes on landing
- [ ] No jitter when moving against walls
- [ ] Smooth acceleration (no instant 0→5 m/s)

---

## Implementation Order Summary

```
Day 1-2:   Folder structure + Interfaces
Day 3-4:   Input System (PlayerInputActions + InputProvider)
Day 5:      Controller/Pawn separation
Day 6-7:    State Machine (Idle, Walking)
Day 8-9:    Movement (Running, Crouching, Jumping, Gravity)
Day 10:     Coyote Time + Polish
```

## Daily Workflow
1. **Morning**: Review previous day's work
2. **Develop**: Implement tasks for the day
3. **Test**: Run test scenarios (see Testing document)
4. **Commit**: Git commit with descriptive message
5. **Document**: Update progress in this file

## Git Commit Convention
```
feat(architecture): add domain layer interfaces
feat(input): implement PlayerInputActions asset
feat(input): add InputProvider with IPlayerInput
feat(player): create PlayerController and PlayerPawn
feat(state-machine): implement base StateMachine class
feat(movement): add Idle and Walking states
feat(movement): implement running and crouching
feat(movement): add jumping with coyote time
test(movement): add unit tests for state transitions
```

## Risk Mitigation
| Risk | Mitigation |
|------|-------------|
| Input System too complex | Start with simple GetAxis, upgrade later |
| CharacterController issues | Test with both CC and Rigidbody |
| State machine bugs | Unit test state transitions |
| Performance problems | Profile early, optimize hot paths |
| Merge conflicts | Commit daily, small focused commits |

## Definition of Done (Phase 1)
- [ ] Player can move in 4 states (Idle, Walk, Run, Crouch)
- [ ] Jump works with coyote time (100ms)
- [ ] Ground detection works on all surfaces
- [ ] Input responsive (< 50ms latency)
- [ ] Architecture follows Controller/Pawn pattern
- [ ] State machine extensible (easy to add states)
- [ ] Code documented with XML comments
- [ ] Unit tests for domain logic (80% coverage)
- [ ] Playtest feedback incorporated (movement feel)
- [ ] All Acceptance Criteria checked

---

**Next Phase**: Camera system (Phase 2)  
**Last Updated**: 2026-05-04
