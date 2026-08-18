using System.Collections.Generic;
using Network.PhiZone.Data;
using Network.PhiZone.Utils;
using UnityEngine;

namespace Network.PhiZone
{
    public class TestPhizone : MonoBehaviour
    {
        private void Start()
        {
            return;
            Qwq();
        }

        private async void Qwq()
        {
            Response response = await "auth/token".RequestPhiZoneWithUwr("POST", false, new Dictionary<string, string>
            {
                {"client_id", ProgramInfo.ClientId},
                {"client_secret", ProgramInfo.ClientSecret},
                {"grant_type", "password"},
                {"username", "3120393927@qq.com"},
                {"password", "Qianhao12"}
            });
            Debug.Log(response.statusCode);
            Debug.Log(response.responsedData.ToString());
        }
    }
}