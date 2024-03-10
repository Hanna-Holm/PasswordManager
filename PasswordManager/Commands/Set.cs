using PasswordManager.VaultHandler;
using System.Text.Json;

namespace PasswordManager.Commands
{
    internal class Set : ICommand
    {
        private UserCommunicator _communicator = new UserCommunicator();
        private string _username;

        public void Run(string[] args)
        {
            Validator validator = new Validator();

            bool isArgumentLengthValid = validator.ValidateArgumentsLength(args, 4);
            bool isUsernameProvided = validator.CheckIfUsernameIsProvided(args);
            if (!isArgumentLengthValid || !isUsernameProvided)
                return;

            VaultDecryptor.LoginToServer(args[1], args[2], CommandType.Update);
            if (VaultDecryptor.DecryptedAccounts == null)
                return;

            try
            {
                _username = args[3];
                string password = SetPassword(args);
                
                Dictionary<string, string> decryptedUsernamesAndPasswords = JsonSerializer.Deserialize<Dictionary<string, string>>(VaultDecryptor.DecryptedAccounts);

                if (!decryptedUsernamesAndPasswords.TryAdd(_username, password))
                    decryptedUsernamesAndPasswords[_username] = password;

                Console.WriteLine($"Successfully stored password: {password} for {_username}");
                string accounts = JsonSerializer.Serialize(decryptedUsernamesAndPasswords);
                VaultEncryptor.LockVault(accounts, VaultDecryptor.ServerInstance);
            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }

        private string SetPassword(string[] args)
        {
            if (args.Length == 5 && (args[4] == "-g" || args[4] == "--generate"))
                return new PasswordGenerator().Generate();
            else
                return _communicator.PromptUserFor($"password to store for {_username}");
        }
    }
}
