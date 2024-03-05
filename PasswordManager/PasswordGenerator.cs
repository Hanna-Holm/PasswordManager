
using System.Text;

namespace PasswordManager
{
    internal class PasswordGenerator
    {
        // Generate a random alphanumeric string with 20 characters that matches the regex /[a-z, A-Z, 0-9] {20}/
        private readonly int _length = 20;
        private readonly string _availableCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public string Generate()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _length; i++)
            {
                Random index = new Random();
                sb.Append(_availableCharacters[index.Next(0, _availableCharacters.Length)]);
            }

            return sb.ToString();
        }
    }
}
