
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordManager
{
    internal class Server
    {
        private string _path;
        private int _lengthOfKey = 16;
        public byte[] IV { get; private set; }
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
            Vault.Add("vault", value);
        }

        public byte[] Encrypt(Rfc2898DeriveBytes vaultKey)
        {
            // Vault key + IV
            string textToEncrypt = JsonSerializer.Serialize(Vault["vault"]);

            using (Aes aes = Aes.Create())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(vaultKey.GetBytes(16), IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(textToEncrypt);
                        }
                    }

                    return msEncrypt.ToArray();
                }
            }
        }

        public void WriteIVAndEncryptedVaultToJSON(byte[] encryptedVaultValue)
        {
            // Gör om encryptedVault (domainspasswords) byte[] till sträng
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
            string simpletext = String.Empty;

            using (Aes aes = Aes.Create())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(vaultKey.GetBytes(16), IV);
                using (MemoryStream memoryStream = new MemoryStream(encryptedVaultAsBytes))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            simpletext = streamReader.ReadToEnd();
                        }
                    }
                }
            }

            return simpletext;
        }
    }
}
