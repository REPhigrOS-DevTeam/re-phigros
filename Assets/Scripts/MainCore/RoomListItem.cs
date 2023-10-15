using Network.Multiplayer.Data;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    public void Set(MPServerTest mpServerTest, RoomSummary info)
    {
        Text text = transform.GetChild(0).GetComponent<Text>();
        text.text = $"房主：{info.Owner}\n房间号：{info.Id}";
        Button button = gameObject.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => mpServerTest.SelectRoom(info.Id));
    }
}
