using System.Collections.Generic;
using LeTai.Effects;
using UnityEngine;
using UnityEngine.Rendering;

namespace LeTai.TrueShadow
{
    public class ShadowFactory
    {
        private static ShadowFactory instance;
        public static ShadowFactory Instance => instance ?? (instance = new ShadowFactory());

        readonly Dictionary<int, ShadowContainer> shadowCache =
            new Dictionary<int, ShadowContainer>();

        readonly CommandBuffer cmd;
        readonly MaterialPropertyBlock materialProps;
        readonly ScalableBlur blurProcessor;
        readonly ScalableBlurConfig blurConfig;

        Material cutoutMaterial;
        Material imprintPostProcessMaterial;
        Material shadowPostProcessMaterial;

        Material CutoutMaterial =>
            cutoutMaterial ? cutoutMaterial : cutoutMaterial = new Material(Shader.Find("Hidden/TrueShadow/Cutout"));

        Material ImprintPostProcessMaterial =>
            imprintPostProcessMaterial
                ? imprintPostProcessMaterial
                : imprintPostProcessMaterial = new Material(Shader.Find("Hidden/TrueShadow/ImprintPostProcess"));

        Material ShadowPostProcessMaterial =>
            shadowPostProcessMaterial
                ? shadowPostProcessMaterial
                : shadowPostProcessMaterial = new Material(Shader.Find("Hidden/TrueShadow/PostProcess"));

        private ShadowFactory()
        {
            cmd = new CommandBuffer {name = "Shadow Commands"};
            materialProps = new MaterialPropertyBlock();
            materialProps.SetVector(ShaderId.CLIP_RECT,
                new Vector4(float.NegativeInfinity, float.NegativeInfinity,
                    float.PositiveInfinity, float.PositiveInfinity));
            materialProps.SetInt(ShaderId.COLOR_MASK,
                (int) ColorWriteMask.All); // Render shadow even if mask hide graphic

            ShaderProperties.Init(8);
            blurConfig = ScriptableObject.CreateInstance<ScalableBlurConfig>();
            blurConfig.hideFlags = HideFlags.HideAndDontSave;
            blurProcessor = new ScalableBlur();
            blurProcessor.Configure(blurConfig);
        }

        ~ShadowFactory()
        {
            cmd.Dispose();
            Utility.SafeDestroy(blurConfig);
            Utility.SafeDestroy(cutoutMaterial);
            Utility.SafeDestroy(imprintPostProcessMaterial);
        }

#if LETAI_TRUESHADOW_DEBUG
    RenderTexture debugTexture;
#endif

        // public int createdContainerCount;
        // public int releasedContainerCount;

        internal void Get(ShadowSettingSnapshot snapshot, ref ShadowContainer container)
        {
            if (float.IsNaN(snapshot.dimensions.x) || snapshot.dimensions.x < 1 ||
                float.IsNaN(snapshot.dimensions.y) || snapshot.dimensions.y < 1)
            {
                ReleaseContainer(container);
                return;
            }

#if LETAI_TRUESHADOW_DEBUG
        RenderTexture.ReleaseTemporary(debugTexture);
        if (snapshot.shadow.alwaysRender)
            debugTexture = GenerateShadow(snapshot).Texture;
#endif

            // Each request need a coresponding shadow texture
            // Texture may be shared by multiple elements
            // Texture are released when no longer used by any element
            // ShadowContainer keep track of texture and their usage


            int requestHash = snapshot.GetHashCode();

            // Case: requester can keep the same texture
            if (container?.requestHash == requestHash)
                return;

            ReleaseContainer(container);

            if (shadowCache.TryGetValue(requestHash, out var existingContainer))
            {
                // Case: requester got texture from someone else
                existingContainer.RefCount++;
                container = existingContainer;
            }
            else
            {
                // Case: requester got new unique texture
                container = shadowCache[requestHash] = GenerateShadow(snapshot);
                // Debug.Log($"Created new container for request\t{requestHash}\tTotal Created: {++createdContainerCount}\t Alive: {createdContainerCount - releasedContainerCount}");
            }
        }

