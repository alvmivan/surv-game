using System;

namespace SurvGame.Health
{
    public class HealthEntity : IDamageable
    {
        private readonly float _maxHealth;
        private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDead => _currentHealth <= 0f;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        public HealthEntity(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            _currentHealth -= amount;
            _currentHealth = Math.Max(0f, _currentHealth);

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (IsDead)
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            _currentHealth = Math.Min(_maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }
}
