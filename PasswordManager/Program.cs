
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
                    Initialize(args);
                    break;
                case "create":
                    CreateNewClient(args);
                    break;
                case "get":
                    GetLoginCredentials(args);
                    break;
                case "set":
                    SetPassword(args);
                    break;
                case "delete":
                    DeleteAccount(args);
                    break;
                case "secret":
                    ShowSecretKey(args);
                    break;
                default:
                    Console.WriteLine("The command is not valid.");
                    break;
            }
        }

        private static void Initialize(string[] args)
        {
            bool isArgumentLengthValid = CheckIfArgumentsLengthIsValid(args, 3);
            if (!isArgumentLengthValid)
            {
                return;
            }

            string clientPath = args[1];
            Client client = new Client(clientPath);
            client.Initialize();

            string serverPath = args[2];
            Server server = new Server(serverPath);
            server.Initialize();

            string masterPassword = client.PromptUser("master password");
            byte[] vaultKey = client.GetVaultKey(masterPassword);
            byte[] encryptedAccountsAsBytes = server.Encrypt(vaultKey);
            File.WriteAllText(serverPath, server.FormatServerToText(encryptedAccountsAsBytes));
        }

        private static void CreateNewClient(string[] args)
        {
            bool isArgumentLengthValid = CheckIfArgumentsLengthIsValid(args, 3);
            if (!isArgumentLengthValid)
            {
                return;
            }

            string clientPath = args[1];
            Client client = new Client(clientPath);
            string masterPassword = client.PromptUser("master password");
            string secretKey = client.PromptUser("secret key");
            client.SetSecretKey(secretKey);
            if (client.SecretKeyAsBytes == null)
            {
                return;
            }

            string serverPath = args[2];
            Server server = new Server(serverPath);
            server.SetIV();
            byte[] encryptedAccounts = server.GetEncryptedAccounts();
            byte[] vaultKey = client.GetVaultKey(masterPassword);
            string decryptedAccounts = server.Decrypt(encryptedAccounts, vaultKey);
            if (decryptedAccounts == null)
            {
                return;
            }

            string newClientPath = args[1];
            FileHandler fileHandler = new FileHandler();
            fileHandler.WriteToJson(newClientPath, "secret", secretKey);
            Console.WriteLine($"Successfully logged in to server and created new client: {newClientPath}");
        }

        private static void GetLoginCredentials(string[] args)
        {
            bool isArgumentLengthValid = CheckIfArgumentsLengthIsValid(args, 3);
            if (!isArgumentLengthValid)
            {
                return;
            }

            string clientPath = args[1];
            Client client = new Client(clientPath);
            client.ReadAndSetSecretKey();
            if (client.SecretKeyAsBytes == null)
            {
                return;
            }

            string masterPassword = client.PromptUser("master password");
            byte[] vaultKey = client.GetVaultKey(masterPassword);

            string serverPath = args[2];
            Server server = new Server(serverPath);
            server.SetIV();
            byte[] encryptedAccounts = server.GetEncryptedAccounts();
            string decryptedAccounts = server.Decrypt(encryptedAccounts, vaultKey);
            try
            {
                Dictionary<string, string> vault = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedAccounts);

                if (args.Length == 4)
                {
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

        private static void SetPassword(string[] args)
        {
            bool isArgumentLengthValid = CheckIfArgumentsLengthIsValid(args, 4);
            if (!isArgumentLengthValid)
            {
                return;
            }

            string clientPath = args[1];
            Client client = new Client(clientPath);
            client.ReadAndSetSecretKey();
            if (client.SecretKeyAsBytes == null)
            {
                return;
            }

            string masterPassword = client.PromptUser("master password");
            byte[] vaultKey = client.GetVaultKey(masterPassword);

            string serverPath = args[2];
            Server server = new Server(serverPath);
            server.SetIV();
            byte[] encryptedAccounts = server.GetEncryptedAccounts();
            string decryptedAccounts = server.Decrypt(encryptedAccounts, vaultKey);

            string username = args[3];
            try
            {
                string password;
                if (args.Length == 5 && (args[4] == "-g" || args[4] == "--generate"))
                {
                    password = new PasswordGenerator().Generate();
                }
                else
                {
                    password = client.PromptUser($"password to store for {username}");
                }

                Dictionary<string, string> decryptedUsernamesAndPasswords = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedAccounts);

                if (decryptedUsernamesAndPasswords.ContainsKey(username))
                {
                    decryptedUsernamesAndPasswords[username] = password;
                }
                else
                {
                    decryptedUsernamesAndPasswords.Add(username, password);
                }

                Console.WriteLine($"Successfully stored password: {password} for domain {username}");

                server.Vault = new Dictionary<string, Dictionary<string, string>>
                {
                    { "vault", decryptedUsernamesAndPasswords }
                };

                encryptedAccounts = server.Encrypt(vaultKey);
                File.WriteAllText(serverPath, server.FormatServerToText(encryptedAccounts));
            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }

        private static void DeleteAccount(string[] args)
        {
            bool isArgumentLengthValid = CheckIfArgumentsLengthIsValid(args, 4);
            if (!isArgumentLengthValid)
            {
                return;
            }

            string clientPath = args[1];
            Client client = new Client(clientPath);
            client.ReadAndSetSecretKey();
            if (client.SecretKeyAsBytes == null)
            {
                return;
            }

            string masterPassword = client.PromptUser("master password");
            byte[] vaultKey = client.GetVaultKey(masterPassword);

            string serverPath = args[2];
            Server server = new Server(serverPath);
            server.SetIV();
            byte[] encryptedAccountsAsBytes = server.GetEncryptedAccounts();
            string decryptedAccountsAsText = server.Decrypt(encryptedAccountsAsBytes, vaultKey);
            Dictionary<string, string> decryptedAccounts = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedAccountsAsText);

            string username = args[3];

            if (decryptedAccounts.ContainsKey(username))
            {
                decryptedAccounts.Remove(username);
                Console.WriteLine($"Successfully removed your account {username}");
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
            File.WriteAllText(serverPath, server.FormatServerToText(encryptedAccountsAsBytes));
        }

        private static void ShowSecretKey(string[] args)
        {
            bool isArgumentLengthValid = CheckIfArgumentsLengthIsValid(args, 2);
            if (!isArgumentLengthValid)
            {
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

        private static bool CheckIfArgumentsLengthIsValid(string[] arguments, int minimalLength)
        {
            if (arguments.Length < minimalLength)
            {
                Console.WriteLine("Invalid number of arguments.");
                return false;
            }

            return true;
        }
    }
}
