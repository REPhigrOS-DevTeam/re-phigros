using Lean.Gui;
using UnityEngine;

namespace MainCore.Common
{
    public class Toggle_Bool_Setting : SettingBase<bool>
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
            (dataContainer as LeanToggle).On = value;
        }

        public override void SaveValue()
        {
            PlayerPrefs.SetInt(dataTag, (dataContainer as LeanToggle).On ? 1 : 0);
        }
    }
}