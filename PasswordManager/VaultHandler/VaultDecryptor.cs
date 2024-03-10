using System.Security.Cryptography;

namespace PasswordManager.VaultHandler
{
    internal static class VaultDecryptor
    {
        public static string Decrypt(byte[] vaultKey, byte[] IV, byte[] encryptedAccountsAsBytes)
        {
            try
            {
                string plaintext = null;

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Key = vaultKey;
                    aesAlg.IV = IV;

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for decryption.
                    using (MemoryStream msDecrypt = new MemoryStream(encryptedAccountsAsBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {

                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }

                return plaintext;
            }
            catch
            {
                Console.WriteLine("Something went wrong.");
                return null;
            }
        }
    }
}