        internal void ReleaseContainer(ShadowContainer container)
        {
            if (container == null)
                return;

            if (--container.RefCount > 0)
                return;

            RenderTexture.ReleaseTemporary(container.Texture);
            shadowCache.Remove(container.requestHash);

            // Debug.Log($"Released container for request\t{container.requestHash}\tTotal Released: {++releasedContainerCount}\t Alive: {createdContainerCount - releasedContainerCount}");
        }

        static readonly Rect UNIT_RECT = new Rect(0, 0, 1, 1);

        ShadowContainer GenerateShadow(ShadowSettingSnapshot snapshot)
        {
            // return GenColoredTexture(request.GetHashCode());

            cmd.Clear();
            cmd.BeginSample("TrueShadow:Capture");

            var bounds = snapshot.shadow.SpriteMesh.bounds;
            var misalignment =
                CalcMisalignment(snapshot.canvas, snapshot.canvasRt, snapshot.shadow.RectTransform, bounds);

            var padding = Mathf.CeilToInt(snapshot.size);
            var imprintViewW = Mathf.RoundToInt(snapshot.dimensions.x + misalignment.bothSS.x);
            var imprintViewH = Mathf.RoundToInt(snapshot.dimensions.y + misalignment.bothSS.y);
            var tw = imprintViewW + padding * 2;
            var th = imprintViewH + padding * 2;

            var shadowTex = RenderTexture.GetTemporary(tw, th, 0, RenderTextureFormat.ARGB32);
            var imprintTexDesc = shadowTex.descriptor;
            imprintTexDesc.msaaSamples =
                snapshot.shouldAntialiasImprint ? Mathf.Max(1, QualitySettings.antiAliasing) : 1;
            var imprintTex = RenderTexture.GetTemporary(imprintTexDesc);

            RenderTexture imprintTexProcessed = null;

            bool needProcessImprint = snapshot.shadow.IgnoreCasterColor || snapshot.shadow.Inset;
            if (needProcessImprint)
                imprintTexProcessed = RenderTexture.GetTemporary(imprintTexDesc);

            var texture = snapshot.shadow.Content;
            if (texture)
                materialProps.SetTexture(ShaderId.MAIN_TEX, texture);
            else
                materialProps.SetTexture(ShaderId.MAIN_TEX, Texture2D.whiteTexture);

            cmd.SetRenderTarget(imprintTex);
            cmd.ClearRenderTarget(true, true, snapshot.shadow.ClearColor);

            cmd.SetViewport(new Rect(padding, padding, imprintViewW, imprintViewH));

            var imprintBoundMin = (Vector2) bounds.min - misalignment.minLS;
            var imprintBoundMax = (Vector2) bounds.max + misalignment.maxLS;
            cmd.SetViewProjectionMatrices(
                Matrix4x4.identity,
                Matrix4x4.Ortho(imprintBoundMin.x, imprintBoundMax.x,
                    imprintBoundMin.y, imprintBoundMax.y,
                    -1, 1)
            );

            snapshot.shadow.ModifyShadowCastingMesh(snapshot.shadow.SpriteMesh);
            snapshot.shadow.ModifyShadowCastingMaterialProperties(materialProps);
            cmd.DrawMesh(snapshot.shadow.SpriteMesh,
                Matrix4x4.identity,
                snapshot.shadow.GetShadowCastingMaterial(),
                0, 0,
                materialProps);

            if (needProcessImprint)
            {
                ImprintPostProcessMaterial.SetKeyword("BLEACH", snapshot.shadow.IgnoreCasterColor);
                ImprintPostProcessMaterial.SetKeyword("INSET", snapshot.shadow.Inset);

                cmd.Blit(imprintTex, imprintTexProcessed, ImprintPostProcessMaterial);
            }

            cmd.EndSample("TrueShadow:Capture");

            var needPostProcess = snapshot.shadow.Spread > 1e-3;

            cmd.BeginSample("TrueShadow:Cast");
            RenderTexture blurSrc = needProcessImprint ? imprintTexProcessed : imprintTex;
            RenderTexture blurDst;
            if (needPostProcess)
                blurDst = RenderTexture.GetTemporary(shadowTex.descriptor);
            else
                blurDst = shadowTex;

            if (snapshot.size < 1e-2)
            {
                cmd.Blit(blurSrc, blurDst);
            }
            else
            {
                blurConfig.Strength = snapshot.size;
                blurProcessor.Blur(cmd, blurSrc, UNIT_RECT, blurDst);
            }

            cmd.EndSample("TrueShadow:Cast");

            var relativeOffset = new Vector2(snapshot.canvasRelativeOffset.x / tw,
                snapshot.canvasRelativeOffset.y / th);
            var overflowAlpha = snapshot.shadow.Inset ? 1 : 0;
            if (needPostProcess)
            {
                cmd.BeginSample("TrueShadow:PostProcess");

                ShadowPostProcessMaterial.SetTexture(ShaderId.SHADOW_TEX, blurDst);
                ShadowPostProcessMaterial.SetVector(ShaderId.OFFSET, relativeOffset);
                ShadowPostProcessMaterial.SetFloat(ShaderId.OVERFLOW_ALPHA, overflowAlpha);
                ShadowPostProcessMaterial.SetFloat(ShaderId.ALPHA_MULTIPLIER,
                    1f / Mathf.Max(1e-6f, 1f - snapshot.shadow.Spread));

                cmd.SetViewport(UNIT_RECT);
                cmd.Blit(blurSrc, shadowTex, ShadowPostProcessMaterial);

                cmd.EndSample("TrueShadow:PostProcess");
            }
            else if (snapshot.shadow.Cutout)
            {
                cmd.BeginSample("TrueShadow:Cutout");

                CutoutMaterial.SetVector(ShaderId.OFFSET, relativeOffset);
                CutoutMaterial.SetFloat(ShaderId.OVERFLOW_ALPHA, overflowAlpha);

                cmd.SetViewport(UNIT_RECT);
                cmd.Blit(blurSrc, shadowTex, CutoutMaterial);

                cmd.EndSample("TrueShadow:Cutout");
            }

            Graphics.ExecuteCommandBuffer(cmd);

            RenderTexture.ReleaseTemporary(imprintTex);
            RenderTexture.ReleaseTemporary(blurSrc);
            if (needPostProcess)
                RenderTexture.ReleaseTemporary(blurDst);

            return new ShadowContainer(shadowTex, snapshot, padding, misalignment.minLS);
        }

