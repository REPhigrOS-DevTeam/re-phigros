#if UNITY_EDITOR
using System.Threading.Tasks;
using MainCore.Utilities;
using Network.Verify.API;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Verify
{
    public class TestNetwork : MonoBehaviour
    {
        [SerializeField] private Button button1;

        void Awake()
        {
            Task.Run(async () =>
            {
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
                await RepAPI.Login("Debug", "RepRunDebug2023");
            }
            //else
            {
                Debug.Log("Verifying...");
                await RepAPI.Verify();
            }
        }
    }
}
#endif