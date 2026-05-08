using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace NextDay.Core
{
    /// <summary>
    /// Quản lý load/unload scene — hỗ trợ async loading.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        #region Fields

        [Header("Settings")]
        [Tooltip("Tên scene mặc định khi load game")]
        [SerializeField] private string _defaultSceneName = "SampleScene";

        private bool _isLoading;

        #endregion

        #region Properties

        public bool IsLoading => _isLoading;

        #endregion

        #region Public Methods

        /// <summary>
        /// Load scene theo tên (async).
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[SceneLoader] Đang load scene khác, bỏ qua yêu cầu.");
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneName));
        }

        /// <summary>
        /// Load scene mặc định.
        /// </summary>
        public void LoadDefaultScene()
        {
            LoadScene(_defaultSceneName);
        }

        /// <summary>
        /// Unload scene theo tên (async).
        /// </summary>
        public void UnloadScene(string sceneName)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                StartCoroutine(UnloadSceneAsync(sceneName));
            }
            else
            {
                Debug.LogWarning($"[SceneLoader] Scene '{sceneName}' chưa được load.");
            }
        }

        /// <summary>
        /// Reload scene hiện tại.
        /// </summary>
        public void ReloadCurrentScene()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            LoadScene(currentSceneName);
        }

        #endregion

        #region Private Methods

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            _isLoading = true;
            Debug.Log($"[SceneLoader] Đang load scene: {sceneName}");

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOp == null)
            {
                Debug.LogError($"[SceneLoader] Không thể load scene: {sceneName}");
                _isLoading = false;
                yield break;
            }

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            _isLoading = false;
            Debug.Log($"[SceneLoader] Load scene hoàn tất: {sceneName}");
        }

        private IEnumerator UnloadSceneAsync(string sceneName)
        {
            Debug.Log($"[SceneLoader] Đang unload scene: {sceneName}");

            AsyncOperation asyncOp = SceneManager.UnloadSceneAsync(sceneName);
            if (asyncOp == null)
            {
                Debug.LogError($"[SceneLoader] Không thể unload scene: {sceneName}");
                yield break;
            }

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            Debug.Log($"[SceneLoader] Unload scene hoàn tất: {sceneName}");
        }

        #endregion
    }
}
