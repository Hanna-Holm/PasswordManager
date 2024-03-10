
namespace PasswordManager
{
    internal class UserCommunicator
    {
        public string PromptUserFor(string prompt)
        {
            string input = "";

            while (input == "")
            {
                Console.WriteLine($"Enter the {prompt}: ");
                input = Console.ReadLine();
                if (input == "")
                {
                    Console.WriteLine($"The {prompt} cannot be empty.");
                }
            }

            return input;
        }
    }
}
