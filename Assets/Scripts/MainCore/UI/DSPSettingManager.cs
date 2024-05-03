using Lean.Gui;
using MainCore.Common;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI
{
    public class DSPSettingManager : MonoBehaviour
    {
        [SerializeField] private Button playMusic;
        [SerializeField] private LeanButton saveExit;
        [SerializeField] private AudioSource source;
        [SerializeField] private Slider_DSP_Setting setting;

        void Start()
        {
            saveExit.OnClick.AddListener(() =>
            {
                source.Stop();
                setting.SaveValue();
                PlayerPrefs.Save();
                SceneTransit.Instance.Back();
            });
            playMusic.onClick.AddListener(() => { source.PlayScheduled(AudioSettings.dspTime); });
        }
    }
}