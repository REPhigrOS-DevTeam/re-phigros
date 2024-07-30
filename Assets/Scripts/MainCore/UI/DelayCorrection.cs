using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Lean.Gui;
using MainCore.Common;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace MainCore.UI
{
    public class DelayCorrection : MonoBehaviour
    {
        [SerializeField] private Button playMusic;
        [SerializeField] private LeanButton touch;
        [SerializeField] private Text msText;
        
        [SerializeField] private LeanButton saveExit;
        [SerializeField] private AudioSource source;

        private bool _isStarted = false;
        private float _totalDelay = 0;
        private const float Duration = .96f * 2f;
        private int _count = 0;
        private readonly Stopwatch _stopwatch = new();

        private float Delay => _totalDelay / _count;
        
        // Start is called before the first frame update
        void Start()
        {
            
            playMusic.onClick.AddListener(delegate
            {
                _stopwatch.Start();
                playMusic.interactable = false;
                playMusic.GetComponentInChildren<Text>().text = "重新测试";
                source.PlayScheduled(AudioSettings.dspTime);
                _isStarted = true;
                _totalDelay = 0;
                _count = 0;
                WaitMusicTime();
            });
            saveExit.OnClick.AddListener(delegate
            {
                source.Stop();
                PlayerPrefs.SetFloat("chart_offset", Convert.ToInt32(Delay * 1000));
                PlayerPrefs.Save();
                SceneTransit.Instance.Back();
            });
            touch.OnDown.AddListener(delegate
            {
                if (!_isStarted) return;
                _count++;
                var sec = _stopwatch.ElapsedMilliseconds / 1000f;
                Debug.Log(sec);
                _totalDelay += sec - Duration * (_count - 1) - .96f/*？？？*/;
                msText.text = $"{(int)(Delay * 1000)}ms";
            });
        }

        private async UniTask WaitMusicTime()
        {
            await UniTask.Delay(8000);
            Reset();
        }

        private void Reset()
        {
            _isStarted = false;
            playMusic.interactable = true;
            _stopwatch.Reset();
        }
    }
}
