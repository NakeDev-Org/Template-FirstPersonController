using System;
using UnityEngine;

namespace nakatimat.Core
{
    /// <summary>
    /// Abstração mínima de input que os sistemas de Interaction/Inspection precisam.
    /// Qualquer InputReader concreto (do template FPS, de um projeto em terceira pessoa, etc.)
    /// implementa essa interface — assim esse pacote nunca depende de um controller específico.
    /// </summary>
    public interface IInteractionInput
    {
        event Action OnInteractionPressed;
        event Action OnCancelPressed;
        Vector2 RawLookInput { get; }
        bool IsGamepad { get; }
        InputDeviceType CurrentDeviceType { get; }
    }
}
