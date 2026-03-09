using UnityEngine;

namespace Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;

        public int CurrentHealth { get; private set; } //get from anywhere but modify only in this class 
        public int MaxHealth => maxHealth;
        public bool IsDead => CurrentHealth <= 0;

        public event System.Action<int, int> OnHealthChanged; // current, max
        public event System.Action OnDeath;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            Debug.Log($"[PlayerHealth] TakeDamage({amount}) -> HP: {CurrentHealth}/{maxHealth}");
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (IsDead)
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}