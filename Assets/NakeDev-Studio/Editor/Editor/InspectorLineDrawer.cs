#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using nakatimat.Core.Inspector;

namespace NakeDev.EditorTools
{
    [CustomPropertyDrawer(typeof(InspectorLineAttribute))]
    public class InspectorLineDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            InspectorLineAttribute sep = (InspectorLineAttribute)attribute;
            return sep.Spacing;
        }

        public override void OnGUI(Rect position)
        {
            InspectorLineAttribute sep = (InspectorLineAttribute)attribute;
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
}
#endif
