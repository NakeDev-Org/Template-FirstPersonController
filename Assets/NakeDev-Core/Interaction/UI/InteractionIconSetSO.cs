using System;
using System.Collections.Generic;
using nakatimat.Core;
using UnityEngine;

namespace nakatimat.InteractionSystem.UI
{
    [Serializable]
    public class DeviceIconEntry
    {
        public InputDeviceType deviceType;
        public Sprite icon;
    }

    /// <summary>
    /// Conjunto reutilizável de ícones de interação (ponto de interesse + um ícone por marca de dispositivo).
    /// Crie um asset uma vez e arraste no IconInteraction de qualquer InteractableObject —
    /// evita reconfigurar os mesmos sprites em cada prefab (plug and play).
    /// </summary>
    [CreateAssetMenu(menuName = "NakeDev/Interaction/Icon Set", fileName = "New Icon Set")]
    public class InteractionIconSetSO : ScriptableObject
    {
        [Tooltip("Ícone de ponto de interesse (Longe, antes de mirar)")]
        public Sprite globalIcon;

        [Tooltip("Ícone por dispositivo (Perto/Mirado). Deixe sem entrada para um tipo pra cair no ícone 'GenericGamepad' como fallback.")]
        public List<DeviceIconEntry> deviceIcons = new List<DeviceIconEntry>();

        public Sprite GetIcon(bool isTargeted, InputDeviceType deviceType)
        {
            if (!isTargeted)
                return globalIcon;

            Sprite fallback = null;

            foreach (var entry in deviceIcons)
            {
                if (entry.deviceType == deviceType && entry.icon != null)
                    return entry.icon;

                if (entry.deviceType == InputDeviceType.GenericGamepad && entry.icon != null)
                    fallback = entry.icon;
            }

            // Nenhum ícone específico pra esse gamepad (ex: Nintendo sem entrada própria) -> usa o genérico.
            return deviceType != InputDeviceType.Keyboard ? fallback : null;
        }
    }
}
