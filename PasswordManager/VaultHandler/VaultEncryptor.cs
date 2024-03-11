
using System.Security.Cryptography;

namespace PasswordManager.VaultHandler
{
    internal static class VaultEncryptor
    {
        public static void LockVault(string decryptedAccounts, Server server)
        {
            byte[] encryptedAccountsAsBytes = Encrypt(decryptedAccounts);
            File.WriteAllText(server.Path, server.FormatServerToText(encryptedAccountsAsBytes));
        }

        public static byte[] Encrypt(string textToEncrypt)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = VaultDecryptor.VaultKey;
                aesAlg.IV = VaultDecryptor.IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write all data to the stream.
                            swEncrypt.Write(textToEncrypt);
                        }

                        return msEncrypt.ToArray();
                    }
                }
            }
        }
    }
}
