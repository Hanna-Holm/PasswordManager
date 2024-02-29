
using System.Security.Cryptography;

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

        // "init"-command
        private static void CreateNewVault(string[] args)
        {
            Client client = new Client(args[1]);
            Server server = new Server(args[2]);

            Rfc2898DeriveBytes vaultKey = client.DeriveVaultKey();
            server.EncryptVault(vaultKey);
        }

        // "create"-command
        private static void CreateNewClientFileToExistingVault()
        {

        }

        // "get"-command
        private static void ShowPropertyValueInVault()
        {

        }

        // "set"-command
        private static void StorePropertyValueInVault()
        {

        }

        // "delete"-command
        private static void DeletePropertyFromVault()
        {

        }

        // "secret"-command
        private static void ShowSecretKey()
        {

        }
    }
}
