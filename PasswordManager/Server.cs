
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordManager
{
    internal class Server
    {
        private string _path;
        private int _lengthOfKey = 16;
        public byte[] InitializationVector { get; }
        public Dictionary<string, string> Vault = new Dictionary<string, string>();

        public Server(string path)
        {
            _path = path;

            // Create unencrypted vault, ska krypteras och sparas till server.json genom att använda Aes-objektet.
            Vault.Add("vault", "");

            InitializationVector = GenerateInitializationVector();

            // Convert IV from byte[] format to string-format.
            string IvAsString = Convert.ToBase64String(InitializationVector);

            // Skapar dictionary med IV som vi serialiserar till jsonsträng
            Dictionary<string, string> IVectors = new Dictionary<string, string>();
            IVectors.Add("IV", IvAsString);
            string jsonDictAsString = JsonSerializer.Serialize(IVectors);

            // skriver jsonsträngen till server.json
            File.WriteAllText(path, jsonDictAsString);
        }


        private byte[] GenerateInitializationVector()
        {
            // Antingen via RandomNumberGenerator.Create() och få byte[] som vi gör om till sträng
            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            byte[] randomBytes = new byte[_lengthOfKey];
            generator.GetBytes(randomBytes);
            return randomBytes;

            // Eller genom att skapa ett Aes-objekt för att generera IV

        }

        public string Encrypt(Rfc2898DeriveBytes vaultKey)
        {
            // Get vault as jsonstring -> encrypt it
            string vaultAsJsonString = JsonSerializer.Serialize(Vault);

            // IV + Vault key i Aes-objekt för att kryptera Vault!
            byte[] encryptedVaultAsBytes = EncryptJsonString(vaultAsJsonString, vaultKey.GetBytes(16), InitializationVector);
            return Convert.ToBase64String(encryptedVaultAsBytes);
        }

        public void WriteEncryptedVaultToJSON(string encryptedVaultAsText)
        {
            File.WriteAllText(_path, encryptedVaultAsText);
        }

        private static byte[] EncryptJsonString(string vaultAsJsonString, byte[] vaultKey, byte[] iv)
        {
            // Create Aes object (vault key + IV)
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(vaultKey, iv);

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
