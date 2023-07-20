using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lean.Gui
{
    public class LeanPolygon : MaskableGraphic
    {
        [SerializeField] private float blur;

        [SerializeField] private float thickness;

        [SerializeField] private List<Vector4> points;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

            Color32 color32 = color;
            vh.Clear();

            if (points.Count == 4)
            {
                vh.AddVert(new Vector3(v.x - points[3].x, v.y), color32, new Vector2(0f, 0f));
                vh.AddVert(new Vector3(v.x - points[2].x, v.w), color32, new Vector2(0f, 1f));
                vh.AddVert(new Vector3(v.z - points[1].x, v.w), color32, new Vector2(1f, 1f));
                vh.AddVert(new Vector3(v.z - points[0].x, v.y), color32, new Vector2(1f, 0f));
            }
            else if (points.Count == 2)
            {
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(1f, 1f));
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(1f, 0f));
                vh.AddVert(new Vector3(v.x - points[1].x, v.y), color32, new Vector2(0f, 0f));
                vh.AddVert(new Vector3(v.x - points[0].x, v.w), color32, new Vector2(0f, 1f));
            }
            else if (points.Count == 0)
            {
                vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0f, 0f));
                vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(0f, 1f));
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(1f, 1f));
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(1f, 0f));
            }

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}