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
        public Action<bool> OnValueChanged = _ => {};
        public string onOnLabel, onOffLabel;
        private bool isOn;
        public bool IsOn
        {
            get => isOn;
            set => ChangeValue(value);
        }

        public bool Interactable
        {
            get => button.interactable;
            set => button.interactable = value;
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
            buttonText.text = isOn ? onOnLabel : onOffLabel;
            OnValueChanged.Invoke(value);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ChangeValue(!value));
        }
    }
}