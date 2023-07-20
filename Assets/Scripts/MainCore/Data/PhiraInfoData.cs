namespace MainCore.Data
{
    public class PhiraInfoData
    {
        public int id = -1;
        public int uploader = 0;
        public string name = "";
        public float difficulty = 0.0f;
        public string level = "";
        public string charter = "";
        public string composer = "";
        public string illustrator = "";
        public string chart = "";
        public object format = null;
        public string music = "";
        public string illustration = "";
        public float? previewStart = null; //单位：秒
        public float? previewEnd = null; //单位：秒
        public float aspectRatio = 16f / 9f;
        public float backgroundDim = 0.6f;
        public float lineLength = 6.0f;
        public float offset = 0.0f; //单位：秒
        public string tip = null;
        public string[] tags = new string[0];
        public string intro = "";
        public bool holdPartialCover = false;
        #region timeVars

        public string created = "1970-01-01T08:00:00.000000Z";
        public string updated = "1970-01-01T08:00:00.000000Z";
        public string chartUpdated = "1970-01-01T08:00:00.000000Z";

        #endregion
    }
}
