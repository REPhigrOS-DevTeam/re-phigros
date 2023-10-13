using System;
using System.Collections;
using System.IO;
using DG.Tweening;
using MainCore.Data;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

namespace MainCore
{
    public class VideoItem : MonoBehaviour
    {
        private MediaPlayer mediaPlayer;
        private DisplayUGUI mediaDisplayer;
        private bool isInited;
        private static readonly Color InvisibleColor = new(0f, 0f, 0f, 0f);
        private Color targetColor;
        private float startTime, duration;
        public void Init(Video video)
        {
            if (isInited) return;
            isInited = true;
            if (!File.Exists(Path.Combine(GlobalSetting.chartFolderPath, video.path)))
            {
                Destroy(gameObject);
                return;
            }
            mediaPlayer = Camera.main.gameObject.AddComponent<MediaPlayer>();
            mediaPlayer.Loop = false;
            mediaPlayer.AutoOpen = false;
            mediaPlayer.AutoStart = false;
            mediaPlayer.AudioMuted = true;
            mediaPlayer.PlaybackRate = GlobalSetting.Pitch;
            gameObject.AddComponent<CanvasRenderer>();
            mediaDisplayer = gameObject.AddComponent<DisplayUGUI>();
            mediaDisplayer.CurrentMediaPlayer = mediaPlayer;
            float grayscale = 1 - video.dim;
            targetColor = new Color(grayscale, grayscale, grayscale, video.alpha);
            mediaDisplayer.color = InvisibleColor;
            mediaDisplayer.ScaleMode = video.scale switch
            {
                "cropCenter" => ScaleMode.ScaleAndCrop,
                "inside" => ScaleMode.ScaleToFit,
                "fit" => ScaleMode.StretchToFill,
                _ => throw new ArgumentOutOfRangeException()
            };
            mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, Path.Combine(GlobalSetting.chartFolderPath, video.path), false);
            mediaPlayer.Stop();
            startTime = video.realTime;
            duration = (float) mediaPlayer.Info.GetDuration();
            WaitForPlay();
        }

        public void Pause(float time)
        {
            if (mediaPlayer.Control.IsPaused() || isWaiting) return;
            mediaPlayer.Pause();
            StopCoroutine(waitForEnd);
            if (startTime + duration < time) return;
            if (mediaPlayer.Control.GetCurrentTime() < time)
            {
                mediaPlayer.Stop();
                mediaPlayer.Control.Seek(0);
                mediaDisplayer.color = InvisibleColor;
                WaitForPlay();
            }
            else
            {
                double originTime = mediaPlayer.Control.GetCurrentTime();
                DOTween.To(() => mediaPlayer.Control.GetCurrentTime() - originTime, x => mediaPlayer.Control.Seek(originTime + x), time, .5f);
            }
        }

        private Coroutine waitForEnd;

        private bool isWaiting = false;

        private async void WaitForPlay()
        {
            isWaiting = true;
            await new WaitWhile(() => startTime < Main.Instance.progressManager.NowTime || GlobalSetting.Paused);
            mediaDisplayer.color = targetColor;
            mediaPlayer.Play();
            isWaiting = false;
            waitForEnd = StartCoroutine(WaitForEnd());
        }

        private IEnumerator WaitForEnd()
        {
            yield return new WaitUntil(mediaPlayer.Control.IsFinished);
            mediaDisplayer.color = InvisibleColor;
            mediaPlayer.Stop();
        }

        public void Resume()
        {
            if (!mediaPlayer.Control.IsPaused() || isWaiting) return;
            WaitForPlay();
        }
    }
}