using UnityEngine;
using UnityEditor;

namespace com.BEISER901.liteglow2d {
    [CustomEditor(typeof(LiteGlow2D))]
    public class LiteGlow2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedProperty typeRender = serializedObject.FindProperty("typeRender");
            SerializedProperty intensity = serializedObject.FindProperty("intensity");
            SerializedProperty size = serializedObject.FindProperty("size");
            SerializedProperty offset = serializedObject.FindProperty("offset");
            SerializedProperty angle = serializedObject.FindProperty("angle");
            SerializedProperty mode = serializedObject.FindProperty("mode");
            SerializedProperty color = serializedObject.FindProperty("color");
            SerializedProperty useTexture = serializedObject.FindProperty("useTexture");
            SerializedProperty _spriteForRender = serializedObject.FindProperty("_spriteForRender");
            SerializedProperty glowRadius = serializedObject.FindProperty("glowRadius");
            SerializedProperty glowSharpness = serializedObject.FindProperty("glowSharpness");

            serializedObject.Update();

            EditorGUILayout.LabelField("Light Sprite 2D", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(typeRender);
            if((LiteGlow2D.TypeRender) typeRender.enumValueIndex == LiteGlow2D.TypeRender.Sprite)
                EditorGUILayout.PropertyField(_spriteForRender);
            
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(intensity);
            EditorGUILayout.PropertyField(size);
            EditorGUILayout.PropertyField(offset);
            EditorGUILayout.PropertyField(angle);
            if((LiteGlow2D.ModeType) mode.enumValueIndex != LiteGlow2D.ModeType.Mask) {        
                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
                EditorGUI.BeginDisabledGroup(
                    mode.enumValueIndex != 2
                );
                EditorGUILayout.PropertyField(glowRadius);
                EditorGUILayout.PropertyField(glowSharpness);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(mode);
            if((LiteGlow2D.ModeType) mode.enumValueIndex != LiteGlow2D.ModeType.Mask) {        
                EditorGUILayout.PropertyField(color);

                EditorGUILayout.PropertyField(useTexture);
            }
                EditorGUILayout.Space(10);
                DrawInfoBox();

                serializedObject.ApplyModifiedProperties();

                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                    EditorApplication.QueuePlayerLoopUpdate();
                }
        }

        void DrawInfoBox()
        {
            var t = (LiteGlow2D)target;
            EditorGUILayout.HelpBox(
                $"Instances: {LiteGlow2D.Instances.Count}\n" +
                $"Mode: {t.Mode}\n" +
                $"Use Texture: {t.UseTexture}",
                MessageType.Info
            );
        }
    }
}