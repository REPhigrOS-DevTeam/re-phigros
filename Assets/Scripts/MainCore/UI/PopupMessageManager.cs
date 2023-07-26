using System.Threading;
using MainCore.Common;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI
{
    public class PopupMessageManager : MonoSingleton<PopupMessageManager>
    {
        private static int unityThread;

        [SerializeField] private Animation animation;
        [SerializeField] private Text content;
        private string animWaitToPlay = "";
        private float duration;

        private string popupMessage = "";

        private bool OnUnityThread => Thread.CurrentThread.ManagedThreadId == unityThread;

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            unityThread = Thread.CurrentThread.ManagedThreadId;
        }

        void Update()
        {
            if (animWaitToPlay != "")
            {
                PlayAnimation(animWaitToPlay);
                animWaitToPlay = "";
            }

            if (popupMessage != "")
            {
                content.text = popupMessage;
                popupMessage = "";
            }

            if (duration == 0)
            {
                return;
            }

            var a = duration - Time.deltaTime;
            if (a <= 0)
            {
                PlayAnimation("PopupMessageDisable");
                duration = 0;
            }
            else
            {
                duration = a;
            }
        }

        /// <summary>
        /// Message a message.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public void Message(string message)
        {
            if (duration != 0) return;
            PlayAnimation("PopupMessage");
            popupMessage = message;
            if (OnUnityThread)
            {
                content.text = popupMessage;
            }

            duration = 3f;
        }

        /// <summary>
        /// Change the message immediately.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public void ChangeContent(string message)
        {
            if (duration == 0) PlayAnimation("PopupMessage");
            popupMessage = message;
            if (OnUnityThread)
            {
                content.text = popupMessage;
            }

            duration = 3f;
        }

        /// <summary>
        /// Clear the message.
        /// </summary>
        public void Clear()
        {
            duration = 1f;
            ChangeContent("");
        }

        private void PlayAnimation(string anim)
        {
            if (!OnUnityThread)
            {
                animWaitToPlay = anim;
            }
            else
            {
                animation.Play(anim);
            }
        }
    }
}