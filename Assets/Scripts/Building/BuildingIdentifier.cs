using UnityEngine;
using NextDay.Events;

namespace NextDay.Building
{
    /// <summary>
    /// Gắn vào mỗi building đã đặt để xác định loại building.
    /// EnemyAI query component này khi tấn công building.
    /// </summary>
    public class BuildingIdentifier : MonoBehaviour
    {
        [SerializeField] private BuildingType _buildingType;

        public BuildingType BuildingType => _buildingType;

        public void Initialize(BuildingType type)
        {
            _buildingType = type;
        }
    }
}
