using UnityEngine;
using System;

namespace NextDay.Events
{
    /// <summary>
    /// Loại tài nguyên trong game.
    /// </summary>
    public enum ResourceType
    {
        Wood,
        Stone,
        Metal
    }

    /// <summary>
    /// Event channel cho hệ thống tài nguyên.
    /// </summary>
    [CreateAssetMenu(menuName = "NextDay/Events/Resource Event Channel")]
    public class ResourceEventChannel : ScriptableObject
    {
        #region Events

        /// <summary>Khi thu thập tài nguyên thành công (loại, số lượng)</summary>
        public event Action<ResourceType, int> OnResourceCollected;

        /// <summary>Khi resource node hết tài nguyên</summary>
        public event Action<GameObject> OnResourceDepleted;

        #endregion

        #region Public Methods

        public void RaiseResourceCollected(ResourceType type, int amount)
        {
            OnResourceCollected?.Invoke(type, amount);
        }

        public void RaiseResourceDepleted(GameObject resourceNode)
        {
            OnResourceDepleted?.Invoke(resourceNode);
        }

        #endregion
    }
}
