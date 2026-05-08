using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RendererUtils;
using GameLocations;
using System.Collections.Generic;
using System.Linq;

namespace com.BEISER901.liteglow2d {
    public class LiteGlow2DFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public Settings settings = new();

        // ======================================================
        // SHARED
        // ======================================================

        public static RTHandle shadowMapRT;
        public static RTHandle lightMapRT;
        public static RTHandle maskMapRT;
        public static float LightScreen = 0f;
        public static Color LightScreenColor = Color.white;
        private static List<LiteGlow2D> LightBuffer = new();
        private static List<LiteGlow2D> MaskBuffer = new();

        // ======================================================
        // SHADOW PASS
        // ======================================================

        class ShadowPass : ScriptableRenderPass
        {
            RenderTextureDescriptor desc;

            public ShadowPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                desc.colorFormat = RenderTextureFormat.ARGB32;

                RenderingUtils.ReAllocateIfNeeded(
                    ref shadowMapRT,
                    desc,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp,
                    name: "shadowMapRT"
                );
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                LightBuffer.Clear();
                MaskBuffer.Clear();

                CommandBuffer cmd = CommandBufferPool.Get("SHADOW_PASS");

                cmd.SetRenderTarget(shadowMapRT);
                cmd.ClearRenderTarget(true, true, Color.clear);

