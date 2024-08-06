using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Gui;
using LeTai.Asset.TranslucentImage;
using MainCore.Common;
using MainCore.PostProcessing;
using MainCore.UI;
using MainCore.UI.Utils;
using Network.Multiplayer.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// ReSharper disable PossibleNullReferenceException
#pragma warning disable CS0618 // Type or member is obsolete

namespace MainCore
{
    public class Main : MonoSingleton<Main>
    {
        public static float MusicTime => _audioSource.time;

        public ProgressManager progressManager;
        public GameObject line;
        public RawImage illustration;
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
        public SpriteRenderer maskSprite;
        public GameObject multiplayerRank;
        public TextMeshProUGUI first, second, third;
        [SerializeField] private Text disconnectWarn;
        [SerializeField] private CanvasGroup uiCanvasGroup;
        
        private const float Standard916Aspect = 16f / 9f;
        private float _totalOffset;
        private static AudioSource _audioSource;
        private AsyncOperation _endingSceneLoadOperation;

        
#region MultipleMode

        private readonly Dictionary<string, int> _runtimeScores = new ();

        private void PrepareMultipleMode()
        {
            if (GlobalSetting.IsMultiplayer)
            {
                foreach (string player in GlobalSetting.PlayerList)
                {
                    _runtimeScores.Add(player, 0);
                }

                SocketManager.OnUpdateScoreReceived += UpdateRuntimeScore;
                SocketManager.OnUserQuitGame += RemoveUserFromRuntimeScore;

                UniTask.Void(async () =>
                {
                    RefreshRuntimeScoreUI();
                    await UniTask.Delay(100);
                    await UniTask.WaitUntil(() => GlobalSetting.GameStarted);
                    while (GlobalSetting.GameStarted)
                    {
                        if (!GlobalSetting.Paused) UploadScore();
                        await UniTask.Delay(1000);
                    }
                });
            }
        }

        private void OnAudioResolutionError()
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "DSPBuffer数值过小", () =>
            {
                Destroy(progressManager);
                Quit();
            }, "返回");
        }

        private async void RemoveUserFromRuntimeScore(string username)
        {
            await UniTask.SwitchToMainThread();
            _runtimeScores.Remove(username);
            RefreshRuntimeScoreUI();
        }

        private async void UpdateRuntimeScore((string, string) data)
        {
            await UniTask.SwitchToMainThread();
            _runtimeScores[data.Item1] = int.Parse(data.Item2);
            RefreshRuntimeScoreUI();
        }

        private int _maximumRankCount = 3;

        private void RefreshRuntimeScoreUI()
        {
            // runtimeScores.OrderBy(pair => pair.Value)
            KeyValuePair<string,int>[] pairs = _runtimeScores.OrderByDescending(pair => pair.Value).ToArray();
            first.text = pairs[0].Value.ToString().PadLeft(7, '0') + "  " + pairs[0].Key;
            if (_maximumRankCount < 2) return;
            if (pairs.Length < 2)
            {
                _maximumRankCount = 1;
                second.gameObject.SetActive(false);
                third.gameObject.SetActive(false);
                return;
            }
            second.text = pairs[1].Value.ToString().PadLeft(7, '0') + "  " + pairs[1].Key;
            if (_maximumRankCount < 3) return;
            if (pairs.Length < 3)
            {
                _maximumRankCount = 2;
                third.gameObject.SetActive(false);
                return;
            }
            third.text = pairs[2].Value.ToString().PadLeft(7, '0') + "  " + pairs[2].Key;
        }
        
        private async void UploadScore()
        {
            await UniTask.SwitchToMainThread();
            float score = GlobalSetting.ScoreCounter.Score;
            await UniTask.SwitchToThreadPool();
            SocketManager.UploadScoreForSync(score);
        }
        
        private async void DisconnectListener()
        {
            await UniTask.SwitchToMainThread();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < 15000)
            {
                if (!GlobalSetting.Paused) break;
                disconnectWarn.text = $"警告：将在 {(15000 - stopwatch.ElapsedMilliseconds) / 1000f:0.0} 秒后断开连接";
                await UniTask.Yield();
            }
            disconnectWarn.text = "警告：将在 0 秒后断开连接";
            if (!GlobalSetting.Paused) return;
            Quit();
        }
        
