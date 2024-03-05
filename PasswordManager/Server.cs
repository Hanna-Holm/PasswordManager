
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

        public void SetIV()
        {
            FileHandler fileHandler = new FileHandler();
            string IVAsString = fileHandler.ReadValueFromJson(Path, "IV");
            byte[] IVAsBytes = Convert.FromBase64String(IVAsString);
            this.IV = IVAsBytes;
        }

        public void CreateVault()
        {
            _domainsWithPasswords = new Dictionary<string, string>
            {
                { "Facebook", "password1" },
                { "Amazon", "password2" },
                { "Apple", "password3" },
                { "Netflix", "password4" },
                { "Google", "password5" }
            };

            Vault = new Dictionary<string, Dictionary<string, string>>();
            Vault.Add("vault", _domainsWithPasswords);
        }

        public byte[] Encrypt(Rfc2898DeriveBytes vaultKey)
        {
            string textToEncrypt = JsonSerializer.Serialize(Vault["vault"]);
            byte[] result;

            using (Aes aes = Aes.Create())
            {
                aes.Padding = PaddingMode.PKCS7; // Set padding mode
                aes.Key = vaultKey.GetBytes(16); // Set the key
                aes.IV = IV; // Set the IV

                ICryptoTransform encryptor = aes.CreateEncryptor();
                
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(textToEncrypt);
                        }
                    }

                    result = memoryStream.ToArray();
                }
            }

            return result;
        }

        public byte[] GetEncryptedVault()
        {
            FileHandler fileHandler = new FileHandler();
            string encryptedVault = fileHandler.ReadValueFromJson(this.Path, "vault");
            return Convert.FromBase64String(encryptedVault);
        }

        public void WriteIVAndEncryptedVaultToJSON(byte[] encryptedVaultValue)
        {
            // Gör om byte[] till sträng
            string encryptedVaultValueAsString = Convert.ToBase64String(encryptedVaultValue);
            string IVAsString = Convert.ToBase64String(IV);

            Dictionary<string, string> serverFileKeyValuePairs = new Dictionary<string, string>();
            serverFileKeyValuePairs["vault"] = encryptedVaultValueAsString;
            serverFileKeyValuePairs["IV"] = IVAsString;

            string serverFileAsJsonText = JsonSerializer.Serialize(serverFileKeyValuePairs, new JsonSerializerOptions()
            {
                /* Using UnsafeRelaxedJsonEscaped to fix that the "+" character was getting converted to "\u00-something"
                 * which caused an error when trying to decrypt with the secret key.
                 */
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }
            );

            File.WriteAllText(Path, serverFileAsJsonText);
        }

        public string Decrypt(byte[] encryptedVaultAsBytes, Rfc2898DeriveBytes vaultKey)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Padding = PaddingMode.PKCS7; // Set padding mode
                    aes.Key = vaultKey.GetBytes(16); // Set the key
                    aes.IV = IV; // Set the IV
                    ICryptoTransform decryptor = aes.CreateDecryptor();
                    
                    using (MemoryStream memoryStream = new MemoryStream(encryptedVaultAsBytes))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch 
            {
                Console.WriteLine("Could not decrypt data.");
                return null;
            }
        }
    }
}
