
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordManager
{
    internal class Server
    {
        private int _lengthOfKey = 28;
        private string _encryptedVault;
        private string _initializationVector;
        private Dictionary<string, string> _vault = new Dictionary<string, string>();

        public Server(string path)
        {
            // Create unencrypted vault
            _vault.Add("vault", "");

            _initializationVector = GenerateInitializationVector();

            // Skapar dictionary med IV som vi serialiserar till jsonsträng
            Dictionary<string, string> IVectors = new Dictionary<string, string>();
            IVectors.Add("IV", _initializationVector);
            string jsonDict = JsonSerializer.Serialize(IVectors);

            // skriver jsonsträngen till server.json
            File.WriteAllText(path, jsonDict);

        }

        private string GenerateInitializationVector()
        {
            // Antingen via RandomNumberGenerator.Create() och få byte[] som vi gör om till sträng
            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            byte[] randomBytes = new byte[_lengthOfKey];
            generator.GetBytes(randomBytes);
            string base64Key = Convert.ToBase64String(randomBytes);
            return base64Key;

            // Eller genom att skapa ett Aes-objekt för att generrera IV

        }
    }
}
