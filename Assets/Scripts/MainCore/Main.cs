using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        public Text scoreText;
        public GameObject managers;
        public Transform instantiateTransform;
        public DoubleTapButton pauseButton;
        public LeanWindow pauseWindow;
        public LeanButton backButton, continueButton, retryButton, terminateButton;
        public Camera uiCamera;
        public Camera particleCamera;
        public PostProcessVolume postProcessVolume;
        public SpriteRenderer maskSprite;
        private float aspect = 16f / 9f;

        private string chart;
        private AsyncOperation operation;

        private bool playedFlag = false;

        public static float MusicTime => audio.time;

        private void OnAudioResolutionError()
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "DSPBuffer数值过小", Quit, "返回");
        }

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            GlobalSetting.lineColors.Add(JudgeLineStat.AP, new Color(0xfe / 256f, 0xff / 256f, 0xad / 256f, 1));
            GlobalSetting.lineColors.Add(JudgeLineStat.FC, new Color(0x8c / 256f, 0xec / 256f, 0xff / 256f, 1));
            GlobalSetting.lineColors.Add(JudgeLineStat.None, new Color(1, 1, 1, 1));
            progressManager.Init(OnAudioResolutionError, OnAudioResolutionError, () => 1.0f);

            InitChart();

            if (GlobalSetting.extraJson != "")
            {
                Camera.main.gameObject.AddComponent<ExtraShaderProvider>().IsGlobal = false;
                uiCamera.gameObject.AddComponent<ExtraShaderProvider>().IsGlobal = true;
                particleCamera.gameObject.AddComponent<ExtraShaderProvider>().IsGlobal = false;
            }


#if UNITY_EDITOR
            Mian = this;
