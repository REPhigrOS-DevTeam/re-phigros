using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    [RequireComponent(typeof(InputField)), ExecuteInEditMode]
    public class InputField_FloatValidation : MonoBehaviour
    {
        private InputField inputField;
        [SerializeField] private float defaultValue;
        private float lastValue;
        public float Value
        {
            get => lastValue;
            set => inputField.text = (lastValue = value).ToString(CultureInfo.InvariantCulture);
        }

        public InputField InputField => inputField ? inputField : gameObject.GetComponent<InputField>();
        [SerializeField] private bool canEqual = true;
        [SerializeField] private ClampMode clampMode = ClampMode.None;
        [SerializeField] private float min = 0f, max = 0f;
        private void Awake()
        {
            inputField = gameObject.GetComponent<InputField>();
            Reset();
            inputField.onEndEdit.AddListener(str =>
            {
                if (float.TryParse(str, out float f))
                {
                    switch (clampMode)
                    {
                        case ClampMode.None:
                            lastValue = f;
                            break;
                        case ClampMode.Left:
                            if (canEqual) lastValue = Mathf.Max(f, min);
                            else if (f > lastValue) lastValue = f;
                            break;
                        case ClampMode.Right:
                            if (canEqual) lastValue = Mathf.Min(f, max);
                            else if (f < lastValue) lastValue = f;
                            break;
                        case ClampMode.Both:
                            if (canEqual) lastValue = Mathf.Clamp(f, min, max);
                            else if (f > min && f < max) lastValue = f;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                inputField.SetTextWithoutNotify(lastValue.ToString(CultureInfo.InvariantCulture));
            });
        }

        public void Reset()
        {
            inputField.text = (lastValue = defaultValue).ToString(CultureInfo.InvariantCulture);
        }
    }
    
    public enum ClampMode
    {
        None = 0,
        Left,
        Right,
        Both
    }
}