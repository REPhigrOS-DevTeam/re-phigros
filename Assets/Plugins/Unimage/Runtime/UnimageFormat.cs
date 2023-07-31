using System;
using UnityEngine;

namespace Unimage
{
    public enum UnimageFormat : byte
    {
        None = 0,
        RGB = 1,
        RGBA = 2
    }

    public static class UnimageFormatExtension
    {
        public static TextureFormat ToTextureFormat(this UnimageFormat format)
        {
            return format switch
            {
                UnimageFormat.RGB => TextureFormat.RGB24,
                UnimageFormat.RGBA => TextureFormat.RGBA32,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static UnimageFormat ToUnimageFormat(this TextureFormat format)
        {
            return format switch
            {
                TextureFormat.RGB24 => UnimageFormat.RGB,
                TextureFormat.RGBA32 => UnimageFormat.RGBA,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}