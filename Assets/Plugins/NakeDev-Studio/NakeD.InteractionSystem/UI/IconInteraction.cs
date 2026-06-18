using nakatimat.InteractionSystem;
using TMPro; // Para textos (Opcional)
using UnityEngine;
using UnityEngine.UI;

namespace nakatimat.InteractionSystem.UI
{
    public class IconInteraction : MonoBehaviour
    {
        [Tooltip("Arraste a Imagem do Canvas aqui")]
        public Image _icon;

        [Tooltip(
            "Opcional: Arraste um TextMeshPro se quiser mostrar o texto 'Abra a Porta' ou 'Pegar'"
        )]
        public TextMeshProUGUI _promptText;

        private IInteractable _currentTarget;

        public void Setup(IInteractable interactable)
        {
            _currentTarget = interactable;

            // Se o objeto tiver um ícone dinâmico específico, ele troca o Sprite!
            if (_icon != null && interactable.InteractionIcon != null)
            {
                _icon.sprite = interactable.InteractionIcon;
            }

            // Se você colocar um Texto no Canvas, ele atualiza automaticamente
            if (_promptText != null)
            {
                _promptText.text = interactable.InteractionPrompt;
            }
        }

        // Update is called once per frame
        void LateUpdate() // LateUpdate é melhor para UI seguir câmera para evitar trepidações
        {
            if (_currentTarget != null)
            {
                // Verifica se o objeto foi destruído (Gotcha clássico de Interfaces na Unity)
                if (_currentTarget as MonoBehaviour == null)
                {
                    gameObject.SetActive(false);
                    _currentTarget = null;
                    return;
                }

                // 1. Persegue o objeto ativamente (suporta NPCs e itens com física)
                transform.position = _currentTarget.GetUIPosition();
            }

            // 2. Faz o Canvas olhar para a câmera
            if (Camera.main != null)
            {
                // O transform deste script (que deve estar no Canvas) vai olhar diretamente para a câmera
                transform.LookAt(Camera.main.transform);
                // Como o LookAt vira as costas pro alvo, rodamos 180 graus pra UI não ficar espelhada!
                transform.Rotate(0, 180, 0);
            }
        }
    }
}
