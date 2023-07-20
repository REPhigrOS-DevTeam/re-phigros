using System.IO;

namespace MainCore.Utilities
{
    public class CSVReader
    {
        private string[][] Array;

        public CSVReader(string path)
        {
            string binAsset = File.ReadAllText(path);

            string[] lineArray = binAsset.Split("\r"[0]);

            Array = new string[lineArray.Length][];

            for (int i = 0; i < lineArray.Length; i++)
            {
                Array[i] = lineArray[i].Split(',');
            }
        }

        public string GetDataByRowAndCol(int nRow, int nCol)
        {
            if (Array.Length <= 0 || nRow >= Array.Length)
                return "";
            if (nCol >= Array[0].Length)
                return "";

            return Array[nRow][nCol];
        }

        /*public string GetDataByIdAndName(int nId, string strName)
    {
        if (Array.Length <= 0)
            return "";

        int nRow = Array.Length;
        int nCol = Array[0].Length;
        for (int i = 1; i < nRow; ++i)
        {
            string strId = string.Format("\n{0}", nId);
            if (Array[i][0] == strId)
            {
                for (int j = 0; j < nCol; ++j)
                {
                    if (Array[0][j] == strName)
                    {
                        return Array[i][j];
                    }
                }
            }
        }

        return "";
    }*/
    }
}