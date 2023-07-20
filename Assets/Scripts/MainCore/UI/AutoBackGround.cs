using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class AutoBackGround : MonoBehaviour
    {
        private const float fixedNum = 1.05f;
        public float delta = 1f;
        public bool noFix;
        private Image _image;
        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
        }

        private void Start()
        {
            var size = _image.sprite.bounds.size * 100f;
            //delta = size.x > 0 && size.y > 0 ? (size.x / size.y) / (16f / 9f) : 1f;

            CatchScreenSize();
        }

#if UNITY_EDITOR
        private void Update() => CatchScreenSize();
#endif

        public void CatchScreenSize()
        {
            float num = (float) Screen.width / (float) Screen.height / (16f / 9f);
            Vector3 temp;
            if (!noFix)
            {
                if (num > 1f) temp = new Vector3(num * fixedNum, num * fixedNum, 1f);
                else temp = Vector3.one * fixedNum;
            }
            else temp = Vector3.one;

            _rect.localScale =
                new Vector3(temp.x * (delta > 1 ? delta : 1f), temp.y * (delta < 1 ? 1f / delta : 1f), 1f);
        }
    }
}