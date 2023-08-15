#if UNITY_EDITOR
using System.Threading.Tasks;
using MainCore.Utilities;
using Network.API;
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
                LoginManager.ReadAccountFromPlayerPrefs();
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
                await LoginManager.Login("Debug", "RepRunDebug2023");
            }
            //else
            {
                Debug.Log("Verifying...");
                await LoginManager.Verify();
            }
        }
    }
}
#endif