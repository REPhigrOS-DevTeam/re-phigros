using System;
using Network.Multiplayer.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Multiplayer.Components
{
    public class ChatMessage : MonoBehaviour
    {
        public Text content;
        private bool isUsed = false;

        public void Init(string from, string message, MessageType type)
        {
            if (isUsed) return;
            isUsed = true;
            switch (type)
            {
                case MessageType.Common:
                    content.text = from + ": " + message;
                    content.color = Color.white;
                    break;
                case MessageType.Self:
                    content.text = from + ": " + message;
                    content.color = new Color(1, 1, 0, 1f);
                    break;
                case MessageType.Server:
                    content.text = message;
                    content.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                    break;
                case MessageType.Error:
                    content.text = message;
                    content.color = Color.red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}