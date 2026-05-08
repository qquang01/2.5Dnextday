using UnityEngine;
using System;

namespace NextDay.Defense
{
    /// <summary>
    /// Component HP tái sử dụng — gắn cho cả enemy và building.
    /// Publish event khi bị damage hoặc chết.
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        #region Fields

        [Header("Health Settings")]
        [Tooltip("HP tối đa")]
        [SerializeField] private float _maxHealth = 100f;

        [Tooltip("HP hiện tại")]
        [SerializeField] private float _currentHealth;

        [Header("Defense")]
        [Tooltip("Giá trị phòng thủ (giảm damage nhận vào)")]
        [SerializeField] private float _defenseValue;

        #endregion

        #region Events

        /// <summary>Khi nhận damage (damage thực tế sau defense)</summary>
        public event Action<float> OnDamaged;

        /// <summary>Khi HP về 0</summary>
        public event Action OnDeath;

        #endregion

        #region Properties

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public float DefenseValue => _defenseValue;
        public bool IsDead => _currentHealth <= 0f;

        /// <summary>Tỷ lệ HP hiện tại (0 → 1).</summary>
        public float HealthPercent => _maxHealth > 0f ? Mathf.Clamp01(_currentHealth / _maxHealth) : 0f;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Khởi tạo HP với giá trị tùy chỉnh — dùng khi spawn enemy từ config.
        /// </summary>
        public void Initialize(float maxHealth, float defenseValue = 0f)
        {
            _maxHealth = maxHealth;
            _defenseValue = defenseValue;
            _currentHealth = _maxHealth;
        }

        /// <summary>
        /// Nhận sát thương. Trừ defense trước khi trừ HP.
        /// </summary>
        public void TakeDamage(float rawDamage)
        {
            if (IsDead)
            {
                return;
            }

            // Tính damage thực tế sau defense (tối thiểu 1)
            float actualDamage = Mathf.Max(1f, rawDamage - _defenseValue);
            _currentHealth = Mathf.Max(0f, _currentHealth - actualDamage);

            OnDamaged?.Invoke(actualDamage);

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Hồi máu.
        /// </summary>
        public void Heal(float amount)
        {
            if (IsDead)
            {
                return;
            }

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        }

        #endregion

        #region Private Methods

        private void Die()
        {
            OnDeath?.Invoke();
        }

        #endregion
    }
}
