
using PasswordManager.Commands;

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
                    new Init().Run(args);
                    break;
                case "create":
                    new Create().Run(args);
                    break;
                case "get":
                    new Get().Run(args);
                    break;
                case "set":
                    new Set().Run(args);
                    break;
                case "delete":
                    new Delete().Run(args);
                    break;
                case "secret":
                    new Secret().Run(args);
                    break;
                default:
                    Console.WriteLine("The command is not valid.");
                    break;
            }
        }
    }
}
