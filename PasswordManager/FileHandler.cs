
using System.Text.Json;

namespace PasswordManager
{
    internal class FileHandler
    {
        public void WriteToJson(string path, string keyText, string valueText)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
            {
                { keyText, valueText }
            };

            string jsonDictAsString = JsonSerializer.Serialize(keyValuePairs);
            File.WriteAllText(path, jsonDictAsString);
        }

        public string ReadValueFromJson(string path, string keyText)
        {
            string fileAsText = File.ReadAllText(path);

            // Deserialize jsontext to Dictionary<string, string>.
            Dictionary<string, string> KeyValuePairs = JsonSerializer.Deserialize<Dictionary<string, string>>(fileAsText);
            return KeyValuePairs[keyText];
        }
    }
}
