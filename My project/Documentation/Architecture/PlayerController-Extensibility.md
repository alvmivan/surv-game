# Player Controller — Extensibility, Events & Roadmap

Complemento del [PlayerController-Design.md](PlayerController-Design.md). Contiene puntos de extensión, eventos, y el roadmap de fases.

---

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

---

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

---

## Testing Strategy (Summary)

- **Unit Tests**: Domain logic (damage calculation, state transitions), data validation
- **Integration Tests**: Player movement in environments, camera mode switching, interaction systems
- **Play Mode Tests**: Full player controller scenarios, performance benchmarks

Ver [01-Phase1-Testing-Strategy.md](../Systems/01-Phase1-Testing-Strategy.md) para el plan completo.

---

## Performance Considerations

### Data-Oriented Design
- Use `ScriptableObject` references, not copies
- Cache component references
- Object pooling for projectiles/effects

### Update Optimization
- State machine only updates active state
- Camera updates only when view mode changes
- Environment checks on trigger/interval, not per frame

### Performance Options (Unity DOTS)
For large-scale scenarios (1000+ entities):
- Use **ECS** for NPCs, keep Player as MonoBehaviour
- **Burst Compiler** for SIMD optimization
- Convert NPCs to Entities for performance

---

## Roadmap de Fases

1. **Phase 1**: Core architecture + movement — [Sprint Backlogs](../Development/Phase1-Sprint1-Backlog.md)
2. **Phase 2**: Camera system (view modes, collision, effects)
3. **Phase 3**: Interaction systems (combat, items, tools)
4. **Phase 4**: Survival systems (health, stamina, injuries, environment)
5. **Phase 5**: Polish + feel (curves, aim assist, camera effects, footsteps)
6. **Phase 6** (Optional): Multiplayer prep

---

## Dependencies

### Unity Packages
- Input System (1.7+)
- Cinemachine (3.0+)
- **Optional**: Entities, Jobs, Burst (for 1000+ NPCs)

### Project Dependencies
- Inventory System (to be designed)
- Item System (ScriptableObject-based)
- UI System (for player HUD)
- Audio System (footsteps, combat, environment)
- Save System (persist PlayerController data)

---

**Relacionado**: [PlayerController-Design.md](PlayerController-Design.md) | [PlayerController-Specification.md](../Systems/PlayerController-Specification.md)  
**Version**: 1.0  
**Last Updated**: 2026-05-04
