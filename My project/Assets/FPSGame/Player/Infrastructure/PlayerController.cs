using UnityEngine;

namespace FPSGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Pawn")]
        [SerializeField] private PlayerPawn possessedPawn;

        public PlayerPawn PossessedPawn => possessedPawn;

        public void Possess(PlayerPawn pawn)
        {
            if (possessedPawn != null)
            {
                Unpossess();
            }
            possessedPawn = pawn;
        }

        public void Unpossess()
        {
            possessedPawn = null;
        }

        private void Update()
        {
            if (possessedPawn == null) return;
        }
    }
}
