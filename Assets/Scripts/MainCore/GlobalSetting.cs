using System;
using System.Collections.Generic;
using MainCore.Common;
using MainCore.Data;
using MainCore.Serialized;
using MainCore.Utilities;
using UnityEngine;

namespace MainCore
{
    public class JudgeTime
    {
        public float bTime = 0f; // time to bad
        public float gTime = 0f; // time to good
        public float judgeTime = 0f; // time to miss
        public float pTime = 0f;

        public JudgeTime()
        {
        }

        public JudgeTime(float bTime, float gTime, float judgeTime, float pTime)
        {
            this.bTime = bTime;
            this.gTime = gTime;
            this.judgeTime = judgeTime;
            this.pTime = pTime;
        }

        public static JudgeTime operator *(JudgeTime a, float b) => new(bTime: a.bTime * b, gTime: a.gTime * b,
            judgeTime: a.judgeTime * b, pTime: a.pTime * b);

        public static JudgeTime operator /(JudgeTime a, float b) => new(bTime: a.bTime / b, gTime: a.gTime / b,
            judgeTime: a.judgeTime / b, pTime: a.pTime / b);
    }

    public static class GlobalSetting
    {
        public static int UnityThreadId;

        public enum YayaMode
        {
            冲,
            结,
            绝冲
        }

        public enum PepoyoMode
        {
            Waraninja,
            Poyoroid_sou,
            Poyoroid_utsu,
            Yande
        }

        //public static string ChartPath = "E:\\DESKTOP\\pumian\\Apollo\\cachedJson.json";
        //public static string ChartFolderPath = "";
        //public static string ChartName = "Apollo";
        //public static string MusicPath = "E:\\DESKTOP\\pumian\\Apollo\\Apollos.wav";
        //public static string IllustrationPath { get; set; } = "E:\\DESKTOP\\pumian\\Apollo\\Apollo.png";
        public static BeatmapInfo CurrentBeatmapInfo { get; set; } = new ();
        public static int FormatVersion = 3;
        public static Dictionary<float, int> HighLightedNotes = new Dictionary<float, int>();
        public static float GlobalNoteScale = 0.25f;
        public static bool HighLight;
        public static bool AutoPlay;
        public static ScoreCounter ScoreCounter = new ScoreCounter();
        public static float NoteSpeedFactor = 1f;
        public static float UserOffset;
        //public static string Difficulty = "Diff";
        public static JudgeLineStat LineStat = JudgeLineStat.AP;
        public static Dictionary<JudgeLineStat, Color> LineColors = new Dictionary<JudgeLineStat, Color>();
        public static float ScreenHeight;
        public static float ScreenWidth;
        //public static string Chart = "";
        //public static CSVReader LineImage;
        public static bool IsMirror;
        public static bool Is3D;
        public static bool DisableBlur;
        public static bool PostProcessing;
        public static bool FxaaEnabled;
        public static float HitVolume = 1f;
        public static float MaskAlpha = .5f;
        public static int MaximumZOrder = 0;
        // public static Sprite illustration;
        public static Resolution OriginResolution;
        //public static string Charter;
        //public static string Composer;
        //public static string Illustrator;
        public static bool IsMultiplayer;
        //public static InfoType InfoType = InfoType.Empty;
        public static string Username;
        public static string VerifyToken;
        public static bool IsOffline => string.IsNullOrEmpty(Username);

        public static YayaMode YayaKawaii = YayaMode.冲;
        public static PepoyoMode PepoyoDaisuki = PepoyoMode.Waraninja;


        // public static bool oldTexture = false;
        //public static Sprite BackgroundImage = null;

        public static readonly List<JudgeLineMovement> Lines = new List<JudgeLineMovement>();

        public static bool GameStarted { get; set; }
        public static bool IsEnding { get; set; }
        public static bool Paused { get; set; }

        public static float Aspect => ScreenWidth / ScreenHeight;

        public static float MusicLength { get; set; }

        private static string _externalSkinName;

        public static SkinInfo CurrentSkinInfo;

        public static bool StrictJudgeMode = false;

        public static bool NewScoreCalcType = false;

        public static bool DisplayAcc = false;

        public static string[] PlayerList;

        public static void PlayNoteSound(int notetype)
        {
            /*if (hitVolume < .01f) return;
            PlayClipAtPoint(tapSounds[notetype], new Vector3(0, 0, -10), hitVolume);*/
            HitSoundManager.Instance.Play(notetype);
        }

        public static void Reset()
        {
            GameStarted = false;
            IsEnding = false;
            Paused = false;
            YayaKawaii = YayaMode.冲;
            PepoyoDaisuki = PepoyoMode.Waraninja;
            HighLightedNotes.Clear();
            ScoreCounter = new ScoreCounter();
            NoteSpeedFactor = 1f;
            Lines.Clear();
            LineColors.Clear();
            LineStat = JudgeLineStat.AP;
            HitEffectManager.GetInstance().Reset();
            NotePool.GetInstance().Reset();
            PlayerList = null;
        }

