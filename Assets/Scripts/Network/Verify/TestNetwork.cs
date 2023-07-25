#if UNITY_EDITOR
using System.Collections;
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
            StartCoroutine(RequestCoroutine());
        }

        private IEnumerator RequestCoroutine()
        {
            //if (!api.IsLoggedIn())
            {
                Debug.Log("Logging in...");
                yield return RepAPI.Login("Debug", "RepRunDebug2023", _ => {});
            }
            //else
            {
                Debug.Log("Verifying...");
                yield return RepAPI.Verify(_ => {});
            }
        }
    }
}
#endif