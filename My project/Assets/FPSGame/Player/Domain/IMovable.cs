using UnityEngine;

namespace FPSGame.Player
{
    public interface IMovable
    {
        void Move(Vector2 input);
        float CurrentSpeed { get; }
        bool IsGrounded { get; }
    }
}
