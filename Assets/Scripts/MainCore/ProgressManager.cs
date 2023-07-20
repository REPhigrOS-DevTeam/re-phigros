using System;
using System.Diagnostics;
using DG.Tweening;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MainCore
{
    public class ProgressManager : MonoBehaviour
    {
        Stopwatch _stopwatch = new();

        public float NowTime;

        //下面是dsp!
        private float startDspTime;
        private float lastUpdateDspTime;

        public delegate void OnResolutionError();

        public delegate float Pitch();

        private OnResolutionError _runtimeError;
        private Pitch _pitch;

        public void Init(OnResolutionError initError, OnResolutionError runtimeError, Pitch pitch)
        {
            _runtimeError = runtimeError;
            _pitch = pitch;
            if (!Stopwatch.IsHighResolution)
            {
                initError.Invoke();
            }
        }

        public void StartTiming()
        {
            _stopwatch.Start();
            startDspTime = lastUpdateDspTime = (float)AudioSettings.dspTime;
        }

        public void StopTiming()
        {
            _stopwatch.Stop();
        }

        public void ContinueTiming()
        {
            _stopwatch.Start();
        }

        public void OnUpdate()
        {
            var currentDspTime = (float)AudioSettings.dspTime;
            if (Math.Abs(lastUpdateDspTime - currentDspTime) > 0.001f)
            {
                lastUpdateDspTime = currentDspTime;
                //仅在真正dsp时间更新的时候比对
                var differenceTime = currentDspTime - startDspTime - _stopwatch.ElapsedMilliseconds / 1000f;
                if (differenceTime < -0.5f)
                {
                    _runtimeError.Invoke();

                    Debug.LogWarning($"当前时差为{differenceTime}ms,Dsp炸啦！！！");
                }
            }

            var tempT = _stopwatch.ElapsedMilliseconds / 1000f - delay;
            NowTime = (tempT < pauseTime ? pauseTime : tempT) * _pitch.Invoke();
        }

        private float pauseTime;
        private float delay;
        public void AddStartDelay(float second)
        {
            delay += second;
            pauseTime = NowTime;
        }

        public void AddDelay(float second)
        {
            delay += second;
        }

        public void TimeGoBack(float time, TweenCallback callback)
        {
            float originOffset = delay;
            DOTween.To(() => delay - originOffset, x => delay = originOffset + x, time, .5f).OnComplete(callback);
        }
    }
}
