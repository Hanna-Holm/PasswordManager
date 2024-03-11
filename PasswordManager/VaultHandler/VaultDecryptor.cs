using System.Security.Cryptography;
using PasswordManager.Commands;

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

        public static void LoginToServer(string clientPath, string serverPath, CommandType commandType)
        {
            Client client = new Client(clientPath);
            client.MasterPassword = new UserCommunicator().PromptUserFor("master password");

            switch (commandType)
            {
                case CommandType.Init:
                    client.Initialize();
                    break;
                case CommandType.Create:
                    client.SetSecretKeyFromPrompt();
                    break;
                default:
                    client.TrySetSecretFromFile();
                    break;
            }

            if (client.SecretKeyAsBytes == null)
                return;

            SecretKey = client.SecretKey;
            VaultKey = client.GetVaultKey();
            ServerInstance = new Server(serverPath);
            IV = ServerInstance.IV;

            if (commandType == CommandType.Init)
                return;

            EncryptedAccounts = ServerInstance.GetEncryptedAccounts();
            DecryptedAccounts = Decrypt();
        }

        public static string Decrypt()
        {
            try
            {
                string plaintext = null;

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Key = VaultKey;
                    aesAlg.IV = IV;

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for decryption.
                    using (MemoryStream msDecrypt = new MemoryStream(EncryptedAccounts))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {

                                // Read the decrypted bytes from the decrypting stream and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }

                return plaintext;
            }
            catch
            {
                Console.WriteLine("Authorization failed.");
                return null;
            }
        }
    }
}
