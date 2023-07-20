using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Utilities
{
    [BurstCompile]
    public static class EaseUtils
    {
        public enum EaseType
        {
            Undefined = 0,
            Linear = 1,
            EaseInSine = 3,
            EaseOutSine = 2,
            EaseInOutSine = 6,
            EaseInQuad = 5,
            EaseOutQuad = 4,
            EaseInOutQuad = 7,
            EaseInCubic = 9,
            EaseOutCubic = 8,
            EaseInOutCubic = 12,
            EaseInQuart = 11,
            EaseOutQuart = 10,
            EaseInOutQuart = 13,
            EaseInQuint = 15,
            EaseOutQuint = 14,
            EaseInOutQuint = 11111,
            EaseInExpo = 17,
            EaseOutExpo = 16,
            EaseInOutExpo = 181111,
            EaseInCirc = 19,
            EaseOutCirc = 18,
            EaseInOutCirc = 22,
            EaseInBack = 21,
            EaseOutBack = 20,
            EaseInOutBack = 23,
            EaseInElastic = 25,
            EaseOutElastic = 24,
            EaseInOutElastic = 29,
            EaseInBounce = 27,
            EaseOutBounce = 26,
            EaseInOutBounce = 28
        }

        private const double PI2 = Math.PI * 2;
        private const double PI05 = Math.PI * 0.5;

        private static readonly Dictionary<EaseType, Func<float, float>> EaseFunctions =
            new Dictionary<EaseType, Func<float, float>>
            {
                {EaseType.Undefined, t => t},
                {EaseType.Linear, t => t},
                {EaseType.EaseInSine, t => (float) -math.cos(t * PI05) + 1f},
                {EaseType.EaseOutSine, t => (float) math.sin(t * PI05)},
                {EaseType.EaseInOutSine, t => (float) (math.cos(t * Math.PI) - 1) / -2},
                {EaseType.EaseInQuad, t => t * t},
                {EaseType.EaseOutQuad, t => 1 - EaseFunctions[EaseType.EaseInQuad](1 - t)},
                {
                    EaseType.EaseInOutQuad, t => t < 0.5
                        ? EaseFunctions[EaseType.EaseInQuad](t * 2) / 2
                        : 1 -
                          EaseFunctions[EaseType.EaseInQuad]((1 - t) * 2) / 2
                },
                {EaseType.EaseInCubic, t => t * t * t},
                {EaseType.EaseOutCubic, t => 1 - EaseFunctions[EaseType.EaseInCubic](1 - t)},
                {
                    EaseType.EaseInOutCubic,
                    t => t < 0.5
                        ? EaseFunctions[EaseType.EaseInCubic](t * 2) / 2
                        : 1 - EaseFunctions[EaseType.EaseInCubic]((1 - t) * 2) / 2
                },
                {EaseType.EaseInQuart, t => t * t * t * t},
                {EaseType.EaseOutQuart, t => 1 - EaseFunctions[EaseType.EaseInQuart](1 - t)},
                {
                    EaseType.EaseInOutQuart,
                    t => t < 0.5
                        ? EaseFunctions[EaseType.EaseInQuart](t * 2) / 2
                        : 1 - EaseFunctions[EaseType.EaseInQuart]((1 - t) * 2) / 2
                },
                {EaseType.EaseInQuint, t => t * t * t * t * t},
                {EaseType.EaseOutQuint, t => 1 - EaseFunctions[EaseType.EaseInQuint](1 - t)},
                {
                    EaseType.EaseInOutQuint,
                    t => t < 0.5
                        ? EaseFunctions[EaseType.EaseInQuint](t * 2) / 2
                        : 1 - EaseFunctions[EaseType.EaseInQuint]((1 - t) * 2) / 2
                },
                {EaseType.EaseInExpo, t => (float) math.pow(2, 10 * (t - 1))},
                {EaseType.EaseOutExpo, t => 1 - EaseFunctions[EaseType.EaseInExpo](1 - t)},
                {
                    EaseType.EaseInOutExpo,
                    t => t < 0.5
                        ? EaseFunctions[EaseType.EaseInExpo](t * 2) / 2
                        : 1 - EaseFunctions[EaseType.EaseInExpo]((1 - t) * 2) / 2
                },
                {EaseType.EaseInCirc, t => -((float) math.sqrt(1 - t * t) - 1)},
                {EaseType.EaseOutCirc, t => 1 - EaseFunctions[EaseType.EaseInCirc](1 - t)},
                {
                    EaseType.EaseInOutCirc,
                    t => t < 0.5
                        ? EaseFunctions[EaseType.EaseInCirc](t * 2) / 2
                        : 1 - EaseFunctions[EaseType.EaseInCirc]((1 - t) * 2) / 2
                },
                {
                    EaseType.EaseInBack, t =>
                    {
                        const float s = 1.70158f;
                        return t * t * ((s + 1) * t - s);
                    }
                },
                {EaseType.EaseOutBack, t => 1 - EaseFunctions[EaseType.EaseInBack](1 - t)},
                {
                    EaseType.EaseInOutBack,
                    t => t < 0.5
                        ? EaseFunctions[EaseType.EaseInBack](t * 2) / 2
                        : 1 - EaseFunctions[EaseType.EaseInBack]((1 - t) * 2) / 2
                },
                {EaseType.EaseInElastic, t => 1 - EaseFunctions[EaseType.EaseOutElastic](1 - t)},
                {
                    EaseType.EaseOutElastic, t =>
                    {
                        const float p = 0.3f;
                        return (float) math.pow(2, -10 * t) * (float) math.sin((t - p / 4) * (PI2) / p) + 1;
                    }
                },
                {
                    EaseType.EaseInOutElastic,
                    t => t < 0.5
                        ? EaseFunctions[EaseType.EaseInElastic](t * 2) / 2
                        : 1 - EaseFunctions[EaseType.EaseInElastic]((1 - t) * 2) / 2
                },
                {EaseType.EaseInBounce, t => 1 - EaseFunctions[EaseType.EaseOutBounce](1 - t)},
                {
                    EaseType.EaseOutBounce, t =>
                    {
                        const float div = 2.75f;
                        const float mult = 7.5625f;

                        if (t < 1 / div)
                        {
                            return mult * t * t;
                        }

                        if (t < 2 / div)
                        {
                            t -= 1.5f / div;
                            return mult * t * t + 0.75f;
                        }

                        if (t < 2.5 / div)
                        {
                            t -= 2.25f / div;
                            return mult * t * t + 0.9375f;
                        }

                        t -= 2.625f / div;
                        return mult * t * t + 0.984375f;
                    }
                },
                {
                    EaseType.EaseInOutBounce,
                    t => t < 0.5
                        ? EaseFunctions[EaseType.EaseInBounce](t * 2) / 2
                        : 1 - EaseFunctions[EaseType.EaseInBounce]((1 - t) * 2) / 2
                }
            };

        private static readonly Dictionary<Type, IEaseResolver> Resolvers = new Dictionary<Type, IEaseResolver>
        {
            {typeof(float), new FloatEaseResolver()},
            {typeof(Vector2), new Vector2EaseResolver()}
        };

        public static float Evaluate(float time, EaseType easeType = EaseType.Linear) => EaseFunctions[easeType](time);

        public static float Evaluate(float time, float start, float end, EaseType easeType = EaseType.Linear) =>
            start + (end - start) * Evaluate(time, easeType);

        public static T Evaluate<T>(float time, T start, T end, EaseType easeType = EaseType.Linear) =>
            GetEaseResolver<T>().Evaluate(time, start, end, easeType);

        public static IEaseResolver<T> GetEaseResolver<T>()
        {
            var easeResolver = Resolvers[typeof(T)];
            return (IEaseResolver<T>) easeResolver;
        }

        public static float Evaluate(EaseType easeType, float time, float duration, float startRange, float endRange)
        {
            if (easeType == EaseType.Undefined) easeType = EaseType.Linear;
            var range = endRange - startRange;
            var realTime = duration * startRange + time * range;
            var startEase = Evaluate(startRange, easeType);
            var endEase = Evaluate(endRange, easeType);
            var easeRange = endEase - startEase;
            return (Evaluate(realTime / duration, easeType) - startEase) / easeRange;
        }

        /// <summary>
        /// 获得指定情况下的缓动插值
        /// </summary>
        /// <param name="easeType">缓动类型</param>
        /// <param name="currentTimeFromStart">当前插值的持续时间</param>
        /// <param name="duration">总持续时间</param>
        /// <param name="startValue">起始数值</param>
        /// <param name="endValue">终止数值</param>
        /// <returns>指定的插值</returns>
        public static float GetEaseResult(EaseType easeType, float currentTimeFromStart, float duration,
            float startValue, float endValue, float startRange = 0, float endRange = 1)
        {
            //抠常数级优化，如果到边界则直接返回边界值
            if (currentTimeFromStart >= duration) return endValue;
            /*var range = endRange - startRange;
            var realTime = duration * startRange + currentTimeFromStart * range;
            var startEase = Evaluate(startRange, easeType);
            var endEase = Evaluate(endRange, easeType);
            var easeRange = endEase - startEase;
            var realTime = currentTimeFromStart / duration * (endRange - startRange) + startRange;
            return realTime <= 0
                ? startValue
                : (Evaluate(realTime, startValue, endValue,
                    easeType)); //- startEase) / easeRange;
            return (Evaluate(realTime, startValue, endValue, easeType) - startEase) / easeRange;*/
            return currentTimeFromStart <= 0
                ? startValue
                : (startValue + (endValue - startValue) *
                    Evaluate(easeType, currentTimeFromStart, duration, startRange, endRange));
        }

        public interface IEaseResolver
        {
        }

        public interface IEaseResolver<T> : IEaseResolver
        {
            T Evaluate(float time, T start, T end, EaseType easeType);
        }

        public class FloatEaseResolver : IEaseResolver<float>
        {
            public float Evaluate(float time, float start, float end, EaseType easeType) =>
                EaseUtils.Evaluate(time, start, end, easeType);
        }

        public class Vector2EaseResolver : IEaseResolver<Vector2>
        {
            public Vector2 Evaluate(float time, Vector2 start, Vector2 end, EaseType easeType)
            {
                return new Vector2(
                    EaseUtils.Evaluate(time, start.x, end.x, easeType),
                    EaseUtils.Evaluate(time, start.y, end.y, easeType)
                );
            }
        }
    }
}