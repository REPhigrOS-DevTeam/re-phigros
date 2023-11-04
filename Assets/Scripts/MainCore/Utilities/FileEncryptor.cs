using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MainCore.Utilities.RSA;

namespace MainCore.Utilities
{
    public static class FileEncryptor
    {
        private const string License = "都看到这里了，那我建议你不要把这玩意儿的加解密方式发出去，毕竟反编译程序本身已经违反tos，你要发出去有人利用了指不定有人违法或直接触犯刑法";
        private const string Key = "(写完你游就去死……？)";
        private const string Iv = "想死（哭）.";
#if UNITY_EDITOR
        private const string RsaPrivateKey = "-----BEGIN RSA PRIVATE KEY-----\n" +
                                             "MIIEowIBAAKCAQEA0/mi8SNlvvVvDX67MRyTxIoIWwLyqb/mZLlxxGZeQHI4e7Gh\n" +
                                             "MUr6+tPBKPVD4WBYt55BnK4KPL2a12NjiCBsOZEJ0Xfg8Wm12E3AvmjTwel1SAUB\n" +
                                             "c+3yTzQj+nENr8ALb8w+atMPnvPWPR0AwEOaJQssOuCN54w3GZY4huExyCs66CfP\n" +
                                             "rLXCTR+bKDbs0sJ8Mqas6iGNL+dBZ0n0jAAn7oBYHqHLGuyPMuWq/PIiBtD+a5lK\n" +
                                             "rABzCArc0I5ADKg2hjNP9SPOXNdf262ZwwH9Az6rUtjvHmjB6MvG/DAcP/ro5RXO\n" +
                                             "wSTSK9r/MhJi8ZvpGCxSsCnsKhgXXIb9Iarc7wIDAQABAoIBAA8ZwT9V0U/VhfgO\n" +
                                             "pED/GatbIykLBtFyUvRJXhVRLshL7fUpD5dTRMzgj+nRV+jmAOJXx+T+MWS0ilbR\n" +
                                             "hQvAaSVMTUhBHRYYyadwM1mWxMF5vNNvTXRLEHaolSJbfzYBdtWCIpvGj5E4KqkH\n" +
                                             "RQBTvVcdOot/mrkfibKGocR0cqQhOwfxScVwnaATY1gNuTK/jMHPxlB6PLBKsnBk\n" +
                                             "NK497471Mavy+AYbknWq9hLIXYihdYbdI8Z3KG1m5L4DAqY0QtbXg9JwRLwg+AvM\n" +
                                             "5RVbbvnUuFRQX9gvEFhqGzCkLUssRCXRbwsr+gjKHOntKhgjmRBj5GEc3Pf6boAh\n" +
                                             "Jb2KH3kCgYEA9Np2kTpFOqjwWVZ3fu3WCySmmXBEM6x+DVkg7r6EcowWTZJB31tC\n" +
                                             "JqKS+KxuCk8WUbY0DlyutQmnuI+9pEvYAn7+c2FqoUjPIrhFr3h1JUOnnFXREyYA\n" +
                                             "pWUUna3Ynbvh5KaFuJcJP/cgYIssuCWO5GbHm1y01eXuQDADL0JVH+cCgYEA3aAC\n" +
                                             "I4AcNvRmQPeVoe1Tr9XcOuuTNLwuO+Wob0sy0vvYgxubnozc5Sz1Y9iBfLccJ29i\n" +
                                             "3VltwcHpGMmJ6JcYeJjS/eKjzxxUB90vKH8mmU/NPzfF+rkeIPZyHA5CjSz5sdeC\n" +
                                             "nyM8aRECDQM+NfVL/oC/ba3gBLCfo9SFdNGE2bkCgYBJbP3jXSsHhUPWNpTNDnuC\n" +
                                             "ifIfz0fUiySd0h2LGrzTMOk7R+HTHiW/Oj+CrQqusrrJtC72I5sMlSGjug8vpsLX\n" +
                                             "NMgPR6ZXSWM8UTAsh53xl9E6k42IBXxqHN7KzihIXOBH1hwBl+FhOjWOXg4CBtlL\n" +
                                             "6vpuv6VHA5Wnz/4UfPrT/wKBgQC5RMPE7ZmojxLUCKT74qvs+DjMwJYkpZN42vm9\n" +
                                             "X/2yxnouz+t91X/rzXOt7hYBLgnJJaJeLB5GtVWpNQGmgHkih48KUmZiAup0UIDV\n" +
                                             "t8WKsF2CFZvZhtsa7yphLcKQxiJOezxk0E31/xPZ0PY2oULQFMzyYUI+aXBqwoR2\n" +
                                             "LZiDcQKBgCdZVmFxXdrX8Qi3B5pKPyL2zJv/VDQOqpsQfmJgVik6QxZ3znfdbYxx\n" +
                                             "h478a3JOOgKQ5pie8YY6kLjSJtwB/x3x6rcdc3HqctxO1Cv4wjsVq8nO0RrE9YHS\n" +
                                             "orPpm4PI5iwNFQTmSjjNQC+17sqROiE+UybiwRaSgLuT8Brma5B/\n" +
                                             "-----END RSA PRIVATE KEY-----\n";
#endif
        private const string RsaPublicKey = "-----BEGIN PUBLIC KEY-----\n" +
                                            "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0/mi8SNlvvVvDX67MRyT\n" +
                                            "xIoIWwLyqb/mZLlxxGZeQHI4e7GhMUr6+tPBKPVD4WBYt55BnK4KPL2a12NjiCBs\n" +
                                            "OZEJ0Xfg8Wm12E3AvmjTwel1SAUBc+3yTzQj+nENr8ALb8w+atMPnvPWPR0AwEOa\n" +
                                            "JQssOuCN54w3GZY4huExyCs66CfPrLXCTR+bKDbs0sJ8Mqas6iGNL+dBZ0n0jAAn\n" +
                                            "7oBYHqHLGuyPMuWq/PIiBtD+a5lKrABzCArc0I5ADKg2hjNP9SPOXNdf262ZwwH9\n" +
                                            "Az6rUtjvHmjB6MvG/DAcP/ro5RXOwSTSK9r/MhJi8ZvpGCxSsCnsKhgXXIb9Iarc\n" +
                                            "7wIDAQAB\n" +
                                            "-----END PUBLIC KEY-----\n";

#if UNITY_EDITOR
        public static byte[] RsaEncrypt(byte[] data)
        {
            if (data is not { Length: > 0 and <= 2048 })
                throw new ArgumentNullException(nameof(data));
            return RsaEncryptUtil.EncryptByPrivateKey(data, RsaPrivateKey, "PKCS1");
        }
#endif

        public static byte[] RsaDecrypt(byte[] data)
        {
            if (data is not { Length: > 0 })
                throw new ArgumentNullException(nameof(data));
            return RsaEncryptUtil.DecryptByPublicKey(data, RsaPublicKey);
        }

        public static byte[] Encrypt(byte[] data)
        {
            if (data == null || data.Length <= 0)
                throw new ArgumentNullException(nameof(data));

            using Aes aesAlg = Aes.Create();
            aesAlg.KeySize = 256;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.Key = Encoding.UTF8.GetBytes(Key);
            aesAlg.IV = Encoding.UTF8.GetBytes(Iv);
            ICryptoTransform encryptor = aesAlg.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public static byte[] Decrypt(byte[] cipherText)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));

            using Aes aesAlg = Aes.Create();
            aesAlg.KeySize = 256;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.Key = Encoding.UTF8.GetBytes(Key);
            aesAlg.IV = Encoding.UTF8.GetBytes(Iv);
            ICryptoTransform descriptor = aesAlg.CreateDecryptor();
            return descriptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
        }

        public static byte[] ComputeSha256(byte[] data)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(data);
            return hash;
        }
    }
}