        public static void SetBeatmap(BeatmapInfo info)
        {
            CurrentBeatmapInfo = info;
        }

        public static void ReadUserSettings()
        {
            HighLight = PlayerPrefsExtension.GetBoolean("high_light", false); //highlightToggle.isOn;
            UserOffset =
                PlayerPrefs.GetFloat("chart_offset", 0) /
                1000f; //int.Parse(GameObject.Find("DelayInput").GetComponent<InputField>().text) / 1000f;
            AutoPlay = PlayerPrefsExtension.GetBoolean("auto_play",
                false); //GameObject.Find("AutoToggle").GetComponent<Toggle>().isOn;
            IsMirror = PlayerPrefsExtension.GetBoolean("mirror",
                false); //GameObject.Find("MirrorToggle").GetComponent<Toggle>().isOn;
            DisableBlur = PlayerPrefsExtension.GetBoolean("blur", false);
            Is3D = false; //PlayerPrefs.GetInt("3d", 0) == 1;//GameObject.Find("3DToggle").GetComponent<Toggle>().isOn;
            PostProcessing =
                PlayerPrefsExtension.GetBoolean("post_processing",
                    false); //GameObject.Find("PostProcessingToggle").GetComponent<Toggle>().isOn;
            GlobalNoteScale = PlayerPrefs.GetFloat("note_size", 0.25f) * GameUtils.ScreenDelta;
            if (PlayerPrefs.HasKey("record_mode"))
            {
                PlayerPrefs.DeleteKey("record_mode");
                PlayerPrefs.Save();
            }
            HitVolume = PlayerPrefs.GetFloat("hit_volume", 1f);
            MaskAlpha = PlayerPrefs.GetFloat("mask_alpha", .5f);
            FxaaEnabled = PlayerPrefsExtension.GetBoolean("fxaa", false);
            if (PlayerPrefs.HasKey("skin")) // 给前人擦屁股.jpg
            {
                int skin = PlayerPrefs.GetInt("skin", 0);
                CurrentSkinInfo = HitEffectManager.GetInstance().GetInternalSkinInfo((Skin)skin);
                PlayerPrefs.DeleteKey("skin");
                PlayerPrefs.SetString("selected_skin", $"i{skin}");
                PlayerPrefs.Save();
            }
            else
            {
                string s = PlayerPrefs.GetString("selected_skin", "i0");
                CurrentSkinInfo = HitEffectManager.GetInstance().GetSkinInfo(s[0] switch // internal external
                {
                    'i' => false,
                    'e' => true,
                    _ => throw new ArgumentException()
                }, s[1..]);
                if (CurrentSkinInfo == null)
                {
                    if (s[0] != 'e') throw new ArgumentException();
                    PlayerPrefs.SetString("selected_skin", "i0");
                    CurrentSkinInfo = HitEffectManager.GetInstance().GetSkinInfo(false, "0");
                    PlayerPrefs.Save();
                }
            }
            HitSoundManager.Instance.RefreshHitSounds();
            HitSoundManager.UpdateVolume();
            if (PlayerPrefs.HasKey("use_course_mode")) // 给前人擦屁股.jpg
            {
                PlayerPrefsExtension.SetBoolean("strict_judge", PlayerPrefsExtension.GetBoolean("use_course_mode"));
                PlayerPrefs.DeleteKey("use_course_mode");
                PlayerPrefs.Save();
            }
            StrictJudgeMode = PlayerPrefsExtension.GetBoolean("strict_judge", false);
            NewScoreCalcType = PlayerPrefsExtension.GetBoolean("score_v2", false);
            DisplayAcc = PlayerPrefsExtension.GetBoolean("display_acc", false);
            _judgeTime = null;
            Pitch = PlayerPrefs.GetFloat("music_speed", 1f);
        }

        public static float Pitch = 1.2f;

        private static readonly JudgeTime easyTime = new JudgeTime
        {
            bTime = 0.16f,
            gTime = 0.08f,
            judgeTime = 0.2f,
        };

        private static readonly JudgeTime hardTime = new JudgeTime
        {
            bTime = 0.08f,
            gTime = 0.04f,
            judgeTime = 0.16f,
        };

        private static JudgeTime _judgeTime;

        public static JudgeTime GetJudgeTime() => _judgeTime ??= (StrictJudgeMode ? hardTime : easyTime) * Pitch;
    }

    public enum InfoType
    {
        Empty = 0,
        InfoTxt,
        RpeJson,
        InfoCsvOld,
        InfoCsv,
        InfoYml,
        Internal // 保持这个为最高
    }
}