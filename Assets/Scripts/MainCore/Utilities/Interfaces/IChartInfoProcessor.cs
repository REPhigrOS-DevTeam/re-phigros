using MainCore.Data;

namespace MainCore.Utilities.Interfaces
{
    public interface IChartInfoProcessor
    {
        public bool CanProcess(string folderPath);
        public (PhiraInfoData, GameFilePathInfo) Process(string infoFilePath);
    }
}
