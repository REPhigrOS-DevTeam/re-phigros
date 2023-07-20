using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    public class InputField_String_Setting : SettingBase<string>
    {
        protected override void OnStart()
        {
            SetValue(GetValue());
        }

        public override string GetValue()
        {
            return PlayerPrefs.GetString(dataTag, defaultValue);
        }

        public override void SetValue(string value)
        {
            (dataContainer as InputField).text = value;
        }

        public override void SaveValue()
        {
            PlayerPrefs.SetString(dataTag, (dataContainer as InputField).text);
        }
    }
}