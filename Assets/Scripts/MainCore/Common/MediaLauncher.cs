using MainCore.Utilities;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

public class MediaLauncher : MonoBehaviour
{
    [SerializeField] private DisplayUGUI displayUGUI;

    private void Awake()
    {
        if (!displayUGUI || !displayUGUI.Player) return;
#if UNITY_EDITOR || !RELEASE_VERSION
        displayUGUI.SetAlpha(1f);
        displayUGUI.Player.Play();
#else
        displayUGUI.color = displayUGUI.color.SetAlpha(0f);
        displayUGUI.Player.Stop();
#endif
    }
}