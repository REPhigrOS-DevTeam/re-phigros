using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    [RequireComponent(typeof(InputField))]
    public class InputField_Limited_Float_Setting : SettingBase<float>
    {
        [SerializeField] private float min = 0f, max = -1f;
        [SerializeField] private List<float> illegalValues = new List<float>();
        private float value;
        protected override void OnStart()
        {
            dataContainer = gameObject.GetComponent<InputField>();
            ((InputField)dataContainer).onEndEdit.AddListener(CheckValue);
            SetValue(GetValue());
        }
        
        public override float GetValue()
        {
            return PlayerPrefs.GetFloat(dataTag, defaultValue);
        }

        public override void SetValue(float value)
        {
            value = MathF.Round(value, 2);
            if (min <= max && (value > max || value < min) || illegalValues.Contains(value)) return; 
            ((InputField)dataContainer).text = (this.value = value).ToString("0.00");
        }

        public override void SaveValue()
        {
            PlayerPrefs.SetFloat(dataTag, value);
        }

        private void CheckValue(string str)
        {
            if (!float.TryParse(str, out float f))
            {
                ((InputField)dataContainer).text = value.ToString("0.00");
                return;
            }
            f = MathF.Round(f, 2);

            if (!(f > max) && !(f < min) || min > max)
            {
                ((InputField)dataContainer).text = (value = f).ToString("0.00");
                return;
            }

            ((InputField)dataContainer).text = value.ToString("0.00");
        }
    }
}