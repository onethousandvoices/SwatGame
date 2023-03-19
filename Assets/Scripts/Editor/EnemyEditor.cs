using UnityEditor;
using UnityEngine;

namespace SWAT.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Enemy))]
    public class EnemyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("Update parameters"))
            {
                Enemy enemy = (Enemy)serializedObject.targetObject;
                enemy.UpdateConfigValues();
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            
            base.OnInspectorGUI();
        }
    }
}