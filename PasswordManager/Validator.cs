
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
    }
}
