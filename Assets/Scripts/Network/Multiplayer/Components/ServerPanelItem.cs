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
        
        public void Init(ServerManager settingsManager)
        {
            _settingsManager = settingsManager;
            gameObject.GetComponent<Button>().onClick.AddListener(OnSelected);
        }

        private void OnSelected()
        {
            _settingsManager.UpdatePanel(id);
        }
    }
}