
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string command = args[0].ToLower();

            switch (command)
            {
                case "init":
                    InitializeVault(args);
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
                default:
                    Console.WriteLine("The command is not valid.");
                    break;
            }
        }

        // "init"-command - the creation and encryption of a vault
        private static void InitializeVault(string[] args)
        {
            FileHandler fileHandler = new FileHandler();
            string clientPath = args[1];
            string serverPath = args[2];

            Client client = new Client(clientPath);
            string secretKey = client.GenerateSecretKey();
            fileHandler.WriteToJson(clientPath, "secret", secretKey);

            Rfc2898DeriveBytes vaultKey = client.DeriveVaultKey();

            Server server = new Server(serverPath);
            server.GenerateIV();
            server.CreateVault();

            byte[] encryptedVaultValuesAsBytes = server.Encrypt(vaultKey);

            server.WriteIVAndEncryptedVaultToJSON(encryptedVaultValuesAsBytes);
        }

        // "create"-command
        private static void CreateNewClientFileToExistingVault(string[] args)
        {
            Client client = new Client(args[1]);
            Server server = new Server(args[2]);

            Console.WriteLine("Enter your secret key: ");
            Rfc2898DeriveBytes vaultKey = client.DeriveVaultKey(Console.ReadLine());

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
