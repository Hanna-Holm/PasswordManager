
using System.Security.Cryptography;

namespace PasswordManager
{
    internal class Client
    {
        private int _lengthOfKey = 28;
        private string _secretKey;
        public string SecretKey => _secretKey;
        private string _path;

        public Client(string path)
        {
            _secretKey = GenerateSecretKey();

            // _path = @path; // Hamnar i bin-foldern! några steg in
            // I projektet behöver vi inte tänka på att inte skriva över, för vi vill skriva över!
            File.WriteAllText(path, _secretKey); // (path, jsonDict) i projektarbetet, skapar fil om inte finns, annars skriver den över

            // string text = File.ReadAllText(path);

        }

        private string GenerateSecretKey()
        {
            byte[] randomBytes = new byte[_lengthOfKey];

            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            generator.GetBytes(randomBytes);
            Console.WriteLine("Here is the random bytes array:");
            Console.WriteLine(randomBytes);
            return randomBytes.ToString();
        }
    }
}