#endregion

        protected override void OnAwake()
        {
            //Init GlobalSetting
            GlobalSetting.LineColors.Clear();
            GlobalSetting.LineColors.Add(JudgeLineStat.AP, GlobalSetting.CurrentSkinInfo.perfectColor);
            GlobalSetting.LineColors.Add(JudgeLineStat.FC, GlobalSetting.CurrentSkinInfo.goodColor);
            GlobalSetting.LineColors.Add(JudgeLineStat.None, new Color(1, 1, 1, 1));
            GlobalSetting.GameStarted = false;
            GlobalSetting.MusicLength = GlobalSetting.CurrentBeatmapInfo.Music.length;
            GlobalSetting.FormatVersion = ChartLoader.Chart.formatVersion;
            GlobalSetting.ScoreCounter.NumOfNotes = ChartLoader.Chart.numOfNotes;
            GlobalSetting.RestartCount++;

            //Init progress controller
            progressManager.Init(OnAudioResolutionError, OnAudioResolutionError);
            
            //Init judgement
            managers.AddComponent<JudgementManager>();
            
            //Init audio
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.clip = GlobalSetting.CurrentBeatmapInfo.Music;
            _audioSource.pitch = GlobalSetting.Pitch;
            
            //Init play
            PrepareMultipleMode();
            InitChartPlay();
        }

        private void Start()
        {
            SetupUI();
            StartCoroutine(StartPlay());
        }

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (Screen.width * 1f / Screen.height >= Standard916Aspect)
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.height * Standard916Aspect;
                maskSprite.transform.localScale = new Vector3(1782, 8000, 0);
            }
            else
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.width;
                maskSprite.transform.localScale = new Vector3(8000, 8000, 0);
            }
#endif
            
            //Progress control
            if (GlobalSetting.GameStarted)
            {
                progressManager.OnUpdate();
            }
            
            //Check ending
            if (progressManager.NowTime >= _audioSource.clip.length && GlobalSetting.GameStarted)
            {
                progressManager.StopTiming();
                GlobalSetting.GameStarted = false;
                if (!GlobalSetting.IsEnding)
                {
                    LoadEnding();
                }
            }

            UpdateUI();

#if UNITY_EDITOR
            if (Camera.main.aspect >= Standard916Aspect)
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.height * Standard916Aspect;
                maskSprite.transform.localScale = new Vector3(1782, 8000, 0);
            }
            else
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.width;
                maskSprite.transform.localScale = new Vector3(8000, 8000, 0);
            }
            
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                _audioSource.time += 5f * GlobalSetting.Pitch;
                progressManager.AddTime(5f);
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                _audioSource.time += 1f * GlobalSetting.Pitch;
                progressManager.AddTime(1f);
            }
