using System.Security.Cryptography;

namespace PasswordManager.VaultHandler
{
    internal static class VaultDecryptor
    {
        public static byte[] EncryptedAccounts { get; private set; }
        public static string DecryptedAccounts { get; private set; }
        public static string SecretKey { get; private set; }
        public static byte[] IV { get; private set; }
        public static byte[] VaultKey { get; private set; }
        public static Server ServerInstance { get; private set; }

        public static void LoginToServer(string clientPath, string serverPath, bool isInitCommand, bool isCreateCommand)
        {
            Client client = new Client(clientPath);
            client.MasterPassword = new UserCommunicator().PromptUserFor("master password");

            if (isInitCommand)
            {
                client.Initialize();
            }
            else if (isCreateCommand)
            {
                client.Create();
            }
            else
            {
                client.TrySetSecretFromFile();
            }

            if (client.SecretKeyAsBytes == null)
            {
                return;
            }

            SecretKey = client.SecretKey;
            VaultKey = client.GetVaultKey();

            ServerInstance = new Server(serverPath);
            EncryptedAccounts = ServerInstance.GetEncryptedAccounts();
            IV = ServerInstance.IV;
            DecryptedAccounts = Decrypt(VaultKey, IV, EncryptedAccounts);
        }

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
