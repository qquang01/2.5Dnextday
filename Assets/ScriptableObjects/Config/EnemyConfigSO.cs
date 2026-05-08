using UnityEngine;
using NextDay.Events;

namespace NextDay.Defense
{
    /// <summary>
    /// Config cho từng loại kẻ thù — stats, tốc độ, damage.
    /// </summary>
    [CreateAssetMenu(menuName = "NextDay/Config/Enemy Config")]
    public class EnemyConfigSO : ScriptableObject
    {
        #region Fields

        [Header("Thông tin kẻ thù")]
        [Tooltip("Loại kẻ thù")]
        [SerializeField] private EnemyType _enemyType;

        [Tooltip("Tên hiển thị")]
        [SerializeField] private string _displayName = "Enemy";

        [Header("Stats")]
        [Tooltip("HP tối đa")]
        [SerializeField] private float _maxHealth = 50f;

        [Tooltip("Sát thương mỗi đòn")]
        [SerializeField] private float _damage = 10f;

        [Tooltip("Tốc độ di chuyển (units/giây)")]
        [SerializeField] private float _moveSpeed = 2f;

        [Tooltip("Khoảng cách phát hiện player/building")]
        [SerializeField] private float _detectionRange = 8f;

        [Tooltip("Khoảng cách tấn công")]
        [SerializeField] private float _attackRange = 1.2f;

        [Tooltip("Thời gian cooldown giữa các đòn tấn công (giây)")]
        [SerializeField] private float _attackCooldown = 1f;

        [Header("Rewards")]
        [Tooltip("Kinh nghiệm khi tiêu diệt")]
        [SerializeField] private int _expReward = 10;

        [Header("Visual")]
        [Tooltip("Sprite kẻ thù")]
        [SerializeField] private Sprite _sprite;

        [Header("Prefab")]
        [Tooltip("Prefab của kẻ thù")]
        [SerializeField] private GameObject _enemyPrefab;

        #endregion

        #region Properties

        public EnemyType EnemyType => _enemyType;
        public string DisplayName => _displayName;
        public float MaxHealth => _maxHealth;
        public float Damage => _damage;
        public float MoveSpeed => _moveSpeed;
        public float DetectionRange => _detectionRange;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public int ExpReward => _expReward;
        public Sprite Sprite => _sprite;
        public GameObject EnemyPrefab => _enemyPrefab;

        #endregion
    }
}
