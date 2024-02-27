using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordManager
{
    internal class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "init":
                    CreateNewVault(args);
                    break;
                case "create":
                    CreateNewClientFileToExistingVault();
                    break;
                case "get":
                    ShowPropertyValueInVault();
                    break;
                case "set":
                    StorePropertyValueInVault();
                    break;
                case "delete":
                    DeletePropertyFromVault();
                    break;
                case "secret":
                    ShowSecretKey();
                    break;
            }
        }

        private static void CreateNewVault(string[] args)
        {
            /* 
             * encrypt your vault stored in < server > using <pwd >.
            */

            Client client = new Client(args[1]);
            Server server = new Server(args[2]);

            Dictionary<string, string> jsonFromFile = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(args[1]));

            string key = jsonFromFile["secret"];
            Console.WriteLine("Enter your master password: ");
            Rfc2898DeriveBytes vaultKey = new Rfc2898DeriveBytes(Console.ReadLine(), client.RandomBytes, 10000, HashAlgorithmName.SHA256);

            Console.WriteLine(vaultKey);
            // TODO:
            // Get secret key from client
            // Get IV from server
            // Create unencrypted vault i server
            // Create vault key (secret key + master pw)
            // Create Aes object (vault key + IV)
            // Encrypt vault by using the Aes object encryption method
            // After implementing init: implement decryption

        }

        private static void CreateNewClientFileToExistingVault()
        {

        }

        private static void ShowPropertyValueInVault()
        {

        }
        private static void StorePropertyValueInVault()
        {

        }
        private static void DeletePropertyFromVault()
        {

        }
        private static void ShowSecretKey()
        {

        }
    }
}
