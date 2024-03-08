using System.Text.Json;

namespace PasswordManager.Commands
{
    internal class Get : ICommand
    {
        public void Run(string[] args)
        {
            bool isArgumentLengthValid = new Validator().ValidateArgumentsLength(args, 3);
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
            if (decryptedAccounts == null)
            {
                return;
            }

            try
            {
                Dictionary<string, string> usernamesAndPasswords = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedAccounts);

                if (args.Length == 4)
                {
                    string username = args[3];
                    string requestedPassword = usernamesAndPasswords[username];
                    Console.WriteLine("The requested password is: ");
                    // The password needs to be in a separate console out for the tests to pass!
                    Console.WriteLine(requestedPassword);
                }
                else
                {
                    PrintKeys(usernamesAndPasswords);
                }
            }
            catch
            {
                Console.WriteLine("");
            }
        }

        private static void PrintKeys(Dictionary<string, string> keyValuePairs)
        {
            foreach (var pair in keyValuePairs)
            {
                Console.WriteLine(pair.Key);
            }
        }
    }
}
