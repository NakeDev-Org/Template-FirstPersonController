using UnityEngine;

namespace nakatimat.TPS.CharacterAnimation
{
    /// <summary>
    /// Coloque este script no MESMO GameObject que contém o componente Animator (ex: a malha do personagem).
    /// Ele captura os eventos de animação (Animation Events) e repassa para o objeto Pai usando SendMessageUpwards.
    /// Dessa forma, você pode manter seus scripts de lógica na cápsula principal, mantendo a arquitetura limpa.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimationEventForwarder : MonoBehaviour
    {
        // Esses métodos capturam os nomes exatos dos eventos disparados pela animação
        // e mandam a Unity procurar por métodos com o mesmo nome nos scripts do objeto Pai (Player).

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnAnimatorMove()
        {
            // Se tiver animação de Root Motion (como o personagem andando pra frente durante o ataque),
            // a Unity chama essa função. Nós pegamos o deslocamento (deltaPosition) e enviamos para o Player.
            if (_animator != null && transform.parent != null)
            {
                transform.parent.SendMessageUpwards(
                    "ApplyRootMotion",
                    _animator,
                    SendMessageOptions.DontRequireReceiver
                );
            }
        }

        private void Equip()
        {
            if (transform.parent != null)
                transform.parent.SendMessageUpwards(
                    "Equip",
                    SendMessageOptions.DontRequireReceiver
                );
        }

        private void Unequip()
        {
            if (transform.parent != null)
                transform.parent.SendMessageUpwards(
                    "Unequip",
                    SendMessageOptions.DontRequireReceiver
                );
        }

        private void Hit()
        {
            if (transform.parent != null)
                transform.parent.SendMessageUpwards("Hit", SendMessageOptions.DontRequireReceiver);
        }

        private void OpenComboWindow()
        {
            if (transform.parent != null)
                transform.parent.SendMessageUpwards(
                    "OpenComboWindow",
                    SendMessageOptions.DontRequireReceiver
                );
        }

        private void CloseComboWindow()
        {
            if (transform.parent != null)
                transform.parent.SendMessageUpwards(
                    "CloseComboWindow",
                    SendMessageOptions.DontRequireReceiver
                );
        }

        private void ResetCombo()
        {
            if (transform.parent != null)
                transform.parent.SendMessageUpwards(
                    "ResetCombo",
                    SendMessageOptions.DontRequireReceiver
                );
        }
    }
}
