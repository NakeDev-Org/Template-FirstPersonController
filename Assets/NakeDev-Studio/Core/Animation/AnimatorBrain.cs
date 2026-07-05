using UnityEngine;

namespace nakatimat.Core.Animation
{
    public class AnimatorBrain : MonoBehaviour
    {
        [SerializeField] private Animator[] _animators;

        public Animator[] Animators => _animators;

        /// <summary>
        /// Toca uma animação via CrossFade usando string.
        /// O ideal é armazenar hashes para não alocar string a cada chamada, mas para simplicidade na API mantemos string.
        /// </summary>
        public void PlayAnimation(string stateName, float crossfadeTime = 0.1f, int layer = 0)
        {
            if (_animators == null) return;
            
            // Usamos CrossFadeInFixedTime para que transições sejam exatas independente da framerate
            for (int i = 0; i < _animators.Length; i++)
            {
                if (_animators[i] != null)
                    _animators[i].CrossFadeInFixedTime(stateName, crossfadeTime, layer);
            }
        }

        public void PlayAnimation(int stateHash, float crossfadeTime = 0.1f, int layer = 0)
        {
            if (_animators == null) return;
            
            for (int i = 0; i < _animators.Length; i++)
            {
                if (_animators[i] != null)
                    _animators[i].CrossFadeInFixedTime(stateHash, crossfadeTime, layer);
            }
        }
        
        public void SetFloat(int id, float value)
        {
            if (_animators == null) return;

            for (int i = 0; i < _animators.Length; i++)
            {
                if (_animators[i] != null)
                    _animators[i].SetFloat(id, value);
            }
        }
        
        /// <summary>
        /// Retorna a duração do clipe atual tocando numa determinada layer
        /// </summary>
        public float GetCurrentClipLength(int layer = 0)
        {
            if (_animators == null || _animators.Length == 0 || _animators[0] == null) return 0f;
            
            var stateInfo = _animators[0].GetCurrentAnimatorStateInfo(layer);
            return stateInfo.length;
        }
    }
}
