# Phase 1: Testing Strategy

## Overview
Comprehensive testing plan for Core Architecture + Movement system. Ensures responsive, bug-free player movement before adding camera and interactions.

## Testing Pyramid

```
           ┌─────────────┐
           │   E2E Tests  │  (5%) - Play Mode scenarios
           └──────┬──────┘
              ┌────▼─────┐
              │ Integration│  (15%) - System interaction tests
              └──────┬────┘
                 ┌────▼─────┐
                 │  Unit Tests │  (80%) - Domain logic tests
                 └───────────┘
```

## 1. Unit Tests (Domain Layer)

### 1.1 State Machine Tests
**File**: `Tests/Editor/Domain/StateMachineTests.cs`

```csharp
[TestFixture]
public class StateMachineTests
{
    private StateMachine _stateMachine;
    private MockState _idleState;
    private MockState _walkState;

    [SetUp]
    public void Setup()
    {
        _stateMachine = new StateMachine();
        _idleState = new MockState();
        _walkState = new MockState();
    }

    [Test]
    public void ChangeState_CallsExitOnOldState()
    {
        _stateMachine.ChangeState(_idleState);
        _stateMachine.ChangeState(_walkState);
        Assert.IsTrue(_idleState.ExitCalled);
    }

    [Test]
    public void ChangeState_CallsEnterOnNewState()
    {
        _stateMachine.ChangeState(_idleState);
        Assert.IsTrue(_idleState.EnterCalled);
    }

    [Test]
    public void Update_CallsUpdateOnCurrentState()
    {
        _stateMachine.ChangeState(_idleState);
        _stateMachine.Update(0.016f);
        Assert.IsTrue(_idleState.UpdateCalled);
    }
}
```

**Test Cases**:
- [ ] State transitions call Exit → Enter in order
- [ ] Update only called on active state
- [ ] Cannot transition to same state twice
- [ ] State machine handles null states gracefully

---

### 1.2 Movement Logic Tests
**File**: `Tests/Editor/Domain/MovementLogicTests.cs`

```csharp
[TestFixture]
public class MovementLogicTests
{
    private PlayerConfig _config;

    [SetUp]
    public void Setup()
    {
        _config = ScriptableObject.CreateInstance<PlayerConfig>();
        _config.WalkSpeed = 5f;
        _config.RunSpeedMultiplier = 1.6f;
    }

    [Test]
    public void CalculateSpeed_ReturnsWalkSpeed_WhenWalking()
    {
        float speed = MovementCalculator.GetSpeed(MovementState.Walking, _config);
        Assert.AreEqual(5f, speed);
    }

    [Test]
    public void CalculateSpeed_ReturnsRunSpeed_WhenRunning()
    {
        float speed = MovementCalculator.GetSpeed(MovementState.Running, _config);
        Assert.AreEqual(8f, speed); // 5 * 1.6
    }

    [Test]
    public void CalculateSpeed_ReturnsCrouchSpeed_WhenCrouching()
    {
        _config.CrouchSpeedMultiplier = 0.5f;
        float speed = MovementCalculator.GetSpeed(MovementState.Crouching, _config);
        Assert.AreEqual(2.5f, speed); // 5 * 0.5
    }
}
```

**Test Cases**:
- [ ] Walk speed = base speed
- [ ] Run speed = base speed × multiplier
- [ ] Crouch speed = base speed × multiplier
- [ ] Zero speed when idle
- [ ] Negative values handled (clamp to 0)

---

### 1.3 Input System Tests
**File**: `Tests/Editor/Infrastructure/InputProviderTests.cs`

```csharp
[Test]
public void MoveInput_ReturnsNormalizedVector()
{
    var input = new InputProvider();
    // Simulate WASD = (1, 1)
    Vector2 move = input.MoveInput;
    Assert.LessOrEqual(move.magnitude, 1f); // Normalized
}

[Test]
public void JumpPressed_ReturnsTrue_OnButtonPress()
{
    var input = new InputProvider();
    // Simulate Space press
    bool pressed = input.JumpPressed;
    Assert.IsTrue(pressed);
}
```

**Test Cases**:
- [ ] Move input normalized (magnitude ≤ 1)
- [ ] Look input scales with sensitivity
- [ ] Button press detected on frame pressed
- [ ] Button release detected immediately

---

## 2. Integration Tests (System Interaction)

### 2.1 Controller/Pawn Tests
**File**: `Tests/Editor/Integration/ControllerPawnTests.cs`

