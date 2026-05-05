namespace FPSGame.Player
{
    public interface IJumpable
    {
        void Jump();
        bool CanJump { get; }
    }
}
