using System;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unimage
{
    public static class UnimageAsyncExtension
    {
        public static Task ResizeAsync(this UnimageProcessor unimageHandle, int width, int height)
        {
            var completionSource = new TaskCompletionSource<bool>();
            lock (UnimageAsyncWorker.Queue)
                UnimageAsyncWorker.Queue.Enqueue(new UnimageAsyncWorker.UnimageAsyncOperation
                {
                    OperationType = UnimageAsyncWorker.UnimageAsyncOperationType.Resize,
                    TaskCompletionSource = completionSource,
                    Handle = unimageHandle,
                    Parameter1 = width,
                    Parameter2 = height
                });
            UnimageAsyncWorker.RunWorkerThreadIfNeed();
            return completionSource.Task;
        }
        
        public static Task ClipAsync(this UnimageProcessor unimageHandle, int x,int y,int width, int height)
        {
            var completionSource = new TaskCompletionSource<bool>();
            lock (UnimageAsyncWorker.Queue)
                UnimageAsyncWorker.Queue.Enqueue(new UnimageAsyncWorker.UnimageAsyncOperation
                {
                    OperationType = UnimageAsyncWorker.UnimageAsyncOperationType.Clip,
                    TaskCompletionSource = completionSource,
                    Handle = unimageHandle,
                    Parameter1 = x,
                    Parameter2 = y,
                    Parameter3 = width,
                    Parameter4 = height,
                });
            UnimageAsyncWorker.RunWorkerThreadIfNeed();
            return completionSource.Task;
        }

        public static Task<bool> LoadAsync(this UnimageProcessor unimageHandle, byte[] data)
        {
            var completionSource = new TaskCompletionSource<bool>();
            lock (UnimageAsyncWorker.Queue)
                UnimageAsyncWorker.Queue.Enqueue(new UnimageAsyncWorker.UnimageAsyncOperation
                {
                    OperationType = UnimageAsyncWorker.UnimageAsyncOperationType.LoadImage,
                    TaskCompletionSource = completionSource,
                    Handle = unimageHandle,
                    Parameter1 = data,
                });
            UnimageAsyncWorker.RunWorkerThreadIfNeed();
            return completionSource.Task;
        }
        
        public static Task<bool> LoadAsync(this UnimageProcessor unimageHandle, Texture2D texture2D)
        {
            var completionSource = new TaskCompletionSource<bool>();
            lock (UnimageAsyncWorker.Queue)
                UnimageAsyncWorker.Queue.Enqueue(new UnimageAsyncWorker.UnimageAsyncOperation
                {
                    OperationType = UnimageAsyncWorker.UnimageAsyncOperationType.LoadPixelData,
                    TaskCompletionSource = completionSource,
                    Handle = unimageHandle,
                    Parameter1 = texture2D.GetRawTextureData(),
                    Parameter2 = texture2D.width,
                    Parameter3 = texture2D.height,
                    Parameter4 = texture2D.format.ToUnimageFormat(),
                    Parameter5 = texture2D, // keep a reference
                });

            UnimageAsyncWorker.RunWorkerThreadIfNeed();
            return completionSource.Task;
        }
        
        public static Task<bool> LoadAsync(this UnimageProcessor unimageHandle, byte[] data, int width, int height, UnimageFormat format)
        {
            var completionSource = new TaskCompletionSource<bool>();
            lock (UnimageAsyncWorker.Queue)
                UnimageAsyncWorker.Queue.Enqueue(new UnimageAsyncWorker.UnimageAsyncOperation
                {
                    OperationType = UnimageAsyncWorker.UnimageAsyncOperationType.LoadPixelData,
                    TaskCompletionSource = completionSource,
                    Handle = unimageHandle,
                    Parameter1 = data,
                    Parameter2 = width,
                    Parameter3 = height,
                    Parameter4 = format
                });
            UnimageAsyncWorker.RunWorkerThreadIfNeed();
            return completionSource.Task;
        }

        public static async Task<Texture2D> GetTextureAsync(this UnimageProcessor unimageHandle, bool mipmap = false, bool linear = true, bool noLongerReadable = true)
        {
            var texture2D = new Texture2D(unimageHandle.Width, unimageHandle.Height,
                unimageHandle.Format.ToTextureFormat(),
                mipmap, linear);
            
            var completionSource = new TaskCompletionSource<bool>();
            lock (UnimageAsyncWorker.Queue)
                unsafe
                {
                    var array = texture2D.GetRawTextureData<byte>();
                    UnimageAsyncWorker.Queue.Enqueue(new UnimageAsyncWorker.UnimageAsyncOperation
                    {
                        OperationType = UnimageAsyncWorker.UnimageAsyncOperationType.CopyToMemeory,
                        TaskCompletionSource = completionSource,
                        Handle = unimageHandle,
                        Parameter1 = new IntPtr(array.GetUnsafePtr()),
                        Parameter4 = texture2D, // keep a reference
                        Parameter5 = array, // keep a reference
                    });
                }

            UnimageAsyncWorker.RunWorkerThreadIfNeed();
            
            await completionSource.Task;
            
            texture2D.Apply(mipmap, noLongerReadable);
            
            return texture2D;
        }
    }
}