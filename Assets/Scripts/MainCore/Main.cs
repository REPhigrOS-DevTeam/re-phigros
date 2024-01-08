using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Gui;
using LeTai.Asset.TranslucentImage;
using MainCore.Common;
using MainCore.Data;
using MainCore.PostProcessing;
using MainCore.UI;
using MainCore.Utilities;
using Network.Multiplayer.Managers;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainCore
{
    public class Main : MonoSingleton<Main>
    {
        private static Chart json = new Chart();

        public static AudioClip music;

        public ProgressManager progressManager;
        private new static AudioSource audio;
        public GameObject line;
        public Image illustration;
        public Text comboText;
        public Text comboIndicator;
        public Text scoreText, accText;
        public GameObject managers;
        public Transform instantiateTransform;
        public DoubleTapButton pauseButton;
        public LeanWindow pauseWindow;
        public LeanButton backButton, continueButton, retryButton, terminateButton;
        public Camera uiCamera;
        public Camera particleCamera;
        public PostProcessVolume postProcessVolume;
        public SpriteRenderer maskSprite;
        public VideoManager videoManager;
        private float aspect = 16f / 9f;

        private string chart;
        private AsyncOperation operation;

        private bool playedFlag = false;

        public static float MusicTime => audio.time;

        private void OnAudioResolutionError()
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "DSPBuffer数值过小", () =>
            {
                Destroy(progressManager);
                Quit();
            }, "返回");
        }

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            GlobalSetting.LineColors.Add(JudgeLineStat.AP, GlobalSetting.CurrentSkinInfo.perfectColor);
            GlobalSetting.LineColors.Add(JudgeLineStat.FC, GlobalSetting.CurrentSkinInfo.goodColor);
            GlobalSetting.LineColors.Add(JudgeLineStat.None, new Color(1, 1, 1, 1));
            progressManager.Init(OnAudioResolutionError, OnAudioResolutionError);

            InitChart();

            if (GlobalSetting.ExtraEvents != null)
            {
                if (GlobalSetting.ExtraEvents.Effects != null)
                {
                    Camera.main.gameObject.AddComponent<ExtraShaderProvider>().IsGlobal = false;
                    uiCamera.gameObject.AddComponent<ExtraShaderProvider>().IsGlobal = true;
                    particleCamera.gameObject.AddComponent<ExtraShaderProvider>().IsGlobal = false;
                }
#if (UNITY_EDITOR || !RELEASE_VERSION) && false // BUG: 为啥放不了
                if (GlobalSetting.extraEvents.Videos != null)
                {
                    GlobalSetting.extraEvents.Bpm.OrderBy(x => x.time.Frac()).ToList().ForEach(x =>
                    {
                        bpms.Add(new BpmEvent(x.bpm, x.time.Frac()));
                        if (bpms.Count >= 2)
                        {
                            bpms[^2].end = bpms[^1].start;
                        }
                    });
                    GlobalSetting.extraEvents.Videos.ForEach(x =>
                    {
                        x.realTime = RecalcTime(x.time.Frac());
                        x.ScaleMode = x.scale switch
                        {
                            "cropCenter" => ScaleMode.ScaleAndCrop,
                            "inside" => ScaleMode.ScaleToFit,
                            "fit" => ScaleMode.StretchToFill,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    });
                    videoManager.Init(GlobalSetting.extraEvents.Videos.ToArray());
                }
                else
                {
                    Destroy(videoManager.gameObject);
                }
#else
                Destroy(videoManager.gameObject);
#endif
            }


#if UNITY_EDITOR
            Mian = this;
#endif
        }

        private List<BpmEvent> bpms = new();

        private float RecalcTime(float time)
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

        void Start()
        {
            if (GlobalSetting.DisableBlur)
            {
                GameObject.Find("BackgroundCamera").GetComponent<TranslucentImageSource>().enabled = false;
            }

            if (Screen.width * 1f / Screen.height >= aspect)
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.height * aspect;
                GlobalSetting.WidthOffset = (Screen.width - GlobalSetting.ScreenWidth) / 2f;
                maskSprite.transform.localScale = new Vector3(1782, 8000, 0);
            }
            else
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.width;
                GlobalSetting.WidthOffset = 0;
                maskSprite.transform.localScale = new Vector3(8000, 8000, 0);
            }

            audio = gameObject.AddComponent<AudioSource>(); // 这段代码留在这里，不要乱动

            managers.AddComponent<JudgementManager>();

            GlobalSetting.Playing = false;

            audio.playOnAwake = false;
            audio.clip = music;
            audio.pitch = GlobalSetting.Pitch;
            GlobalSetting.MusicLength = music.length;

            illustration.sprite = GlobalSetting.BackgroundImage;
            GlobalSetting.FormatVersion = json.formatVersion;
            GlobalSetting.ScoreCounter.numOfNotes = json.numOfNotes;

            foreach (GameObject i in GameObject.FindGameObjectsWithTag("Lines"))
            {
                GlobalSetting.Lines.Add(i.GetComponentInChildren<JudgeLineMovement>());
                i.GetComponentInChildren<Animation>().Play("StartGradient");
            }

            foreach (GameObject i in GameObject.FindGameObjectsWithTag("UI"))
            {
                i.GetComponent<Animation>().Play("StartGradientChartName");
                i.GetComponent<Text>().text = $"{GlobalSetting.ChartName}\n\n";
            }

            GameObject.Find("SongNameLeftBottom").GetComponent<Text>().text = "   " + GlobalSetting.ChartName;
            GameObject.Find("DiffText").GetComponent<Text>().text = GlobalSetting.Difficulty + "  ";

            if (GlobalSetting.LineImage != null)
            {
                LoadCsvLineImage();
            }

            Camera.main.orthographic = true;
            Camera.main.GetComponent<PostProcessLayer>().antialiasingMode = GlobalSetting.FxaaEnabled
                ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing
                : PostProcessLayer.Antialiasing.None;
            Camera.main.GetComponent<PostProcessLayer>().fastApproximateAntialiasing = GlobalSetting.FxaaEnabled
                ? new FastApproximateAntialiasing
                {
                    fastMode = true,
                    keepAlpha = false
                }
                : null;
            postProcessVolume.enabled = GlobalSetting.PostProcessing;

            if (!GlobalSetting.PostProcessing && !GlobalSetting.FxaaEnabled)
            {
                Camera.main.GetComponent<PostProcessLayer>().enabled = false;
            }
            
            accText.gameObject.SetActive(GlobalSetting.DisplayAcc);

            StartPlay();
        }

        void Update()
        {
            if (Screen.width * 1f / Screen.height >= aspect)
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.height * aspect;
                GlobalSetting.WidthOffset = (Screen.width - GlobalSetting.ScreenWidth) / 2f;
                maskSprite.transform.localScale = new Vector3(1782, 8000, 0);
            }
            else
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.width;
                GlobalSetting.WidthOffset = 0;
                maskSprite.transform.localScale = new Vector3(8000, 8000, 0);
            }

            if (GlobalSetting.Playing)
            {
                progressManager.OnUpdate();
            }


            if (progressManager.NowNoDelayTime >= audio.clip.length && GlobalSetting.Playing)
            {
                progressManager.StopTiming();
                GlobalSetting.Playing = false;
                if (!GlobalSetting.IsEnding)
                {
                    GameObject.Find("CutInOut").GetComponent<Animation>().Play("CutOut");
                    maskSprite.DOFade(0f, 2f);
                    LoadEnding();
                }

                return;
            }

