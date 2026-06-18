#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace nakatimat.AttackPreview
{
    [CustomEditor(typeof(AttackPreviewHelper))]
    public class AttackPreviewHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AttackPreviewHelper helper = (AttackPreviewHelper)target;

            DrawDefaultInspector();

            EditorGUILayout.Space();

            if (GUILayout.Button("Apply Animation Data Attack"))
            {
                helper.ApplyDataToPreview();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Save Modification"))
            {
                helper.SaveData();
            }
        }
    }
}
#endif