#endif
        }

        void Start()
        {
            if (GlobalSetting.disableBlur)
            {
                GameObject.Find("UICamera").GetComponent<TranslucentImageSource>().enabled = false;
            }

            if (Camera.main.aspect >= aspect)
            {
                GlobalSetting.screenHeight = Screen.height;
                GlobalSetting.screenWidth = Screen.height * aspect;
                GlobalSetting.widthOffset = (Screen.width - GlobalSetting.screenWidth) / 2f;
                maskSprite.transform.localScale = new Vector3(1782, 8000, 0);
            }
            else
            {
                GlobalSetting.screenHeight = Screen.height;
                GlobalSetting.screenWidth = Screen.width;
                GlobalSetting.widthOffset = 0;
                maskSprite.transform.localScale = new Vector3(8000, 8000, 0);
            }

            managers.AddComponent<JudgementManager>();

            GlobalSetting.Playing = false;

            audio = gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.clip = music;
            
            GlobalSetting.MusicLength = music.length;
            illustration.sprite = GlobalSetting.backgroundImage;
            GlobalSetting.formatVersion = json.formatVersion;
            GlobalSetting.scoreCounter.numOfNotes = json.numOfNotes;

            foreach (GameObject i in GameObject.FindGameObjectsWithTag("Lines"))
            {
                GlobalSetting.lines.Add(i.GetComponentInChildren<JudgeLineMovement>());
                i.GetComponentInChildren<Animation>().Play("StartGradient");
            }

            foreach (GameObject i in GameObject.FindGameObjectsWithTag("UI"))
            {
                i.GetComponent<Animation>().Play("StartGradientChartName");
                i.GetComponent<Text>().text = $"{GlobalSetting.chartName}\n\n";
            }

            GameObject.Find("SongNameLeftBottom").GetComponent<Text>().text = "   " + GlobalSetting.chartName;
            GameObject.Find("DiffText").GetComponent<Text>().text = GlobalSetting.difficulty + "  ";
            GameObject.Find("VersionText").GetComponent<Text>().text =
                $"RE:Phigros {Application.version} by kagari939\n";

            if (GlobalSetting.lineImage != null)
            {
                LoadCsvLineImage();
            }

            Camera.main.orthographic = true;
            Camera.main.GetComponent<PostProcessLayer>().antialiasingMode = GlobalSetting.fxaaEnabled
                ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing
                : PostProcessLayer.Antialiasing.None;
            Camera.main.GetComponent<PostProcessLayer>().fastApproximateAntialiasing = GlobalSetting.fxaaEnabled
                ? new FastApproximateAntialiasing
                {
                    fastMode = true,
                    keepAlpha = false
                }
                : null;
            postProcessVolume.enabled = GlobalSetting.postProcessing;

            if (!GlobalSetting.postProcessing && !GlobalSetting.fxaaEnabled)
            {
                Camera.main.GetComponent<PostProcessLayer>().enabled = false;
            }

            StartCoroutine(StartPlay());
        }

        void Update()
        {
            if (audio.time >= 5f && GlobalSetting.Playing)
            {
                playedFlag = true;
            }

            if (GlobalSetting.Playing)
            {
                progressManager.OnUpdate();
            }

            if (audio.time <= 1f && playedFlag)
            {
                progressManager.StopTiming();
                GlobalSetting.Playing = false;
                if (!GlobalSetting.IsEnding)
                {
                    GameObject.Find("CutInOut").GetComponent<Animation>().Play("CutOut");
                    maskSprite.DOFade(0f, 2f);
                    StartCoroutine(LoadEnding());
                }

                return;
            }

#if UNITY_EDITOR

            TEST_COUNT = 0;
            if (Camera.main.aspect >= aspect)
            {
                GlobalSetting.screenHeight = Screen.height;
                GlobalSetting.screenWidth = Screen.height * aspect;
                GlobalSetting.widthOffset = (Screen.width - GlobalSetting.screenWidth) / 2f;
                maskSprite.transform.localScale = new Vector3(1782, 8000, 0);
            }
            else
            {
                GlobalSetting.screenHeight = Screen.height;
                GlobalSetting.screenWidth = Screen.width;
                GlobalSetting.widthOffset = 0;
                maskSprite.transform.localScale = new Vector3(8000, 8000, 0);
            }

            //float factor = (float)Math.Sqrt(aspect / GlobalSetting.aspect);
            //Camera.main.orthographicSize = 5 * factor;
            //Camera.main.fieldOfView = 60 * factor;
            //particleCamera.orthographicSize = 5 * factor;
            //particleCamera.fieldOfView = 60 * factor;
#endif

            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                audio.time += 5;
                progressManager.AddDelay(5f);
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                audio.time += 1;
                progressManager.AddDelay(1f);
            }

            comboText.text = GlobalSetting.scoreCounter.combo < 3 ? "" : $"{GlobalSetting.scoreCounter.combo}";
            if (GlobalSetting.scoreCounter.combo < 3)
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
            else if (GlobalSetting.autoPlay)
            {
                comboIndicator.text = GlobalSetting.recordMode ? "RECORD" : "AUTOPLAY";
            }
            else
            {
                comboIndicator.text = "COMBO";
            }

            scoreText.text = $"{Mathf.RoundToInt(GlobalSetting.scoreCounter.Score).ToString().PadLeft(7, '0')} ";
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

        private IEnumerator StartPlay()
        {
            //GameObject.Find("CutInOut").GetComponent<Animation>().Play("CutIn");

            //We pre-generate one HitFX to avoid the high Disk usage of reading the prefab.
            var hitFX = HitEffectManager.GetInstance()
                .GetObj(HitFxJudgeType.Perfect);
            hitFX.transform.localPosition = new Vector3(1000, 1000, 0);
            hitFX = HitEffectManager.GetInstance()
                .GetObj(HitFxJudgeType.Good);
            hitFX.transform.localPosition = new Vector3(1000, 1000, 0);

            maskSprite.DOFade(GlobalSetting.maskAlpha, 3f);
            audio.PlayScheduled(AudioSettings.dspTime + 4f);
            progressManager.AddStartDelay(json.offset + GlobalSetting.userOffset);
            //totalOffset -= .05f; //fixed delay
            yield return new WaitForSeconds(4);
            GlobalSetting.Playing = true;
            progressManager.StartTiming();
            RegisterPauseMenu();
        }

        private void RegisterPauseMenu()
        {
            pauseButton.OnDoubleTap.AddListener(Pause);
            backButton.OnClick.AddListener(Quit);
            continueButton.OnClick.AddListener(UnPause);
            retryButton.OnClick.AddListener(() =>
            {
                GlobalSetting.Reset();
                SceneTransit.Instance.TransitTo("PlayingScene");
            });
            terminateButton.OnClick.AddListener(() =>
            {
                playedFlag = true;
                audio.time = 0;
            });
            if (GlobalSetting.isMultiplayer)
            {
                backButton.OnClick.AddListener(() => SocketManager.QuitGame());
                terminateButton.OnClick.AddListener(() => SocketManager.QuitGame());
            }
        }

        private void Quit()
        {
            SceneTransit.Instance.TransitTo("ChartSelectorScene");
        }

        private IEnumerator LoadEnding()
        {
            GlobalSetting.IsEnding = true;
            operation = SceneManager.LoadSceneAsync("LevelOver 1");
            operation.allowSceneActivation = false;
            yield return new WaitForSeconds(2);
            operation.allowSceneActivation = true;
            yield return operation;
            //SceneTransit.Instance.TransitTo("LevelOver 1");
        }

        public static async Task InitChartAuto(string path, bool showMessage = true)
        {
            var cts = new CancellationTokenSource();
            Task.Run(delegate
            {
                var sec = 0;
                while (true)
                {
                    if (showMessage) PopupMessageManager.Instance.ChangeContent($"Reading chart. Waiting for {sec}s");
                    Thread.Sleep(1000);
                    sec++;
                    if (cts.IsCancellationRequested)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                    }
                }
            }, cts.Token);
            var ch = await File.ReadAllTextAsync(path, cts.Token).ConfigureAwait(false);
            cts.Cancel();
            GlobalSetting.chart = ch;
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
                GlobalSetting.lines.Add(jlm);
                i++;
            }

            foreach (var l in json.judgeLineList)
            {
                foreach (note n in l.notesAbove)
                {
                    int t;
                    if (GlobalSetting.highLightedNotes.TryGetValue(n.time, out t))
                        GlobalSetting.highLightedNotes[n.time]++;
                    else
                        GlobalSetting.highLightedNotes.Add(n.time, 1);
                    n.isAbove = true;
                }

                foreach (note n in l.notesBelow)
                {
                    int t;
                    if (GlobalSetting.highLightedNotes.TryGetValue(n.time, out t))
                        GlobalSetting.highLightedNotes[n.time]++;
                    else
                        GlobalSetting.highLightedNotes.Add(n.time, 1);
                    n.isAbove = false;
                }
            }

            foreach (var l in json.judgeLineList)
            {
                foreach (note n in l.notesAbove)
                {
                    if (GlobalSetting.highLightedNotes[n.time] > 1 && GlobalSetting.highLight)
                    {
                        n.isMulti = true;
                    }
                }

                foreach (note n in l.notesBelow)
                {
                    if (GlobalSetting.highLightedNotes[n.time] > 1 && GlobalSetting.highLight)
                    {
                        n.isMulti = true;
                    }
                }
            }

            GlobalSetting.highLightedNotes.Clear();
            GlobalSetting.maximumZOrder = json.judgeLineList.Count;
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
            for (int i = 0; i < GlobalSetting.lines.Count; i++)
            {
                try
                {
                    int lineId = int.Parse(GlobalSetting.lineImage.GetDataByRowAndCol(i + 1, 1));
                    var t1 = float.Parse(GlobalSetting.lineImage.GetDataByRowAndCol(i + 1, 3));
                    WWW a = new WWW("file://" + Path.Combine(PlayerPrefs.GetString("chartFolderPath", ""),
                        GlobalSetting.lineImage.GetDataByRowAndCol(i + 1, 2)));
                    while (!a.isDone)
                    {
                    }

                    ;
                    t1 = t1 > 0 ? t1 : Mathf.Abs(t1);
                    t1 = (200 * t1 * Camera.main.orthographicSize / a.texture.height);
                    var t2 = t1 / float.Parse(GlobalSetting.lineImage.GetDataByRowAndCol(i + 1, 4));
                    Sprite sprite = Sprite.Create(a.texture, new Rect(0, 0, a.texture.width, a.texture.height),
                        Vector2.one / 2f);
                    GlobalSetting.lines[lineId].GetComponent<SpriteRenderer>().sprite = sprite;
                    GlobalSetting.lines[lineId].TargetScale = new Vector3(t1, t2, 1);
                    GlobalSetting.lines[lineId].IsImage = true;
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
                audio.time -= 3f;
                progressManager.TimeGoBack(3f, () => pauseWindow.TurnOn());
            }
        }

        async void UnPause()
        {
            if (GlobalSetting.Playing && GlobalSetting.Paused)
            {
                pauseWindow.TurnOff();
                // audio.time = Stopwatch.ElapsedMilliseconds * .001f;
                progressManager.ContinueTiming();
                audio.UnPause();
                DOTween.To(() => audio.volume, (x) => audio.volume = x, 1f, 2f);
                await Task.Delay(3000);
                GlobalSetting.Paused = false;
            }
        }

        public static void OverloadInfoWithPhiraYaml(PhiraInfoData phiraInfoData)
        {
            if (phiraInfoData == null)
            {
                GlobalSetting.IsPhira = false;
                return;
            }

            json.offset += phiraInfoData.offset;


            if (!string.IsNullOrEmpty(phiraInfoData.music))
            {
                GlobalSetting.musicPath = Path.Combine(GlobalSetting.chartFolderPath,
                    phiraInfoData.music);
            }

            if (!string.IsNullOrEmpty(phiraInfoData.illustration))
            {
                GlobalSetting.illustrationPath =
                    Path.Combine(GlobalSetting.chartFolderPath, phiraInfoData.illustration);
            }

            GlobalSetting.IsPhira = true;
            GlobalSetting.charter = LoadIntoManager.Charter = phiraInfoData.charter;
            GlobalSetting.composer = LoadIntoManager.Composer = phiraInfoData.composer;
            GlobalSetting.illustrator = LoadIntoManager.Illustrator = phiraInfoData.illustrator;
            if (GlobalSetting.YayaKawaii != GlobalSetting.YayaMode.绝冲 &&
                GlobalSetting.PepoyoDaisuki != GlobalSetting.PepoyoMode.Yande)
            {
                GlobalSetting.chartName = phiraInfoData.name;
                GlobalSetting.difficulty = phiraInfoData.level;
            }
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
        public int badCnt;
        public int combo;
        public int early;
        public int goodCnt;
        public int late;
        public int maxcombo;
        public int missCnt;
        public int numOfNotes;
        public int perfectCnt;
        public float Score => 1e6f * (perfectCnt * 0.9f + goodCnt * 0.585f + maxcombo * 0.1f) / numOfNotes;
        public float Accuracy => (perfectCnt + goodCnt * 0.65f) / numOfNotes;

        public void Add(NoteStat status)
        {
            switch (status)
            {
                case NoteStat.Perfect:
                    perfectCnt++;
                    combo++;
                    break;
                case NoteStat.Good:
                    goodCnt++;
                    combo++;
                    break;
                case NoteStat.Bad:
                    badCnt++;
                    combo = 0;
                    break;
                case NoteStat.Miss:
                    missCnt++;
                    combo = 0;
                    break;
                case NoteStat.Early:
                    goodCnt++;
                    early++;
                    combo++;
                    break;
                case NoteStat.Late:
                    goodCnt++;
                    late++;
                    combo++;
                    break;
            }

            if (combo > maxcombo)
                maxcombo = combo;
            if (GlobalSetting.lineStat == JudgeLineStat.AP && goodCnt != 0)
                GlobalSetting.lineStat = JudgeLineStat.FC;
            if (GlobalSetting.lineStat != JudgeLineStat.None && (badCnt != 0 || missCnt != 0))
                GlobalSetting.lineStat = JudgeLineStat.None;
        }
    }
}