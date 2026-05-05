# FPSGame - Layer 1

Generic First Person Shooter framework. Base layer for all FPS games.

## Structure
- `Player/` - Player controller, pawn, motor, camera
- `Weapons/` - Weapon system (future)
- `Camera/` - Camera modes (future)

## Dependencies
None. This layer is standalone.

## Usage
1. Create a `PlayerConfig` ScriptableObject
2. Create a GameObject with `PlayerPawn` and `CharacterController`
3. Add a Camera as child
4. Assign the config
