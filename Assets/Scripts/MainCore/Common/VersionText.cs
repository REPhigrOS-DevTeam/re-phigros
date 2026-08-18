using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    [RequireComponent(typeof(Text))]
    public class VersionText : MonoBehaviour
    {
        private void Awake()
        {
            Text text = GetComponent<Text>();
#if !RELEASE_VERSION || UNITY_EDITOR
            text.text = $"Development Version - RE:PhityOS {Application.version} by kagari939\n";
#else
        text.text = $"RE:PhityOS {Application.version} by kagari939\n";
#endif
        }
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
