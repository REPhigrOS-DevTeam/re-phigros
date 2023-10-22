using System;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    [RequireComponent(typeof(Button)), ExecuteInEditMode]
    public class Toggle_Button : MonoBehaviour
    {
        private Button button;
        private Text buttonText;
        private Image buttonImage;
        public Action<bool> OnValueChanged = _ => {};
        public string onOnLabel, onOffLabel;
        public Sprite onOnSprite, onOffSprite;
        public ToggleButtonSwitch mode = ToggleButtonSwitch.Text;
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
            Transform textObj = transform.Find("Text");
            buttonText = textObj ? textObj.gameObject.GetComponent<Text>() : null;
            Transform imageObj = transform.Find("Image");
            buttonImage = imageObj ? imageObj.gameObject.GetComponent<Image>() : null;
            IsOn = IsOn;
        }

        [ContextMenu("Toggle")]
        private void Toggle()
        {
            IsOn = !IsOn;
        }

        private void ChangeValue(bool value)
        {
            isOn = value;
            if (buttonText) buttonText.text = mode.HasFlag(ToggleButtonSwitch.Text) ? isOn ? onOnLabel : onOffLabel : "";
            if (buttonImage)
            {
                if (mode.HasFlag(ToggleButtonSwitch.Sprite))
                {
                    buttonImage.color = Color.white;
                    buttonImage.sprite = isOn ? onOnSprite : onOffSprite;                
                }
                else
                {
                    buttonImage.color = new Color(0f, 0f, 0f, 0f);
                }
            }
            OnValueChanged.Invoke(value);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ChangeValue(!value));
        }
    }

    [Flags]
    public enum ToggleButtonSwitch
    {
        Text = 1 << 0,
        Sprite = 1 << 1        
    }
}