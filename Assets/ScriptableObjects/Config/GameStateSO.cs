using UnityEngine;

namespace NextDay.Core
{
    /// <summary>
    /// Shared state cho toàn bộ game — thay thế Singleton runtime.
    /// Lưu trữ trạng thái hiện tại của game: ngày/đêm, pause, số ngày.
    /// </summary>
    [CreateAssetMenu(menuName = "NextDay/Config/Game State")]
    public class GameStateSO : ScriptableObject
    {
        #region Fields

        [Header("Game Progress")]
        [Tooltip("Ngày hiện tại (bắt đầu từ 1)")]
        [SerializeField] private int _currentDay = 1;

        [Tooltip("Đang là ban đêm hay không")]
        [SerializeField] private bool _isNight;

        [Tooltip("Game đang pause hay không")]
        [SerializeField] private bool _isPaused;

        [Header("Player Stats")]
        [Tooltip("Tổng số kẻ thù đã tiêu diệt")]
        [SerializeField] private int _totalEnemiesKilled;

        [Tooltip("Tổng số building đã xây")]
        [SerializeField] private int _totalBuildingsPlaced;

        #endregion

        #region Properties

        public int CurrentDay
        {
            get => _currentDay;
            set => _currentDay = Mathf.Max(1, value);
        }

        public bool IsNight
        {
            get => _isNight;
            set => _isNight = value;
        }

        public bool IsPaused
        {
            get => _isPaused;
            set => _isPaused = value;
        }

        public int TotalEnemiesKilled
        {
            get => _totalEnemiesKilled;
            set => _totalEnemiesKilled = Mathf.Max(0, value);
        }

        public int TotalBuildingsPlaced
        {
            get => _totalBuildingsPlaced;
            set => _totalBuildingsPlaced = Mathf.Max(0, value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reset toàn bộ state về mặc định — gọi khi bắt đầu game mới.
        /// </summary>
        public void ResetState()
        {
            _currentDay = 1;
            _isNight = false;
            _isPaused = false;
            _totalEnemiesKilled = 0;
            _totalBuildingsPlaced = 0;
        }

        /// <summary>
        /// Chuyển sang ngày mới — tăng số ngày và set ban ngày.
        /// </summary>
        public void AdvanceToNextDay()
        {
            _currentDay++;
            _isNight = false;
        }

        /// <summary>
        /// Chuyển sang đêm.
        /// </summary>
        public void SetNight()
        {
            _isNight = true;
        }

        #endregion
    }
}
