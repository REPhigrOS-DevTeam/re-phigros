using System.Collections.Generic;
using MainCore.Utilities;
using UnityEngine;

namespace MainCore
{
    public static class GlobalSetting
    {
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
        public static bool autoPlay = false;
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
        public static string extraJson = "";
        public static CSVReader lineImage = null;
        public static InfoTxtReader infoTxt = null;
        public static bool usingApi = false;
        public static bool isMirror = false;
        public static bool is3D = false;
        public static bool disableBlur = false;
        public static bool postProcessing = false;
        public static bool recordMode = false;
        public static bool fxaaEnabled = false;
        public static float hitVolume = 1f;
        public static float maskAlpha = .5f;
        public static int maximumZOrder = 0;
        public static Sprite illustration;
        public static Resolution OriginResolution;
        public static string charter;
        public static string composer;
        public static string illustrator;
        public static bool multiplayer;

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
        
        public static HitFxType HitFxType = HitFxType.StarPinkXz;

        public static bool IsPhira = false;

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
            infoTxt = null;
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
            IsPhira = false;
            composer = "Unknown";
            charter = "Unknown";
            illustrator = "Unknown";
        }
    }
}