#endif
            
        }

        private void SetupUI()
        {
            uiCanvasGroup.alpha = 0;
            uiCanvasGroup.DOFade(1.0f, 1f);
            maskSprite.DOFade(GlobalSetting.MaskAlpha, 1f);
            
            illustration.texture = GlobalSetting.CurrentBeatmapInfo.Illustration;
            if (GlobalSetting.DisableBlur)
            {
                GameObject.Find("BackgroundCamera").GetComponent<TranslucentImageSource>().enabled = false;
            }
            if (Screen.width * 1f / Screen.height >= Standard916Aspect)
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.height * Standard916Aspect;
                maskSprite.transform.localScale = new Vector3(1782, 8000, 0);
            }
            else
            {
                GlobalSetting.ScreenHeight = Screen.height;
                GlobalSetting.ScreenWidth = Screen.width;
                maskSprite.transform.localScale = new Vector3(8000, 8000, 0);
            }
            GameObject.Find("SongNameLeftBottom").GetComponent<Text>().text = "   " + GlobalSetting.CurrentBeatmapInfo.SongName;
            GameObject.Find("DiffText").GetComponent<Text>().text = GlobalSetting.CurrentBeatmapInfo.SongLevel + "  ";
            
            Camera.main.orthographic = true;

            accText.gameObject.SetActive(GlobalSetting.DisplayAcc);
            
            multiplayerRank.gameObject.SetActive(GlobalSetting.IsMultiplayer);

            disconnectWarn.gameObject.SetActive(false);
            if (GlobalSetting.IsMultiplayer)
            {
                pauseButton.gameObject.SetActive(false);
                Destroy(retryButton.gameObject);
                Destroy(terminateButton.gameObject);
            }

            RegisterPauseMenu();
        }

        private void UpdateUI()
        {
            comboText.text = GlobalSetting.ScoreCounter.Combo < 3 ? "" : $"{GlobalSetting.ScoreCounter.Combo}";
            if (GlobalSetting.ScoreCounter.Combo < 3)
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
        
        private void RegisterPauseMenu()
        {
            backButton.OnClick.AddListener(Quit);
            continueButton.OnClick.AddListener(() => UnPause().Forget());
            if (GlobalSetting.IsMultiplayer) return;
            pauseButton.OnDoubleTap.AddListener(Pause);
            retryButton.OnClick.AddListener(() =>
            {
                GlobalSetting.Reset();
                SceneTransit.Instance.JumpScene("PlayingScene");
            });
            terminateButton.OnClick.AddListener(() =>
            {
                _audioSource.time = 0;
                progressManager.AddTime(_audioSource.clip.length * 2);
            });
        }

        private IEnumerator StartPlay()
        {
            // We pre-generate one HitFX to avoid the high Disk usage of reading the prefab.
            // 预生成HitFX，避免读取prefab时吃硬盘
            var hitFX = HitEffectManager.GetInstance()
                .GetObj(HitFxJudgeType.Perfect, GlobalSetting.CurrentSkinInfo);
            hitFX.transform.localPosition = new Vector3(5000, 5000, 0);
            hitFX.PlayEffect();
            hitFX = HitEffectManager.GetInstance()
                .GetObj(HitFxJudgeType.Good, GlobalSetting.CurrentSkinInfo);
            hitFX.transform.localPosition = new Vector3(5000, 5000, 0);
            hitFX.PlayEffect();

            _totalOffset = ChartLoader.Chart.offset + GlobalSetting.UserOffset;
            
            progressManager.AddStartDelay(_totalOffset);
            //totalOffset -= .05f; //fixed delay
            yield return new WaitForSeconds(1);
            GlobalSetting.IsMirror = false;
            _audioSource.PlayScheduled(AudioSettings.dspTime);
            GlobalSetting.GameStarted = true;
            progressManager.StartTiming();
        }

        private async void LoadEnding()
        {
            uiCanvasGroup.DOFade(0f, 1f);
            maskSprite.DOFade(0f, 1f);
            GlobalSetting.IsEnding = true;
            _endingSceneLoadOperation = SceneManager.LoadSceneAsync("LevelOver 1");
            _endingSceneLoadOperation.allowSceneActivation = false;
            await UniTask.Delay(1000);
            _endingSceneLoadOperation.allowSceneActivation = true;
            await _endingSceneLoadOperation;
        }
        
        private void InitChartPlay()
        {
            var i = 0;
            foreach (var l in ChartLoader.Chart.judgeLineList)
            {
                var t = Instantiate(line, instantiateTransform);
                var jlm = t.GetComponentInChildren<JudgeLineMovement>();
                jlm.ID = i;
                jlm.Line = l;
                GlobalSetting.Lines.Add(jlm);
                i++;
            }

            foreach (var l in ChartLoader.Chart.judgeLineList)
            {
                foreach (var n in l.notesAbove)
                {
                    if (!GlobalSetting.HighLightedNotes.TryAdd(n.time, 1))
                        GlobalSetting.HighLightedNotes[n.time]++;

                    n.isAbove = true;
                }

                foreach (var n in l.notesBelow)
                {
                    if (!GlobalSetting.HighLightedNotes.TryAdd(n.time, 1))
                        GlobalSetting.HighLightedNotes[n.time]++;

                    n.isAbove = false;
                }
            }

            foreach (var l in ChartLoader.Chart.judgeLineList)
            {
                foreach (var n in l.notesAbove.Where(n => GlobalSetting.HighLightedNotes[n.time] > 1 && GlobalSetting.HighLight))
                {
                    n.isMulti = true;
                }

                foreach (var n in l.notesBelow.Where(n => GlobalSetting.HighLightedNotes[n.time] > 1 && GlobalSetting.HighLight))
                {
                    n.isMulti = true;
                }
            }
            
            if (GlobalSetting.CurrentBeatmapInfo.ExtraEvents != null)
            {
                if (GlobalSetting.CurrentBeatmapInfo.ExtraEvents.Effects != null)
                {
                    Camera.main.gameObject.AddComponent<ExtraShaderProvider>().IsGlobal = false;
                    uiCamera.gameObject.AddComponent<ExtraShaderProvider>().IsGlobal = true;
                    particleCamera.gameObject.AddComponent<ExtraShaderProvider>().IsGlobal = false;
                }
            }

            GlobalSetting.HighLightedNotes.Clear();
            GlobalSetting.MaximumZOrder = ChartLoader.Chart.judgeLineList.Count;
            
            if (GlobalSetting.CurrentBeatmapInfo.LineImage != null)
            {
                ChartLoader.LoadCsvLineImage();
            }
        }
        
        private static void Quit()
        {
            if (GlobalSetting.IsMultiplayer) SocketManager.QuitGame();
            SceneTransit.Instance.Back();
        }

        private void Pause()
        {
            if (!GlobalSetting.GameStarted || GlobalSetting.Paused) return;
            GlobalSetting.Paused = true;
            progressManager.StopTiming();
            _audioSource.Pause();
            _audioSource.volume = 0;
            float delta = Mathf.Min(3f, _audioSource.time);
            _audioSource.time = Mathf.Max(_audioSource.time - 3f, 0f);
            progressManager.TimeGoBack(delta, () => pauseWindow.TurnOn());
            if (GlobalSetting.IsMultiplayer) DisconnectListener();
        }

        private async UniTaskVoid UnPause()
        {
            if (GlobalSetting.GameStarted && GlobalSetting.Paused)
            {
                pauseWindow.TurnOff();
                // audio.time = Stopwatch.ElapsedMilliseconds * .001f;
                progressManager.ContinueTiming();
                _audioSource.UnPause();
                DOTween.To(() => _audioSource.volume, (x) => _audioSource.volume = x, 1f, 2f);
                await Task.Delay(3000);
                GlobalSetting.Paused = false;
            }
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
    }
}