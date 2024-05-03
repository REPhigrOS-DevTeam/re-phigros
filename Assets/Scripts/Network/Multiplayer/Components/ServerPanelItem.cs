using System;
using Network.Multiplayer.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Multiplayer.Components
{
    [RequireComponent(typeof(Button))]
    public class ServerPanelItem : MonoBehaviour
    {
        [SerializeField] private int id;

        private ServerManager _settingsManager;
        
        private Func<bool>  _onValidate;

        public void Init(ServerManager settingsManager, Func<bool> onValidate = null)
        {
            _onValidate = onValidate ?? (() => true);
            _settingsManager = settingsManager;
            gameObject.GetComponent<Button>().onClick.AddListener(OnSelected);
        }

        private void OnSelected()
        {
            if (!_onValidate()) return;
            _settingsManager.UpdatePanel(id);
        }
    }
}