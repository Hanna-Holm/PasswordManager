using System.Text.Json;

namespace PasswordManager
{
    internal class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "init":
                    CreateNewVault(args);
                    break;
                case "create":
                    CreateNewClientFileToExistingVault();
                    break;
                case "get":
                    ShowPropertyValueInVault();
                    break;
                case "set":
                    StorePropertyValueInVault();
                    break;
                case "delete":
                    DeletePropertyFromVault();
                    break;
                case "secret":
                    ShowSecretKey();
                    break;
            }

            Employee emp1 = new Employee(30000, "Kalle Kallesson");
            //Serialisera till JSON:
            string jsonstring = JsonSerializer.Serialize(emp1);
            // Console.WriteLine(jsonstring); // skriver ut hela JSON-texten i JSON-format.

            // Man kan även lägga in flera värden i jsonformat
            List<Employee> employees = new List<Employee>()
            {
                new Employee(35000, "Monikaaa"),
                new Employee(45044, "Riken")
            };
            string jsonemployees = JsonSerializer.Serialize(employees);
            // Console.WriteLine(jsonemployees);  //Skriver ut hela listan

            // Deserialisering
            Employee e3 = JsonSerializer.Deserialize<Employee>(jsonstring);

            // Dictionaries bra att lagra key value pairs, passar bra ihop med JSON.
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("Sverige","Uppsala");
            // Console.WriteLine(dict["Sverige"]); // Får ut värdet på key Sverige

            string jsonDict = JsonSerializer.Serialize(dict);
            // Kan enkelt arbeta med json serialisering/deserialisering med dictionary 

            // Write to file:
            // System.IO statisk klass File -> enkelt lagra fil

            

            // Läsa från fil:
            // metoden ReadAllText(path)
            // string text = File.ReadAllText(path);
            // Lagrar filens innehåll i variabeln text!

            // Dictionary<string, string> newDict = JsonSerializer.Deserialize<Dictionary<string, string>>(text);


        }

        private static void CreateNewVault(string[] args)
        {
            /* 
             * encrypt your vault stored in < server > using <pwd >.
            */

            Client client = new Client(args[1]);
            Server server = new Server(args[2]);

            // Get IV from server
            // Get secret key from client
            // Create unencrypted vault i server
            // Create vault key (secret key + master pw)
            // Create Aes object (vault key + IV)
            // Encrypt vault by using the Aes object encryption method

        }

        private static void CreateNewClientFileToExistingVault()
        {

        }

        private static void ShowPropertyValueInVault()
        {

        }
        private static void StorePropertyValueInVault()
        {

        }
        private static void DeletePropertyFromVault()
        {

        }
        private static void ShowSecretKey()
        {

        }

        // Efter init så ska man implemetera dekryptering.
    }
}
