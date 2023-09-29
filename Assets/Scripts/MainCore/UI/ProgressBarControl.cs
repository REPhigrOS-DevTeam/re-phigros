using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace MainCore.UI
{
    [RequireComponent(typeof(Image))]
    public class ProgressBarControl : MonoBehaviour
    {
        private float length = -1f;
        private Image progressImage;

        // Start is called before the first frame update
        void Awake()
        {
            progressImage = GetComponent<Image>();
            ResetLength();
        }

        // Update is called once per frame
        void Update()
        {
            progressImage.rectTransform.localScale =
                new Vector3(Main.MusicTime / GlobalSetting.MusicLength, 1f, 1f);
        }

        void ResetLength()
        {
            length = progressImage.rectTransform.sizeDelta.x * GameUtils.ScreenDelta;
            progressImage.rectTransform.sizeDelta = new Vector2(length, 7);
        }
    }
}