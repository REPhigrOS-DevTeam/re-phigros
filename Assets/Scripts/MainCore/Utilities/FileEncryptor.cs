using System;
using System.Security.Cryptography;

namespace MainCore.Utilities
{
    public static class FileEncryptor
    {
        // Padding: PKCS#1 v1.5
#if UNITY_EDITOR
        private const string PrivateKey =
            "<RSAKeyValue><Modulus>rAPKb6vGiLTY2QHCnIMxtvoHcAp2Pbk+8x8wkFMBpX5LRouQFf1/bTW4mW6AZokVmzFrYQXnG2sbHr9z+GkEe8Fc7KfiSTDenxwJLJL9CHnEdXBDxR3ykyRiuN472hiJ78E+kddxoomk9Js6rYZ0YIBhoq5kr9qfYOR9D9tDQfs=</Modulus><Exponent>AQAB</Exponent><P>s8NHGPPhua0Ou455htOPI3c2UO61dVhd3bMOKk7+ymWGSmYzz733nM9NSDke1GLKY6Bd0EbLSlc2XuOi4rhpgQ==</P><Q>9PdRitqU6tFd0wNd4+aU7IOtgsZfGV/c/8VYwJDjHxxVJshXDb3qRASL1Y0lgGMJ5qirl/fUgbG/XRaGGycRew==</Q><DP>BZ4UmrMEWskNrM7G/W+fCXywNdc/1Grug/8Ucj4FuE1z5N9MvzEwi7XutFMUo45yxKo+REPyFmCjUlPKw0sAAQ==</DP><DQ>4lNWTVniWImTjBACQTuawGJwfvDUkFcXkmA8vb2fefDtY2WZuKKMvMcOgwFjcpkOXsPbtg5Nkn4s9c6HnLKd3Q==</DQ><InverseQ>QgAM+VTi2wrv0KFY92pgHGHHkecIXtylK29vXzBUX+fzGiphh7VlXSz9Rb9JuR9mjkl0AZYA+gGdSwIHmkNWsQ==</InverseQ><D>kLp7yCuKVpl63lM50AAegyqpuV5EEDjduydh7/y3JOw3H7rrV2U7osKReB7eT+dFU5doFnEl+w7J+bvyMm8Bwk0Zv4DSwNM1WTNn1TGT3qUXYLmN4QYRfBbmrl8I4a+Ip7aZVZVYbWpczjlnCd8wUm5tkgDQo6/yZWTnhqF9gwE=</D></RSAKeyValue>";

#endif
        private const string PublicKey =
            "<RSAKeyValue><Modulus>rAPKb6vGiLTY2QHCnIMxtvoHcAp2Pbk+8x8wkFMBpX5LRouQFf1/bTW4mW6AZokVmzFrYQXnG2sbHr9z+GkEe8Fc7KfiSTDenxwJLJL9CHnEdXBDxR3ykyRiuN472hiJ78E+kddxoomk9Js6rYZ0YIBhoq5kr9qfYOR9D9tDQfs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

#if UNITY_EDITOR
        public static byte[] Encrypt(byte[] data)
        {
            if (data == null || data.Length <= 0)
                throw new ArgumentNullException(nameof(data));
            if (PrivateKey == null || PrivateKey.Length <= 0)
                throw new ArgumentNullException(nameof(PrivateKey));

            using RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(PrivateKey);
            return rsa.Encrypt(data, false);
        }
#endif

        public static byte[] Decrypt(byte[] cipherText)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (PublicKey == null || PublicKey.Length <= 0)
                throw new ArgumentNullException(nameof(PublicKey));

            using RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(PublicKey);
            return rsa.Decrypt(cipherText, false);
        }
    }
}