#if UNITY_EDITOR

            TEST_COUNT = 0;
            if (Camera.main.aspect >= aspect)
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.height * aspect;
                GlobalSetting.WidthOffset = (Screen.width - GlobalSetting.ScreenWidth) / 2f;
                maskSprite.transform.localScale = new Vector3(1782, 8000, 0);
            }
            else
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.width;
                GlobalSetting.WidthOffset = 0;
                maskSprite.transform.localScale = new Vector3(8000, 8000, 0);
            }

            //float factor = (float)Math.Sqrt(aspect / GlobalSetting.aspect);
            //Camera.main.orthographicSize = 5 * factor;
            //Camera.main.fieldOfView = 60 * factor;
            //particleCamera.orthographicSize = 5 * factor;
            //particleCamera.fieldOfView = 60 * factor;
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                audio.time += 5f * GlobalSetting.Pitch;
                progressManager.AddTime(5f);
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                audio.time += 1f * GlobalSetting.Pitch;
                progressManager.AddTime(1f);
            }
#endif
            comboText.text = GlobalSetting.ScoreCounter.combo < 3 ? "" : $"{GlobalSetting.ScoreCounter.combo}";
            if (GlobalSetting.ScoreCounter.combo < 3)
            {
                comboIndicator.text = "";
            }
            else if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Yande)
            {
                comboIndicator.text = "PEPOYO DAISUKI";
            }
            else if (GlobalSetting.YayaKawaii == GlobalSetting.YayaMode.绝冲)
            {
                comboIndicator.text = "YAYA KAWAII";
            }
            else if (GlobalSetting.AutoPlay)
            {
                comboIndicator.text = "AUTOPLAY";
            }
            else
            {
                comboIndicator.text = "COMBO";
            }

            scoreText.text = $"{Mathf.RoundToInt(GlobalSetting.ScoreCounter.Score).ToString().PadLeft(7, '0')} ";
            accText.text = $"{100f * GlobalSetting.ScoreCounter.RuntimeAccuracy:0.00}%";
        }

