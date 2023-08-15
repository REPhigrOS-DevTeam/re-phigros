using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baracuda.Threading;
using Cysharp.Threading.Tasks;
using MainCore.Data;
using MainCore.UI;
using UnityEngine;

namespace MainCore.Utilities
{
    public static class Pec2Json
    {
        public static Task<Chart> Chart123(string chart)
        {
            string[] rawChart = chart.Split(new string[] {"\r\n"}, StringSplitOptions.None);
            Chart retChart = new Chart();
            retChart.formatVersion = 114514;
            GlobalSetting.composer = GlobalSetting.charter = GlobalSetting.illustrator = "Unkown";
            BpmEvent[] bpms = new BpmEvent[0];
            if (!double.IsNaN(Convert.ToDouble(rawChart[0].Trim())))
                retChart.offset = ((float) Convert.ToDouble(rawChart[0]) / 1000f - .2f);

            int GetLine(int lineId)
            {
                while (retChart.judgeLineList.Count <= lineId)
                {
                    retChart.judgeLineList.Add(new judgeLine());
                }

                return lineId;
            }

            for (int i = 0; i < rawChart.Length; i++)
            {
                string t = rawChart[i].Trim();
                if (t.Length == 0)
                {
                    continue;
                }

                if (t[0] == 'b') //读bpm
                {
                    //bpms.Append());
                    var a = bpms.ToList();
                    a.Add(new BpmEvent(ToFloat(t.Split(' ')[2]), ToFloat(t.Split(' ')[1])));
                    bpms = a.ToArray();
                    if (bpms.Length >= 2)
                    {
                        bpms[^2].end = bpms[^1].start;
                    }
                }
                else if (t[0] == 'n') //读notes
                {
                    string[] splitted = t.Split(' ');
                    if (t[1] != '2')
                    {
                        int lineId = GetLine(ToInt(splitted[1]));
                        float time = RecalcTime(bpms, ToFloat(splitted[2]));
                        float posX = ToFloat(splitted[3]) / 115.2f;
                        bool isAbove = ToInt(splitted[4]) == 1;
                        bool isFake = ToInt(splitted[5]) == 1;
                        float speed = ToFloat(rawChart[i + 1].Trim().Split(' ')[1]);
                        switch (t[1])
                        {
                            case '1':
                                retChart.judgeLineList[lineId].PushNote(1, isAbove, time, posX, speed, 0, 0, isFake);
                                break;
                            case '3':
                                retChart.judgeLineList[lineId].PushNote(4, isAbove, time, posX, speed, 0, 0, isFake);
                                break;
                            case '4':
                                retChart.judgeLineList[lineId].PushNote(2, isAbove, time, posX, speed, 0, 0, isFake);
                                break;
                        }
                    }
                    else
                    {
                        int lineId = GetLine(ToInt(splitted[1]));
                        float time = RecalcTime(bpms, ToFloat(splitted[2]));
                        float timeEnd = RecalcTime(bpms, ToFloat(splitted[3]));
                        float posX = ToFloat(splitted[4]) / 115.2f;
                        float speed = ToFloat(rawChart[i + 1].Trim().Split(' ')[1]);
                        bool isAbove = ToInt(splitted[5]) == 1;
                        bool isFake = ToInt(splitted[6]) == 1;
                        retChart.judgeLineList[lineId]
                            .PushNote(3, isAbove, time, posX, speed, 0, timeEnd - time, isFake);
                    }

                    i += 2;
                }
                else if (t[0] == 'c') //读line事件
                {
                    string[] splitted = t.Split(' ');
                    int lineId = GetLine(ToInt(splitted[1]));
                    if ("vpda".Contains(t[1]))
                    {
                        float time = RecalcTime(bpms, ToFloat(splitted[2]));
                        float v11 = ToFloat(splitted[3]);
                        switch (t[1])
                        {
                            case 'v':
                                retChart.judgeLineList[lineId]
                                    .PushSpeedEvent(time, time, Convert.ToDouble(splitted[3]) / 7d);
                                break;
                            case 'p':
                                float v12 = ToFloat(splitted[4]);
                                retChart.judgeLineList[lineId].PushEvent(3, 1, time, time, v11 / 2048f, v11 / 2048f,
                                    v12 / 1400f, v12 / 1400f);
                                break;
                            case 'd':
                                retChart.judgeLineList[lineId].PushEvent(4, 1, time, time, -v11, -v11, 0, 0);
                                break;
                            case 'a':
                                int temp = ToInt(splitted[3]);
                                var aMode = AlphaExtendMode.VisibleAll;
                                var visibleTime = 0f;
                                if (temp == -1)
                                {
                                    aMode = AlphaExtendMode.InvisibleAll;
                                }
                                else if (temp == -2)
                                {
                                    aMode = AlphaExtendMode.VisibleUpside;
                                }
                                else if (temp is <= -100 and > -1000)
                                {
                                    aMode = AlphaExtendMode.VisibleAfterTime;
                                    visibleTime = RecalcTime(bpms, time + (-100 - temp) / 10f) - RecalcTime(bpms, time);
                                }

                                retChart.judgeLineList[lineId].PushEvent(2, 1, time, time, temp / 255f, temp / 255f, 0,
                                    0,
                                    aMode, visibleTime);
                                break;
                        }
                    }
                    else
                    {
                        float startTime = RecalcTime(bpms, ToFloat(splitted[2]));
                        float endTime = RecalcTime(bpms, ToFloat(splitted[3]));
                        float v11 = ToFloat(splitted[4]);
                        if (t[1] == 'm')
                        {
                            float v12 = ToFloat(splitted[5]);
                            int easeType = ToInt(splitted[6]);
                            float orgv1 = 0f, orgv2 = 0f;
                            int temp = retChart.judgeLineList[lineId].judgeLineMoveEvents.Count;
                            if (temp > 0)
                            {
                                orgv1 = retChart.judgeLineList[lineId].judgeLineMoveEvents[temp - 1].end;
                                orgv2 = retChart.judgeLineList[lineId].judgeLineMoveEvents[temp - 1].end2;
                            }

                            retChart.judgeLineList[lineId].PushEvent(3, easeType, startTime, endTime, orgv1,
                                v11 / 2048f,
                                orgv2, v12 / 1400f);
                        }
                        else
                        {
                            float orgv = 0;
                            int temp;
                            int easeType = 1;
                            switch (t[1])
                            {
                                case 'r':
                                    easeType = ToInt(splitted[5]);
                                    temp = retChart.judgeLineList[lineId].judgeLineRotateEvents.Count;
                                    if (temp > 0)
                                    {
                                        orgv = retChart.judgeLineList[lineId].judgeLineRotateEvents[temp - 1].end;
                                    }

                                    retChart.judgeLineList[lineId]
                                        .PushEvent(4, easeType, startTime, endTime, orgv, -v11, 0, 0);
                                    break;
                                case 'f':
                                    temp = retChart.judgeLineList[lineId].judgeLineDisappearEvents.Count;
                                    if (temp > 0)
                                    {
                                        orgv = retChart.judgeLineList[lineId].judgeLineDisappearEvents[temp - 1].end;
                                    }

                                    int aVal = ToInt(splitted[4]);
                                    var aMode = AlphaExtendMode.VisibleAll;
                                    var visibleTime = 0f;
                                    if (aVal == -1)
                                    {
                                        aMode = AlphaExtendMode.InvisibleAll;
                                    }
                                    else if (aVal == -2)
                                    {
                                        aMode = AlphaExtendMode.VisibleUpside;
                                    }
                                    else if (aVal is <= -100 and > -1000)
                                    {
                                        aMode = AlphaExtendMode.VisibleAfterTime;
                                        visibleTime = RecalcTime(bpms, startTime + (-100 - aVal) / 10f) -
                                                      RecalcTime(bpms, startTime);
                                    }

                                    retChart.judgeLineList[lineId].PushEvent(2, easeType, startTime, endTime, orgv,
                                        aVal / 255f, 0, 0, aMode, visibleTime);
                                    break;
                            }
                        }
                    }
                }
            }

            //排序
            foreach (var line in retChart.judgeLineList)
            {
                retChart.numOfNotes += line.numOfNotes;
                line.isCover = false;
                line.speedEvents.Sort((a, b) =>
                {
                    return Math.Abs(a.startTime - b.startTime) > .000001f
                        ? a.startTime.CompareTo(b.startTime)
                        : a.endTime.CompareTo(b.endTime);
                });
                line.judgeLineDisappearEvents.Sort((a, b) =>
                {
                    return Math.Abs(a.startTime - b.startTime) > .000001f
                        ? a.startTime.CompareTo(b.startTime)
                        : a.endTime.CompareTo(b.endTime);
                });
                line.judgeLineMoveEvents.Sort((a, b) =>
                {
                    return Math.Abs(a.startTime - b.startTime) > .000001f
                        ? a.startTime.CompareTo(b.startTime)
                        : a.endTime.CompareTo(b.endTime);
                });
                line.judgeLineRotateEvents.Sort((a, b) =>
                {
                    return Math.Abs(a.startTime - b.startTime) > .000001f
                        ? a.startTime.CompareTo(b.startTime)
                        : a.endTime.CompareTo(b.endTime);
                });

                float a = 0, b = 0;

                foreach (var e in line.judgeLineDisappearEvents)
                {
                    e.start = a;
                    a = e.end;
                }

                a = 0;
                foreach (var e in line.judgeLineMoveEvents)
                {
                    e.start = a;
                    a = e.end;
                    e.start2 = b;
                    b = e.end2;
                }

                a = 0;
                foreach (var e in line.judgeLineRotateEvents)
                {
                    e.start = a;
                    a = e.end;
                }

                line.notesAbove.Sort((a, b) => { return a.time.CompareTo(b.time); });
                line.notesBelow.Sort((a, b) => { return a.time.CompareTo(b.time); });
            }

            //规范floorPosition
            foreach (var line in retChart.judgeLineList)
            {
                var s = line.speedEvents;
                for (var j = 0; j < s.Count; j++)
                {
                    s[j].endTime = (j < s.Count - 1 ? s[j + 1].startTime : 1e9);
                    if (s[j].startTime < 0) s[j].startTime = 0;
                    s[j].endValue = s[j].value;
                }

                line.speedEvents = s;

                foreach (var j in line.notesAbove)
                {
                    var qwqwq = 0d;
                    var qwqwq2 = 0d;
                    var qwqwq3 = 0d;
                    foreach (var k in line.speedEvents)
                    {
                        if (j.time > k.endTime) continue;
                        if (j.time < k.startTime) break;
                        qwqwq = k.floorPosition;
                        qwqwq2 = k.value;
                        qwqwq3 = j.time - k.startTime;
                    }

                    j.floorPosition = qwqwq + qwqwq2 * qwqwq3;
                    //if (j.type == 3 && qwqwq2 != 0) j.speed *= qwqwq2;
                }

                foreach (var j in line.notesBelow)
                {
                    var qwqwq = 0d;
                    var qwqwq2 = 0d;
                    var qwqwq3 = 0d;
                    foreach (var k in line.speedEvents)
                    {
                        if (j.time > k.endTime) continue;
                        if (j.time < k.startTime) break;
                        qwqwq = k.floorPosition;
                        qwqwq2 = k.value;
                        qwqwq3 = j.time - k.startTime;
                    }

                    j.floorPosition = qwqwq + qwqwq2 * qwqwq3;
                    //if (j.type == 3 && qwqwq2 != 0) j.speed *= qwqwq2;
                }
            }

            return Task.FromResult(retChart);
        }


