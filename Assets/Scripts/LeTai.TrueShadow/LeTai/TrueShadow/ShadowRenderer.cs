using UnityEngine;
using UnityEngine.UI;

namespace LeTai.TrueShadow
{
    [AddComponentMenu("")]
    [ExecuteAlways]
    public partial class ShadowRenderer : MonoBehaviour, ILayoutIgnorer, IMaterialModifier, IMeshModifier
    {
        RawImage graphic;
        RectTransform rt;

        TrueShadow shadow;
        Texture shadowTexture;

        bool willBeDestroyed;

        internal CanvasRenderer CanvasRenderer { get; private set; }

        protected virtual void LateUpdate()
        {
            // Destroy events are not consistently called for some reason, have to poll
            if (!shadow)
                Dispose();
        }

        protected virtual void OnDestroy()
        {
            willBeDestroyed = true;
        }

        public bool ignoreLayout => true;

        public void ModifyMesh(VertexHelper vertexHelper)
        {
            if (!shadow)
                return;

            shadow.ModifyShadowRendererMesh(vertexHelper);
        }

        public void ModifyMesh(Mesh mesh)
        {
            Debug.Assert(true, "This should only be called on old unsupported Unity version");
        }

        public static void Initialize(TrueShadow shadow, ref ShadowRenderer renderer)
        {
            if (renderer && renderer.shadow == shadow)
            {
                renderer.gameObject.SetActive(true);
                return;
            }

            var obj = new GameObject($"{shadow.gameObject.name}'s Shadow")
            {
#if LETAI_TRUESHADOW_DEBUG
            hideFlags = DebugSettings.Instance.showObjects
                            ? HideFlags.DontSave
                            : HideFlags.HideAndDontSave
#else
                hideFlags = HideFlags.HideAndDontSave
#endif
            };

#if LETAI_TRUESHADOW_DEBUG && UNITY_EDITOR
        UnityEditor.SceneVisibilityManager.instance.DisablePicking(obj, true);
#endif

            shadow.SetHierachyDirty();

            var rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;

            var graphic = obj.AddComponent<RawImage>();
            graphic.raycastTarget = false;
            graphic.color = shadow.Color;

            renderer = obj.AddComponent<ShadowRenderer>();
            renderer.shadow = shadow;
            renderer.rt = rt;
            renderer.graphic = graphic;

            // renderer.RecreateGraphic(shadow.Baked ? GraphicType.Image : GraphicType.RawImage);

            renderer.UpdateMaterial();

            renderer.CanvasRenderer = obj.GetComponent<CanvasRenderer>();
            renderer.CanvasRenderer.SetColor(shadow.IgnoreCasterColor ? Color.white : shadow.CanvasRenderer.GetColor());
            renderer.CanvasRenderer.SetAlpha(shadow.CanvasRenderer.GetAlpha());

            renderer.ReLayout();
        }

        public void UpdateMaterial()
        {
            var mat = shadow.BlendMode.GetMaterial();
            graphic.material = mat ? mat : shadow.GetShadowRenderingNormalMaterial();
        }

        internal void ReLayout()
        {
            if (!isActiveAndEnabled)
                return;

            var casterRt = shadow.RectTransform;
            if (!casterRt)
            {
                CanvasRenderer.SetAlpha(0);
                return;
            }

            if (!shadowTexture)
            {
                CanvasRenderer.SetAlpha(0);
                return;
            }

            var container = shadow.ShadowContainer;
            var canvasScale = container?.Snapshot?.canvasScale ?? graphic.canvas.scaleFactor;

            var shadowTexSize = new Vector2(shadowTexture.width, shadowTexture.height) / canvasScale;
            rt.sizeDelta = shadowTexSize;

            // pivot should be relative to the un-blurred part of the texture, not the whole mesh
            var casterPivotLS = -(Vector2) shadow.SpriteMesh.bounds.min;
            var padding = (container?.Padding ?? Mathf.CeilToInt(shadow.Size * canvasScale)) / canvasScale *
                          Vector2.one;
            var misalign = container?.PxMisalignmentAtMinLS ?? Vector2.zero;
            rt.pivot = (casterPivotLS + padding + misalign) / shadowTexSize;


            var canvasRelativeOffset = container?.Snapshot?.canvasRelativeOffset / canvasScale ?? shadow.Offset;
            var offset = shadow.ShadowAsSibling
                ? shadow.Offset.WithZ(0)
                : canvasRelativeOffset.WithZ(0);
            rt.localPosition = shadow.ShadowAsSibling
                ? casterRt.localPosition + offset
                : offset;

            rt.localRotation = shadow.ShadowAsSibling ? casterRt.localRotation : Quaternion.identity;
            rt.localScale = shadow.ShadowAsSibling ? casterRt.localScale : Vector3.one;


            var color = shadow.Color;
            if (shadow.UseCasterAlpha)
                color.a *= shadow.Graphic.color.a;
            graphic.color = color;

            CanvasRenderer.SetColor(shadow.IgnoreCasterColor ? Color.white : shadow.CanvasRenderer.GetColor());
            CanvasRenderer.SetAlpha(shadow.CanvasRenderer.GetAlpha());

            graphic.Rebuild(CanvasUpdate.PreRender);
        }

        public void SetTexture(Texture texture)
        {
            shadowTexture = texture;
            CanvasRenderer.SetTexture(texture);
            graphic.texture = texture;
        }

        public void SetMaterialDirty()
        {
            graphic.SetMaterialDirty();
        }

        public void Dispose()
        {
            if (willBeDestroyed) return;

            if (shadow && shadow.ShadowAsSibling)
            {
                // Destroy does not happen immediately. Want out of hierarchy.
                gameObject.SetActive(false);
                transform.SetParent(null);
            }

#if UNITY_EDITOR
            // This look redundant but is necessary!
            if (!Application.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                DestroyImmediate(gameObject);
            else if (Application.isPlaying)
                Destroy(gameObject);
#else
            Destroy(gameObject);
#endif
        }
    }
}