using System.Collections.Generic;
using System.Linq;
using LeTai.TrueShadow.PluginInterfaces;
using UnityEngine;
using UnityEngine.UI;

namespace LeTai.TrueShadow
{
    public partial class TrueShadow
    {
        readonly List<Color32> meshColors = new List<Color32>(4);
        ITrueShadowCasterClearColorProvider casterClearColorProvider;
        ITrueShadowCasterMaterialPropertiesModifier casterMaterialPropertiesModifier;
        ITrueShadowCasterMaterialProvider casterMaterialProvider;
        ITrueShadowCasterMeshModifier casterMeshModifier;
        List<Color32> meshColorsOpaque = new List<Color32>(4);
        ITrueShadowRendererMaterialModifier rendererMaterialModifier;
        ITrueShadowRendererMaterialProvider rendererMaterialProvider;
        ITrueShadowRendererMeshModifier rendererMeshModifier;

        public bool UsingRendererMaterialProvider => rendererMaterialProvider != null;

        void InitializePlugins()
        {
            casterMaterialProvider = GetComponent<ITrueShadowCasterMaterialProvider>();
            casterMaterialPropertiesModifier = GetComponent<ITrueShadowCasterMaterialPropertiesModifier>();
            casterMeshModifier = GetComponent<ITrueShadowCasterMeshModifier>();
            casterClearColorProvider = GetComponent<ITrueShadowCasterClearColorProvider>();
            if (casterClearColorProvider != null)
                ColorBleedMode = ColorBleedMode.Plugin;

            rendererMaterialProvider = GetComponent<ITrueShadowRendererMaterialProvider>();
            rendererMaterialModifier = GetComponent<ITrueShadowRendererMaterialModifier>();
            rendererMeshModifier = GetComponent<ITrueShadowRendererMeshModifier>();
        }

        public virtual Material GetShadowCastingMaterial()
        {
            return casterMaterialProvider != null
                ? casterMaterialProvider.GetTrueShadowCasterMaterial()
                : Graphic.material;
        }

        public virtual void ModifyShadowCastingMaterialProperties(MaterialPropertyBlock propertyBlock)
        {
            casterMaterialPropertiesModifier?.ModifyTrueShadowCasterMaterialProperties(propertyBlock);
        }

        public virtual void ModifyShadowCastingMesh(Mesh mesh)
        {
            casterMeshModifier?.ModifyTrueShadowCasterMesh(mesh);

            // Caster can be semi-transparent, but cutout requires mostly opaque stencil.
            // Setting alpha to 1 in fragment can't work because of antialiasing.
            MakeOpaque(mesh);
        }

        void MakeOpaque(Mesh mesh)
        {
            if (shadowAsSibling)
                return;

            mesh.GetColors(meshColors);
            var meshColorCount = meshColors.Count;

            if (meshColorCount < 1) return;

            if (meshColorsOpaque.Count == meshColorCount)
            {
                // Assuming vertex colors are identical
                // TODO: This is the case for builtin graphics, but userscript may invalidate that.
                if (meshColors[0].a == meshColorsOpaque[0].a)
                    return;
            }
            else
            {
                // TODO: This assumed vertex count change infrequently. Is not the case with Text
                meshColorsOpaque.Clear();
                meshColorsOpaque.AddRange(Enumerable.Repeat(new Color32(0, 0, 0, 0), meshColorCount));
            }

            for (var i = 0; i < meshColorCount; i++)
            {
                var c = meshColors[i];
                c.a = 255;

                meshColorsOpaque[i] = c;
            }

            mesh.SetColors(meshColorsOpaque);
        }

        public virtual Material GetShadowRenderingNormalMaterial()
        {
            return rendererMaterialProvider?.GetTrueShadowRendererMaterial();
        }

        public virtual void ModifyShadowRendererMaterial(Material baseMaterial)
        {
            rendererMaterialModifier?.ModifyTrueShadowRendererMaterial(baseMaterial);
        }

        public virtual void ModifyShadowRendererMesh(VertexHelper vertexHelper)
        {
            rendererMeshModifier?.ModifyTrueShadowRenderMesh(vertexHelper);
        }
    }
}