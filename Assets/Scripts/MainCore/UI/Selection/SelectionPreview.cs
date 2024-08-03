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
        [SerializeField] private Image illustration;
        [SerializeField] private Text songName, composer, charter, illustrator, level;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private TranslucentImageSource translucentSource;
        [SerializeField] private PullableScrollRect refreshControl;

        private bool _firstTime = false;

        public static BeatmapInfo SelectedInfo { get; private set; } = null;

        void Start()
        {
            illustration.SetAlpha(0.0f);
            songName.text = "";
            composer.text = "";
            charter.text = "";
            illustrator.text = "";
            level.text = "";
            refreshControl.OnRefresh.AddListener(PreviewIllustration);
            refreshControl.PullDistanceRequiredRefresh = 150f;
            backgroundImage.GetComponent<Button>().onClick.AddListener(StopPreviewIllustration);
        }

        public void UpdatePreview(BeatmapInfo info)
        {
            SelectedInfo = info;
            illustration.sprite = info.Illustration;
            backgroundImage.sprite = info.Illustration;
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
        }

        private void PreviewIllustration()
        {
            if (!HasSelected())
            {
                return;
            }
            translucentSource.preview = false;
            mainCanvasGroup.blocksRaycasts = false;
            backgroundImage.DOFade(1, .6f);
            mainCanvasGroup.DOFade(0, .6f);
        }
        
        private void StopPreviewIllustration()
        {
            if (!HasSelected())
            {
                return;
            }
            translucentSource.preview = true;
            backgroundImage.DOFade(0, .6f);
            mainCanvasGroup.DOFade(1, .6f).OnComplete(() => mainCanvasGroup.blocksRaycasts = true);
        }

        private bool HasSelected() => SelectedInfo != null;
    }
}