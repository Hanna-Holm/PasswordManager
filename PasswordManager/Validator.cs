
namespace PasswordManager
{
    internal class Validator
    {
        public bool ValidateArgumentsLength(string[] arguments, int minimalLength)
        {
            if (arguments.Length < minimalLength)
            {
                Console.WriteLine("Invalid number of arguments.");
                return false;
            }

            return true;
        }

        public bool CheckIfUsernameIsProvided(string[] args)
        {
            try
            {
                if (args[3] == "-g" || args[3] == "--generate")
                {
                    Console.WriteLine("No username provided.");
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
