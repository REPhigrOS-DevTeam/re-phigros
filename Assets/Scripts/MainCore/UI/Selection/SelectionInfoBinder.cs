using Cysharp.Threading.Tasks;
using MainCore.Serialized;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI.Selection
{
    public class SelectionInfoBinder : MonoBehaviour
    {
        [SerializeField] private Image illustration;
        [SerializeField] private TMP_Text songName;
        [SerializeField] private Button button;
        public BeatmapInfo Info { get; private set; }

        private bool _updated = false;

        public SelectionInfoBinder SetInfo(BeatmapInfo info)
        {
            Info = info;
            button.onClick.AddListener(UpdatePreview);
            return this;
        }

        private async void UpdatePreview()
        {
            await UniTask.WaitUntil(() => _updated);
            SelectionPreview.Instance.UpdatePreview(Info);
        }

        public async void NotifyUpdate()
        {
            _updated = false;
            songName.text= Info.SongName;
            await Info.LoadIllustration(forceRefresh: true);
            illustration.sprite = Info.Illustration;
            _updated = true;
        }
    }
}