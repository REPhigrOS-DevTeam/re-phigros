using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    public class Slider_Float_Setting : SettingBase<float>
    {
        protected override void OnStart()
        {
            SetValue(GetValue());
        }

        public override float GetValue()
        {
            return PlayerPrefs.GetFloat(dataTag, defaultValue);
        }

        public override void SetValue(float value)
        {
            (dataContainer as Slider).value = value;
        }

        public override void SaveValue()
        {
            PlayerPrefs.SetFloat(dataTag, (dataContainer as Slider).value);
        }
    }
}