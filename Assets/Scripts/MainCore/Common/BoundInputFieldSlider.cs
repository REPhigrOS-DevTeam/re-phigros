using System;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    public class BoundInputFieldSlider : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Slider slider;
        [SerializeField] private bool useKnobs = true;
        private bool IsInteger => slider.wholeNumbers;
        public int digits = 2;
        private float value;
        private string Format => digits > 0 ? $"0.{new string('0', digits)}" : "0";

        private void Awake()
        {
            inputField.onEndEdit.AddListener(ChangeValueFromInputField);
            slider.onValueChanged.AddListener(ChangeValueFromSlider);
            Transform sliderBackground = slider.transform.Find("Background");
            if (IsInteger && useKnobs)
            {
                Transform dynamicKnobsParent = sliderBackground.Find("DynamicKnobs");
                GameObject endKnob = sliderBackground.Find("EndKnob").gameObject;
                int count = (int)slider.maxValue - (int)slider.minValue;
                for (int i = 0; i < count; i++)
                {
                    Instantiate(endKnob, dynamicKnobsParent).name = $"Knob{i}";
                }
                sliderBackground.Find("EndKnob").gameObject.SetActive(true);
            }
            else
            {
                sliderBackground.Find("DynamicKnobs").gameObject.SetActive(false);
                sliderBackground.Find("EndKnob").gameObject.SetActive(false);
            }
        }

        private void ChangeValueFromSlider(Single value)
        {
            this.value = value;
            inputField.text = this.value.ToString(Format);
        }

        private void ChangeValueFromInputField(string value)
        {
            if (IsInteger)
            {
                if (!int.TryParse(value, out int i) || i < slider.minValue || i > slider.maxValue)
                {
                    inputField.text = (int) this.value + "";
                    return;
                }
                this.value = i;
                slider.value = i;
            }
            else
            {
                if (!float.TryParse(value, out float f) || f < slider.minValue || f > slider.maxValue)
                {
                    inputField.text = this.value.ToString(Format);
                    return;
                }
                this.value = f;
                inputField.text = f.ToString(Format);
            }
        }
    }
}