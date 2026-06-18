using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nakatimat.Core.Inspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SeparatorAttribute : PropertyAttribute
    {
        public readonly string Title;
        public readonly float R,
            G,
            B;
        public readonly float Spacing;

        /// <summary>
        /// Desenha uma linha colorida no Inspector com um título opcional no meio.
        /// As cores vão de 0 a 255.
        /// </summary>
        public SeparatorAttribute(
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

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SeparatorAttribute))]
    public class SeparatorDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            SeparatorAttribute sep = (SeparatorAttribute)attribute;
            return sep.Spacing;
        }

        public override void OnGUI(Rect position)
        {
            SeparatorAttribute sep = (SeparatorAttribute)attribute;
            Color color = new Color(sep.R, sep.G, sep.B, 1f);

            float middleY = position.y + (sep.Spacing / 2f);

            if (!string.IsNullOrEmpty(sep.Title))
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = color;
                style.alignment = TextAnchor.MiddleCenter;

                Vector2 textSize = style.CalcSize(new GUIContent(sep.Title));
                float lineWidth = (position.width - textSize.x - 20f) / 2f;

                // Linha Esquerda
                EditorGUI.DrawRect(
                    new Rect(position.x, middleY, lineWidth, 2),
                    color
                );

                // Texto Centralizado
                EditorGUI.LabelField(
                    new Rect(position.x, middleY - 8, position.width, 20),
                    sep.Title,
                    style
                );

                // Linha Direita
                EditorGUI.DrawRect(
                    new Rect(
                        position.x + lineWidth + textSize.x + 20f,
                        middleY,
                        lineWidth,
                        2
                    ),
                    color
                );
            }
            else
            {
                // Apenas a linha
                EditorGUI.DrawRect(
                    new Rect(position.x, middleY, position.width, 2),
                    color
                );
            }
        }
    }
#endif
}