                foreach (var light in LiteGlow2D.Instances)
                {
                    if((LiteGlow2D.ModeType) light.Mode == LiteGlow2D.ModeType.Mask) {
                        MaskBuffer.Add(light);
                        continue;
                    }
                    else if (light.intensity >= 0f){
                        LightBuffer.Add(light);
                        continue;
                    }

                    DrawNormalLight(
                        cmd,
                        light,
                        shadowMapRT,
                        CoreUtils.CreateEngineMaterial("Hidden/LiteGlow2D/LightMask")
                    );
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        // ======================================================
        // MASK PASS
        // ======================================================

        class MaskPass : ScriptableRenderPass
        {
            RenderTextureDescriptor desc;

            public MaskPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                desc.colorFormat = RenderTextureFormat.ARGB32;

                RenderingUtils.ReAllocateIfNeeded(
                    ref maskMapRT,
                    desc,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp,
                    name: "maskMapRT"
                );
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("MASK_PASS");

                cmd.SetRenderTarget(maskMapRT);
                cmd.ClearRenderTarget(true, true, Color.clear);

                foreach (var light in MaskBuffer)
                {
                    DrawNormalLight(
                        cmd,
                        light,
                        maskMapRT,
                        CoreUtils.CreateEngineMaterial("Hidden/LiteGlow2D/LightMask")
                    );
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        // ======================================================
        // LIGHT PASS
        // ======================================================

        class LightPass : ScriptableRenderPass
        {
            RenderTextureDescriptor desc;

            public LightPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                desc.colorFormat = RenderTextureFormat.ARGB32;

                RenderingUtils.ReAllocateIfNeeded(
                    ref lightMapRT,
                    desc,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp,
                    name: "lightMapRT"
                );
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("LIGHT_PASS");

                cmd.SetRenderTarget(lightMapRT);
                cmd.ClearRenderTarget(true, true, Color.clear);

                foreach (var light in LightBuffer)
                {
                    DrawNormalLight(
                        cmd,
                        light,
                        lightMapRT,
                        CoreUtils.CreateEngineMaterial("Hidden/LiteGlow2D/LightMask")
                    );
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        // ======================================================
        // COMPOSITE PASS
        // ======================================================

        class CompositePass : ScriptableRenderPass
        {
            public CompositePass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {

            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("COMPOSITE_PASS");

                Material runtimeMat =
                    CoreUtils.CreateEngineMaterial("Hidden/LiteGlow2D/Composite");

                runtimeMat.SetTexture("_MainTex", shadowMapRT);
                runtimeMat.SetTexture("_LightTex", lightMapRT);
                runtimeMat.SetTexture("_MaskTex", maskMapRT);

                runtimeMat.SetFloat("_Light", LightScreen);
                runtimeMat.SetColor("_LightColor", LightScreenColor);

                var target =
                    renderingData.cameraData.renderer.cameraColorTargetHandle;

                cmd.SetRenderTarget(target);
                cmd.Blit(shadowMapRT, target, runtimeMat);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        // ======================================================
        // PASSES
        // ======================================================

        private ShadowPass shadowPass;
        private LightPass lightPass;
        private MaskPass maskPass;
        private CompositePass compositePass;

        public override void Create()
        {
            shadowPass = new ShadowPass();
            maskPass = new MaskPass();
            lightPass = new LightPass();
            compositePass = new CompositePass();
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            renderer.EnqueuePass(shadowPass);
            renderer.EnqueuePass(maskPass);
            renderer.EnqueuePass(lightPass);
            renderer.EnqueuePass(compositePass);
        }

        protected override void Dispose(bool disposing)
        {
            shadowMapRT?.Release();
            lightMapRT?.Release();
            maskMapRT?.Release();
        }

        // ======================================================
        // DRAW HELPERS
        // ======================================================

        static void DrawSprite(
            CommandBuffer cmd,
            Sprite s,
            Material m,
            MaterialPropertyBlock propBlock,
            Matrix4x4 matrix)
        {
            if (!s) return;

            Mesh mesh = CreateMesh(s);
            cmd.DrawMesh(mesh, matrix, m, 0, 0, propBlock);
        }

        static Mesh CreateMesh(Sprite sprite)
        {
            Mesh mesh = new Mesh();

            Vector2[] v = sprite.vertices;
            ushort[] t = sprite.triangles;

            Vector3[] verts = new Vector3[v.Length];

            for (int i = 0; i < v.Length; i++)
                verts[i] = new Vector3(v[i].x, v[i].y, 0);

            int[] tris = new int[t.Length];

            for (int i = 0; i < t.Length; i++)
                tris[i] = t[i];

            mesh.vertices = verts;
            mesh.uv = sprite.uv;
            mesh.triangles = tris;

            mesh.RecalculateBounds();

            return mesh;
        }

        static void DrawNormalLight(
            CommandBuffer cmd,
            LiteGlow2D light,
            RTHandle targetRT,
            Material m)
        {
            cmd.SetRenderTarget(targetRT);

            var propBlock = new MaterialPropertyBlock();

            propBlock.SetFloat("_Intensity", light.intensity);
            LiteGlow2D.ModeType mode =
                (LiteGlow2D.ModeType)light.Mode == LiteGlow2D.ModeType.Mask
                    ? LiteGlow2D.ModeType.PlainAlpha
                    : (LiteGlow2D.ModeType)light.Mode;

            propBlock.SetFloat("_Mode", (float)mode);
            propBlock.SetFloat("_UseTexture", light.UseTexture ? 1f : 0f);
            propBlock.SetFloat("_GlowRadius", light.GlowRadius);
            propBlock.SetFloat("_GlowSharpness", light.GlowSharpness);
            propBlock.SetColor("_Color", (LiteGlow2D.ModeType) light.Mode == LiteGlow2D.ModeType.Mask ? Color.white : light.color);
            propBlock.SetFloat("_AlphaCutoff", light.AlphaCutoff);

            if (light.typeRender == LiteGlow2D.TypeRender.Sprite)
            {
                Sprite sprite = light._spriteForRender;

                if (sprite == null)
                    return;

                propBlock.SetTexture("_MainTex", sprite.texture);

                Matrix4x4 matrix = Matrix4x4.TRS(
                    light.transform.position + (Vector3)light.offset,
                    light.transform.rotation * Quaternion.Euler(0f, 0f, light.angle),
                    Vector3.Scale(light.transform.lossyScale, (Vector3)light.size)
                );

                DrawSprite(cmd, sprite, m, propBlock, matrix);
            }
            else
            {
                foreach (var sr in light.spriteRenderers
                    .OrderBy(s => s.sortingLayerID)
                    .ThenBy(s => s.sortingOrder)
                    .ThenBy(s => s.transform.GetSiblingIndex()))
                {
                    if (sr == null)
                        continue;

                    Sprite sprite = sr.sprite;

                    if (sprite == null)
                        continue;

                    propBlock.SetTexture("_MainTex", sprite.texture);

                    Matrix4x4 matrix = Matrix4x4.TRS(
                        sr.transform.position + (Vector3)light.offset,
                        sr.transform.rotation * Quaternion.Euler(0f, 0f, light.angle),
                        Vector3.Scale(sr.transform.lossyScale, (Vector3)light.size)
                    );

                    DrawSprite(cmd, sprite, m, propBlock, matrix);
                }
            }
        }
    }
}