using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using XC.RSAUtil;

namespace MainCore.Utilities.RSA
{
    public static class RsaEncryptUtil
    {
        /// <summary>
        /// 私钥加密 .Net平台默认是使用公钥进行加密，私钥进行解密。私钥加密需要自己实现或者使用第三方dll
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key">私钥,格式:PKCS8</param>
        /// <returns></returns>
        public static byte[] EncryptByPrivateKey(byte[] data, string key)
        {
            string priKey = key.Trim();
            string xmlPrivateKey = RSAPrivateKeyJava2DotNet(priKey);
            //加载私钥  
            RSACryptoServiceProvider privateRsa = new RSACryptoServiceProvider();
            privateRsa.FromXmlString(xmlPrivateKey);
            //转换密钥  
            AsymmetricCipherKeyPair keyPair = DotNetUtilities.GetKeyPair(privateRsa);
            IBufferedCipher c = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");// 参数与Java中加密解密的参数一致       
            c.Init(true, keyPair.Private); //第一个参数为true表示加密，为false表示解密；第二个参数表示密钥 
            byte[] outBytes = c.DoFinal(data);//加密  
            return outBytes;

        }

        /// <summary>
        /// 私钥加密
        /// </summary>
        /// <param name="data">明文</param>
        /// <param name="key">私钥</param>
        /// <param name="keyFormat">私钥格式:PKCS1,PKCS8</param>
        /// <returns></returns>
        public static byte[] EncryptByPrivateKey(byte[] data, string key,string keyFormat)
        {
            string priKey = key.Trim();
             
            //加载私钥  
            RSACryptoServiceProvider privateRsa = RsaUtil.LoadPrivateKey(key, keyFormat);
             
            //转换密钥  
            AsymmetricCipherKeyPair keyPair = DotNetUtilities.GetKeyPair(privateRsa);
            IBufferedCipher c = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");// 参数与Java中加密解密的参数一致       
            c.Init(true, keyPair.Private); //第一个参数为true表示加密，为false表示解密；第二个参数表示密钥 
            byte[] outBytes = c.DoFinal(data);//加密  
            return outBytes;

        }

        /// <summary>
        /// RSA私钥格式转换，java->.net
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        private static string RSAPrivateKeyJava2DotNet(string privateKey)
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));

            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()), Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        /// <summary>
        /// 用公钥解密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] DecryptByPublicKey(byte[] data, string key)
        {
            string pubKey = key.Trim();
            string xmlPublicKey = RsaKeyConvert.PublicKeyPemToXml(pubKey);

            RSACryptoServiceProvider publicRsa = new RSACryptoServiceProvider();
            publicRsa.FromXmlString(xmlPublicKey);

            AsymmetricKeyParameter keyPair = DotNetUtilities.GetRsaPublicKey(publicRsa);
            //转换密钥  
            // AsymmetricCipherKeyPair keyPair = DotNetUtilities.GetRsaKeyPair(publicRsa);
            IBufferedCipher c = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");// 参数与Java中加密解密的参数一致       
            c.Init(false, keyPair); //第一个参数为true表示加密，为false表示解密；第二个参数表示密钥 
            byte[] outBytes = c.DoFinal(data);//解密  
            return outBytes;
        }
    }
}