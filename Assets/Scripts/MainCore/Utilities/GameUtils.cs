using System;
using System.Collections.Generic;
using MainCore;
using MainCore.Data;
using UnityEngine;

namespace Utilities
{
    public static class GameUtils
    {
        private static float _screenDelta = -10;

        public static float ScreenDelta
        {
            get
            {
#if UNITY_EDITOR
                _screenDelta = Mathf.Min((float) Screen.width / Screen.height * 0.5625f, 1f);
#else
                if (_screenDelta < 0)
                    _screenDelta = Mathf.Min((float)Screen.width / Screen.height * 0.5625f, 1f);
#endif
                return _screenDelta;
            }
        }

        public static Color SetAlpha(this Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);

        public static Vector3 SetZ(this Vector2 pos, float z) => new Vector3(pos.x, pos.y, z);

        public static Vector2 GetTransformedXY(Vector2 xy)
        {
            return new Vector2(xy.x * _screenDelta, xy.y);
        }

        public static float GetAspectX(float x)
        {
            return x * _screenDelta;
        }

        public static bool ResetDSPBuffer(float pow)
        {
            var config = AudioSettings.GetConfiguration();
            config.dspBufferSize = (int) Math.Pow(2, (int) pow);
            return AudioSettings.Reset(config);
        }

        public static void AddTestCount()
        {
#if UNITY_EDITOR
            Main.Mian.TEST_COUNT++;
#endif
        }

        #region TEMPPPP

        public static judgeLineEvent GetEventFromCurrentTime(List<judgeLineEvent> events, float time)
        {
            if (!GlobalSetting.Playing || events.Count == 0)
                return null;

            //Binary search
            int l, r, length;
            length = events.Count - 1;
            l = 0;
            r = length;
            var tempIndex = 0;

            while (l <= r)
            {
                int mid = (l + r) / 2;
                if (events[mid].startTime < time)
                {
                    if (mid < length)
                    {
                        if (events[mid + 1].startTime >= time)
                        {
                            tempIndex = mid;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    l = mid + 1;
                }
                else if (events[mid].startTime >= time)
                {
                    if (mid >= 1)
                    {
                        if (events[mid - 1].startTime < time)
                        {
                            tempIndex = mid - 1;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    r = mid - 1;
                }
            }

            return events[tempIndex];
        }

        public static judgeLineColorEvent GetEventFromCurrentTime(List<judgeLineColorEvent> events, float time)
        {
            if (!GlobalSetting.Playing || events.Count == 0)
                return null;

            //Binary search
            int l, r, length;
            length = events.Count - 1;
            l = 0;
            r = length;
            var tempIndex = 0;

            while (l <= r)
            {
                int mid = (l + r) / 2;
                if (events[mid].startTime < time)
                {
                    if (mid < length)
                    {
                        if (events[mid + 1].startTime >= time)
                        {
                            tempIndex = mid;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    l = mid + 1;
                }
                else if (events[mid].startTime >= time)
                {
                    if (mid >= 1)
                    {
                        if (events[mid - 1].startTime < time)
                        {
                            tempIndex = mid - 1;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    r = mid - 1;
                }
            }

            return events[tempIndex];
        }

        public static judgeLineTextEvent GetEventFromCurrentTime(List<judgeLineTextEvent> events, float time)
        {
            if (!GlobalSetting.Playing || events.Count == 0)
                return null;

            //Binary search
            int l, r, length;
            length = events.Count - 1;
            l = 0;
            r = length;
            var tempIndex = 0;

            while (l <= r)
            {
                int mid = (l + r) / 2;
                if (events[mid].startTime < time)
                {
                    if (mid < length)
                    {
                        if (events[mid + 1].startTime >= time)
                        {
                            tempIndex = mid;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    l = mid + 1;
                }
                else if (events[mid].startTime >= time)
                {
                    if (mid >= 1)
                    {
                        if (events[mid - 1].startTime < time)
                        {
                            tempIndex = mid - 1;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    r = mid - 1;
                }
            }

            return events[tempIndex];
        }

        public static judgeLineSpeedEvent GetEventFromCurrentTime(List<judgeLineSpeedEvent> events, float time)
        {
            if (events.Count == 0)
                return null;

            //Binary search
            int l, r, length;
            length = events.Count - 1;
            l = 0;
            r = length;
            var tempIndex = 0;

            while (l <= r)
            {
                int mid = (l + r) / 2;
                if (events[mid].startTime < time)
                {
                    if (mid < length)
                    {
                        if (events[mid + 1].startTime >= time)
                        {
                            tempIndex = mid;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    l = mid + 1;
                }
                else if (events[mid].startTime >= time)
                {
                    if (mid >= 1)
                    {
                        if (events[mid - 1].startTime < time)
                        {
                            tempIndex = mid - 1;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    r = mid - 1;
                }
            }

            return events[tempIndex];
        }

        public static noteControl GetEventFromCurrentTime(List<noteControl> events, float time)
        {
            if (!GlobalSetting.Playing || events.Count == 0)
                return null;

            //Binary search
            int l, r, length;
            length = events.Count - 1;
            l = 0;
            r = length;
            var tempIndex = 0;

            while (l <= r)
            {
                int mid = (l + r) / 2;
                if (events[mid].start < time)
                {
                    if (mid < length)
                    {
                        if (events[mid + 1].start >= time)
                        {
                            tempIndex = mid;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    l = mid + 1;
                }
                else if (events[mid].start >= time)
                {
                    if (mid >= 1)
                    {
                        if (events[mid - 1].start < time)
                        {
                            tempIndex = mid - 1;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    r = mid - 1;
                }
            }

            return events[tempIndex];
        }

        #endregion
    }
}