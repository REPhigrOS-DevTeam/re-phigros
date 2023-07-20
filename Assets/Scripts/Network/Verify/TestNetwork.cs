using Network.Verify.API;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Verify
{
    public class TestNetwork : MonoBehaviour
    {
        [SerializeField] private Button button1;

        private RepAPI api = null;

        void Awake()
        {
            return;
            api = new RepAPI();
            button1.onClick.AddListener(Request);
        }

        void Request()
        {
            //if (!api.IsLoggedIn())
            {
                Debug.Log("Logging in...");
                api.Login("Debug", "RepRunDebug2023");
            }
            //else
            {
                Debug.Log("Verifying...");
                api.Verify();
            }
        }
    }
}