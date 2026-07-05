using System;
using UnityEngine;

namespace nakatimat.Core
{
    public enum GameState
    {
        Playing,
        Paused,
        InventoryOpen
    }

    /// <summary>
    /// Gerenciador Global de Estado usando ScriptableObject (arquitetura desacoplada).
    /// Evita a necessidade de Singletons pesados. Qualquer script pode referenciar esse SO para saber o estado do jogo.
    /// </summary>
    [CreateAssetMenu(fileName = "GameStateManager", menuName = "NakeDev/Core/GameStateManager")]
    public class GameStateSO : ScriptableObject
    {
        public GameState CurrentState { get; private set; }
        
        public event Action<GameState> OnStateChanged;

        private void OnEnable()
        {
            // Garante que o estado começa jogando (útil em Editor)
            CurrentState = GameState.Playing;
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);

            // Gerenciamento automático do Cursor do Mouse
            if (CurrentState == GameState.Playing)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                
                // Pausa o jogo fisicamente se estiver no menu de pause ou inventário (opcional)
                Time.timeScale = 0f; 
            }
        }

        public bool IsPlaying() => CurrentState == GameState.Playing;
    }
}
