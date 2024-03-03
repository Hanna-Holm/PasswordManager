
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager
{
    public class Program
    {
        public static void Main(string[] args)
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

            Server server = new Server(serverPath);
            server.GenerateIV();
            server.CreateVault();

            Rfc2898DeriveBytes vaultKey = client.DeriveVaultKey();
            byte[] encryptedVaultValuesAsBytes = server.Encrypt(vaultKey);
            server.WriteIVAndEncryptedVaultToJSON(encryptedVaultValuesAsBytes);
        }

        // "create" command: creates a new client-file to an existing server
        private static void CreateNewClientFileToExistingVault(string[] args)
        {
            FileHandler fileHandler = new FileHandler();
            Authenticator authenticator = new Authenticator();

            // NOTE! The master password NEEDS to be prompted first in order for the tests to pass!!
            Console.WriteLine("Enter your master password: ");
            string masterPassword = Console.ReadLine();
            Console.WriteLine("Enter your secret key: ");
            string secretKey = Console.ReadLine();
            byte[] SecretKeyAsBytes = new byte[16];

            try
            {
                SecretKeyAsBytes = Convert.FromBase64String(secretKey);
            }
            catch
            {
                Console.WriteLine("Invalid secret key.");
            }

            Rfc2898DeriveBytes vaultKey = new Rfc2898DeriveBytes(masterPassword, SecretKeyAsBytes, 10000, HashAlgorithmName.SHA256);

            string serverPath = args[2];
            bool couldAuthenticate = authenticator.TryAuthenticateClient(vaultKey, serverPath);
            if (couldAuthenticate)
            {
                string newClientPath = args[1];
                fileHandler.WriteToJson(newClientPath, "secret", secretKey);
                Console.WriteLine($"Successfully logged in to server and created new client: {newClientPath}");
            }
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
