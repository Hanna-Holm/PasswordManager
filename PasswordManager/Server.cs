
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordManager
{
    internal class Server
    {
        private string _path;
        private int _lengthOfKey = 16;
        public byte[] InitializationVector { get; private set; }
        public Dictionary<string, string> Vault { get; private set; }

        public Server(string path)
        {
            _path = path;

            GenerateInitializationVector();
            // FormatAndSaveIVToJSON();

            CreateVault();
        }

        private void GenerateInitializationVector()
        {
            // Antingen via RandomNumberGenerator:
            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            InitializationVector = new byte[_lengthOfKey];
            generator.GetBytes(InitializationVector);

            // Eller genom att skapa ett Aes-objekt för att generera IV

        }

        private void FormatAndSaveIVToJSON()
        {
            // Convert IV from byte[] to string.
            string IVAsString = Convert.ToBase64String(InitializationVector);

            Console.WriteLine("The IV is: " + IVAsString);

            Dictionary<string, string> IVectors = new Dictionary<string, string>();
            IVectors.Add("IV", IVAsString);

            string jsonDictAsString = JsonSerializer.Serialize(IVectors);
            File.WriteAllText(_path, jsonDictAsString);
        }

        private void CreateVault()
        {
            Vault = new Dictionary<string, string>();
        }

        public void SaveEncryptedVaultToJSON(byte[] encryptedVault)
        {
            // Get vault as jsonstring
            string encryptedVaultAsString = Convert.ToBase64String(encryptedVault);

            // Skriv till server.json
            File.WriteAllText(_path, encryptedVaultAsString);
        }

        public byte[] Encrypt(Rfc2898DeriveBytes vaultKey)
        {
            // IV + Vault key (i Aes-objekt) för att kryptera Vault!
            string vaultAsJsonString = JsonSerializer.Serialize(Vault);

            using (Aes aes = Aes.Create())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(vaultKey.GetBytes(16), InitializationVector);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(vaultAsJsonString);
                        }
                    }

                    return msEncrypt.ToArray();
                }
            }
        }
    }
}
