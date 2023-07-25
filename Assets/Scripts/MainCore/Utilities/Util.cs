using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#else
using UnityEngine;
#endif

namespace MainCore.Utilities
{
    public static class Util
    {
        public static void QuitApp()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        

        public static TaskAwaiter<object> GetAwaiter(this UnityWebRequestAsyncOperation op)
        {
            var tcs = new TaskCompletionSource<object>();
            op.completed += (obj) =>
            {
                tcs.SetResult(null);
            };
            return tcs.Task.GetAwaiter();
        }
    }
}