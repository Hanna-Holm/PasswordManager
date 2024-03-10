using PasswordManager.VaultHandler;
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

            VaultDecryptor.LoginToServer(args[1], args[2], false, false);
            if (VaultDecryptor.DecryptedAccounts == null)
            {
                return;
            }

            try
            {
                Dictionary<string, string> usernamesAndPasswords = JsonSerializer.Deserialize<Dictionary<string, string>>(VaultDecryptor.DecryptedAccounts);

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
