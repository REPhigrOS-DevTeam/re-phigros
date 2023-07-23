#if UNITY_EDITOR
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
            return;
            RepAPI.Init();
            button1.onClick.AddListener(Request);
        }

        void Request()
        {
            //if (!api.IsLoggedIn())
            {
                Debug.Log("Logging in...");
                RepAPI.Login("Debug", "RepRunDebug2023");
            }
            //else
            {
                Debug.Log("Verifying...");
                RepAPI.Verify();
            }
        }
    }
}
#endif