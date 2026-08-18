namespace MainCore
{
    public enum NoteStat
    {
        Perfect,
        Good,
        Bad,
        Miss,
        None,
        Early,
        Late
    }

    public enum JudgeLineStat
    {
        AP,
        // ReSharper disable once InconsistentNaming
        FC,
        None
    }

    public class ScoreCounter
    {
        public int PerfectCnt;
        public int GoodCnt;
        public int BadCnt;
        public int MissCnt;
        public int Combo;
        public int Early;
        public int Late;
        public int Maxcombo;
        public int NumOfNotes;

        private int _elapsedNoteCnt;

        public float Score => GlobalSetting.NewScoreCalcType
            ? 1e6f * (PerfectCnt + GoodCnt * 0.65f) / NumOfNotes // 判定分100w
            : 1e6f * (PerfectCnt * 0.9f + GoodCnt * 0.585f + Maxcombo * 0.1f) / NumOfNotes; // 判定分90w 连击分10w

        public float Accuracy => (PerfectCnt + GoodCnt * 0.65f) / NumOfNotes;
        public float RuntimeAccuracy => _elapsedNoteCnt == 0 ? 1f : (PerfectCnt + GoodCnt * 0.65f) / _elapsedNoteCnt;

        public void Add(NoteStat status)
        {
            switch (status)
            {
                case NoteStat.Perfect:
                    PerfectCnt++;
                    Combo++;
                    _elapsedNoteCnt++;
                    break;
                case NoteStat.Good:
                    GoodCnt++;
                    Combo++;
                    _elapsedNoteCnt++;
                    break;
                case NoteStat.Bad:
                    BadCnt++;
                    Combo = 0;
                    _elapsedNoteCnt++;
                    break;
                case NoteStat.Miss:
                    MissCnt++;
                    Combo = 0;
                    _elapsedNoteCnt++;
                    break;
                case NoteStat.Early:
                    GoodCnt++;
                    Early++;
                    Combo++;
                    _elapsedNoteCnt++;
                    break;
                case NoteStat.Late:
                    GoodCnt++;
                    Late++;
                    Combo++;
                    _elapsedNoteCnt++;
                    break;
            }

            if (Combo > Maxcombo)
                Maxcombo = Combo;
            if (GlobalSetting.LineStat == JudgeLineStat.AP && GoodCnt != 0)
                GlobalSetting.LineStat = JudgeLineStat.FC;
            if (GlobalSetting.LineStat != JudgeLineStat.None && (BadCnt != 0 || MissCnt != 0))
                GlobalSetting.LineStat = JudgeLineStat.None;
        }
    }
}