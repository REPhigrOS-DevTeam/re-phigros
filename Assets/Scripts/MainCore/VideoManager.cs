using System.Collections.Generic;
using System.IO;
using System.Linq;
using MainCore.Data;
using MainCore.Utilities;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

namespace MainCore
{
    public class VideoManager : MonoBehaviour
     {
         private MediaPlayer mediaPlayer;
         private DisplayUGUI mediaDisplayer;
         private bool isInited;
         private static readonly Color InvisibleColor = new(0f, 0f, 0f, 0f);
         private int counter = 0;
         private Video[] videos;
         private Color[] targetColors;
         private double[] durations;
         public void Init(Video[] videos)
         {
             if (isInited) return;
             isInited = true;
             List<Video> list = videos.ToList();
             list.Sort((v1, v2) => v1.time.Frac() < v2.time.Frac() ? -1 : 1);
             this.videos = list.ToArray();
             if (videos.Any(video => !File.Exists(Path.Combine(GlobalSetting.chartFolderPath, video.path))))
             {
                 Destroy(gameObject);
                 return;
             }
             mediaPlayer = Camera.main.gameObject.GetComponent<MediaPlayer>();
             mediaPlayer.PlaybackRate = GlobalSetting.Pitch;
             mediaDisplayer = gameObject.GetComponent<DisplayUGUI>();
             mediaDisplayer.color = InvisibleColor;
             targetColors = new Color[this.videos.Length];
             for (int i = 0; i < targetColors.Length; i++)
             {
                 float grayscale = 1 - this.videos[i].dim;
                 targetColors[i] = new Color(grayscale, grayscale, grayscale, this.videos[i].alpha);
             }
             durations = new double[videos.Length];
             MediaPlayer addComponent = new GameObject().AddComponent<MediaPlayer>();
             addComponent.AutoOpen = false;
             addComponent.AutoStart = false;
             addComponent.AudioMuted = true;
             for (var i = 0; i < this.videos.Length; i++)
             {
                 var video = this.videos[i];
                 addComponent.OpenMedia(MediaPathType.AbsolutePathOrURL,
                     Path.Combine(GlobalSetting.chartFolderPath, video.path), false);
                 durations[i] = addComponent.Info.GetDuration();
             }
             Destroy(addComponent.gameObject);

             WaitForPlay();
         }

         public void Pause()
         {
             if (mediaPlayer.Control.IsFinished() || !mediaPlayer.Control.IsPlaying()) return;
             mediaPlayer.Pause();
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
         
         private async void WaitForPlay()
         {
             bool loaded = false;
             mediaPlayer.Events.AddListener((a, b, c) =>
             {
                 if (b == MediaPlayerEvent.EventType.ReadyToPlay) loaded = true;
             });
             for (int i = 0; i < videos.Length; i++)
             {
                 var i1 = i;
                 if (i > 0) await new WaitWhile(() => videos[i1 - 1].realTime + durations[i1 - 1] > Main.Instance.progressManager.NowTime);
                 mediaDisplayer.color = InvisibleColor;
                 mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, Path.Combine(GlobalSetting.chartFolderPath, videos[0].path), false);
                 mediaPlayer.Control.Seek(0);
                 mediaPlayer.Pause();
                 await new WaitWhile(() => !loaded || videos[i1].realTime < Main.Instance.progressManager.NowTime || GlobalSetting.Paused);
                 mediaDisplayer.color = targetColors[i];
                 mediaDisplayer.ScaleMode = videos[i].ScaleMode;
                 mediaPlayer.Play();
             }
             mediaDisplayer.color = InvisibleColor;
         }

         public void Resume()
         {
             if (mediaPlayer.Control.IsPlaying()) return;
             mediaPlayer.Play();
         }

         public void OnDestroy()
         {
             Destroy(Camera.main.GetComponent<MediaPlayer>());
         }
     }
}