using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MainCore.Utilities.ResourceManager
{
    public static class TextureReader
    {
        private const int MaximumLoadsInBatch = 3;
        private const int DelayBetweenBatches = 100;
        private static readonly List<LoadHandler> LoadingHandlers = new();
        private static readonly Queue<LoadHandler> PendingHandlers = new();

        // ReSharper disable Unity.PerformanceAnalysis
        public static async UniTask<Texture2D> ReadTextureByUrl(string url)
        {
            var handler = new LoadHandler {Url = url};
            PendingHandlers.Enqueue(handler);
            await ScheduleLoadTasks();
            await UniTask.WaitUntil(() => handler.Completed);
            return handler.Texture;
        }
        
        public static async UniTask<Texture2D> ReadLocalTextureByPath(string path)
        {
            var url = $"file://{path}";
            var handler = new LoadHandler {Url = url};
            PendingHandlers.Enqueue(handler);
            await ScheduleLoadTasks();
            await UniTask.WaitUntil(() => handler.Completed);
            return handler.Texture;
        }

        private static async UniTask ScheduleLoadTasks()
        {
            while (LoadingHandlers.Count < MaximumLoadsInBatch && PendingHandlers.Any())
            {
                var handler = PendingHandlers.Dequeue();
                LoadingHandlers.Add(handler);
                if (LoadingHandlers.Count == MaximumLoadsInBatch)
                {
                    await UniTask.Delay(DelayBetweenBatches); //Let threads rest...
                }
                LoadTexture(handler).Forget();
            }
        }

        private static async UniTask LoadTexture(LoadHandler handler)
        {
            var url = handler.Url;
            using var uwr = UnityWebRequestTexture.GetTexture(url);
            await uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[TextureReader] Unable to load texture {url}");
                return;
            }

            handler.Texture = DownloadHandlerTexture.GetContent(uwr);
            handler.Completed = true;
            LoadingHandlers.Remove(handler);

            await ScheduleLoadTasks();
        }

        private class LoadHandler
        {
            public string Url;
            public bool Completed;
            public Texture2D Texture;
        }
    }
}