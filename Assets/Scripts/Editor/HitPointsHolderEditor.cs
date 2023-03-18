using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SWAT.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(HitPointsHolder))]
    public class HitPointsHolderEditor : UnityEditor.Editor
    {
        private SerializedProperty _hitPoint0;
        private SerializedProperty _hitPoint0Value;
        private SerializedProperty _hitPoint1;
        private SerializedProperty _hitPoint1Value;
        private SerializedProperty _hitPoint2;
        private SerializedProperty _hitPoint2Value;
        private SerializedProperty _hitPoint3;
        private SerializedProperty _hitPoint3Value;
        private SerializedProperty _hitPoint4;
        private SerializedProperty _hitPoint4Value;

        private GUIStyle _default;
        private GUIStyle _red;
        private GUIStyle _green;

        private HitPointsHolder _holder;
        private int             _sum;
        
        private void OnEnable()
        {
            _holder = (HitPointsHolder)target;
            
            _hitPoint0      = serializedObject.FindProperty("_hitPoint0");
            _hitPoint0Value = serializedObject.FindProperty("_hitPoint0Value");
            _hitPoint1      = serializedObject.FindProperty("_hitPoint1");
            _hitPoint1Value = serializedObject.FindProperty("_hitPoint1Value");
            _hitPoint2      = serializedObject.FindProperty("_hitPoint2");
            _hitPoint2Value = serializedObject.FindProperty("_hitPoint2Value");
            _hitPoint3      = serializedObject.FindProperty("_hitPoint3");
            _hitPoint3Value = serializedObject.FindProperty("_hitPoint3Value");
            _hitPoint4      = serializedObject.FindProperty("_hitPoint4");
            _hitPoint4Value = serializedObject.FindProperty("_hitPoint4Value");
            
            _default = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                normal =
                {
                    textColor = Color.white
                },
                fontStyle = FontStyle.Bold,
                fontSize  = 13
            };
            
            _red = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                normal =
                {
                    textColor = Color.red
                },
                fontStyle = FontStyle.BoldAndItalic,
                fontSize  = 16
            };
            
            _green = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                normal =
                {
                    textColor = Color.green
                },
                fontStyle = FontStyle.BoldAndItalic,
                fontSize  = 16
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _sum = _holder.Sum;
            
            EditorGUILayout.BeginVertical();

            GUIStyle current = _sum == 100 ? _green : _red;
            
            EditorGUILayout.LabelField($"Total sum is: {_sum}", current);
            EditorGUILayout.LabelField($"Remaining points {100 - _sum}", _default);
            EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------------------");

            EditorGUILayout.LabelField(BreakCamelCase(_hitPoint0.name), _default);
            EditorGUILayout.IntSlider(_hitPoint0Value, 1, 96, "Value");
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField(BreakCamelCase(_hitPoint1.name), _default);
            EditorGUILayout.IntSlider(_hitPoint1Value, 1, 96, "Value");
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField(BreakCamelCase(_hitPoint2.name), _default);
            EditorGUILayout.IntSlider(_hitPoint2Value, 1, 96, "Value");
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField(BreakCamelCase(_hitPoint3.name), _default);
            EditorGUILayout.IntSlider(_hitPoint3Value, 1, 96, "Value");
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField(BreakCamelCase(_hitPoint4.name), _default);
            EditorGUILayout.IntSlider(_hitPoint4Value, 1, 96, "Value");
            EditorGUILayout.LabelField("");

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private static string BreakCamelCase(string str)
        {
            TextInfo text     = new CultureInfo("en-US", false).TextInfo;
            string   replaced = string.Concat(string.Concat(str.Split('_')).Select(c => char.IsUpper(c) || char.IsDigit(c) ? " " + c : c.ToString()));
            return text.ToTitleCase(replaced);
        }
    }
}