using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace LeTai.TrueShadow
{
    public partial class ShadowRenderer
    {
        // TODO: cleanup unused mask materials
        static readonly Dictionary<(bool, Material), Material> MASK_MATERIALS_CACHE =
            new Dictionary<(bool, Material), Material>();

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!shadow)
                return baseMaterial;

            shadow.ModifyShadowRendererMaterial(baseMaterial);

            if (!baseMaterial.HasProperty(ShaderId.COLOR_MASK) ||
                !baseMaterial.HasProperty(ShaderId.STENCIL_OP))
                return baseMaterial;

            bool casterIsMask = shadow.GetComponent<Mask>() != null;

            MASK_MATERIALS_CACHE.TryGetValue((casterIsMask, baseMaterial), out var mat);

            if (!mat)
            {
                mat = new Material(baseMaterial);

                if (shadow.ShadowAsSibling)
                {
                    // Prevent shadow from writing to stencil mask
                    mat.SetInt(ShaderId.COLOR_MASK, (int) ColorWriteMask.All);
                    mat.SetInt(ShaderId.STENCIL_OP, (int) StencilOp.Keep);
                }
                else if (casterIsMask)
                {
                    // Escape mask if we have one
                    var baseStencilId = mat.GetInt(ShaderId.STENCIL_ID) + 1;
                    int stencilDepth = 0;
                    for (; stencilDepth < 8; stencilDepth++)
                    {
                        if (((baseStencilId >> stencilDepth) & 1) == 1)
                            break;
                    }

                    stencilDepth = Mathf.Max(0, stencilDepth - 1);
                    var stencilId = (1 << stencilDepth) - 1;

                    mat.SetInt(ShaderId.STENCIL_ID, stencilId);
                    mat.SetInt(ShaderId.STENCIL_READ_MASK, stencilId);
                }

                MASK_MATERIALS_CACHE[(casterIsMask, baseMaterial)] = mat;
            }

            return mat;
        }

        internal static void ClearMaskMaterialCache()
        {
            foreach (var keyValuePair in MASK_MATERIALS_CACHE)
            {
                if (Application.isPlaying)
                    Destroy(keyValuePair.Value);
                else
                    DestroyImmediate(keyValuePair.Value);
            }

            MASK_MATERIALS_CACHE.Clear();
        }
    }
}