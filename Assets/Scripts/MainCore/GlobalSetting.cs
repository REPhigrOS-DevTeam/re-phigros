using System.Collections.Generic;
using MainCore.Common;
using MainCore.Data;
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

        public static string chartPath = "E:\\DESKTOP\\pumian\\Apollo\\cachedJson.json";
        public static string chartFolderPath = "";
        public static string chartName = "Apollo";
        public static string musicPath = "E:\\DESKTOP\\pumian\\Apollo\\Apollos.wav";
        public static string illustrationPath = "E:\\DESKTOP\\pumian\\Apollo\\Apollo.png";
        public static int formatVersion = 3;
        public static Dictionary<float, int> highLightedNotes = new Dictionary<float, int>();
        public static float globalNoteScale = 0.25f;
        public static bool highLight;
        public static bool autoPlay;
        public static ScoreCounter scoreCounter = new ScoreCounter();
        public static float noteSpeedFactor = 1f;
        public static float userOffset;
        public static string difficulty = "Diff";
        public static JudgeLineStat lineStat = JudgeLineStat.AP;
        public static Dictionary<JudgeLineStat, Color> lineColors = new Dictionary<JudgeLineStat, Color>();
        public static Dictionary<int, AudioClip> tapSounds = new Dictionary<int, AudioClip>();
        public static float screenHeight;
        public static float screenWidth;
        public static float widthOffset = 0f;
        public static string chart = "";
        public static Extra extraEvents = null;
        public static CSVReader lineImage;
        public static bool usingApi = false;
        public static bool isMirror;
        public static bool is3D;
        public static bool disableBlur;
        public static bool postProcessing;
        public static bool recordMode;
        public static bool fxaaEnabled;
        public static float hitVolume = 1f;
        public static float maskAlpha = .5f;
        public static int maximumZOrder = 0;
        public static Sprite illustration;
        public static Resolution OriginResolution;
        public static string charter;
        public static string composer;
        public static string illustrator;
        public static bool isMultiplayer;
        public static InfoType infoType = InfoType.Empty;
        public static string username;
        public static string verifyToken;

        public static YayaMode YayaKawaii = YayaMode.冲;
        public static PepoyoMode PepoyoDaisuki = PepoyoMode.Waraninja;


        public static bool oldTexture = false;
        public static Sprite backgroundImage = null;

        public static List<JudgeLineMovement> lines = new List<JudgeLineMovement>();

        private static float orthographicSize = -1f;
        public static bool Playing { get; set; }
        public static bool IsEnding { get; set; }
        public static bool Paused { get; set; }

        public static float aspect
        {
            get { return screenWidth / screenHeight; }
        }

        public static float MusicLength { get; set; }

        public static float OrthographicSize
        {
            get
            {
#if UNITY_EDITOR
                orthographicSize = Camera.main.orthographicSize;
#else
            if (orthographicSize < 0)
            {
                orthographicSize = Camera.main.orthographicSize;
            }
#endif

                return orthographicSize;
            }
        }

        public static bool IsExternalSkin = false;
        private static Skin skin = Skin.Official;
        private static string externalSkinName;

        public static string ExternalSkinName
        {
            get => externalSkinName;
            set
            {
                externalSkinName = value;
                if (IsExternalSkin); // TODO: 外置读取
            }
        }
        public static Skin Skin
        {
            get => skin;
            set
            {
                skin = value;
                if (!IsExternalSkin) CurrentSkinInfo = HitEffectManager.GetSkinInfo(skin);
            }
        }

        public static SkinInfo CurrentSkinInfo;

        public static bool useCourseMode = false;

        public static void PlayNoteSound(int notetype)
        {
            /*if (hitVolume < .01f) return;
            PlayClipAtPoint(tapSounds[notetype], new Vector3(0, 0, -10), hitVolume);*/
            HitSoundManager.Instance.Play(notetype);
        }

        public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume)
        {
            GameObject gameObject = new GameObject("One shot audio")
            {
                transform =
                {
                    position = position
                }
            };
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.spatialBlend = 1f;
            audioSource.volume = volume;
            audioSource.PlayScheduled(AudioSettings.dspTime);
            Object.Destroy(gameObject, clip.length * Time.timeScale);
        }

        public static void Reset()
        {
            Playing = false;
            IsEnding = false;
            Paused = false;
            YayaKawaii = YayaMode.冲;
            PepoyoDaisuki = PepoyoMode.Waraninja;
            highLightedNotes.Clear();
            scoreCounter = new ScoreCounter();
            noteSpeedFactor = 1f;
            lines.Clear();
            lineColors.Clear();
            tapSounds.Clear();
            lineStat = JudgeLineStat.AP;
            // ObjectPool.GetInstance().reset();
            HitEffectManager.GetInstance().Reset();
            NotePool.GetInstance().Reset();
            lineImage = null;
            composer = "Unknown";
            charter = "Unknown";
            illustrator = "Unknown";
            infoType = InfoType.Empty;
        }

        public static void ReadUserSettings()
        {
            highLight = PlayerPrefsExtension.GetBoolean("high_light", false); //highlightToggle.isOn;
            userOffset =
                PlayerPrefs.GetFloat("chart_offset", 0) /
                1000f; //int.Parse(GameObject.Find("DelayInput").GetComponent<InputField>().text) / 1000f;
            autoPlay = PlayerPrefsExtension.GetBoolean("auto_play",
                false); //GameObject.Find("AutoToggle").GetComponent<Toggle>().isOn;
            isMirror = PlayerPrefsExtension.GetBoolean("mirror",
                false); //GameObject.Find("MirrorToggle").GetComponent<Toggle>().isOn;
            disableBlur = PlayerPrefsExtension.GetBoolean("blur", false);
            is3D = false; //PlayerPrefs.GetInt("3d", 0) == 1;//GameObject.Find("3DToggle").GetComponent<Toggle>().isOn;
            postProcessing =
                PlayerPrefsExtension.GetBoolean("post_processing",
                    false); //GameObject.Find("PostProcessingToggle").GetComponent<Toggle>().isOn;
            globalNoteScale = PlayerPrefs.GetFloat("note_size", 0.25f) * GameUtils.ScreenDelta;
            recordMode = PlayerPrefsExtension.GetBoolean("record_mode", false);
            hitVolume = PlayerPrefs.GetFloat("hit_volume", 1f);
            maskAlpha = PlayerPrefs.GetFloat("mask_alpha", .5f);
            fxaaEnabled = PlayerPrefsExtension.GetBoolean("fxaa", false);
            Skin = (Skin)PlayerPrefs.GetInt("skin", 0);
            HitSoundManager.Instance.RefreshHitSounds(Skin);
            HitSoundManager.UpdateVolume();
            useCourseMode = PlayerPrefsExtension.GetBoolean("use_course_mode", false);
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

        public static JudgeTime GetJudgeTime() => _judgeTime ??= (useCourseMode ? hardTime : easyTime) * Pitch;
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