using System.Collections.Generic;
using UnityEngine;

namespace nakatimat.Core.Animation
{
    /// <summary>
    /// Desliga a atualização contínua do Animator e o faz avançar em blocos de tempo (steps).
    /// Cria o efeito clássico de PS1 (Low FPS / Stop Motion) preservando 100% das lógicas, eventos e blend trees.
    /// Suporta múltiplos animators (ex: Braços e Corpo) rodando com sincronia perfeita.
    /// </summary>
    public class RetroAnimator : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Quantidade de frames por segundo que a animação vai rodar. PS1 usava entre 12 e 15.")]
        [Range(1, 60)]
        public int TargetFPS = 15;

        private Animator[] _animators;
        private float _timeSinceLastFrame;

        private void Awake()
        {
            // Tenta pegar o cérebro primeiro (Single Source of Truth)
            var brain = GetComponent<AnimatorBrain>();
            if (brain != null && brain.Animators != null && brain.Animators.Length > 0)
            {
                _animators = brain.Animators;
            }
            else
            {
                // Fallback: Pega o Animator local (Ex: Objetos simples, portas, hélices)
                var localAnim = GetComponent<Animator>();
                if (localAnim != null) _animators = new Animator[] { localAnim };
            }
        }

        private void OnEnable()
        {
            if (_animators == null) return;
            
            // 1. Desligamos todos os Animators nativos para que a Unity pare de forçar 60/144fps neles.
            for (int i = 0; i < _animators.Length; i++)
            {
                if (_animators[i] != null) _animators[i].enabled = false;
            }
        }

        private void OnDisable()
        {
            if (_animators == null) return;
            
            // Devolve o controle para a Unity caso esse script seja desativado (Feature opcional)
            for (int i = 0; i < _animators.Length; i++)
            {
                if (_animators[i] != null) _animators[i].enabled = true;
            }
        }

        private void Update()
        {
            if (_animators == null || _animators.Length == 0) return;

            // 2. Acumulamos o tempo normal do jogo
            _timeSinceLastFrame += Time.deltaTime;

            // 3. Calculamos o limite de tempo para 1 "Frame de PS1"
            float frameLength = 1f / TargetFPS;

            // 4. Quando o tempo passar do limite, nós disparamos a atualização
            if (_timeSinceLastFrame >= frameLength)
            {
                // Usamos o 'for' tradicional em vez de 'foreach' para garantir 0-allocation (Sem criar lixo na RAM)
                for (int i = 0; i < _animators.Length; i++)
                {
                    if (_animators[i] != null)
                    {
                        // Atualiza exatamente no mesmo milissegundo, garantindo sincronia perfeita
                        _animators[i].Update(_timeSinceLastFrame);
                    }
                }
                
                // Usamos o módulo (%) para zerar o acumulador.
                // Isso garante que se o PC do jogador lagar, a gente não perca frações de matemática!
                _timeSinceLastFrame %= frameLength;
            }
        }
    }
}
