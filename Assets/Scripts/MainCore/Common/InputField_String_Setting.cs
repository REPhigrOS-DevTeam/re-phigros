using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    public class InputField_String_Setting : SettingBase<InputField, string>
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
            DataContainer.text = value;
        }

        public override void SaveValue()
        {
            if (!gameObject.activeSelf) Debug.Log("qwq");
            PlayerPrefs.SetString(dataTag, DataContainer.text);
        }
    }
}