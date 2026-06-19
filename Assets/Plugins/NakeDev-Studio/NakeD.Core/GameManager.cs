using UnityEngine;

namespace nakatimat.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        protected virtual void Awake()
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
            LockCursor();
        }

        /// <summary>
        /// Trava o mouse e esconde ele (Padrão para Gameplay)
        /// </summary>
        public virtual void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Pode ser chamado por menus de Pause no futuro para destravar o mouse
        /// </summary>
        public virtual void UnlockCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        /// Pausa a engine física do jogo e libera o mouse. 
        /// O projeto final pode dar override para abrir menus de UI.
        /// </summary>
        public virtual void PauseGame()
        {
            Time.timeScale = 0f;
            UnlockCursor();
        }

        /// <summary>
        /// Retoma a engine física do jogo e prende o mouse.
        /// </summary>
        public virtual void ResumeGame()
        {
            Time.timeScale = 1f;
            LockCursor();
        }
    }
}
