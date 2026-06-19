using UnityEngine;

namespace nakatimat.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        protected virtual void Start()
        {
            ApplyCursorBehaviour();
        }

        protected virtual void ApplyCursorBehaviour()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Pode ser chamado por menus de Pause no futuro para destravar o mouse
        /// </summary>
        public void UnlockCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
