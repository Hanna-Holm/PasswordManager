
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordManager
{
    internal class Server
    {
        private int _lengthOfKey = 16;
        public byte[] InitializationVector;
        public Dictionary<string, string> Vault = new Dictionary<string, string>();

        public Server(string path)
        {
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

        public void Encrypt()
        {
        }
    }
}
