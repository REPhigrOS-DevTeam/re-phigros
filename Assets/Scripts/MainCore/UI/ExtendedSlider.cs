using System;
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
        [Range(-1, 15)] [SerializeField] private int fraction = -1;
        [SerializeField] private Type valueType = Type.Normal;

        private Slider slider;

        void Start()
        {
            slider = GetComponent<Slider>();
            if (minus) minus.onClick.AddListener(() =>
            {
                slider.value -= step;
                if (fraction != 0) return;
                if ((int) slider.value % (int) step != 0) slider.value = (int) (slider.value / step) * (int) step;
            });
            if (add) add.onClick.AddListener(() =>
            {
                slider.value += step; 
                if (fraction != 0) return;
                if ((int) slider.value % (int) step != 0) slider.value = (int) (slider.value / step) * (int) step;
            });
            Update();
        }

        void Update()
        {
            if (valueType == Type.Normal)
            {
                text.text = string.Format(textFormat,
                    fraction < 0 ? slider.value :
                    fraction == 0 ? slider.value = Mathf.RoundToInt(slider.value) : slider.value = (float) Math.Round(slider.value, fraction));
            }
            else
            {
                text.text = string.Format(textFormat,
                    fraction < 0 ? slider.value * 100 :
                    fraction == 0 ? (slider.value = Mathf.RoundToInt(slider.value)) * 100 : (slider.value = (float) Math.Round(slider.value, fraction)) * 100);
            }
        }
        
        public enum Type
        {
            Normal = 0,
            Percent = 1
        }
    }
}