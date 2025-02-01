using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MainCore.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class AsyncMethodSequencer
    {
        private readonly SemaphoreSlim _semaphore;

        private readonly Func<UniTask> _action;

        public AsyncMethodSequencer(Func<UniTask> action, int initialCount = 1, int maxCount = 1)
        {
            _semaphore = new SemaphoreSlim(initialCount, maxCount);
            _action = action;
        }
        
        public async UniTask Invoke()
        {
            await _semaphore.WaitAsync(); // 等待信号量，确保只有一个调用可以进入

            try
            {
                await _action(); // 调用你的异步方法
            }
            finally
            {
                _semaphore.Release(); // 释放信号量，允许下一个调用进入
            }
        }
    }
}