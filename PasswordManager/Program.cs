
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("A command is needed to run this app.");
                return;
            }

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
            if (args.Length < 3)
            {
                Console.WriteLine("Invalid number of arguments.");
                return;
            }

            string clientPath = args[1];
            Client client = new Client(clientPath);
            string secretKey = client.GenerateSecretKey();
            FileHandler fileHandler = new FileHandler();
            fileHandler.WriteToJson(clientPath, "secret", secretKey);

            string serverPath = args[2];
            Server server = new Server(serverPath);
            server.GenerateIV();
            server.CreateVault();

            Console.WriteLine("Enter your master password: ");
            Rfc2898DeriveBytes authentication = client.Authenticate(Console.ReadLine());
            byte[] vaultKey = authentication.GetBytes(16);
            byte[] encryptedVaultValuesAsBytes = server.Encrypt(vaultKey);
            File.WriteAllText(serverPath, server.FormatVaultToText(encryptedVaultValuesAsBytes));
        }

        // "create" command: creates a new client-file to an existing server
        private static void CreateNewClientFileToExistingVault(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Invalid number of arguments.");
                return;
            }

            FileHandler fileHandler = new FileHandler();

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

            Rfc2898DeriveBytes authentication = new Rfc2898DeriveBytes(masterPassword, SecretKeyAsBytes, 10000, HashAlgorithmName.SHA256);
            byte[] vaultKey = authentication.GetBytes(16);

            string serverPath = args[2];
            Server server = new Server(serverPath);
            server.SetIV();

            //bool couldAuthenticate = authenticator.TryAuthenticateClient(vaultKey, server);
            byte[] encryptedVault = server.GetEncryptedAccounts();
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
            if (args.Length < 3)
            {
                Console.WriteLine("Invalid number of arguments.");
                return;
            }

            string clientPath = args[1];
            Client client = new Client(clientPath);
            FileHandler fileHandler = new FileHandler();
            string secretKeyAsString = fileHandler.ReadValueFromJson(clientPath, "secret");
            client.SetSecretKey(secretKeyAsString);
            Console.WriteLine("Enter your master password: ");
            Rfc2898DeriveBytes authentication = client.Authenticate(Console.ReadLine());
            byte[] vaultKey = authentication.GetBytes(16);

            string serverPath = args[2];
            Server server = new Server(serverPath);
            server.SetIV();
            byte[] encryptedVault = server.GetEncryptedAccounts();
            string decryptedVault = server.Decrypt(encryptedVault, vaultKey);
            try
            {
                Dictionary<string, string> vault = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

                if (args.Length == 4)
                {
                    // args[3] är property (domänen) för vilken vi vill hämta ut specifikt lösenord
                    string propertyRequest = args[3];
                    string requestedPassword = vault[propertyRequest];
                    Console.WriteLine("The requested password is: ");
                    // The password needs to be in a separate console out for the tests to pass!
                    Console.WriteLine(requestedPassword);
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
            if (args.Length < 4)
            {
                Console.WriteLine("Invalid number of arguments.");
                return;
            }

            string clientPath = args[1];
            string serverPath = args[2];

            Client client = new Client(clientPath);
            FileHandler fileHandler = new FileHandler();
            string secretKeyAsText = fileHandler.ReadValueFromJson(clientPath, "secret");
            client.SetSecretKey(secretKeyAsText);

            Console.WriteLine("Enter your master password: ");
            Rfc2898DeriveBytes authentication = client.Authenticate(Console.ReadLine());
            byte[] vaultKey = authentication.GetBytes(16);

            Server server = new Server(serverPath);
            server.SetIV();

            string encryptedVaultAsText = fileHandler.ReadValueFromJson(serverPath, "vault");
            byte[] encryptedVaultAsBytes = Convert.FromBase64String(encryptedVaultAsText);
            string decryptedVault = server.Decrypt(encryptedVaultAsBytes, vaultKey);

            string key = args[3];
            string password = "";

            try
            {
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

                server.Vault = new Dictionary<string, Dictionary<string, string>>
                {
                    { "vault", decryptedVaultKeyValuePairs }
                };

                byte[] encryptedVaultValuesAsBytes;
                encryptedVaultValuesAsBytes = server.Encrypt(vaultKey);

                File.WriteAllText(serverPath, server.FormatVaultToText(encryptedVaultValuesAsBytes));
            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }

        // "delete"-command: remove account from vault
        private static void DeletePropertyFromVault(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Invalid number of arguments.");
                return;
            }

            string clientPath = args[1];
            string serverPath = args[2];

            Client client = new Client(clientPath);
            FileHandler fileHandler = new FileHandler();
            string secretKeyAsText = fileHandler.ReadValueFromJson(clientPath, "secret");
            client.SetSecretKey(secretKeyAsText);

            Console.WriteLine("Enter your master password: ");
            Rfc2898DeriveBytes authentication = client.Authenticate(Console.ReadLine());
            byte[] vaultKey = authentication.GetBytes(16);

            Server server = new Server(serverPath);
            server.SetIV();

            string encryptedAccountsAsText = fileHandler.ReadValueFromJson(serverPath, "vault");
            byte[] encryptedAccountsAsBytes = Convert.FromBase64String(encryptedAccountsAsText);
            string decryptedAccountsAsText = server.Decrypt(encryptedAccountsAsBytes, vaultKey);

            string key = args[3];

            Dictionary<string, string> decryptedAccounts = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedAccountsAsText);

            if (decryptedAccounts.ContainsKey(key))
            {
                decryptedAccounts.Remove(key);
                Console.WriteLine($"Successfully removed your account {key}");

            }
            else
            {
                Console.WriteLine("Account does not exist");
                return;
            }

            server.Vault = new Dictionary<string, Dictionary<string, string>>
            {
                { "vault", decryptedAccounts }
            };

            encryptedAccountsAsBytes = server.Encrypt(vaultKey);

            File.WriteAllText(serverPath, server.FormatVaultToText(encryptedAccountsAsBytes));

        }

        // "secret"-command
        private static void ShowSecretKey(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid number of arguments.");
                return;
            }

            string clientPath = args[1];

            if (File.Exists(clientPath))
            {
                FileHandler fileHandler = new FileHandler();
                
                Console.WriteLine(fileHandler.ReadValueFromJson(clientPath, "secret"));
                return;
            }

            Console.WriteLine("File does not exist");
        }
    }
}
