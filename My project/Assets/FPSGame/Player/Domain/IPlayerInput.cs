using UnityEngine;

namespace FPSGame.Player
{
    public interface IPlayerInput
    {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool JumpPressed { get; }
        bool RunHeld { get; }
        bool CrouchToggled { get; }
    }
}
