using UnityEngine;
using System;

namespace NextDay.Events
{
    /// <summary>
    /// Loại kẻ thù trong game.
    /// </summary>
    public enum EnemyType
    {
        Basic,
        Fast,
        Tank
    }

    /// <summary>
    /// Event channel cho hệ thống phòng thủ.
    /// </summary>
    [CreateAssetMenu(menuName = "NextDay/Events/Defense Event Channel")]
    public class DefenseEventChannel : ScriptableObject
    {
        #region Events

        /// <summary>Khi kẻ thù spawn (loại, vị trí)</summary>
        public event Action<EnemyType, Vector3> OnEnemySpawned;

        /// <summary>Khi kẻ thù chết (loại)</summary>
        public event Action<EnemyType> OnEnemyDeath;

        /// <summary>Khi building bị damage (loại building, damage)</summary>
        public event Action<BuildingType, float> OnBuildingDamaged;

        #endregion

        #region Public Methods

        public void RaiseEnemySpawned(EnemyType type, Vector3 position)
        {
            OnEnemySpawned?.Invoke(type, position);
        }

        public void RaiseEnemyDeath(EnemyType type)
        {
            OnEnemyDeath?.Invoke(type);
        }

        public void RaiseBuildingDamaged(BuildingType buildingType, float damage)
        {
            OnBuildingDamaged?.Invoke(buildingType, damage);
        }

        #endregion
    }
}
