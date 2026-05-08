using UnityEngine;
using NextDay.Events;

namespace NextDay.Core
{
    /// <summary>
    /// Điều phối game state, day/night cycle, và các event chính.
    /// Không dùng Singleton — dùng GameStateSO làm shared state.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Fields

        [Header("Shared State")]
        [Tooltip("ScriptableObject chứa trạng thái game hiện tại")]
        [SerializeField] private GameStateSO _gameStateSO;

        [Tooltip("ScriptableObject quản lý chu kỳ ngày/đêm")]
        [SerializeField] private DayNightCycleSO _dayNightCycleSO;

        [Header("Event Channels")]
        [Tooltip("Channel phát event game state (day/night, pause, death)")]
        [SerializeField] private GameEventChannel _gameEventChannel;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnPlayerDeath += HandlePlayerDeath;
            }
        }

        private void OnDisable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnPlayerDeath -= HandlePlayerDeath;
            }
        }

        private void Update()
        {
            if (_gameStateSO == null || _gameStateSO.IsPaused)
            {
                return;
            }

            UpdateDayNightCycle();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Khởi tạo game — reset state và bắt đầu ngày đầu tiên.
        /// </summary>
        public void Initialize()
        {
            if (_gameStateSO != null)
            {
                _gameStateSO.ResetState();
            }

            if (_dayNightCycleSO != null)
            {
                _dayNightCycleSO.ResetCycle();
            }

            OnDayStart();
        }

        /// <summary>
        /// Pause hoặc unpause game.
        /// </summary>
        public void SetPaused(bool paused)
        {
            if (_gameStateSO != null)
            {
                _gameStateSO.IsPaused = paused;
            }

            if (_gameEventChannel != null)
            {
                _gameEventChannel.RaiseGamePaused(paused);
            }

            Time.timeScale = paused ? 0f : 1f;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Cập nhật chu kỳ ngày/đêm mỗi frame.
        /// </summary>
        private void UpdateDayNightCycle()
        {
            if (_dayNightCycleSO == null)
            {
                return;
            }

            bool phaseChanged = _dayNightCycleSO.UpdateCycle(Time.deltaTime, out bool transitionedToNight);

            if (phaseChanged)
            {
                if (transitionedToNight)
                {
                    OnNightStart();
                }
                else
                {
                    // Đêm kết thúc → ngày mới
                    if (_gameStateSO != null)
                    {
                        _gameStateSO.AdvanceToNextDay();
                    }
                    OnDayStart();
                }
            }
        }

        /// <summary>
        /// Xử lý khi ngày bắt đầu.
        /// </summary>
        private void OnDayStart()
        {
            if (_gameStateSO != null)
            {
                _gameStateSO.IsNight = false;
            }

            if (_gameEventChannel != null)
            {
                _gameEventChannel.RaiseDayStart();
            }

            Debug.Log($"[GameManager] Ngày {(_gameStateSO != null ? _gameStateSO.CurrentDay : 0)} bắt đầu!");
        }

        /// <summary>
        /// Xử lý khi đêm bắt đầu.
        /// </summary>
        private void OnNightStart()
        {
            if (_gameStateSO != null)
            {
                _gameStateSO.SetNight();
            }

            if (_gameEventChannel != null)
            {
                _gameEventChannel.RaiseNightStart();
            }

            Debug.Log($"[GameManager] Đêm {(_gameStateSO != null ? _gameStateSO.CurrentDay : 0)} bắt đầu!");
        }

        /// <summary>
        /// Xử lý khi player chết — hiển thị game over hoặc respawn.
        /// </summary>
        private void HandlePlayerDeath()
        {
            Debug.Log("[GameManager] Player đã chết! Respawn trong buồng hồi sinh...");
            // TODO: Implement respawn logic — hồi sinh trong buồng hồi sinh
        }

        #endregion
    }
}
