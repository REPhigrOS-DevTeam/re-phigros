using Lean.Gui;
using UnityEngine;

namespace MainCore.Common
{
    public class Toggle_Bool_Setting : SettingBase<LeanToggle, bool>
    {
        protected override void OnStart()
        {
            SetValue(GetValue());
        }

        public override bool GetValue()
        {
            return PlayerPrefs.GetInt(dataTag, 0) == 1;
        }

        public override void SetValue(bool value)
        {
            DataContainer.On = value;
        }

        public override void SaveValue()
        {
            PlayerPrefs.SetInt(dataTag, DataContainer.On ? 1 : 0);
        }
    }
}