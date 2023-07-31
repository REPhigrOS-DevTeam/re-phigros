using System;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    [RequireComponent(typeof(Button))]
    public class Toggle_Button : MonoBehaviour
    {
        private Button button;
        private Text buttonText;
        public Action<Button, Text, bool> OnValueChanged = (_, _, _) => {};
        private bool isOn;
        public bool IsOn
        {
            get => isOn;
            set => ChangeValue(value);
        }

        private void Awake()
        {
            if (button != null) return;
            button = gameObject.GetComponent<Button>();
            buttonText = transform.GetChild(0).gameObject.GetComponent<Text>();
            IsOn = false;
        }

        private void ChangeValue(bool value)
        {
            if (button == null) Awake();
            isOn = value;
            OnValueChanged.Invoke(button, buttonText, value);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ChangeValue(!value));
        }
    }
}