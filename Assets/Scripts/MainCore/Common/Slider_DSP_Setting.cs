using System;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace MainCore.Common
{
    public class Slider_DSP_Setting : SettingBase<int>
    {
        [SerializeField] private Text shower;

        protected override void OnStart()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            defaultValue = 10;
#elif UNITY_IPHONE || UNITY_ANDROID
            defaultValue = 8;
#endif
            SetValue(GetValue());
            (dataContainer as Slider).onValueChanged.AddListener(OnValueChanged);
        }


        public override int GetValue()
        {
            return PlayerPrefs.GetInt(dataTag, defaultValue);
        }

        public override void SetValue(int value)
        {
            shower.text = $"{(int)Math.Pow(2, (int)value)}";
            (dataContainer as Slider).value = value;
        }

        public override void SaveValue()
        {
            PlayerPrefs.SetInt(dataTag, (int)(dataContainer as Slider).value);
        }

        private void OnValueChanged(float val)
        {
            shower.text = $"{(int)Math.Pow(2, (int)val)}";
            GameUtils.ResetDSPBuffer(val);
        }
    }
}