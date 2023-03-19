using SWAT.LevelScripts;
using System;
using UnityEditor;
using UnityEngine;

namespace SWAT.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Level))]
    public class LevelEditor : UnityEditor.Editor
    {
        private Level _level;

        private void OnEnable()
        {
            _level = (Level)serializedObject.targetObject;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Add Stage"))
            {
                _level.CreateStage();
            }
        }
    }
}