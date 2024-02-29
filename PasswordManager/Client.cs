
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordManager
{
    internal class Client
    {
        private string _path;
        private int _lengthOfKey = 16;
        private string _secretKey;
        public string SecretKey => _secretKey;
        public byte[] RandomBytes;

        public Client(string path)
        {
            // Generate and save secret key unencrypted
            _path = path;

            // Get secret key as byte[]
            RandomBytes = GenerateRandomBytes();

            // Convert secret key from byte[] format to string-format.
            _secretKey = Convert.ToBase64String(RandomBytes);

            // Add secret key to dictionary
            Dictionary<string, string> secretKeys = new Dictionary<string, string>();
            secretKeys.Add("secret", _secretKey);

            // Serialize dictionary to string
            string jsonDict = JsonSerializer.Serialize(secretKeys);
            Console.WriteLine(jsonDict);

            // Saves string-formatted dictionary to JSON file.
            File.WriteAllText(_path, jsonDict); 

            // string text = File.ReadAllText(path);
        }


        private byte[] GenerateRandomBytes()
        {
            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            byte[] randomBytes = new byte[_lengthOfKey];
            generator.GetBytes(randomBytes);
            return randomBytes;
        }

        public Rfc2898DeriveBytes GenerateVaultKey()
        {
            // master password + secret key + Rfc2898DeriveBytes = vault key
            // string secret = GetValueFromJSONFile(args[1], "secret"); // Behövs inte just nu, men kanske sedan i dekryptering?
            Console.WriteLine("Enter your master password: ");
            return new Rfc2898DeriveBytes(Console.ReadLine(), RandomBytes, 10000, HashAlgorithmName.SHA256);
        }

        private static string GetValueFromJSONFile(string pathToFile, string key)
        {
            string fileAsText = File.ReadAllText(pathToFile);
            Dictionary<string, string> KeyValuePairs = JsonSerializer.Deserialize<Dictionary<string, string>>(fileAsText);
            return KeyValuePairs[key];
        }

    }
}
