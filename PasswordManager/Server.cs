
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordManager
{
    internal class Server
    {
        private string _path;
        private int _lengthOfKey = 16;
        public byte[] IV { get; set; }
        public Dictionary<string, string> Vault { get; private set; }
        private Dictionary<string, string> _domainsWithPasswords;

        public Server(string path)
        {
            _path = path;
        }

        public void GenerateIV()
        {
            // Antingen via RandomNumberGenerator:
            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            IV = new byte[_lengthOfKey];
            generator.GetBytes(IV);

            // Eller genom att skapa ett Aes-objekt för att generera IV

        }

        public void CreateVault()
        {
            _domainsWithPasswords = new Dictionary<string, string>();

            Vault = new Dictionary<string, string>();
            string value = JsonSerializer.Serialize(_domainsWithPasswords);
            Vault.Add("vault", "goooogle");
        }

        public byte[] Encrypt(Rfc2898DeriveBytes vaultKey)
        {
            // Vault key + IV
            string textToEncrypt = JsonSerializer.Serialize(Vault["vault"]);
            byte[] result;

            using (Aes aes = Aes.Create())
            {
                aes.Key = vaultKey.GetBytes(aes.KeySize / 8); // Use the full derived key
                aes.IV = IV;
                //ICryptoTransform encryptor = aes.CreateEncryptor(vaultKey.GetBytes(aes.KeySize / 8), IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
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

        public void WriteIVAndEncryptedVaultToJSON(byte[] encryptedVaultValue)
        {
            // Gör om byte[] till sträng
            string encryptedVaultValueAsString = Convert.ToBase64String(encryptedVaultValue);
            string IVAsString = Convert.ToBase64String(IV);

            Dictionary<string, string> serverFileKeyValuePairs = new Dictionary<string, string>();
            serverFileKeyValuePairs["vault"] = encryptedVaultValueAsString;
            serverFileKeyValuePairs["IV"] = IVAsString;

            string serverFileAsJsonText = JsonSerializer.Serialize(serverFileKeyValuePairs);
            File.WriteAllText(_path, serverFileAsJsonText);
        }

        public string Decrypt(byte[] encryptedVaultAsBytes, Rfc2898DeriveBytes vaultKey)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = vaultKey.GetBytes(aes.KeySize / 8); // Use the full derived key
                    aes.IV = IV;
                    //ICryptoTransform decryptor = aes.CreateDecryptor(vaultKey.GetBytes(aes.KeySize / 8), IV);
                    using (MemoryStream memoryStream = new MemoryStream(encryptedVaultAsBytes))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                Console.WriteLine(streamReader.ReadToEnd());
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
