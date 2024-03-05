
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

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
                    ShowPropertyValueInVault(args);
                    break;
                case "set":
                    StorePropertyValueInVault(args);
                    break;
                case "delete":
                    DeletePropertyFromVault(args);
                    break;
                case "secret":
                    ShowSecretKey(args);
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
                Console.WriteLine("Something went wrong.");
            }

            Rfc2898DeriveBytes vaultKey = new Rfc2898DeriveBytes(masterPassword, SecretKeyAsBytes, 10000, HashAlgorithmName.SHA256);

            string serverPath = args[2];
            Server server = new Server(serverPath);
            server.SetIV();

            //bool couldAuthenticate = authenticator.TryAuthenticateClient(vaultKey, server);
            byte[] encryptedVault = server.GetEncryptedVault();
            string decryptedVault = server.Decrypt(encryptedVault, vaultKey);

            if (decryptedVault == null)
            {
                Console.WriteLine("Could not authenticate client.");
                return;
            }

            string newClientPath = args[1];
            fileHandler.WriteToJson(newClientPath, "secret", secretKey);
            Console.WriteLine($"Successfully logged in to server and created new client: {newClientPath}");

        }

        // "get"-command
        private static void ShowPropertyValueInVault(string[] args)
        {
            string clientPath = args[1];
            string serverPath = args[2];

            Client client = new Client(clientPath);
            client.SetSecretKey();
            Rfc2898DeriveBytes vaultKey = client.DeriveVaultKey();

            Server server = new Server(serverPath);
            server.SetIV();
            byte[] encryptedVault = server.GetEncryptedVault();
            string decryptedVault = server.Decrypt(encryptedVault, vaultKey);
            try
            {
                Dictionary<string, string> vault = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

                if (args.Length == 4)
                {
                    // args[3] är property (domänen) för vilken vi vill hämta ut specifikt lösenord
                    string propertyRequest = args[3];
                    string requestedPassword = vault[propertyRequest];
                    Console.WriteLine($"The requested password is: {requestedPassword}");
                }
                else
                {
                    foreach (var v in vault)
                    {
                        Console.WriteLine(v.Key);
                    }
                }
            }
            catch
            {
                Console.WriteLine("");
            }
        }

        // "set"-command: save domain with password in server.json "vault"
        private static void StorePropertyValueInVault(string[] args)
        {
            if (args.Length <= 3)
            {
                Console.WriteLine("Invalid number of arguments.");
                return;
            }

            string clientPath = args[1];
            string serverPath = args[2];

            Client client = new Client(clientPath);
            client.SetSecretKey();
            Rfc2898DeriveBytes vaultKey = client.DeriveVaultKey();

            Server server = new Server(serverPath);
            server.SetIV();

            string serverFileAsText = File.ReadAllText(serverPath);
            Dictionary<string, string> KeyValuePairs = JsonSerializer.Deserialize<Dictionary<string, string>>(serverFileAsText);
            string encryptedVault = KeyValuePairs["vault"];
            Console.WriteLine("GetEncryptedVault returned: " + encryptedVault);
            byte[] encryptedVaultAsBytes = Convert.FromBase64String(encryptedVault);

            try
            {
                string decryptedVault = server.Decrypt(encryptedVaultAsBytes, vaultKey);

                string key = args[3];
                string password = "";

                if (args.Length == 5 && (args[4] == "-g" || args[4] == "--generate"))
                {
                    password = new PasswordGenerator().Generate();
                }
                else
                {
                    Console.WriteLine($"What password would you like to store for {key}?");
                    password = Console.ReadLine();
                }

                Dictionary<string, string> decryptedVaultKeyValuePairs = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

                if (decryptedVaultKeyValuePairs.ContainsKey(key))
                {
                    decryptedVaultKeyValuePairs[key] = password;
                }
                else
                {
                    decryptedVaultKeyValuePairs.Add(key, password);
                }

                Console.WriteLine($"Successfully stored password: {password} for domain {key}");

                server.Vault = new Dictionary<string, Dictionary<string, string>>();
                server.Vault.Add("vault", decryptedVaultKeyValuePairs);

                byte[] encryptedVaultValuesAsBytes;
                encryptedVaultValuesAsBytes = server.Encrypt(vaultKey);

                server.WriteIVAndEncryptedVaultToJSON(encryptedVaultValuesAsBytes);
            }
            catch
            {

            }
        }

        // "delete"-command
        private static void DeletePropertyFromVault(string[] args)
        {

        }

        // "secret"-command
        private static void ShowSecretKey(string[] args)
        {

        }
    }
}
