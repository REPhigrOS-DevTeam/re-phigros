using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace MainCore.Utilities
{
    public static class SaveManager
    {
        private static string Md5Sum(byte[] bytesToEncrypt)
        {
            // 创建md5 对象
            MD5 md5 = MD5.Create();

            // 生成16位的二进制校验码
            byte[] hashBytes = md5.ComputeHash(bytesToEncrypt);

            // 转为32位字符串
            string hashString = "";
            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }

            return hashString.PadLeft(32, '0');
        }

        private static string Md5Sum(string strToEncrypt)
        {
            // 将需要加密的字符串转为byte数组
            byte[] bs = Encoding.UTF8.GetBytes(strToEncrypt);
            return Md5Sum(bs);
        }

        private static byte[] AESDecrypt(byte[] data, byte[] key)
        {
            byte[] toEncryptArray = data;
            byte[] keyArray = key;
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] b = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return b;
        }

        private static byte[] AESEncrypt(byte[] data, byte[] key)
        {
            byte[] toEncryptArray = data;
            byte[] keyArray = key;
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] b = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return b;
        }

        private static string AESEncrypt(string data, string key)
        {
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(data);
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            return Convert.ToBase64String(AESEncrypt(toEncryptArray, keyArray));
        }

        private static string AESDecrypt(string data, string key)
        {
            byte[] toEncryptArray = Convert.FromBase64String(data);
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            return Encoding.UTF8.GetString(AESDecrypt(toEncryptArray, keyArray));
        }

        public static void SaveScore(string chart, string score)
        {
            chart = Md5Sum(chart) + Md5Sum("totorowldox1314");
            string data = AESEncrypt(score, Md5Sum("totorowldox1314"));
            PlayerPrefs.SetString(chart, data);
        }

        public static int GetScore(string chart)
        {
            chart = Md5Sum(chart) + Md5Sum("totorowldox1314");
            string t = PlayerPrefs.GetString(chart, "123");
            if (t == "123")
                return 0;
            string a = AESDecrypt(t, Md5Sum("totorowldox1314"));
            return int.Parse(a);
        }

        ///<summary>Reset this chart's score.USE WITH CAUTION!</summary>
        public static void ResetScore(string chart)
        {
            chart = Md5Sum(chart) + Md5Sum("totorowldox1314");
            string t = PlayerPrefs.GetString(chart, "123");
            if (t == "123")
                return;
            PlayerPrefs.DeleteKey(chart);
        }
    }
}