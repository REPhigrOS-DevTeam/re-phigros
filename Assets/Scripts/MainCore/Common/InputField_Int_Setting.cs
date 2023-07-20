using System;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    public class InputField_Int_Setting : SettingBase<int>
    {
        protected override void OnStart()
        {
            SetValue(GetValue());
        }

        public override int GetValue()
        {
            return PlayerPrefs.GetInt(dataTag, defaultValue);
        }

        public override void SetValue(int value)
        {
            (dataContainer as InputField).text = value.ToString();
        }

        public override void SaveValue()
        {
            PlayerPrefs.SetInt(dataTag, Convert.ToInt32((dataContainer as InputField).text));
        }
    }
}