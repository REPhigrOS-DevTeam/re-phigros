using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MainCore.Utilities
{
    public static class FileEncryptor
    {
        private const string Key = "12345678123456781234567812345678";
        private const string Iv = "1234567812345678";

        public static byte[] Encrypt(byte[] data)
        {
            if (data == null || data.Length <= 0)
                throw new ArgumentNullException(nameof(data));
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            if (Iv == null || Iv.Length <= 0)
                throw new ArgumentNullException(nameof(Iv));

            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = Encoding.UTF8.GetBytes(Key);
                aesAlg.IV = Encoding.UTF8.GetBytes(Iv);
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (BufferedStream bfStream = new BufferedStream(csEncrypt))
                        {
                            bfStream.Write(data, 0, data.Length);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        public static byte[] Decrypt(byte[] cipherText)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            if (Iv == null || Iv.Length <= 0)
                throw new ArgumentNullException(nameof(Iv));

            string plaintext = null;
            using Aes aesAlg = Aes.Create();
            aesAlg.KeySize = 256;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.Key = Encoding.UTF8.GetBytes(Key);
            aesAlg.IV = Encoding.UTF8.GetBytes(Iv);
            ICryptoTransform descriptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using MemoryStream msDecrypt = new MemoryStream(cipherText);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, descriptor, CryptoStreamMode.Read);
            using BufferedStream bfStream = new BufferedStream(csDecrypt);
            using MemoryStream memoryStream = new MemoryStream();
            byte[] buffer = new byte[1024];
            int length;
            while ((length = bfStream.Read(buffer)) > 0)
            {
                memoryStream.Write(buffer, 0, length);
            }

            return memoryStream.ToArray();
        }
    }
}