```csharp
[Test]
public void Possess_SetsPawnReference()
{
    var controller = new GameObject().AddComponent<PlayerController>();
    var pawn = new GameObject().AddComponent<PlayerPawn>();
    
    controller.Possess(pawn);
    
    Assert.AreEqual(pawn, controller.PossessedPawn);
}

[Test]
public void Unpossess_ClearsPawnReference()
{
    var controller = new GameObject().AddComponent<PlayerController>();
    var pawn = new GameObject().AddComponent<PlayerPawn>();
    controller.Possess(pawn);
    
    controller.Unpossess();
    
    Assert.IsNull(controller.PossessedPawn);
}
```

**Test Cases**:
- [ ] Controller can possess pawn
- [ ] Controller can unpossess pawn
- [ ] Multiple pawns can be possessed sequentially
- [ ] Input routes through controller to pawn

---

### 2.2 Ground Detection Tests
**File**: `Tests/Editor/Integration/GroundDetectionTests.cs`

```csharp
[Test]
public void IsGrounded_ReturnsTrue_WhenOnGround()
{
    var motor = CreateMotorOnPlane();
    Assert.IsTrue(motor.IsGrounded);
}

[Test]
public void IsGrounded_ReturnsFalse_WhenFalling()
{
    var motor = CreateMotorInAir();
    Assert.IsFalse(motor.IsGrounded);
}

[Test]
public void CoyoteTime_AllowsJump_AfterLeavingGround()
{
    var motor = CreateMotorOnEdge();
    motor.Update(0.05f); // 50ms after leaving
    Assert.IsTrue(motor.CanJump); // Coyote time active
}
```

**Test Cases**:
- [ ] Grounded when on flat surface
- [ ] Not grounded when falling
- [ ] Coyote time = 100ms window
- [ ] Slope limit detected (45° max)

---

## 3. Play Mode Tests (E2E Scenarios)

### 3.1 Basic Movement Scenarios
**File**: `Tests/PlayMode/Scenarios/BasicMovementTests.cs`

```csharp
[UnityTest]
public IEnumerator Player_MovesForward_WhenWPressed()
{
    yield return new EnterPlayMode();
    
    var player = SetupPlayer();
    PressKey(KeyCode.W);
    yield return new WaitForSeconds(1f);
    
    Assert.Greater(player.transform.position.z, 0f);
}

[UnityTest]
public IEnumerator Player_Jumpes_WhenSpacePressed()
{
    yield return new EnterPlayMode();
    
    var player = SetupPlayer();
    float startY = player.transform.position.y;
    PressKey(KeyCode.Space);
    yield return new WaitForSeconds(0.5f);
    
    Assert.Greater(player.transform.position.y, startY);
}
```

**Test Scenarios**:
- [ ] **Scenario 1**: WASD movement in all directions
- [ ] **Scenario 2**: Shift+WASD runs faster
- [ ] **Scenario 3**: Ctrl toggles crouch (height reduces)
- [ ] **Scenario 4**: Space jumps when grounded
- [ ] **Scenario 5**: Cannot jump when not grounded (without coyote)

---

### 3.2 Coyote Time Scenarios
**File**: `Tests/PlayMode/Scenarios/CoyoteTimeTests.cs`

```csharp
[UnityTest]
public IEnumerator CanJump_Within100ms_AfterWalkingOffLedge()
{
    yield return new EnterPlayMode();
    
    var player = SetupPlayerOnLedge();
    yield return new WaitForSeconds(0.05f); // 50ms
    PressKey(KeyCode.Space);
    
    Assert.IsTrue(player.IsJumping);
}

[UnityTest]
public IEnumerator CannotJump_After150ms_AfterWalkingOffLedge()
{
    yield return new EnterPlayMode();
    
    var player = SetupPlayerOnLedge();
    yield return new WaitForSeconds(0.15f); // 150ms
    PressKey(KeyCode.Space);
    
    Assert.IsFalse(player.IsJumping);
}
```

**Test Scenarios**:
- [ ] **Scenario 6**: Can jump 50ms after leaving ground
- [ ] **Scenario 7**: Can jump 100ms after leaving ground
- [ ] **Scenario 8**: Cannot jump 150ms after leaving ground
- [ ] **Scenario 9**: Jump input buffered 100ms before landing

---

### 3.3 Edge Case Scenarios
**Test Scenarios**:
- [ ] **Scenario 10**: Player doesn't get stuck on walls
- [ ] **Scenario 11**: Player slides on steep slopes (>45°)
- [ ] **Scenario 12**: Player can traverse stairs (auto-step)
- [ ] **Scenario 13**: Multiple inputs don't conflict
- [ ] **Scenario 14**: Input works after scene reload

