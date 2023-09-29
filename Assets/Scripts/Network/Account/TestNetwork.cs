#if UNITY_EDITOR
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Account
{
    public class TestNetwork : MonoBehaviour
    {
        [SerializeField] private Button button1;

        void Awake()
        {
            return;
            Task.Run(async () =>
            {
                LoginManagerOld.ReadAccountFromPlayerPrefs();
                if (await RepAPI.Init())
                {
                    button1.onClick.AddListener(Request);
                }
                else
                {
                    Util.QuitApp();
                }
            });
        }

        async void Request()
        {
            //if (!api.IsLoggedIn())
            {
                Debug.Log("Logging in...");
                await LoginManagerOld.Login("Debug", "RepRunDebug2023");
            }
            //else
            {
                Debug.Log("Verifying...");
                await LoginManagerOld.Verify();
            }
        }
    }
}
#endif