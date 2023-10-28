using Newtonsoft.Json;
using UnityEngine;

namespace MainCore.Data
{
    public class ExternalCharacterInfo
    {
        public string Id;
        public float PixelsPerUnit;
        public float PivotX;
        public float PivotY;
        [JsonIgnore] public Vector2 Pivot => new Vector2(PivotX, PivotY);
    }
}