        private static float RecalcTime(BpmEvent[] bpms, float time)
        {
            var timePhi = 0f;
            foreach (var i in bpms)
            {
                if (time > i.end)
                {
                    timePhi += (i.end - i.start) * (60f / i.bpm);
                }
                else if (time >= i.start)
                {
                    timePhi += (time - i.start) * (60f / i.bpm);
                }
            }

            return timePhi;
        }

        private static float ToFloat(string s) => (float) Convert.ToDouble(s);

        private static int ToInt(string s) => Convert.ToInt32(s);

        private class BpmEvent
        {
            public float bpm;
            public float end;
            public float start;

            public BpmEvent(float b, float s)
            {
                bpm = b;
                start = s;
                end = 1e9f;
            }
        }
    }

    public static class Rpe2Json
    {
        public static async Task<Chart> Chart123(string chart)
        {
            RpeChartData rpeChartData = JsonUtility.FromJson<RpeChartData>(chart);
            Chart retChart = new Chart();
            retChart.formatVersion = 1919810;
            var bpms = new List<BpmEvent>();

            //Convert meta
            retChart.offset = rpeChartData.META.RPEVersion < 0 ? 0f : rpeChartData.META.offset / 1000f; // + 0.05f;
            retChart.numOfNotes = rpeChartData.judgeLineList
                .Where(x => x.notes != null)
                .Sum(x => x.notes
                    .Where(y => !y.isFake)
                    .ToArray().Length);
            if (rpeChartData.META.RPEVersion >= 0)
            {
                GlobalSetting.musicPath = Path.Combine(GlobalSetting.chartFolderPath,
                    rpeChartData.META.song);
                GlobalSetting.illustrationPath =
                    Path.Combine(GlobalSetting.chartFolderPath, rpeChartData.META.background);
                GlobalSetting.charter = rpeChartData.META.charter;
                GlobalSetting.composer = rpeChartData.META.composer;
                GlobalSetting.illustrator = "Unknown";
                if (GlobalSetting.YayaKawaii != GlobalSetting.YayaMode.绝冲 && GlobalSetting.PepoyoDaisuki != GlobalSetting.PepoyoMode.Yande)
                {
                    GlobalSetting.chartName = rpeChartData.META.name;
                    GlobalSetting.difficulty = rpeChartData.META.level;
                }
            }

            //Convert BPM
            rpeChartData.BPMList.OrderBy(x => Frac(x.startTime)).ToList().ForEach(x =>
            {
                bpms.Add(new BpmEvent(x.bpm, Frac(x.startTime)));
                if (bpms.Count >= 2)
                {
                    bpms[^2].end = bpms[^1].start;
                }
            });

            //Convert lines
            retChart.judgeLineList = new(rpeChartData.judgeLineList.Count);
            for (var j = 0; j < rpeChartData.judgeLineList.Count; j++)
            {
                retChart.judgeLineList.Add(new());
            }

            for (var i = 0; i < rpeChartData.judgeLineList.Count; i++)
            {
                retChart.judgeLineList[i].numOfNotes = rpeChartData.judgeLineList[i].numOfNotes;
                retChart.judgeLineList[i].father = rpeChartData.judgeLineList[i].father;
                retChart.judgeLineList[i].zOrder = rpeChartData.judgeLineList[i].zOrder;
                retChart.judgeLineList[i].isCover = rpeChartData.judgeLineList[i].isCover == 1;
                bool attachUIFlag = rpeChartData.judgeLineList[i].attachUI != "**tHiSisnOne AtTaCH U_i TEmPlAtE**";
                if (attachUIFlag)
                {
                    retChart.judgeLineList[i].attachUI = rpeChartData.judgeLineList[i].attachUI;
                }

                //Convert extended
                if (rpeChartData.judgeLineList[i].Texture != "line.png")
                {
                    var path = Path.Combine(GlobalSetting.chartFolderPath,
                        rpeChartData.judgeLineList[i].Texture);
                    if (File.Exists(path))
                    {
                        retChart.judgeLineList[i].useImage = true;
                        PopupMessageManager.Instance.ChangeContent(
                            $"Loading custom judge line image:\n{rpeChartData.judgeLineList[i].Texture}");
                        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                        byte[] bytes = new byte[fs.Length];
                        fs.Read(bytes, 0, (int) fs.Length);
                        await fs.DisposeAsync();
                        await UniTask.SwitchToMainThread();
                        var t2d = new Texture2D(512, 270);
                        t2d.LoadImage(bytes);
                        Sprite sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.one / 2f);
                        retChart.judgeLineList[i].customImage = sprite;
                    }
                }

                foreach (var e in rpeChartData.judgeLineList[i].extended.scaleXEvents)
                {
                    retChart.judgeLineList[i].extended.scaleXEvents.Add(new judgeLineEvent
                    {
                        start = e.start,
                        end = e.end,
                        startTime = RecalcTime(bpms, Frac(e.startTime)),
                        endTime = RecalcTime(bpms, Frac(e.endTime)),
                        easeType = e.easingType
                    });
                }

                foreach (var e in rpeChartData.judgeLineList[i].extended.scaleYEvents)
                {
                    retChart.judgeLineList[i].extended.scaleYEvents.Add(new judgeLineEvent
                    {
                        start = e.start,
                        end = e.end,
                        startTime = RecalcTime(bpms, Frac(e.startTime)),
                        endTime = RecalcTime(bpms, Frac(e.endTime)),
                        easeType = e.easingType
                    });
                }

                foreach (var e in rpeChartData.judgeLineList[i].extended.colorEvents)
                {
                    retChart.judgeLineList[i].extended.colorEvents.Add(new judgeLineColorEvent
                    {
                        start = ToColor(e.start),
                        end = ToColor(e.end),
                        startTime = RecalcTime(bpms, Frac(e.startTime)),
                        endTime = RecalcTime(bpms, Frac(e.endTime)),
                        easeType = e.easingType
                    });
                }

                foreach (var e in rpeChartData.judgeLineList[i].extended.textEvents)
                {
                    retChart.judgeLineList[i].extended.textEvents.Add(new judgeLineTextEvent
                    {
                        start = e.start,
                        end = e.end,
                        startTime = RecalcTime(bpms, Frac(e.startTime)),
                        endTime = RecalcTime(bpms, Frac(e.endTime)),
                        easingType = e.easingType
                    });
                }

                foreach (var e in rpeChartData.judgeLineList[i].extended.inclineEvents)
                {
                    retChart.judgeLineList[i].extended.inclineEvents.Add(new judgeLineEvent
                    {
                        start = e.start,
                        end = e.end,
                        startTime = RecalcTime(bpms, Frac(e.startTime)),
                        endTime = RecalcTime(bpms, Frac(e.endTime)),
                        easeType = e.easingType == 0 ? 1 : e.easingType,
                        easingLeft = e.easingLeft,
                        easingRight = e.easingRight
                    });
                }

                var noteControlScaler = 90f;

                #region Convert noteControls

                var preVal = 1f;
                var preDis = 0f;
                var preEase = 0;
                foreach (var e in rpeChartData.judgeLineList[i].posControl)
                {
                    if (preEase == 0)
                    {
                        preVal = e.pos;
                        preDis = e.x;
                        preEase = e.easing;
                        continue;
                    }

                    retChart.judgeLineList[i].posControl.Add(new noteControl
                    {
                        easeType = e.easing,
                        startValue = preVal,
                        endValue = e.pos,
                        start = preDis / noteControlScaler,
                        end = e.x / noteControlScaler
                    });
                    preVal = e.pos;
                    preDis = e.x;
                    preEase = e.easing;
                }

                retChart.judgeLineList[i].posControl.Add(new noteControl
                {
                    easeType = preEase == 0 ? 1 : preEase,
                    startValue = preVal,
                    endValue = preVal,
                    start = preDis / noteControlScaler,
                    end = float.MaxValue
                });
                preVal = 1f;
                preDis = 0f;
                preEase = 0;
                foreach (var e in rpeChartData.judgeLineList[i].skewControl)
                {
                    if (preEase == 0)
                    {
                        preVal = e.skew;
                        preDis = e.x;
                        preEase = e.easing;
                        continue;
                    }

                    retChart.judgeLineList[i].skewControl.Add(new noteControl
                    {
                        easeType = e.easing,
                        startValue = preVal,
                        endValue = e.skew,
                        start = preDis / noteControlScaler,
                        end = e.x / noteControlScaler
                    });
                    preVal = e.skew;
                    preDis = e.x;
                    preEase = e.easing;
                }

                retChart.judgeLineList[i].skewControl.Add(new noteControl
                {
                    easeType = preEase == 0 ? 1 : preEase,
                    startValue = preVal,
                    endValue = preVal,
                    start = preDis / noteControlScaler,
                    end = float.MaxValue
                });
                preVal = 1f;
                preDis = 0f;
                preEase = 0;
                foreach (var e in rpeChartData.judgeLineList[i].sizeControl)
                {
                    if (preEase == 0)
                    {
                        preVal = e.size;
                        preDis = e.x;
                        preEase = e.easing;
                        continue;
                    }

                    retChart.judgeLineList[i].sizeControl.Add(new noteControl
                    {
                        easeType = e.easing,
                        startValue = preVal,
                        endValue = e.size,
                        start = preDis / noteControlScaler,
                        end = e.x / noteControlScaler
                    });
                    preVal = e.size;
                    preDis = e.x;
                    preEase = e.easing;
                }

                retChart.judgeLineList[i].sizeControl.Add(new noteControl
                {
                    easeType = preEase == 0 ? 1 : preEase,
                    startValue = preVal,
                    endValue = preVal,
                    start = preDis / noteControlScaler,
                    end = float.MaxValue
                });
                preVal = 1f;
                preDis = 0f;
                preEase = 0;
                foreach (var e in rpeChartData.judgeLineList[i].yControl)
                {
                    if (preEase == 0)
                    {
                        preVal = e.y;
                        preDis = e.x;
                        preEase = e.easing;
                        continue;
                    }

                    retChart.judgeLineList[i].yControl.Add(new noteControl
                    {
                        easeType = e.easing,
                        startValue = preVal,
                        endValue = e.y,
                        start = preDis / noteControlScaler,
                        end = e.x / noteControlScaler
                    });
                    preVal = e.y;
                    preDis = e.x;
                    preEase = e.easing;
                }

                retChart.judgeLineList[i].yControl.Add(new noteControl
                {
                    easeType = preEase == 0 ? 1 : preEase,
                    startValue = preVal,
                    endValue = preVal,
                    start = preDis / noteControlScaler,
                    end = float.MaxValue
                });
                preVal = 1f;
                preDis = 0f;
                preEase = 0;
                foreach (var e in rpeChartData.judgeLineList[i].alphaControl)
                {
                    if (preEase == 0)
                    {
                        preVal = e.alpha;
                        preDis = e.x;
                        preEase = e.easing;
                        continue;
                    }

                    retChart.judgeLineList[i].alphaControl.Add(new noteControl
                    {
                        easeType = e.easing,
                        startValue = preVal,
                        endValue = e.alpha,
                        start = preDis / noteControlScaler,
                        end = e.x / noteControlScaler
                    });
                    preVal = e.alpha;
                    preDis = e.x;
                    preEase = e.easing;
                }

                retChart.judgeLineList[i].alphaControl.Add(new noteControl
                {
                    easeType = preEase == 0 ? 1 : preEase,
                    startValue = preVal,
                    endValue = preVal,
                    start = preDis / noteControlScaler,
                    end = float.MaxValue
                });

                #endregion

                //Convert layers
                for (var j = 0; j < rpeChartData.judgeLineList[i].eventLayers.Count; j++)
                {
                    retChart.judgeLineList[i].rpeLayers.Add(new());

                    //Convert speed
                    foreach (var e in rpeChartData.judgeLineList[i].eventLayers[j].speedEvents)
                    {
                        retChart.judgeLineList[i].rpeLayers[j].speedEvents.Add(new judgeLineSpeedEvent
                        {
                            value = e.start / 4.5f,
                            endValue = e.end / 4.5f,
                            startTime = RecalcTime(bpms, Frac(e.startTime)),
                            endTime = RecalcTime(bpms, Frac(e.endTime))
                        });
                    }

                    retChart.judgeLineList[i].rpeLayers[j].speedEvents = retChart.judgeLineList[i].rpeLayers[j]
                        .speedEvents.OrderBy(x => x.startTime).ToList();
                    var temp = new List<judgeLineSpeedEvent>();
                    for (int k = 1; k < retChart.judgeLineList[i].rpeLayers[j].speedEvents.Count; k++)
                    {
                        temp.Add(retChart.judgeLineList[i].rpeLayers[j].speedEvents[k - 1]);
                        if (retChart.judgeLineList[i].rpeLayers[j].speedEvents[k].startTime >
                            retChart.judgeLineList[i].rpeLayers[j].speedEvents[k - 1].endTime)
                        {
                            temp.Add(new()
                            {
                                endTime = retChart.judgeLineList[i].rpeLayers[j].speedEvents[k].startTime,
                                endValue = retChart.judgeLineList[i].rpeLayers[j].speedEvents[k - 1].endValue,
                                value = retChart.judgeLineList[i].rpeLayers[j].speedEvents[k - 1].endValue,
                                startTime = retChart.judgeLineList[i].rpeLayers[j].speedEvents[k - 1].endTime
                            });
                        }
                    }

                    if (retChart.judgeLineList[i].rpeLayers[j].speedEvents.Count != 0)
                    {
                        temp.Add(retChart.judgeLineList[i].rpeLayers[j].speedEvents[^1]);
                    }

                    if (temp.Count != 0)
                    {
                        temp.Add(new judgeLineSpeedEvent
                        {
                            value = temp[^1].endValue,
                            endValue = temp[^1].endValue,
                            startTime = temp[^1].endTime,
                            endTime = 99999,
                        });
                    }

                    retChart.judgeLineList[i].rpeLayers[j].speedEvents = temp;

                    //Convert alpha
                    foreach (var e in rpeChartData.judgeLineList[i].eventLayers[j].alphaEvents)
                    {
                        retChart.judgeLineList[i].rpeLayers[j].alphaEvents.Add(new judgeLineEvent
                        {
                            start = e.start / 255f,
                            end = e.end / 255f,
                            startTime = RecalcTime(bpms, Frac(e.startTime)),
                            endTime = RecalcTime(bpms, Frac(e.endTime)),
                            easeType = e.easingType,
                            easingLeft = e.easingLeft,
                            easingRight = e.easingRight
                        });
                    }

                    retChart.judgeLineList[i].rpeLayers[j].alphaEvents = retChart.judgeLineList[i].rpeLayers[j]
                        .alphaEvents.OrderBy(x => x.startTime).ToList();
                    //Convert rotation
                    foreach (var e in rpeChartData.judgeLineList[i].eventLayers[j].rotateEvents)
                    {
                        retChart.judgeLineList[i].rpeLayers[j].rotateEvents.Add(new judgeLineEvent
                        {
                            start = -e.start,
                            end = -e.end,
                            startTime = RecalcTime(bpms, Frac(e.startTime)),
                            endTime = RecalcTime(bpms, Frac(e.endTime)),
                            easeType = e.easingType,
                            easingLeft = e.easingLeft,
                            easingRight = e.easingRight
                        });
                    }

                    retChart.judgeLineList[i].rpeLayers[j].rotateEvents = retChart.judgeLineList[i].rpeLayers[j]
                        .rotateEvents.OrderBy(x => x.startTime).ToList();
                    //Convert moveX
                    foreach (var e in rpeChartData.judgeLineList[i].eventLayers[j].moveXEvents)
                    {
                        retChart.judgeLineList[i].rpeLayers[j].moveXEvents.Add(new judgeLineEvent
                        {
                            start = e.start / 675f / 2f + .5f,
                            end = e.end / 675f / 2f + .5f,
                            startTime = RecalcTime(bpms, Frac(e.startTime)),
                            endTime = RecalcTime(bpms, Frac(e.endTime)),
                            easeType = e.easingType,
                            easingLeft = e.easingLeft,
                            easingRight = e.easingRight
                        });
                    }

                    retChart.judgeLineList[i].rpeLayers[j].moveXEvents = retChart.judgeLineList[i].rpeLayers[j]
                        .moveXEvents.OrderBy(x => x.startTime).ToList();
                    //Convert moveY
                    foreach (var e in rpeChartData.judgeLineList[i].eventLayers[j].moveYEvents)
                    {
                        retChart.judgeLineList[i].rpeLayers[j].moveYEvents.Add(new judgeLineEvent
                        {
                            start = e.start / 450f / 2f + .5f,
                            end = e.end / 450f / 2f + .5f,
                            startTime = RecalcTime(bpms, Frac(e.startTime)),
                            endTime = RecalcTime(bpms, Frac(e.endTime)),
                            easeType = e.easingType,
                            easingLeft = e.easingLeft,
                            easingRight = e.easingRight
                        });
                    }

                    retChart.judgeLineList[i].rpeLayers[j].moveYEvents = retChart.judgeLineList[i].rpeLayers[j]
                        .moveYEvents.OrderBy(x => x.startTime).ToList();
                }

                //Convert notes
                for (var j = 0; j < rpeChartData.judgeLineList[i].notes.Count; j++)
                {
                    var t = rpeChartData.judgeLineList[i].notes[j];

                    if (t.type != 2)
                    {
                        retChart.judgeLineList[i].PushNote(NoteType(t.type), t.above == 1,
                            RecalcTime(bpms, Frac(t.startTime)), t.positionX / 70.3125f / 1.08f, t.speed,
                            0, 0, t.isFake, t.yOffset / 450f / 1.08f, t.size, t.visibleTime, t.alpha / 255f);
                    }
                    else
                    {
                        retChart.judgeLineList[i].PushNote(NoteType(t.type), t.above == 1,
                            RecalcTime(bpms, Frac(t.startTime)), t.positionX / 70.3125f / 1.08f, t.speed,
                            0, RecalcTime(bpms, Frac(t.endTime)) - RecalcTime(bpms, Frac(t.startTime)), t.isFake,
                            t.yOffset / 450f / 1.08f, t.size, t.visibleTime, t.alpha / 255f);
                    }
                }
            }


