using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace com.BEISER901.liteglow2d {
    public class LiteGlow2DDebugWindow : EditorWindow
    {
        [MenuItem("LiteGlow2D/Debug")]
        public static void Open()
        {
            GetWindow<LiteGlow2DDebugWindow>("LiteGlow2D ( Debug )");
        }

        private Vector2 scroll;
        private bool showInstances = false;

        public void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.LabelField("Lite Glow 2D", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Global Light", EditorStyles.boldLabel);

            LiteGlow2DFeature.LightScreen =
                EditorGUILayout.Slider("Light", LiteGlow2DFeature.LightScreen, -1f, 1f);

            LiteGlow2DFeature.LightScreenColor =
                EditorGUILayout.ColorField("Light Color", LiteGlow2DFeature.LightScreenColor);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            DrawRTPreview("Shadow Map", LiteGlow2DFeature.shadowMapRT);
            DrawRTPreview("Light Map", LiteGlow2DFeature.lightMapRT);
            DrawRTPreview("Mask Map", LiteGlow2DFeature.maskMapRT);

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical("box");
            showInstances = EditorGUILayout.Foldout(showInstances, "Light / Shadow", true, EditorStyles.foldoutHeader);

            EditorGUILayout.LabelField("Count: " + LiteGlow2D.Instances.Count);

            if (showInstances)
            {
                EditorGUI.indentLevel++;

                for (int i = 0; i < LiteGlow2D.Instances.Count; i++)
                {
                    var l = LiteGlow2D.Instances[i];

                    if (l == null)
                        continue;

                    SerializedObject so = new SerializedObject(l);
                    so.Update();
                    EditorGUI.BeginChangeCheck();

                    SerializedProperty typeRender = so.FindProperty("typeRender");
                    SerializedProperty intensity = so.FindProperty("intensity");
                    SerializedProperty size = so.FindProperty("size");
                    SerializedProperty offset = so.FindProperty("offset");
                    SerializedProperty angle = so.FindProperty("angle");
                    SerializedProperty mode = so.FindProperty("mode");
                    SerializedProperty color = so.FindProperty("color");
                    SerializedProperty useTexture = so.FindProperty("useTexture");
                    SerializedProperty sprite = so.FindProperty("_spriteForRender");
                    SerializedProperty glowRadius = so.FindProperty("glowRadius");
                    SerializedProperty glowSharpness = so.FindProperty("glowSharpness");

                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.LabelField(l.name, EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(typeRender);

                    if ((LiteGlow2D.TypeRender)typeRender.enumValueIndex == LiteGlow2D.TypeRender.Sprite)
                        EditorGUILayout.PropertyField(sprite);

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(intensity);
                    EditorGUILayout.PropertyField(size);
                    EditorGUILayout.PropertyField(offset);
                    EditorGUILayout.PropertyField(angle);

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

                    EditorGUI.BeginDisabledGroup((LiteGlow2D.ModeType)mode.enumValueIndex != LiteGlow2D.ModeType.PlainAlpha);
                    EditorGUILayout.PropertyField(glowRadius);
                    EditorGUILayout.PropertyField(glowSharpness);
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(mode);

                    if ((LiteGlow2D.ModeType)mode.enumValueIndex != LiteGlow2D.ModeType.Mask)
                    {
                        EditorGUILayout.PropertyField(color);
                        EditorGUILayout.PropertyField(useTexture);
                    }

                    EditorGUILayout.EndVertical();

                    so.ApplyModifiedProperties();

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(l);
                        SceneView.RepaintAll();
                        EditorApplication.QueuePlayerLoopUpdate();
                    }

                    EditorGUILayout.Space(5);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        void DrawRTPreview(string title, RTHandle rt)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            if (rt == null || rt.rt == null)
            {
                EditorGUILayout.HelpBox("RT is null", MessageType.Warning);
                return;
            }

            Rect r = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(true));

            Texture tex = rt.rt;

            bool isGameCamera = IsGameCamera();

            if (isGameCamera)
            {
                EditorGUI.DrawTextureTransparent(r, tex, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.DrawTextureTransparent(r, tex, ScaleMode.ScaleToFit);
            }

            EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.05f));
        }

        bool IsGameCamera()
        {
            var cam = SceneView.lastActiveSceneView?.camera;

            if (cam == null)
                return true;

            return cam.cameraType == CameraType.Game;
        }
    }
}