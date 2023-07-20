using UnityEngine;

namespace LeTai.TrueShadow
{
    class ShadowContainer
    {
        public readonly int requestHash;

        internal ShadowContainer(RenderTexture texture,
            ShadowSettingSnapshot snapshot,
            int padding,
            Vector2 pxMisalignmentAtMinLS)
        {
            Texture = texture;
            Snapshot = snapshot;
            Padding = padding;
            PxMisalignmentAtMinLS = pxMisalignmentAtMinLS;
            RefCount = 1;
            requestHash = snapshot.GetHashCode();
        }

        public RenderTexture Texture { get; }
        public ShadowSettingSnapshot Snapshot { get; }
        public int Padding { get; }
        public Vector2 PxMisalignmentAtMinLS { get; }

        public int RefCount { get; internal set; }
    }
}