using UnityEngine;

namespace NextDay.Core
{
    /// <summary>
    /// Config và state cho chu kỳ ngày/đêm.
    /// Quản lý thời gian trong game, tính toán chuyển ngày/đêm.
    /// </summary>
    [CreateAssetMenu(menuName = "NextDay/Config/Day Night Cycle")]
    public class DayNightCycleSO : ScriptableObject
    {
        #region Fields

        [Header("Cấu hình thời gian (giây thực)")]
        [Tooltip("Thời lượng ban ngày (giây)")]
        [SerializeField] private float _dayDuration = 120f;

        [Tooltip("Thời lượng ban đêm (giây)")]
        [SerializeField] private float _nightDuration = 60f;

        [Header("State hiện tại")]
        [Tooltip("Thời gian hiện tại trong chu kỳ (giây)")]
        [SerializeField] private float _currentTime;

        [Tooltip("Đang là ban đêm")]
        [SerializeField] private bool _isNight;

        #endregion

        #region Properties

        public float DayDuration => _dayDuration;
        public float NightDuration => _nightDuration;
        public float CurrentTime => _currentTime;
        public bool IsNight => _isNight;

        /// <summary>
        /// Tỷ lệ tiến trình trong phase hiện tại (0 → 1).
        /// </summary>
        public float CurrentPhaseProgress
        {
            get
            {
                float duration = _isNight ? _nightDuration : _dayDuration;
                return duration > 0f ? Mathf.Clamp01(_currentTime / duration) : 0f;
            }
        }

        /// <summary>
        /// Tổng thời gian một chu kỳ ngày + đêm.
        /// </summary>
        public float TotalCycleDuration => _dayDuration + _nightDuration;

        #endregion

        #region Public Methods

        /// <summary>
        /// Reset chu kỳ về ban ngày, thời gian = 0.
        /// </summary>
        public void ResetCycle()
        {
            _currentTime = 0f;
            _isNight = false;
        }

        /// <summary>
        /// Cập nhật thời gian chu kỳ. Trả về true nếu vừa chuyển phase (ngày↔đêm).
        /// </summary>
        /// <param name="deltaTime">Time.deltaTime</param>
        /// <param name="transitionedToNight">True nếu vừa chuyển sang đêm, false nếu vừa chuyển sang ngày</param>
        /// <returns>True nếu có chuyển phase</returns>
        public bool UpdateCycle(float deltaTime, out bool transitionedToNight)
        {
            transitionedToNight = false;
            _currentTime += deltaTime;

            float currentPhaseDuration = _isNight ? _nightDuration : _dayDuration;

            if (_currentTime >= currentPhaseDuration)
            {
                _currentTime -= currentPhaseDuration;

                if (_isNight)
                {
                    // Đêm kết thúc → chuyển sang ngày mới
                    _isNight = false;
                    transitionedToNight = false;
                }
                else
                {
                    // Ngày kết thúc → chuyển sang đêm
                    _isNight = true;
                    transitionedToNight = true;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra đang là ban đêm hay không.
        /// </summary>
        public bool CheckIsNight()
        {
            return _isNight;
        }

        #endregion
    }
}
