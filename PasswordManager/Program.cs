
using System.Security.Cryptography;
using System.Text;

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
                    CreateNewClientFileToExistingVault(args);
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
            client.GenerateSecretKey();
            client.FormatAndSaveSecretKeyToJSON();

            Server server = new Server(args[2]);
            server.GenerateInitializationVector();
            // server.FormatAndSaveIVToJSON();

            server.CreateVault();

            Rfc2898DeriveBytes vaultKey = client.DeriveVaultKey();
            byte[] encryptedVault = server.Encrypt(vaultKey);
            server.SaveEncryptedVaultToJSON(encryptedVault);
        }

        // "create"-command
        private static void CreateNewClientFileToExistingVault(string[] args)
        {
            Client client = new Client(args[1]);
            Server server = new Server(args[2]);

            Console.WriteLine("Enter your secret key: ");
            string secretKey = Console.ReadLine();
            Rfc2898DeriveBytes vaultKey = client.DeriveVaultKey(secretKey);

            string encryptedVaultAsText = File.ReadAllText(args[2]);
            byte[] encryptedVaultAsBytes = Encoding.UTF8.GetBytes(encryptedVaultAsText);
            string decryptedVault = server.Decrypt(encryptedVaultAsBytes, vaultKey);
            Console.WriteLine(decryptedVault);

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
