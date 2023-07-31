using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unimage
{
    public class UnimageHandle : IDisposable
    {
        private unsafe void* _handle = null;
        private bool _disposed = false;

        public UnimageHandle()
        {
            Create();
        }

        ~UnimageHandle()
        {
            Dispose();
        }

        private unsafe void Create()
        {
            _handle = Interop.unimage_create();
        }

        public unsafe void Dispose()
        {
            if (_disposed) return;
            Interop.unimage_free(_handle);
            _disposed = true;
        }

        private unsafe void ThrowException()
        {
            var messagePtr = new IntPtr(Interop.unimage_get_error_message(_handle));
            var message = Marshal.PtrToStringAnsi(messagePtr);
            throw new UnimageException(message);
        }

        public unsafe void Load(byte[] data, int width, int height, UnimageFormat format)
        {
            fixed (void* dataPtr = data)
            {
                Interop.unimage_load_raw(_handle, dataPtr, width, height, (byte) format);
            }
        }

        public unsafe void Load(byte[] data)
        {
            fixed (void* dataPtr = data)
            {
                if (Interop.unimage_load(_handle, dataPtr, (uint) data.Length) == 0)
                    ThrowException();
            }
        }

        public unsafe void Load(UnimageHandle handle)
        {
            if (Interop.unimage_copy_from(_handle, handle._handle) == 0)
                ThrowException();
        }

        public unsafe int GetWidth()
        {
            return Interop.unimage_get_width(_handle);
        }

        public unsafe int GetHeight()
        {
            return Interop.unimage_get_height(_handle);
        }

        public unsafe UnimageFormat GetFormat()
        {
            return (UnimageFormat) Interop.unimage_get_format(_handle);
        }

        public unsafe void CopyToMemory(void* memory)
        {
            if (Interop.unimage_copy_to_memory(_handle, memory) == 0)
                ThrowException();
        }

        public unsafe void CopyToMemory(IntPtr ptr)
        {
            CopyToMemory(ptr.ToPointer());
        }

        public unsafe void CopyToMemory(NativeArray<byte> array)
        {
            CopyToMemory(array.GetUnsafePtr());
        }

        public unsafe void Resize(int width, int height)
        {
            if (Interop.unimage_resize(_handle, width, height) == 0)
                ThrowException();
        }

        public unsafe void Clip(int x, int y, int width, int height)
        {
            if (Interop.unimage_clip(_handle, x, y, width, height) == 0)
                ThrowException();
        }

        public unsafe void* GetBuffer()
        {
            return Interop.unimage_get_buffer(_handle);
        }
    }
}