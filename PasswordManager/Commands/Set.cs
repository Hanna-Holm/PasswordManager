using System.Text.Json;

namespace PasswordManager.Commands
{
    internal class Set : ICommand
    {
        public void Run(string[] args)
        {
            bool isArgumentLengthValid = new Validator().ValidateArgumentsLength(args, 4);
            if (!isArgumentLengthValid)
            {
                return;
            }

            if (args[3] == "-g" || args[3] == "--generate")
            {
                Console.WriteLine("No username provided.");
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
            if (decryptedAccounts == null)
            {
                return;
            }

            try
            {
                string password;
                string username = args[3];

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

                Console.WriteLine($"Successfully stored password: {password} for {username}");

                server.CreateVault(decryptedUsernamesAndPasswords);
                encryptedAccounts = server.Encrypt(vaultKey);
                File.WriteAllText(serverPath, server.FormatServerToText(encryptedAccounts));
            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }
    }
}
