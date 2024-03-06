
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace PasswordManager
{
    internal class Server
    {
        public string Path;
        private int _lengthOfKey = 16;
        public byte[] IV { get; set; }
        public Dictionary<string, Dictionary<string, string>> Vault;
        private Dictionary<string, string> _domainsWithPasswords;

        public Server(string path)
        {
            Path = path;
        }

        public void GenerateIV()
        {
            // Antingen via RandomNumberGenerator:
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                IV = new byte[_lengthOfKey];
                generator.GetBytes(IV);
            }

            // Eller genom att skapa ett Aes-objekt för att generera IV

        }

        public void CreateVault()
        {
            _domainsWithPasswords = new Dictionary<string, string>();
            Vault = new Dictionary<string, Dictionary<string, string>>
            {
                { "vault", _domainsWithPasswords }
            };
        }

        public byte[] Encrypt(byte[] vaultKey)
        {
            string textToEncrypt = JsonSerializer.Serialize(Vault["vault"]);
            byte[] result;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = vaultKey;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(textToEncrypt);
                        }
                        result = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return result;
        }

        public void SetIV()
        {
            FileHandler fileHandler = new FileHandler();
            string IVAsString = fileHandler.ReadValueFromJson(Path, "IV");
            byte[] IVAsBytes = Convert.FromBase64String(IVAsString);
            IV = IVAsBytes;
        }

        public byte[] GetEncryptedAccounts()
        {
            FileHandler fileHandler = new FileHandler();
            string encryptedAccounts = fileHandler.ReadValueFromJson(this.Path, "vault");
            return Convert.FromBase64String(encryptedAccounts);
        }

        public string FormatVaultToText(byte[] encryptedAccounts)
        {
            string encryptedAccountsAsText = Convert.ToBase64String(encryptedAccounts);
            string IVAsText = Convert.ToBase64String(IV);

            Dictionary<string, string> server = new Dictionary<string, string>();
            server["vault"] = encryptedAccountsAsText;
            server["IV"] = IVAsText;

            //string serverFileAsJsonText = JsonSerializer.Serialize(serverFileKeyValuePairs);
            string serverFileAsJsonText = JsonSerializer.Serialize(server, new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }
            );

            return serverFileAsJsonText;
        }

        public string Decrypt(byte[] encryptedVaultAsBytes, byte[] vaultKey)
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
                    using (MemoryStream msDecrypt = new MemoryStream(encryptedVaultAsBytes))
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
                Console.WriteLine("Could not decrypt data.");
                return null;
            }
        }
    }
}
