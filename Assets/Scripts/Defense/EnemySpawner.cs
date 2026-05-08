using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NextDay.Events;
using NextDay.Core;

namespace NextDay.Defense
{
    /// <summary>
    /// Cấu hình cho một wave kẻ thù.
    /// </summary>
    [System.Serializable]
    public class EnemyWave
    {
        [Tooltip("Config kẻ thù trong wave này")]
        public EnemyConfigSO EnemyConfig;

        [Tooltip("Số lượng kẻ thù")]
        public int Count = 3;

        [Tooltip("Thời gian chờ giữa mỗi lần spawn (giây)")]
        public float SpawnInterval = 1f;
    }

    /// <summary>
    /// Spawn kẻ thù theo wave khi đêm bắt đầu.
    /// Quản lý spawn points và wave progression.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        #region Fields

        [Header("Spawn Config")]
        [Tooltip("Danh sách wave kẻ thù")]
        [SerializeField] private EnemyWave[] _waves;

        [Tooltip("Các vị trí spawn")]
        [SerializeField] private Transform[] _spawnPoints;

        [Tooltip("Thời gian chờ trước khi bắt đầu wave đầu tiên (giây)")]
        [SerializeField] private float _initialDelay = 3f;

        [Tooltip("Thời gian chờ giữa các wave (giây)")]
        [SerializeField] private float _waveCooldown = 10f;

        [Header("Scaling")]
        [Tooltip("Tăng số lượng enemy mỗi ngày (%)")]
        [SerializeField] private float _dailyScaling = 0.1f;

        [Header("References")]
        [Tooltip("Game state để biết ngày hiện tại")]
        [SerializeField] private GameStateSO _gameStateSO;

        [Header("Event Channels")]
        [Tooltip("Channel phát event defense")]
        [SerializeField] private DefenseEventChannel _defenseEventChannel;

        [Tooltip("Channel lắng nghe event game state")]
        [SerializeField] private GameEventChannel _gameEventChannel;

        private int _currentWaveIndex;
        private int _totalEnemiesSpawned;
        private int _totalEnemiesAlive;
        private bool _isSpawning;
        private List<GameObject> _activeEnemies = new List<GameObject>();

        #endregion

        #region Properties

