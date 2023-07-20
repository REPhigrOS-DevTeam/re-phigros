using System.Collections.Generic;
using Lean.Gui;
using UnityEngine.Events;

namespace MainCore.Settings
{
    public class SpecialEvent
    {
        private int NowId = 0;
        private int max;
        private Dictionary<int, UnityAction> a = new();
        private LeanToggle[] toggles;
        private UnityAction resetAction;
        public delegate void OnTriggered();

        private OnTriggered onTriggered;
        public SpecialEvent(LeanToggle[] leanToggles, int[] triggers, OnTriggered onTriggered)
        {
            this.onTriggered = onTriggered;
            resetAction = () => Trigger(0);
            toggles = leanToggles;
            if (triggers.Length == 0) return;
            max = triggers.Length - 1;
            for (var index = 0; index < leanToggles.Length; index++)
            {
                var leanToggle = leanToggles[index];
                bool isTriggered = false;
                for (var i = 0; i < triggers.Length; i++)
                {
                    var trigger = triggers[i] - 1;
                    if (trigger != index) continue;
                    var i1 = i;
                    UnityAction action = () => Trigger(i1);
                    a.Add(index, action);
                    leanToggle.OnOff.AddListener(action);
                    isTriggered = true;
                    break;
                }
                if (isTriggered) continue;
                a.Add(index, null);
                leanToggle.OnOn.AddListener(resetAction);
                leanToggle.OnOff.AddListener(resetAction);
            }
        }

        private void Trigger(int id)
        {
            if (NowId != id - 1)
            {
                NowId = 0;
                return;
            }

            NowId++;

            if (NowId != max) return;
            foreach (KeyValuePair<int, UnityAction> pair in a)
            {
                if (pair.Value != null)
                {
                    toggles[pair.Key].OnOff.RemoveListener(pair.Value);
                    continue;
                }

                toggles[pair.Key].OnOn.RemoveListener(resetAction);
                toggles[pair.Key].OnOff.RemoveListener(resetAction);
            }

            NowId = 0;
            onTriggered?.Invoke();
        }
    }
}