            var bucket = new Dictionary<int, int>();
            foreach (var t in rpeChartData.judgeLineList)
            {
                if (!bucket.ContainsKey(t.zOrder))
                {
                    bucket.Add(t.zOrder, 1);
                }
                else
                {
                    bucket[t.zOrder]++;
                }
            }

            var maximumZOrder = 0;

            foreach (var i in bucket)
            {
                maximumZOrder = Math.Max(maximumZOrder, i.Value);
            }

            retChart.maxZOrder = maximumZOrder;

            return retChart;
        }

        private static float RecalcTime(List<BpmEvent> bpms, float time)
        {
            var timePhi = 0f;
            foreach (var i in bpms)
            {
                if (time > i.end)
                {
                    timePhi += (i.end - i.start) * (60f / i.bpm);
                }
                else if (time >= i.start)
                {
                    timePhi += (time - i.start) * (60f / i.bpm);
                }
            }

            return timePhi;
        }

        private static float Frac(int[] frac)
        {
            if (frac.Length == 3)
            {
                if (frac.Length == 3) return frac[0] + (float) frac[1] / frac[2];
                return frac[0];
            }

            return frac.Length > 0 ? frac[0] : 0f;
        }

        private static Color ToColor(int[] frac)
        {
            if (frac.Length == 3)
            {
                return new Color(frac[0] / 255f, frac[1] / 255f, frac[2] / 255f, 1f);
            }

            return new Color(0f, 0f, 0f, 0f);
        }

        private static int NoteType(int x)
        {
            return x switch
            {
                1 => 1,
                3 => 4,
                4 => 2,
                2 => 3,
                _ => 1
            };
        }

        private class BpmEvent
        {
            public float bpm;
            public float end;
            public float start;

            public BpmEvent(float b, float s)
            {
                bpm = b;
                start = s;
                end = 1e9f;
            }
        }
    }
}