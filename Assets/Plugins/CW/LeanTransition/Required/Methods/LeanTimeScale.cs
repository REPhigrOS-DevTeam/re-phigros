using System.Collections.Generic;
using UnityEngine;

namespace Lean.Transition.Method
{
    /// <summary>This component allows you to transition <b>Time.timeScale</b> to the target value.</summary>
    [HelpURL(LeanTransition.HelpUrlPrefix + "LeanTimeScale")]
    [AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Time.timeScale" + LeanTransition.MethodsMenuSuffix +
                      "(LeanTimeScale)")]
    public class LeanTimeScale : LeanMethodWithState
    {
        public State Data;

        public override void Register()
        {
            PreviousState = Register(Data.TimeScale, Data.Duration, Data.Ease);
        }

        public static LeanState Register(float fillAmount, float duration, LeanEase ease = LeanEase.Smooth)
        {
            var state = LeanTransition.Spawn(State.Pool);

            state.TimeScale = fillAmount;
            state.Ease = ease;

            return LeanTransition.Register(state, duration);
        }

        [System.Serializable]
        public class State : LeanState
        {
            public static Stack<State> Pool = new Stack<State>();

            [Tooltip("The timeScale we will transition to.")]
            public float TimeScale = 1.0f;

            [Tooltip("The ease method that will be used for the transition.")]
            public LeanEase Ease = LeanEase.Smooth;

            [System.NonSerialized] private float oldTimeScale;

            public override int CanFill
            {
                get { return Time.timeScale != TimeScale ? 1 : 0; }
            }

            public override void Fill()
            {
                TimeScale = Time.timeScale;
            }

            public override void Begin()
            {
                oldTimeScale = Time.timeScale;
            }

            public override void Update(float progress)
            {
                Time.timeScale = Mathf.LerpUnclamped(oldTimeScale, TimeScale, Smooth(Ease, progress));
            }

            public override void Despawn()
            {
                Pool.Push(this);
            }
        }
    }
}

namespace Lean.Transition
{
    public static partial class LeanExtensions
    {
        public static T timeScaleTransition<T>(this T target, float timeScale, float duration,
            LeanEase ease = LeanEase.Smooth)
            where T : Component
        {
            Method.LeanTimeScale.Register(timeScale, duration, ease);
            return target;
        }

        public static GameObject timeScaleTransition(this GameObject target, float timeScale, float duration,
            LeanEase ease = LeanEase.Smooth)
        {
            Method.LeanTimeScale.Register(timeScale, duration, ease);
            return target;
        }
    }
}