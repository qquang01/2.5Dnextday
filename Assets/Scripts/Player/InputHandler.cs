using UnityEngine;

namespace NextDay.Player
{
    /// <summary>
    /// Wrapper cho Unity Input — đọc input di chuyển, tương tác, tấn công.
    /// Dùng Input cũ (UnityEngine.Input) để đơn giản cho vertical slice.
    /// Có thể nâng cấp lên New Input System sau.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        #region Fields

        [Header("Keybindings")]
        [Tooltip("Key để tương tác với object (thu thập, mở cửa...)")]
        [SerializeField] private KeyCode _interactKey = KeyCode.E;

        [Tooltip("Key để tấn công")]
        [SerializeField] private KeyCode _attackKey = KeyCode.Mouse0;

        [Tooltip("Key để mở build menu")]
        [SerializeField] private KeyCode _buildMenuKey = KeyCode.B;

        [Tooltip("Key để pause game")]
        [SerializeField] private KeyCode _pauseKey = KeyCode.Escape;

        private Vector2 _movementInput;

        #endregion

        #region Properties

        /// <summary>Vector di chuyển đã normalize (WASD/Arrow keys).</summary>
        public Vector2 MovementInput => _movementInput;

        /// <summary>True nếu đang có input di chuyển.</summary>
        public bool IsMoving => _movementInput.sqrMagnitude > 0.01f;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            ReadMovementInput();
        }

        #endregion

        #region Public Methods

        /// <summary>Trả về true trong frame nhấn key tương tác.</summary>
        public bool GetInteractInput()
        {
            return Input.GetKeyDown(_interactKey);
        }

        /// <summary>Trả về true trong frame nhấn key tấn công.</summary>
        public bool GetAttackInput()
        {
            return Input.GetKeyDown(_attackKey);
        }

        /// <summary>Trả về true khi đang giữ key tấn công.</summary>
        public bool GetAttackHoldInput()
        {
            return Input.GetKey(_attackKey);
        }

        /// <summary>Trả về true trong frame nhấn key build menu.</summary>
        public bool GetBuildMenuInput()
        {
            return Input.GetKeyDown(_buildMenuKey);
        }

        /// <summary>Trả về true trong frame nhấn key pause.</summary>
        public bool GetPauseInput()
        {
            return Input.GetKeyDown(_pauseKey);
        }

        /// <summary>Trả về vị trí chuột trong world space.</summary>
        public Vector3 GetMouseWorldPosition(Camera cam)
        {
            if (cam == null)
            {
                return Vector3.zero;
            }

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = cam.nearClipPlane;
            return cam.ScreenToWorldPoint(mousePos);
        }

        #endregion

        #region Private Methods

        /// <summary>Đọc input WASD/Arrow keys, normalize vector.</summary>
        private void ReadMovementInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            _movementInput = new Vector2(horizontal, vertical).normalized;
        }

        #endregion
    }
}
