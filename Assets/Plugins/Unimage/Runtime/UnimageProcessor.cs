using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unimage
{
    public class UnimageProcessor : ICloneable, IDisposable
    {
        public UnimageHandle Handle { get; } = new();

        public int Width => Handle.GetWidth();
        public int Height => Handle.GetHeight();
        public UnimageFormat Format => Handle.GetFormat();

        object ICloneable.Clone()
        {
            return Clone();
        }
        
        public UnimageProcessor Clone()
        {
            var newProcessor = new UnimageProcessor();
            newProcessor.Handle.Load(Handle);
            return newProcessor;
        }

        public void Resize(int width, int height)
        {
            Handle.Resize(width, height);
        }

        public void Clip(int x, int y, int width, int height)
        {
            Handle.Clip(x, y, width, height);
        }

        public Texture2D GetTexture(bool mipmap = false, bool linear = true, bool noLongerReadable = true)
        {
            var texture = new Texture2D(Width, Height, Format.ToTextureFormat(), mipmap, linear);
            var array = texture.GetRawTextureData<byte>();

            Handle.CopyToMemory(array);

            texture.Apply(mipmap, noLongerReadable);
            return texture;
        }

        public void Load(byte[] imageFile)
        {
            Handle.Load(imageFile);
        }

        public void Load(Texture2D texture2D)
        {
            Handle.Load(texture2D.GetRawTextureData(), texture2D.width, texture2D.height,
                texture2D.format.ToUnimageFormat());
        }

        public void Load(byte[] pixelData, int width, int height, UnimageFormat format)
        {
            Handle.Load(pixelData, width, height, format);
        }

        public void Dispose()
        {
            Handle?.Dispose();
        }
    }
}