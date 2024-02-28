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
                    // Encrypt server vault using master password
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
            Client client = new Client(args[1]);
            Server server = new Server(args[2]);

            // Vault key används i Aes för att kryptera Vault.
            Rfc2898DeriveBytes vaultKey = client.GenerateVaultKey();

            // Get vault as jsonstring -> encrypt it
            string encryptedVaultAsString = server.Encrypt(vaultKey);

            // Skriv till server.json
            server.WriteEncryptedVaultToJSON(encryptedVaultAsString);
        }

        private static void GenerateVaultKey()
        {
            
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
