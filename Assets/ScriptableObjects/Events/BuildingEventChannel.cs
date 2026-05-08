using UnityEngine;
using System;

namespace NextDay.Events
{
    /// <summary>
    /// Loại công trình trong game.
    /// </summary>
    public enum BuildingType
    {
        Wall,
        Tower,
        Storage
    }

    /// <summary>
    /// Event channel cho hệ thống xây dựng.
    /// </summary>
    [CreateAssetMenu(menuName = "NextDay/Events/Building Event Channel")]
    public class BuildingEventChannel : ScriptableObject
    {
        #region Events

        /// <summary>Khi đặt building thành công (loại, vị trí)</summary>
        public event Action<BuildingType, Vector3> OnBuildingPlaced;

        /// <summary>Khi xóa building (loại, vị trí)</summary>
        public event Action<BuildingType, Vector3> OnBuildingRemoved;

        #endregion

        #region Public Methods

        public void RaiseBuildingPlaced(BuildingType type, Vector3 position)
        {
            OnBuildingPlaced?.Invoke(type, position);
        }

        public void RaiseBuildingRemoved(BuildingType type, Vector3 position)
        {
            OnBuildingRemoved?.Invoke(type, position);
        }

        #endregion
    }
}
