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
                    // The creation and encryption of a vault
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

            Rfc2898DeriveBytes vaultKey = client.DeriveVaultKey();
            server.EncryptVault(vaultKey);
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
