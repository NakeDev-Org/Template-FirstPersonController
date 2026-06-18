using nakatimat.ComboFramework.Data;
using UnityEditor;
using UnityEngine;

namespace nakatimat.EditorTools
{
    [CustomEditor(typeof(ComboNode))]
    public class ComboNodeEditor : Editor
    {
        private int currentTab = 0;
        private readonly string[] tabs = { "Combat & Branching", "Timings", "Visuals & Physics" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            currentTab = GUILayout.Toolbar(currentTab, tabs);
            EditorGUILayout.Space();

            switch (currentTab)
            {
                case 0:
                    DrawCombatTab();
                    break;
                case 1:
                    DrawTimingsTab();
                    break;
                case 2:
                    DrawVisualsTab();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCombatTab()
        {
            EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationStateName"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat Stats", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("damageMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaCost"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("requireGrounded"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredMoveDirection"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Branching", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nextPossibleNodes"), true);
        }

        private void DrawTimingsTab()
        {
            EditorGUILayout.LabelField("Normalized Timings (0.0 to 1.0)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hitboxStartTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hitboxEndTime"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("comboWindowStartTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("comboWindowEndTime"));
        }

        private void DrawVisualsTab()
        {
            EditorGUILayout.LabelField("Editor Preview", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("previewClip"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("VFX Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vfxPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vfxPosition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vfxRotation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vfxScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vfxColor"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("HitBox Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hitBoxCenter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hitBoxSize"));
        }
    }
}
