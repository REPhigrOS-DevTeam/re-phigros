using System;
using System.Diagnostics;
using DG.Tweening;
using UnityEngine;

namespace MainCore
{
    public class ProgressManager : MonoBehaviour
    {
        Stopwatch _stopwatch = new();
        
        public float NowNoDelayRealTime;
        public float NowRealTime;
        public float NowNoDelayTime;
        public float NowTime;

        //下面是dsp!
        private float lastUpdateDspTime;

        public delegate void OnResolutionError();

        public delegate float Pitch();

        private OnResolutionError _runtimeError;

        public void Init(OnResolutionError initError, OnResolutionError runtimeError)
        {
            _runtimeError = runtimeError;
            if (!Stopwatch.IsHighResolution)
            {
                initError.Invoke();
            }
        }

        public void StartTiming()
        {
            _stopwatch.Start();
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
                // //仅在真正dsp时间更新的时候比对
                // var differenceTime = currentDspTime - startDspTime - _stopwatch.ElapsedMilliseconds / 1000f;
                // if (differenceTime < -0.5f)
                // {
                //     Debug.LogWarning($"当前时差为{differenceTime}ms,Dsp炸啦！！！");
                //
                //     _runtimeError.Invoke();
                // }
            }

            var tempT = _stopwatch.ElapsedMilliseconds / 1000f + offset;
            NowNoDelayRealTime = tempT < pauseTime ? pauseTime : tempT;
            NowRealTime = NowNoDelayRealTime - delay;
            NowNoDelayTime = NowNoDelayRealTime * GlobalSetting.Pitch;
            NowTime = NowRealTime * GlobalSetting.Pitch;
        }

        private float pauseTime;
        private float delay;
        private float offset;
        public void AddStartDelay(float second)
        {
            delay += second;
            pauseTime = NowNoDelayRealTime;
        }

        public void AddDelay(float second)
        {
            delay += second;
        }

        public void AddTime(float second)
        {
            offset += second;
        }

        public void TimeGoBack(float time, TweenCallback callback)
        {
            float originOffset = delay;
            DOTween.To(() => delay - originOffset, x => delay = originOffset + x, time, .5f).OnComplete(callback);
        }
    }
}
