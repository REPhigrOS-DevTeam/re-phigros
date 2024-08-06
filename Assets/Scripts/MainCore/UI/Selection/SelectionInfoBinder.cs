using Cysharp.Threading.Tasks;
using Lean.Gui;
using MainCore.Serialized;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI.Selection
{
    public class SelectionInfoBinder : MonoBehaviour
    {
        [SerializeField] private RawImage illustration;
        [SerializeField] private Texture2D fallbackImage;
        [SerializeField] private TMP_Text songName;
        [SerializeField] private LeanButton button;
        private BeatmapInfo Info { get; set; }

        private bool _updated;

        public void SetInfo(BeatmapInfo info)
        {
            Info = info;
            button.OnClick.AddListener(UpdatePreview);
        }

        private async void UpdatePreview()
        {
            await UniTask.WaitUntil(() => _updated);
            SelectionPreview.Instance.UpdatePreview(Info);
        }

        public async void NotifyUpdate(bool forceRefresh = true)
        {
            _updated = false;
            songName.text= Info.SongName;
            await Info.LoadIllustration(forceRefresh);
            illustration.texture = Info.Illustration;
            _updated = true;
        }

        public async void Unload()
        {
            illustration.texture = fallbackImage;
        }
    }
}