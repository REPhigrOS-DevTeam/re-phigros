using Cysharp.Threading.Tasks;
using DG.Tweening;
using LeTai.Asset.TranslucentImage;
using MainCore.Common;
using MainCore.Serialized;
using MainCore.UI.Utils;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI.Selection
{
    public class SelectionPreview : MonoSingleton<SelectionPreview>
    {
        [SerializeField] private RawImage illustration;
        [SerializeField] private Text songName, composer, charter, illustrator, level, path;
        [SerializeField] private RawImage backgroundImage;
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private TranslucentImageSource translucentSource;
        [SerializeField] private PullableScrollRect refreshControl;

        private bool _firstTime = false;
        private bool _previewingIllustration = false;

        public static BeatmapInfo SelectedInfo { get; private set; } = null;

        void Start()
        {
            illustration.SetAlpha(0.0f);
            songName.text = "";
            composer.text = "";
            charter.text = "";
            illustrator.text = "";
            level.text = "";
            path.text = "";
            refreshControl.OnRefresh.AddListener(PreviewIllustration);
            refreshControl.PullDistanceRequiredRefresh = 150f;
            backgroundImage.GetComponent<Button>().onClick.AddListener(StopPreviewIllustration);
            backgroundImage.GetComponent<Button>().interactable = false;
        }

        public void UpdatePreview(BeatmapInfo info)
        {
            SelectedInfo = info;
            illustration.texture = info.Illustration;
            backgroundImage.texture = info.Illustration;
            if (!_firstTime)
            {
                illustration.DOFade(1f, 1f);
                _firstTime = true;
            }

            songName.text = info.SongName;
            composer.text = info.Composer;
            charter.text = info.Charter;
            illustrator.text = info.Illustrator;
            level.text = info.SongLevel;
            path.text = info.BasePath;
        }

        public static void Reset() => SelectedInfo = null;

        private async void PreviewIllustration()
        {
            if (!HasSelected() || _previewingIllustration)
            {
                return;
            }
            _previewingIllustration = true;
            translucentSource.preview = false;
            mainCanvasGroup.blocksRaycasts = false;
            backgroundImage.DOFade(1, .6f);
            mainCanvasGroup.DOFade(0, .6f);
            await UniTask.Delay(600);
            backgroundImage.GetComponent<Button>().interactable = true;
        }
        
        private async void StopPreviewIllustration()
        {
            if (!HasSelected())
            {
                return;
            }
            backgroundImage.GetComponent<Button>().interactable = false;
            translucentSource.preview = true;
            backgroundImage.DOKill();
            mainCanvasGroup.DOKill();
            backgroundImage.DOFade(0, .6f);
            mainCanvasGroup.DOFade(1, .6f);
            await UniTask.Delay(600);
            mainCanvasGroup.blocksRaycasts = true;
            _previewingIllustration = false;
        }

        private bool HasSelected() => SelectedInfo != null;
    }
}