        public int CurrentWaveIndex => _currentWaveIndex;
        public int TotalEnemiesAlive => _totalEnemiesAlive;
        public bool IsSpawning => _isSpawning;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnNightStart += HandleNightStart;
                _gameEventChannel.OnDayStart += HandleDayStart;
            }

            if (_defenseEventChannel != null)
            {
                _defenseEventChannel.OnEnemyDeath += HandleEnemyDeath;
            }
        }

        private void OnDisable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnNightStart -= HandleNightStart;
                _gameEventChannel.OnDayStart -= HandleDayStart;
            }

            if (_defenseEventChannel != null)
            {
                _defenseEventChannel.OnEnemyDeath -= HandleEnemyDeath;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Bắt đầu spawn waves — gọi khi đêm bắt đầu.
        /// </summary>
        public void StartSpawning()
        {
            if (_isSpawning)
            {
                return;
            }

            _currentWaveIndex = 0;
            _totalEnemiesSpawned = 0;
            _totalEnemiesAlive = 0;
            _isSpawning = true;

            StartCoroutine(SpawnWavesCoroutine());
            Debug.Log("[EnemySpawner] Bắt đầu spawn kẻ thù!");
        }

        /// <summary>
        /// Dừng spawn — gọi khi ngày bắt đầu.
        /// </summary>
        public void StopSpawning()
        {
            _isSpawning = false;
            StopAllCoroutines();
            DestroyAllEnemies();
            Debug.Log("[EnemySpawner] Dừng spawn, xóa tất cả kẻ thù.");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Coroutine spawn từng wave theo thứ tự.
        /// </summary>
        private IEnumerator SpawnWavesCoroutine()
        {
            yield return new WaitForSeconds(_initialDelay);

            while (_isSpawning && _currentWaveIndex < _waves.Length)
            {
                yield return StartCoroutine(SpawnWave(_waves[_currentWaveIndex]));
                _currentWaveIndex++;

                if (_currentWaveIndex < _waves.Length)
                {
                    yield return new WaitForSeconds(_waveCooldown);
                }
            }

            _isSpawning = false;
            Debug.Log("[EnemySpawner] Tất cả waves đã spawn xong.");
        }

        /// <summary>
        /// Spawn một wave kẻ thù.
        /// </summary>
        private IEnumerator SpawnWave(EnemyWave wave)
        {
            if (wave == null || wave.EnemyConfig == null)
            {
                yield break;
            }

            // Tính số lượng có scaling theo ngày
            int currentDay = _gameStateSO != null ? _gameStateSO.CurrentDay : 1;
            float scalingMultiplier = 1f + (_dailyScaling * (currentDay - 1));
            int scaledCount = Mathf.CeilToInt(wave.Count * scalingMultiplier);

            Debug.Log($"[EnemySpawner] Wave {_currentWaveIndex + 1}: {scaledCount}x {wave.EnemyConfig.DisplayName}");

            for (int i = 0; i < scaledCount; i++)
            {
                if (!_isSpawning)
                {
                    yield break;
                }

                SpawnEnemy(wave.EnemyConfig);
                yield return new WaitForSeconds(wave.SpawnInterval);
            }
        }

        /// <summary>
        /// Spawn một kẻ thù tại spawn point ngẫu nhiên.
        /// </summary>
        private void SpawnEnemy(EnemyConfigSO config)
        {
            if (config == null || _spawnPoints == null || _spawnPoints.Length == 0)
            {
                return;
            }

            // Chọn spawn point ngẫu nhiên
            Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            Vector3 spawnPosition = spawnPoint.position;

            // Thêm offset ngẫu nhiên nhỏ để không chồng nhau
            spawnPosition += new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                0f
            );

            GameObject enemy;

            if (config.EnemyPrefab != null)
            {
                enemy = Instantiate(config.EnemyPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                // Tạo enemy placeholder nếu chưa có prefab
                enemy = CreatePlaceholderEnemy(config, spawnPosition);
            }

            // Khởi tạo AI
            if (enemy.TryGetComponent(out EnemyAI ai))
            {
                ai.Initialize(config, _defenseEventChannel);
            }

            _activeEnemies.Add(enemy);
            _totalEnemiesSpawned++;
            _totalEnemiesAlive++;

            // Publish event
            if (_defenseEventChannel != null)
            {
                _defenseEventChannel.RaiseEnemySpawned(config.EnemyType, spawnPosition);
            }
        }

        /// <summary>
        /// Tạo enemy placeholder khi chưa có prefab.
        /// </summary>
        private GameObject CreatePlaceholderEnemy(EnemyConfigSO config, Vector3 position)
        {
            GameObject enemy = new GameObject($"Enemy_{config.DisplayName}");
            enemy.transform.position = position;

            // Thêm components cơ bản
            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            CircleCollider2D collider = enemy.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;

            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            sr.color = Color.red;
            if (config.Sprite != null)
            {
                sr.sprite = config.Sprite;
            }

            enemy.AddComponent<HealthComponent>();
            enemy.AddComponent<EnemyAI>();

            return enemy;
        }

        /// <summary>
        /// Xóa tất cả enemy đang active — gọi khi ngày bắt đầu.
        /// </summary>
        private void DestroyAllEnemies()
        {
            foreach (GameObject enemy in _activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            _activeEnemies.Clear();
            _totalEnemiesAlive = 0;
        }

        /// <summary>
        /// Khi đêm bắt đầu → start spawning.
        /// </summary>
        private void HandleNightStart()
        {
            StartSpawning();
        }

        /// <summary>
        /// Khi ngày bắt đầu → stop spawning, xóa enemies.
        /// </summary>
        private void HandleDayStart()
        {
            StopSpawning();
        }

        /// <summary>
        /// Khi enemy chết → giảm counter.
        /// </summary>
        private void HandleEnemyDeath(EnemyType type)
        {
            _totalEnemiesAlive = Mathf.Max(0, _totalEnemiesAlive - 1);

            // Cập nhật game state
            if (_gameStateSO != null)
            {
                _gameStateSO.TotalEnemiesKilled++;
            }
        }

        #endregion
    }
}
