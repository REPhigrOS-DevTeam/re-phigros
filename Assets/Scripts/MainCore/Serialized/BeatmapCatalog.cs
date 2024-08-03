using System;
using System.Collections.Generic;

namespace MainCore.Serialized
{
    [Serializable]
    public class BeatmapCatalog
    {
        public Dictionary<string, BeatmapInfo> Infos { get; set; } = new();
    }
}