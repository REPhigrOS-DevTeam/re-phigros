using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI
{
    [RequireComponent(typeof(Slider))]
    public class ExtendedSlider : MonoBehaviour
    {
        [SerializeField] private Button minus, add;
        [SerializeField] private Text text;
        [SerializeField] private float step;
        [SerializeField] private string textFormat;

        private Slider slider;

        void Start()
        {
            slider = GetComponent<Slider>();
            minus.onClick.AddListener(() => { slider.value -= step; });
            add.onClick.AddListener(() => { slider.value += step; });
        }

        void Update()
        {
            text.text = string.Format(textFormat, slider.value);
        }
    }
}