---

## 4. Performance Tests

### 4.1 Frame Rate Tests
**File**: `Tests/PlayMode/Performance/FrameRateTests.cs`

```csharp
[UnityTest]
public IEnumerator Movement_UpdateTakesLessThan2ms()
{
    yield return new EnterPlayMode();
    
    var motor = SetupPlayer().GetComponent<PlayerMotor>();
    float startTime = Time.realtimeSinceStartup;
    
    for (int i = 0; i < 1000; i++)
    {
        motor.Update();
    }
    
    float elapsed = Time.realtimeSinceStartup - startTime;
    float avgMs = (elapsed / 1000f) * 1000f;
    
    Assert.Less(avgMs, 2f, "Movement update should be < 2ms");
}
```

**Benchmarks**:
- [ ] Movement update < 2ms per frame
- [ ] State machine update < 1ms per frame
- [ ] Input processing < 0.5ms per frame
- [ ] 60 FPS maintained with player active

---

## 5. Playtest Checklist

### 5.1 Movement Feel
**Tester**: ___________  
**Date**: ___________

#### Responsiveness
- [ ] Movement feels instant (< 50ms input lag)
- [ ] Jump feels responsive
- [ ] Crouch toggles smoothly

#### Speed
- [ ] Walk speed feels appropriate
- [ ] Run speed feels fast but controlled
- [ ] Crouch speed feels slow

#### Physics
- [ ] Gravity feels natural (not too floaty, not too heavy)
- [ ] Landing doesn't bounce
- [ ] No jitter when moving against walls

#### Coyote Time
- [ ] Can jump after barely walking off ledge
- [ ] Feels forgiving, not frustrating
- [ ] Doesn't feel "cheaty"

#### Bugs
- [ ] No getting stuck on geometry
- [ ] No falling through floor
- [ ] No infinite jump glitch
- [ ] No speed hacking by diagonal movement

---

## 6. Automated Test Runner

### 6.1 Running Tests via CLI
```bash
# Run all unit tests
unity -runTests -projectPath . -testResults results.xml -testPlatform EditMode

# Run play mode tests
unity -runTests -projectPath . -testResults results.xml -testPlatform PlayMode

# Run specific test category
unity -runTests -projectPath . -testFilter "Movement"
```

### 6.2 CI/CD Integration
```yaml
# .github/workflows/tests.yml
name: Run Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Run Unity Tests
        run: |
          /opt/unity/Editor/Unity -runTests ...
```

---

## 7. Bug Report Template

```
BUG REPORT: [Title]

**Severity**: Critical / High / Medium / Low
**Reproducibility**: Always / Sometimes / Rarely

**Steps to Reproduce**:
1. 
2. 
3. 

**Expected Behavior**:

**Actual Behavior**:

**Screenshots/Video**:

**Environment**:
- Unity Version: 
- Platform: 
- Input Device: 
```

---

## Test Coverage Goals

| Component | Unit Tests | Integration | Play Mode | Coverage |
|-----------|-------------|-------------|-----------|----------|
| State Machine | 10 tests | 3 tests | 2 scenarios | 80% |
| Movement Logic | 15 tests | 5 tests | 5 scenarios | 85% |
| Input System | 8 tests | 2 tests | 3 scenarios | 75% |
| Ground Detection | 6 tests | 4 tests | 2 scenarios | 90% |
| **TOTAL** | **39 tests** | **14 tests** | **12 scenarios** | **82%** |

---

## Sign-off Criteria (Phase 1 Complete)

### Code Quality
- [ ] All unit tests pass (100%)
- [ ] All integration tests pass (100%)
- [ ] All play mode scenarios pass (100%)
- [ ] Code coverage ≥ 80% on domain layer
- [ ] No critical or high bugs open

### Performance
- [ ] Movement update < 2ms
- [ ] State machine update < 1ms
- [ ] 60 FPS stable in test scene
- [ ] Memory usage normal (no leaks)

### Playtest
- [ ] 5+ playtesters tried movement
- [ ] Average responsiveness rating ≥ 4/5
- [ ] No game-breaking bugs found
- [ ] Movement feel approved by lead

### Documentation
- [ ] All public methods have XML docs
- [ ] Architecture diagram updated
- [ ] Testing strategy followed
- [ ] Known issues documented

---

**Next Phase**: Camera system testing (Phase 2)  
**Last Updated**: 2026-05-04