#if !UNITY_EDITOR
        private void OnApplicationFocus(bool isFocus)
        {
            if (!GlobalSetting.Paused)
            {
                Pause();
            }
        }

        private void OnApplicationPause(bool isPause)
        {
            if (!GlobalSetting.Paused)
            {
                Pause();
            }
        }
#endif

        private float totalOffset;

        private async void StartPlay()
        {
            //GameObject.Find("CutInOut").GetComponent<Animation>().Play("CutIn");

            // We pre-generate one HitFX to avoid the high Disk usage of reading the prefab.
            // 预生成HitFX，避免读取prefab时吃硬盘
            var hitFX = HitEffectManager.GetInstance()
                .GetObj(HitFxJudgeType.Perfect, GlobalSetting.CurrentSkinInfo);
            hitFX.transform.localPosition = new Vector3(5000, 5000, 0);
            hitFX.PlayEffect();
            hitFX.PlayParticle();
            hitFX = HitEffectManager.GetInstance()
                .GetObj(HitFxJudgeType.Good, GlobalSetting.CurrentSkinInfo);
            hitFX.transform.localPosition = new Vector3(5000, 5000, 0);
            hitFX.PlayEffect();
            hitFX.PlayParticle();

            totalOffset = json.offset + GlobalSetting.UserOffset;
            maskSprite.DOFade(GlobalSetting.MaskAlpha, 3f);
            audio.PlayScheduled(AudioSettings.dspTime + 4f);
            progressManager.AddStartDelay(totalOffset);
            //totalOffset -= .05f; //fixed delay
            await new WaitForSeconds(4f);
            GlobalSetting.Playing = true;
            progressManager.StartTiming();
            RegisterPauseMenu();
        }

        private void RegisterPauseMenu()
        {
            pauseButton.OnDoubleTap.AddListener(Pause);
            backButton.OnClick.AddListener(Quit);
            continueButton.OnClick.AddListener(() => UnPause().Forget());
            retryButton.OnClick.AddListener(() =>
            {
                GlobalSetting.Reset();
                SceneTransit.Instance.JumpScene("PlayingScene");
            });
            terminateButton.OnClick.AddListener(() =>
            {
                playedFlag = true;
                audio.time = 0;
                progressManager.AddTime(audio.clip.length);
            });
            if (GlobalSetting.IsMultiplayer)
            {
                backButton.OnClick.AddListener(() => SocketManager.QuitGame());
                terminateButton.OnClick.AddListener(() => SocketManager.QuitGame());
            }
        }

        private void Quit()
        {
            SceneTransit.Instance.Back();
        }

        private async void LoadEnding()
        {
            GlobalSetting.IsEnding = true;
            operation = SceneManager.LoadSceneAsync("LevelOver 1");
            operation.allowSceneActivation = false;
            await new WaitForSeconds(2);
            operation.allowSceneActivation = true;
            await operation;
            //SceneTransit.Instance.TransitTo("LevelOver 1");
        }

        public static async Task InitChartAuto(string path, bool isInternal, bool showMessage = true)
        {
            var cts = new CancellationTokenSource();
            if (showMessage)
                Task.Run(delegate
                {
                    var sec = 0;
                    while (true)
                    {
                        PopupMessageManager.Instance.ChangeContent($"Reading chart. Waiting for {sec}s");
                        Thread.Sleep(1000);
                        sec++;
                        if (cts.IsCancellationRequested)
                        {
                            cts.Token.ThrowIfCancellationRequested();
                        }
                    }
                }, cts.Token);
            var ch = isInternal
                ? Resources.Load<TextAsset>(path).text
                : await File.ReadAllTextAsync(path, cts.Token).ConfigureAwait(false);
            cts.Cancel();
            ch = ch.Replace("\r\n", "\n");
            GlobalSetting.Chart = ch;
            if (!ch.Contains("}") && ch.Contains("bp"))
            {
                await InitPecChart(ch);
            }
            else if (ch.Contains("}") && ch.Contains("formatVersion"))
            {
                await InitPgrChart(ch);
            }
            else if (ch.Contains("}") && ch.Contains("numOfNotes"))
            {
                await InitRpeChart(ch, showMessage);
            }
        }

        private static async Task InitPgrChart(string ch)
        {
            json = JsonUtility.FromJson<Chart>(ch);
            PreparationPgrChart();
            ConvertEventsToLayer();
        }

        private static async Task InitPecChart(string ch)
        {
            json = await Pec2Json.Chart123(ch).ConfigureAwait(false);
            ConvertEventsToLayer();
        }

        private static async Task InitRpeChart(string ch, bool showMessage)
        {
            json = await Rpe2Json.Chart123(ch, showMessage).ConfigureAwait(false);
        }

        private void InitChart()
        {
            var i = 0;
            foreach (var l in json.judgeLineList)
            {
                var t = Instantiate(line, instantiateTransform);
                var jlm = t.GetComponentInChildren<JudgeLineMovement>();
                jlm.ID = i;
                jlm.Line = l;
                GlobalSetting.Lines.Add(jlm);
                i++;
            }

            foreach (var l in json.judgeLineList)
            {
                foreach (note n in l.notesAbove)
                {
                    int t;
                    if (GlobalSetting.HighLightedNotes.TryGetValue(n.time, out t))
                        GlobalSetting.HighLightedNotes[n.time]++;
                    else
                        GlobalSetting.HighLightedNotes.Add(n.time, 1);
                    n.isAbove = true;
                }

                foreach (note n in l.notesBelow)
                {
                    int t;
                    if (GlobalSetting.HighLightedNotes.TryGetValue(n.time, out t))
                        GlobalSetting.HighLightedNotes[n.time]++;
                    else
                        GlobalSetting.HighLightedNotes.Add(n.time, 1);
                    n.isAbove = false;
                }
            }

            foreach (var l in json.judgeLineList)
            {
                foreach (note n in l.notesAbove)
                {
                    if (GlobalSetting.HighLightedNotes[n.time] > 1 && GlobalSetting.HighLight)
                    {
                        n.isMulti = true;
                    }
                }

                foreach (note n in l.notesBelow)
                {
                    if (GlobalSetting.HighLightedNotes[n.time] > 1 && GlobalSetting.HighLight)
                    {
                        n.isMulti = true;
                    }
                }
            }

            GlobalSetting.HighLightedNotes.Clear();
            GlobalSetting.MaximumZOrder = json.judgeLineList.Count;
        }

        private static void PreparationPgrChart()
        {
            int noteCount = 0;
            foreach (var t in json.judgeLineList)
            {
                t.numOfNotes = t.notesAbove.Count + t.notesBelow.Count;
                noteCount += t.numOfNotes;
                float tempBpm = t.bpm;
                float factor = 1.875f / tempBpm;
                foreach (note n in t.notesAbove)
                {
                    n.time = n.time * factor;
                    n.holdTime = n.holdTime * factor;
                    if (n.type == 3)
                    {
                        n.speed = 1;
                    }
                }

                foreach (note n in t.notesBelow)
                {
                    n.time = n.time * factor;
                    n.holdTime = n.holdTime * factor;
                    if (n.type == 3)
                    {
                        n.speed = 1;
                    }
                }

                foreach (judgeLineSpeedEvent e in t.speedEvents)
                {
                    e.startTime = e.startTime * factor;
                    e.endTime = e.endTime * factor;
                    e.endValue = e.value;
                }

                foreach (judgeLineEvent e in t.judgeLineDisappearEvents)
                {
                    e.startTime = e.startTime * factor;
                    e.endTime = e.endTime * factor;
                }

                foreach (judgeLineEvent e in t.judgeLineRotateEvents)
                {
                    e.startTime = e.startTime * factor;
                    e.endTime = e.endTime * factor;
                }

                foreach (judgeLineEvent e in t.judgeLineMoveEvents)
                {
                    e.startTime = e.startTime * factor;
                    e.endTime = e.endTime * factor;
                }
            }

            json.numOfNotes = noteCount;
        }

        private static void LoadCsvLineImage()
        {
            for (int i = 0; i < GlobalSetting.Lines.Count; i++)
            {
                try
                {
                    int lineId = int.Parse(GlobalSetting.LineImage.GetDataByRowAndCol(i + 1, 1));
                    var t1 = float.Parse(GlobalSetting.LineImage.GetDataByRowAndCol(i + 1, 3));
                    WWW a = new WWW("file://" + Path.Combine(PlayerPrefs.GetString("chartFolderPath", ""),
                        GlobalSetting.LineImage.GetDataByRowAndCol(i + 1, 2)));
                    while (!a.isDone)
                    {
                    }

                    ;
                    t1 = t1 > 0 ? t1 : Mathf.Abs(t1);
                    t1 = (200 * t1 * Camera.main.orthographicSize / a.texture.height);
                    var t2 = t1 / float.Parse(GlobalSetting.LineImage.GetDataByRowAndCol(i + 1, 4));
                    Sprite sprite = Sprite.Create(a.texture, new Rect(0, 0, a.texture.width, a.texture.height),
                        Vector2.one / 2f);
                    GlobalSetting.Lines[lineId].GetComponent<SpriteRenderer>().sprite = sprite;
                    GlobalSetting.Lines[lineId].TargetScale = new Vector3(t1, t2, 1);
                    GlobalSetting.Lines[lineId].IsImage = true;
                }
                catch
                {
                    continue;
                }
            }
        }

        private static void ConvertEventsToLayer()
        {
            foreach (var l in json.judgeLineList)
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

        void Pause()
        {
            if (GlobalSetting.Playing && !GlobalSetting.Paused && MusicTime > 3f && !GlobalSetting.Paused)
            {
                GlobalSetting.Paused = true;
                progressManager.StopTiming();
                audio.Pause();
                audio.volume = 0;
                float delta = Mathf.Min(3f, audio.time);
                audio.time = Mathf.Max(audio.time - 3f, 0f);
                progressManager.TimeGoBack(delta, () => pauseWindow.TurnOn());
                videoManager.Pause();
            }
        }

        async UniTaskVoid UnPause()
        {
            if (GlobalSetting.Playing && GlobalSetting.Paused)
            {
                pauseWindow.TurnOff();
                // audio.time = Stopwatch.ElapsedMilliseconds * .001f;
                progressManager.ContinueTiming();
                audio.UnPause();
                DOTween.To(() => audio.volume, (x) => audio.volume = x, 1f, 2f);
                await Task.Delay(3000);
                videoManager.Resume();
                GlobalSetting.Paused = false;
            }
        }

        public static void ApplyPhiraOffset(float? f)
        {
            if (f == null)
            {
                return;
            }

            json.offset += (float)f;
        }

#if UNITY_EDITOR
        public static Main Mian;
        public int TEST_COUNT { get; set; }
#endif
    }

    public enum NoteStat
    {
        Perfect,
        Good,
        Bad,
        Miss,
        None,
        Early,
        Late
    }

    public enum JudgeLineStat
    {
        AP,
        FC,
        None
    }

    public class ScoreCounter
    {
        public int perfectCnt;
        public int goodCnt;
        public int badCnt;
        public int missCnt;
        public int combo;
        public int early;
        public int late;
        public int maxcombo;
        public int numOfNotes;

        private int elapsedNoteCnt;
        public float Score => GlobalSetting.NewScoreCalcType
            ? 1e6f * (perfectCnt + goodCnt * 0.65f) / numOfNotes // 判定分100w
            : 1e6f * (perfectCnt * 0.9f + goodCnt * 0.585f + maxcombo * 0.1f) / numOfNotes; // 判定分90w 连击分10w

        public float Accuracy => (perfectCnt + goodCnt * 0.65f) / numOfNotes;
        public float RuntimeAccuracy => elapsedNoteCnt == 0 ? 1f : (perfectCnt + goodCnt * 0.65f) / elapsedNoteCnt;

        public void Add(NoteStat status)
        {
            switch (status)
            {
                case NoteStat.Perfect:
                    perfectCnt++;
                    combo++;
                    elapsedNoteCnt++;
                    break;
                case NoteStat.Good:
                    goodCnt++;
                    combo++;
                    elapsedNoteCnt++;
                    break;
                case NoteStat.Bad:
                    badCnt++;
                    combo = 0;
                    elapsedNoteCnt++;
                    break;
                case NoteStat.Miss:
                    missCnt++;
                    combo = 0;
                    elapsedNoteCnt++;
                    break;
                case NoteStat.Early:
                    goodCnt++;
                    early++;
                    combo++;
                    elapsedNoteCnt++;
                    break;
                case NoteStat.Late:
                    goodCnt++;
                    late++;
                    combo++;
                    elapsedNoteCnt++;
                    break;
            }

            if (combo > maxcombo)
                maxcombo = combo;
            if (GlobalSetting.LineStat == JudgeLineStat.AP && goodCnt != 0)
                GlobalSetting.LineStat = JudgeLineStat.FC;
            if (GlobalSetting.LineStat != JudgeLineStat.None && (badCnt != 0 || missCnt != 0))
                GlobalSetting.LineStat = JudgeLineStat.None;
        }
    }
}