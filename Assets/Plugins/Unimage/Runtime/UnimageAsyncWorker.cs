using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unimage
{
    public static class UnimageAsyncWorker
    {
        internal static readonly Queue<UnimageAsyncOperation> Queue = new();
        internal class UnimageAsyncOperation
        {
            public UnimageAsyncOperationType OperationType;
            public UnimageProcessor Handle;
            public TaskCompletionSource<bool> TaskCompletionSource;
            public object Parameter1;
            public object Parameter2;
            public object Parameter3;
            public object Parameter4;
            public object Parameter5;
        }
        internal enum UnimageAsyncOperationType
        {
            LoadPixelData,
            LoadImage,
            Resize,
            Clip,
            CopyToMemeory
        }

        private static object _workerThreadLock = new();
        private static Thread _workerThread = null;

        private static void WorkerThread()
        {
            var idleTime = 0;
            while (true)
            {
                if (idleTime > 20)
                {
                    break;
                }

                UnimageAsyncOperation asyncOperation = null;
                lock (Queue)
                {
                    if (!Queue.TryDequeue(out asyncOperation))
                        idleTime++;
                    else idleTime = 0;
                }

                if (asyncOperation != null) ExecuteAsyncOperation(asyncOperation);
            }

            lock (_workerThreadLock) _workerThread = null;
        }

        private static void ExecuteAsyncOperation(UnimageAsyncOperation operation)
        {
            try
            {
                switch (operation.OperationType)
                {
                    case UnimageAsyncOperationType.LoadPixelData:
                        ExecuteLoadPixelDataOperation(operation);
                        break;

                    case UnimageAsyncOperationType.Resize:
                        ExecuteResizeOperation(operation);
                        break;
                    
                    case UnimageAsyncOperationType.LoadImage:
                        ExecuteLoadImageOperation(operation);
                        break;
                    
                    case UnimageAsyncOperationType.Clip:
                        ExecuteClipOperation(operation);
                        break;
                        
                    case UnimageAsyncOperationType.CopyToMemeory:
                        ExecuteCopyToMemoryOperation(operation);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                operation.TaskCompletionSource.SetResult(true);
            }
            catch (Exception ex)
            {
                operation.TaskCompletionSource.SetException(ex);
            }
        }

        private static void ExecuteCopyToMemoryOperation(UnimageAsyncOperation operation)
        {
            operation.Handle.Handle.CopyToMemory((IntPtr) operation.Parameter1);
        }

        private static void ExecuteClipOperation(UnimageAsyncOperation operation)
        {
            operation.Handle.Clip((int) operation.Parameter1, (int) operation.Parameter2, (int) operation.Parameter3,
                (int) operation.Parameter4);
        }

        private static void ExecuteLoadImageOperation(UnimageAsyncOperation operation)
        {
            operation.Handle.Load((byte[]) operation.Parameter1);
        }

        private static void ExecuteLoadPixelDataOperation(UnimageAsyncOperation operation)
        {
            operation.Handle.Load((byte[]) operation.Parameter1, (int) operation.Parameter2, (int) operation.Parameter3,
                (UnimageFormat) operation.Parameter4);
        }

        private static void ExecuteResizeOperation(UnimageAsyncOperation operation)
        {
            operation.Handle.Resize((int) operation.Parameter1, (int) operation.Parameter2);
        }

        public static void RunWorkerThreadIfNeed()
        {
            lock (_workerThreadLock)
            {
                if (_workerThread != null) return;
                
                _workerThread = new Thread(WorkerThread);
                _workerThread.Start();
            }
        }
    }
}