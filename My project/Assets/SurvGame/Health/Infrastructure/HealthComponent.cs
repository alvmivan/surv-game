using UnityEngine;
using FPSGame.Player;

namespace SurvGame.Health
{
    public class HealthComponent : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float maxHealth = 100f;

        private HealthEntity healthEntity;

        public float CurrentHealth => healthEntity.CurrentHealth;
        public float MaxHealth => healthEntity.MaxHealth;

        private void Awake()
        {
            healthEntity = new HealthEntity(maxHealth);
        }

        private void OnEnable()
        {
            healthEntity.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            healthEntity.OnDeath -= HandleDeath;
        }

        public void TakeDamage(float amount)
        {
            healthEntity.TakeDamage(amount);
        }

        public void Heal(float amount)
        {
            healthEntity.Heal(amount);
        }

        private void HandleDeath()
        {
            Debug.Log("Player died");
        }
    }
}
