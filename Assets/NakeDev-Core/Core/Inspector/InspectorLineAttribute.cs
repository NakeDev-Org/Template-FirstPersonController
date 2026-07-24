using System;
using UnityEngine;

namespace nakatimat.Core.Inspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class InspectorLineAttribute : PropertyAttribute
    {
        public readonly string Title;
        public readonly float R, G, B;
        public readonly float Spacing;

        /// <summary>
        /// Desenha uma linha colorida no Inspector com um título opcional no meio.
        /// As cores vão de 0 a 255.
        /// </summary>
        public InspectorLineAttribute(
            string title = "",
            float r = 0,
            float g = 255,
            float b = 255,
            float spacing = 35f
        )
        {
            Title = title;
            R = r / 255f;
            G = g / 255f;
            B = b / 255f;
            Spacing = spacing;
        }
    }
}
