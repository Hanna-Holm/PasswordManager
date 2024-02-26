
namespace PasswordManager
{
    internal class Server
    {
        private string _path;
        private string EncryptedVault;
        private string InitializationVector;
        private Dictionary<string, string> _vault = new Dictionary<string, string>();

        public Server(string path)
        {
            // _path = path;
            _vault.Add("vault", "");
            GenerateInitializationVector();

        }

        private void GenerateInitializationVector()
        {
            // throw new NotImplementedException();
        }
    }
}
