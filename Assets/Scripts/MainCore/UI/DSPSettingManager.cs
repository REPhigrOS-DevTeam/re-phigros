using Lean.Gui;
using MainCore.Common;
using UnityEngine;

namespace MainCore.UI
{
    public class DSPSettingManager : MonoBehaviour
    {
        [SerializeField] private LeanButton playMusic;
        [SerializeField] private LeanButton saveExit;
        [SerializeField] private AudioSource source;
        [SerializeField] private Slider_DSP_Setting setting;
        private const string SceneName = "DSPScene";

        void Start()
        {
            saveExit.OnClick.AddListener(() =>
            {
                source.Stop();
                setting.SaveValue();
                PlayerPrefs.Save();
                SceneTransit.Instance.LeaveAdditiveScene(SceneName);
            });
            playMusic.OnClick.AddListener(() => { source.PlayScheduled(AudioSettings.dspTime); });
        }
    }
}