using MainCore.Data;

namespace MainCore.Utilities.Interfaces
{
    public interface IChartInfoProcessor
    {
        public bool CanProcess(string folderPath);
        public (PhiraChartInfoData, GameFilePathInfo) Process(string infoFilePath);
    }
}
