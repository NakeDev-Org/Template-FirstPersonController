using UnityEngine;
using UnityEngine.InputSystem;

namespace nakatimat.InspectSystem
{
    public class InspectSystem : MonoBehaviour
    {
        [Tooltip("Arraste o InputReader do Player (ou da Cena) aqui")]
        public nakatimat.Player.InputReader inputReader;

        public Transform objectToInspect;
        public float rotationSpeed = 10f; // Ajustado para o delta do Novo Input System

        void Update()
        {
            if (inputReader == null || objectToInspect == null) return;

            // Opção B: Rotação livre (não precisa segurar botão)
            Vector2 lookDelta = inputReader.RawLookInput;
            
            // Usando unscaledDeltaTime para girar mesmo com o jogo pausado (TimeScale = 0)
            float rotationX = lookDelta.y * rotationSpeed * Time.unscaledDeltaTime;
            float rotationY = -lookDelta.x * rotationSpeed * Time.unscaledDeltaTime;

            Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
            objectToInspect.rotation = rotation * objectToInspect.rotation;
        }
    }
}