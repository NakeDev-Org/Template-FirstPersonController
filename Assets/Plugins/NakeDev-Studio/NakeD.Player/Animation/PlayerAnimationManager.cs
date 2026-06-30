using UnityEngine;

namespace nakatimat.TPS.Player.Modular
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationManager : MonoBehaviour
    {
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Toca uma animação via CrossFade usando string.
        /// O ideal é armazenar hashes para não alocar string a cada chamada, mas para simplicidade na API mantemos string.
        /// </summary>
        public void PlayAnimation(string stateName, float crossfadeTime = 0.1f, int layer = 0)
        {
            if (_animator == null) return;
            
            // Usamos CrossFadeInFixedTime para que transições sejam exatas independente da framerate
            _animator.CrossFadeInFixedTime(stateName, crossfadeTime, layer);
        }

        public void PlayAnimation(int stateHash, float crossfadeTime = 0.1f, int layer = 0)
        {
            if (_animator == null) return;
            _animator.CrossFadeInFixedTime(stateHash, crossfadeTime, layer);
        }
        
        /// <summary>
        /// Retorna a duração do clipe atual tocando numa determinada layer
        /// </summary>
        public float GetCurrentClipLength(int layer = 0)
        {
            if (_animator == null) return 0f;
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(layer);
            return stateInfo.length;
        }
    }
}
