using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using nakatimat.InteractionSystem;

namespace NakeDev.EditorTools
{
    [CustomEditor(typeof(InteractableObject))]
    public class InteractableObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty _iconPrefabProp;
        private SerializedProperty _actionsToExecuteProp;
        private ReorderableList _actionsList;

        private void OnEnable()
        {
            _iconPrefabProp = serializedObject.FindProperty("_iconPrefab");
            _actionsToExecuteProp = serializedObject.FindProperty("_actionsToExecute");

            // Configuração da lista premium (Estilo UnityEvent)
            _actionsList = new ReorderableList(serializedObject, _actionsToExecuteProp, true, true, true, true);

            // Cabeçalho escuro da lista
            _actionsList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Actions To Execute (Scriptable Objects)");
            };

            // Desenha os itens dentro da caixa cinza
            _actionsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _actionsList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                
                // Desenha a propriedade sem o texto (label) ao lado, usando a largura toda
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    element, 
                    GUIContent.none
                );
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configuração de UI", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_iconPrefabProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Lógica Modular", EditorStyles.boldLabel);
            
            // O segredo para desenhar a lista bonitinha
            _actionsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
