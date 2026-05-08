using UnityEngine;
using System;

namespace NextDay.Events
{
    /// <summary>
    /// Event channel chung cho game state — Day/Night, Pause, Player Death.
    /// </summary>
    [CreateAssetMenu(menuName = "NextDay/Events/Game Event Channel")]
    public class GameEventChannel : ScriptableObject
    {
        #region Events

        /// <summary>Khi ngày bắt đầu</summary>
        public event Action OnDayStart;

        /// <summary>Khi đêm bắt đầu</summary>
        public event Action OnNightStart;

        /// <summary>Khi game pause/unpause</summary>
        public event Action<bool> OnGamePaused;

        /// <summary>Khi player chết</summary>
        public event Action OnPlayerDeath;

        #endregion

        #region Public Methods

        public void RaiseDayStart()
        {
            OnDayStart?.Invoke();
        }

        public void RaiseNightStart()
        {
            OnNightStart?.Invoke();
        }

        public void RaiseGamePaused(bool isPaused)
        {
            OnGamePaused?.Invoke(isPaused);
        }

        public void RaisePlayerDeath()
        {
            OnPlayerDeath?.Invoke();
        }

        #endregion
    }
}
