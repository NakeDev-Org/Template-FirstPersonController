using System.Collections.Generic;
using nakatimat.ComboFramework.Data;
using UnityEditor;
using UnityEngine;

namespace nakatimat.EditorTools
{
    [CustomEditor(typeof(ComboGraph))]
    public class ComboGraphEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("entryNode"));

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Combo Tree Map", EditorStyles.boldLabel);

            ComboGraph graph = (ComboGraph)target;
            if (graph.entryNode != null)
            {
                EditorGUI.indentLevel++;
                DrawNodeTree(graph.entryNode, new HashSet<ComboNode>());
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Assign an Entry Node to visualize the tree.",
                    MessageType.Info
                );
            }
        }

        private void DrawNodeTree(ComboNode node, HashSet<ComboNode> visitedNodes)
        {
            if (node == null)
                return;

            if (visitedNodes.Contains(node))
            {
                EditorGUILayout.LabelField($"[Loop Detected] -> {node.name}");
                return;
            }

            visitedNodes.Add(node);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"-> {node.name}", EditorStyles.label);
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = node;
            }
            EditorGUILayout.EndHorizontal();

            if (node.nextPossibleNodes != null && node.nextPossibleNodes.Count > 0)
            {
                EditorGUI.indentLevel++;
                foreach (var branch in node.nextPossibleNodes)
                {
                    EditorGUILayout.LabelField($"[{branch.requiredInput}]");
                    DrawNodeTree(branch.nextNode, new HashSet<ComboNode>(visitedNodes));
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
