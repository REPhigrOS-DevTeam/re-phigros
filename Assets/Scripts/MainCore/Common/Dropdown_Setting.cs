using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    public class Dropdown_Setting : SettingBase<Dropdown, int>
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
            DataContainer.value = value;
        }

        public override void SaveValue()
        {
            PlayerPrefs.SetInt(dataTag, DataContainer.value);
        }
    }
}