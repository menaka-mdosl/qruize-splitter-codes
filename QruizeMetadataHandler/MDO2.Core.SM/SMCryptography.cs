using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MDO2.Core.SM
{
    public class SMCryptography
    {
        #region static objects
        public const uint KEY_LENGTH = 32;
        public const uint IV_LENGTH = 16;

        public static byte[] CreateAESKeyOrIv(string keyOrIvText, uint length)
        {
            if (string.IsNullOrWhiteSpace(keyOrIvText))
            {
                throw new ArgumentException(nameof(keyOrIvText));
            }
            if (length < 16)
            {
                throw new ArgumentOutOfRangeException(nameof(keyOrIvText));
            }

            using (var hashProvider1 = new SHA256Managed())
            {
                var bytes = Encoding.UTF8.GetBytes(keyOrIvText);
                var hash = hashProvider1.ComputeHash(bytes);
                var keyIvString = BitConverter.ToString(hash).Replace("-", "").ToLower().Substring(0, (int)length);
                var keyIvBytes = Encoding.UTF8.GetBytes(keyIvString);
                return keyIvBytes;
            }
        }
        #endregion

        private readonly byte[] key;
        private readonly byte[] iv;

        public SMCryptography(byte[] key, byte[] iv)
        {
            if (key is null || key.Length == 0)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (iv is null || iv.Length == 0)
            {
                throw new ArgumentNullException(nameof(iv));
            }

            this.key = key;
            this.iv = iv;
        }

        private SymmetricAlgorithm GetAlgo()
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 256;
            aes.Key = key;
            aes.IV = iv;
            return aes;
        }

        public string EncryptString(string plainText)
        {
            try
            {
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

                using (var algo = GetAlgo())
                using (var encryptor = algo.CreateEncryptor(algo.Key, algo.IV))
                using (var ms = new MemoryStream())
                using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();

                    var b64Once = Convert.ToBase64String(ms.ToArray());
                    var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(b64Once));
                    return b64;
                }
            }
            catch (Exception ex)
            {
                throw new SMCryptographyOperationException($"Cannot encrypt the provided text", ex);
            }
        }
        public string DecryptString(string cipherText)
        {
            try
            {
                var b64Once = Convert.FromBase64String(cipherText);
                var b64 = Convert.FromBase64String(Encoding.UTF8.GetString(b64Once));

                using (var algo = GetAlgo())
                using (var decryptor = algo.CreateDecryptor(algo.Key, algo.IV))
                using (var ms = new MemoryStream(b64))
                using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cryptoStream))
                {
                    var plainText = sr.ReadToEnd();
                    return plainText;
                }
            }
            catch (Exception ex)
            {
                throw new SMCryptographyOperationException($"Cannot decrypt the provided text", ex);
            }
        }
    }
}
