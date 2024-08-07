using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MainCore.Data;
using MainCore.UI;
using MainCore.Utilities;
using UnityEngine;

namespace MainCore
{
    public static class ChartLoader
    {
        public static Chart Chart { get; private set; } = new ();

        [CanBeNull]
        public static async Task<string> InitChartAuto(string path, bool isInternal, bool showMessage = true)
        {
            var cts = new CancellationTokenSource();
            if (showMessage)
            {
                _ = Task.Run(delegate
                {
                    var sec = 0;
                    while (true)
                    {
                        if (cts.IsCancellationRequested)
                        {
                            PopupMessageManager.Instance.Clear();
                            cts.Token.ThrowIfCancellationRequested();
                            return;
                        }
                        Thread.Sleep(1000);
                        sec++;
                        PopupMessageManager.Instance.ChangeContent($"Reading chart. Waiting for {sec}s");
                    }
                }, cts.Token);
            }
            var rawChart = isInternal
                ? Resources.Load<TextAsset>(path).text
                : await File.ReadAllTextAsync(path, cts.Token).ConfigureAwait(false);
            
            cts.Cancel();
            rawChart = rawChart.Replace("\r\n", "\n");
            if (!rawChart.Contains("}") && rawChart.Contains("bp"))
            {
                await InitPecChart(rawChart);
            }
            else if (rawChart.Contains("}") && rawChart.Contains("formatVersion"))
            {
                await InitPgrChart(rawChart);
            }
            else if (rawChart.Contains("}") && rawChart.Contains("numOfNotes"))
            {
                await InitRpeChart(rawChart, showMessage);
            }
            return rawChart;
        }

        private static Task InitPgrChart(string ch)
        {
            Chart = JsonUtility.FromJson<Chart>(ch);
            PreparePgrChart();
            ConvertEventsToLayer();
            return Task.CompletedTask;
        }

        private static async Task InitPecChart(string ch)
        {
            Chart = await Pec2Json.Chart123(ch).ConfigureAwait(false);
            ConvertEventsToLayer();
        }

        private static async Task InitRpeChart(string ch, bool showMessage)
        {
            Chart = await Rpe2Json.Chart123(ch, showMessage).ConfigureAwait(false);
        }

        private static void PreparePgrChart()
        {
            int noteCount = 0;
            foreach (var t in Chart.judgeLineList)
            {
                t.numOfNotes = t.notesAbove.Count + t.notesBelow.Count;
                noteCount += t.numOfNotes;
                float tempBpm = t.bpm;
                float factor = 1.875f / tempBpm;
                foreach (note n in t.notesAbove)
                {
                    n.time *= factor;
                    n.holdTime *= factor;
                    if (n.type == 3)
                    {
                        n.speed = 1;
                    }
                }

                foreach (note n in t.notesBelow)
                {
                    n.time *= factor;
                    n.holdTime *= factor;
                    if (n.type == 3)
                    {
                        n.speed = 1;
                    }
                }

                foreach (judgeLineSpeedEvent e in t.speedEvents)
                {
                    e.startTime *= factor;
                    e.endTime *= factor;
                    e.endValue = e.value;
                }

                ArrangeLineEvent(t.judgeLineDisappearEvents, factor);
                ArrangeLineEvent(t.judgeLineRotateEvents, factor);
                ArrangeLineEvent(t.judgeLineMoveEvents, factor);
            }

            Chart.numOfNotes = noteCount;

            void ArrangeLineEvent(List<judgeLineEvent> evs, float factor)
            {
                foreach (judgeLineEvent e in evs)
                {
                    e.startTime *= factor;
                    e.endTime *= factor;
                }
            }
        }

        public static void LoadCsvLineImage()
        {
            for (int i = 0; i < GlobalSetting.Lines.Count; i++)
            {
                try
                {
                    int lineId = int.Parse(GlobalSetting.CurrentBeatmapInfo.LineImage.GetDataByRowAndCol(i + 1, 1));
                    var t1 = float.Parse(GlobalSetting.CurrentBeatmapInfo.LineImage.GetDataByRowAndCol(i + 1, 3));
                    WWW a = new WWW("file://" + Path.Combine(GlobalSetting.CurrentBeatmapInfo.BasePath,
                        GlobalSetting.CurrentBeatmapInfo.LineImage.GetDataByRowAndCol(i + 1, 2)));
                    while (!a.isDone)
                    {
                    }
                    t1 = t1 > 0 ? t1 : Mathf.Abs(t1);
                    t1 = (200 * t1 * Camera.main.orthographicSize / a.texture.height);
                    var t2 = t1 / float.Parse(GlobalSetting.CurrentBeatmapInfo.LineImage.GetDataByRowAndCol(i + 1, 4));
                    Sprite sprite = Sprite.Create(a.texture, new Rect(0, 0, a.texture.width, a.texture.height),
                        Vector2.one / 2f);
                    GlobalSetting.Lines[lineId].GetComponent<SpriteRenderer>().sprite = sprite;
                    GlobalSetting.Lines[lineId].TargetScale = new Vector3(t1, t2, 1);
                    GlobalSetting.Lines[lineId].IsImage = true;
                }
                catch
                {
                    // Ignore
                }
            }
        }

        private static void ConvertEventsToLayer()
        {
            foreach (var l in Chart.judgeLineList)
            {
                l.rpeLayers.Add(new judegeLineEventLayer());
                l.rpeLayers[0].alphaEvents = l.judgeLineDisappearEvents;
                l.rpeLayers[0].rotateEvents = l.judgeLineRotateEvents;
                foreach (var e in l.judgeLineMoveEvents)
                {
                    l.rpeLayers[0].moveXEvents.Add(new judgeLineEvent()
                    {
                        start = e.start,
                        end = e.end,
                        startTime = e.startTime,
                        endTime = e.endTime,
                        easeType = e.easeType
                    });
                    l.rpeLayers[0].moveYEvents.Add(new judgeLineEvent()
                    {
                        start = e.start2,
                        end = e.end2,
                        startTime = e.startTime,
                        endTime = e.endTime,
                        easeType = e.easeType
                    });
                }

                l.rpeLayers[0].speedEvents = l.speedEvents;

                l.speedEvents = null;
                l.judgeLineDisappearEvents = null;
                l.judgeLineRotateEvents = null;
                l.judgeLineMoveEvents = null;
            }
        }

        public static void ApplyPhiraOffset(float? f)
        {
            if (f == null)
            {
                return;
            }

            Chart.offset += (float)f;
        }
    }
}