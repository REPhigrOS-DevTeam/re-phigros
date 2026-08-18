using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MainCore.Data;
using MainCore.Utilities;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

namespace MainCore
{
    public class VideoManager : MonoBehaviour
    {
        [SerializeField] private GameObject videoRoot;
        private MediaPlayer[] mediaPlayers;
        private DisplayUGUI mediaDisplayer;
        private bool isInited;
        private int counter = 0;
        private Video[] videos;
        private Color[] targetColors;
        private double[] durations;
        private Camera camera;

        public void Init()
        {
            if (isInited) return;
            isInited = true;
            camera = Camera.main;
            if (!camera)
            {
                Destroy(videoRoot);
                return;
            }

            List<Video> list = GlobalSetting.CurrentBeatmapInfo.ExtraEvents.Videos;
            if (list.Count < 1)
            {
                Destroy(videoRoot);
                return;
            }

            list.Sort((v1, v2) => v1.time.Frac() < v2.time.Frac() ? -1 : 1);
            videos = list.ToArray();
            if (list.Any(video => !File.Exists(Path.Combine(GlobalSetting.CurrentBeatmapInfo.BasePath, video.path))))
            {
                Destroy(videoRoot);
                return;
            }

            mediaDisplayer = videoRoot.GetComponent<DisplayUGUI>();
            mediaDisplayer.color = Color.clear;
            mediaDisplayer.CurrentMediaPlayer = null;
            targetColors = new Color[videos.Length];
            for (int i = 0; i < targetColors.Length; i++)
            {
                float grayscale = Mathf.Clamp01(1 - videos[i].dim);
                targetColors[i] = new Color(grayscale, grayscale, grayscale, videos[i].alpha);
                Debug.Log(targetColors[i]);
            }

            durations = new double[videos.Length];
            mediaPlayers = new MediaPlayer[videos.Length];
            for (var i = 0; i < videos.Length; i++)
            {
                var video = videos[i];
                var mediaPlayer = mediaPlayers[i] = camera.gameObject.AddComponent<MediaPlayer>();
                mediaPlayer.PlaybackRate = GlobalSetting.Pitch;
                mediaPlayer.AutoOpen = false;
                mediaPlayer.AutoStart = false;
                mediaPlayer.AudioMuted = true;
                mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL,
                    Path.Combine(GlobalSetting.CurrentBeatmapInfo.BasePath, video.path), false);
                durations[i] = mediaPlayer.Info.GetDuration();
            }

            WaitForPlay();
        }

        public void Pause()
        {
            if (inVideo) mediaPlayers[videoIndex].Pause();
            // StopCoroutine(waitForEnd);
            // if (startTime + duration < time) return;
            // if (mediaPlayer.Control.GetCurrentTime() < time)
            // {
            //     mediaPlayer.Stop();
            //     mediaPlayer.Control.Seek(0);
            //     mediaDisplayer.color = InvisibleColor;
            //     WaitForPlay();
            // }
            // else
            // {
            //     double originTime = mediaPlayer.Control.GetCurrentTime();
            //     DOTween.To(() => mediaPlayer.Control.GetCurrentTime() - originTime, x => mediaPlayer.Control.Seek(originTime + x), time, .5f);
            // }
        }

        private int videoIndex;
        private bool inVideo = false;

        private async void WaitForPlay()
        {
            for (videoIndex = 0; videoIndex < videos.Length; videoIndex++)
            {
                if (videoIndex > 0)
                    await UniTask.WaitWhile(() =>
                        videos[videoIndex - 1].realTime + durations[videoIndex - 1] >
                        Main.Instance.progressManager.NowTime);
                inVideo = false;
                mediaDisplayer.color = Color.clear;
                await UniTask.WaitWhile(() =>
                    videos[videoIndex].realTime < Main.Instance.progressManager.NowTime || GlobalSetting.Paused);
                inVideo = true;
                mediaDisplayer.color = targetColors[videoIndex];
                mediaDisplayer.ScaleMode = videos[videoIndex].ScaleMode;
                var mediaPlayer = mediaPlayers[videoIndex];
                mediaDisplayer.CurrentMediaPlayer = mediaPlayer;
                mediaPlayer.Stop();
                mediaPlayer.Control.Seek(0);
                mediaPlayer.Play();
            }

            mediaDisplayer.color = Color.clear;
        }

        public void Resume()
        {
            if (inVideo) mediaPlayers[videoIndex].Play();
        }

        public void OnDestroy()
        {
            if (camera) Destroy(camera.GetComponent<MediaPlayer>());
        }
    }
}