        readonly struct PixelMisalignment
        {
            public readonly Vector2 bothSS;
            public readonly Vector2 minLS;
            public readonly Vector2 maxLS;

            public PixelMisalignment(Vector2 bothSS, Vector2 minLS, Vector2 maxLS)
            {
                this.bothSS = bothSS;
                this.minLS = minLS;
                this.maxLS = maxLS;
            }
        }

        PixelMisalignment CalcMisalignment(Canvas canvas, RectTransform canvasRt, RectTransform casterRt,
            Bounds meshBound)
        {
            PixelMisalignment misalignment;

            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                misalignment = new PixelMisalignment();
            }
            else
            {
                var referenceCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;

                var pxMisalignmentAtMin = casterRt.LocalToScreenPoint(meshBound.min, referenceCamera).Frac();
                var pxMisalignmentAtMax =
                    Vector2.one - casterRt.LocalToScreenPoint(meshBound.max, referenceCamera).Frac();
                if (pxMisalignmentAtMax.x > 1 - 1e-5)
                    pxMisalignmentAtMax.x = 0;
                if (pxMisalignmentAtMax.y > 1 - 1e-5)
                    pxMisalignmentAtMax.y = 0;

                misalignment = new PixelMisalignment(
                    pxMisalignmentAtMin + pxMisalignmentAtMax,
                    canvasRt.ScreenToCanvasSize(pxMisalignmentAtMin, referenceCamera),
                    canvasRt.ScreenToCanvasSize(pxMisalignmentAtMax, referenceCamera)
                );
            }

            return misalignment;
        }

        RenderTexture GenColoredTexture(int hash)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixels32(new[] {new Color32((byte) (hash >> 8), (byte) (hash >> 16), (byte) (hash >> 24), 255)});
            tex.Apply();

            var rt = RenderTexture.GetTemporary(1, 1);
            Graphics.Blit(tex, rt);

            return rt;
        }
    }
}