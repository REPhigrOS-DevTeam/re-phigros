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

        public void Awake()
        {
            button = gameObject.GetComponent<Button>();
            buttonText = button.transform.GetChild(0).GetComponent<Text>();
            IsOn = false;
        }

        private void ChangeValue(bool value)
        {
            isOn = value;
            OnValueChanged.Invoke(button, buttonText, value);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ChangeValue(!value));
        }
    }
}