using UnityEditor;
using UnityEngine;

namespace MornLib
{
    [CustomPropertyDrawer(typeof(MornNovelPoseAnimation))]
    internal class MornNovelPoseAnimationDrawer : PropertyDrawer
    {
        private MornNovelSettings _settings;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_settings == null)
            {
                var guids = AssetDatabase.FindAssets("t:MornNovelSettings");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _settings = AssetDatabase.LoadAssetAtPath<MornNovelSettings>(path);
                }
            }

            EditorGUI.BeginProperty(position, label, property);
            {
                DrawProperty(position, property, label);
            }
            EditorGUI.EndProperty();
        }

        private void DrawProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            var animationType = property.FindPropertyRelative("AnimationType");
            var targetTypeProperty = property.FindPropertyRelative("TargetType");
            var stillsProperty = property.FindPropertyRelative("Stills");

            // Adjust the width and height for each PropertyField
            var fieldHeight = EditorGUI.GetPropertyHeight(animationType);
            var fieldWidth = position.width;

            fieldHeight += EditorGUI.GetPropertyHeight(targetTypeProperty);

            EditorGUI.PropertyField(new Rect(position.x, position.y, fieldWidth, fieldHeight), animationType,
                new GUIContent("Animation Type"));
            position.y += fieldHeight + EditorGUIUtility.standardVerticalSpacing;

            var intervalType = (MornNovelPoseAnimationType)animationType.enumValueIndex;

            if (intervalType == MornNovelPoseAnimationType.Custom)
            {
                fieldHeight = EditorGUI.GetPropertyHeight(targetTypeProperty);
                EditorGUI.PropertyField(new Rect(position.x, position.y, fieldWidth, fieldHeight), targetTypeProperty,
                    new GUIContent("Target Type"));
                position.y += fieldHeight + EditorGUIUtility.standardVerticalSpacing;

                fieldHeight = EditorGUI.GetPropertyHeight(stillsProperty);
                EditorGUI.PropertyField(new Rect(position.x, position.y, fieldWidth, fieldHeight), stillsProperty,
                    new GUIContent("Stills"));
            }
            else if (intervalType == MornNovelPoseAnimationType.Blink)
            {
                if (stillsProperty.arraySize == 0)
                {
                    stillsProperty.ClearArray();

                    stillsProperty.InsertArrayElementAtIndex(0);
                    stillsProperty.InsertArrayElementAtIndex(0);
                }
                
                stillsProperty.GetArrayElementAtIndex(0)
                    .FindPropertyRelative("TimeRange")
                    .vector2Value = _settings.EyeOpenRange;

                stillsProperty.GetArrayElementAtIndex(1)
                    .FindPropertyRelative("TimeRange")
                    .vector2Value = _settings.EyeCloseRange;

                var element0 = stillsProperty.GetArrayElementAtIndex(0)
                    .FindPropertyRelative("Sprite");
                var element1 = stillsProperty.GetArrayElementAtIndex(1)
                    .FindPropertyRelative("Sprite");


                fieldHeight = EditorGUI.GetPropertyHeight(element0);
                EditorGUI.PropertyField(new Rect(position.x, position.y, fieldWidth, fieldHeight), element0,
                    new GUIContent("Open Eye"));
                position.y += fieldHeight + EditorGUIUtility.standardVerticalSpacing;

                fieldHeight = EditorGUI.GetPropertyHeight(element1);
                EditorGUI.PropertyField(new Rect(position.x, position.y, fieldWidth, fieldHeight), element1,
                    new GUIContent("Close Eye"));

                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var animationTypeProperty = property.FindPropertyRelative("AnimationType");
            var targetTypeProperty = property.FindPropertyRelative("TargetType");
            var stillsProperty = property.FindPropertyRelative("Stills");

            var height = EditorGUI.GetPropertyHeight(animationTypeProperty);

            var intervalType = (MornNovelPoseAnimationType)animationTypeProperty.enumValueIndex;

            if (intervalType == MornNovelPoseAnimationType.Custom)
            {
                height += EditorGUI.GetPropertyHeight(stillsProperty) + EditorGUI.GetPropertyHeight(targetTypeProperty);
            }
            else
            {
                if (stillsProperty.arraySize == 0)
                {
                    stillsProperty.InsertArrayElementAtIndex(0);
                    stillsProperty.InsertArrayElementAtIndex(0);
                }

                var element0 = stillsProperty.GetArrayElementAtIndex(0)
                    .FindPropertyRelative("Sprite");
                var element1 = stillsProperty.GetArrayElementAtIndex(1)
                    .FindPropertyRelative("Sprite");

                height += EditorGUI.GetPropertyHeight(element0)
                          + EditorGUI.GetPropertyHeight(element1);
            }

            return height;
        }
    }
}