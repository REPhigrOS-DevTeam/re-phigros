using Lean.Gui;

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
            return PlayerPrefsExtension.GetBoolean(dataTag, defaultValue);
        }

        public override void SetValue(bool value)
        {
            DataContainer.On = value;
        }

        public override void SaveValue()
        {
            PlayerPrefsExtension.SetBoolean(dataTag, DataContainer.On);
        }
    }
}