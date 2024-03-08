namespace PasswordManager.Commands
{
    internal class Secret : ICommand
    {
        private FileHandler _fileHandler = new FileHandler();

        public void Run(string[] args)
        {
            bool isArgumentLengthValid = new Validator().ValidateArgumentsLength(args, 2);
            if (!isArgumentLengthValid)
            {
                return;
            }

            string clientPath = args[1];

            if (File.Exists(clientPath))
            {
                Console.WriteLine(_fileHandler.ReadValueFromJson(clientPath, "secret"));
                return;
            }

            Console.WriteLine("File does not exist");
        }
    }
}
