using UnityEngine;
using FPSGame.Player;
using SurvGame.Health;

namespace SurvivalProject.Player
{
    public class SurvivalPlayerPawn : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private PlayerPawn fpsPawn;
        [SerializeField] private HealthComponent health;

        private void Awake()
        {
            if (fpsPawn == null) fpsPawn = GetComponent<PlayerPawn>();
            if (health == null) health = GetComponent<HealthComponent>();
        }

        public void TakeDamage(float amount)
        {
            if (health != null) health.TakeDamage(amount);
        }

        public float Health => health?.CurrentHealth ?? 0f;
    }
}
