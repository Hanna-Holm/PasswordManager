
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
        public byte[] SecretKeyAsBytes { get; private set; }

        public Client(string path)
        {
            _path = path;
            GenerateSecretKey();
            FormatAndSaveSecretKeyToJSON();

            Console.WriteLine($"The secret key is: {_secretKey}");
        }

        private void GenerateSecretKey()
        {
            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            SecretKeyAsBytes = new byte[_lengthOfKey];
            generator.GetBytes(SecretKeyAsBytes);
        }

        private void FormatAndSaveSecretKeyToJSON()
        {
            // Convert secret key(?) from byte[] to string.
            _secretKey = Convert.ToBase64String(SecretKeyAsBytes);

            Dictionary<string, string> secretKeys = new Dictionary<string, string>();
            secretKeys.Add("secret", _secretKey);

            string jsonDictAsString = JsonSerializer.Serialize(secretKeys);
            File.WriteAllText(_path, jsonDictAsString);
        }

        public Rfc2898DeriveBytes DeriveVaultKey()
        {
            // master password + secret key + Rfc2898DeriveBytes = vault key
            Console.WriteLine("Enter your master password: ");
            return new Rfc2898DeriveBytes(Console.ReadLine(), SecretKeyAsBytes, 10000, HashAlgorithmName.SHA256);
        }

        private static string GetValueFromJSONFile(string pathToFile, string key)
        {
            // string secret = GetValueFromJSONFile(args[1], "secret"); 
            string fileAsText = File.ReadAllText(pathToFile);
            Dictionary<string, string> KeyValuePairs = JsonSerializer.Deserialize<Dictionary<string, string>>(fileAsText);
            return KeyValuePairs[key];
        }
    }
}
