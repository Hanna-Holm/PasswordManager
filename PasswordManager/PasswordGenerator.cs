
using System.Text;

namespace PasswordManager
{
    internal class PasswordGenerator
    {
        private readonly int _length = 20;
        private readonly string _availableCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public string Generate()
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < _length; i++)
            {
                int index = random.Next(0, _availableCharacters.Length);
                sb.Append(_availableCharacters[index]);
            }

            return sb.ToString();
        }
    }
}
