
using System.Security.Cryptography;

namespace PasswordManager
{
    internal class Authenticator
    {
        FileHandler fileHandler = new FileHandler();

        public bool TryAuthenticateClient(Rfc2898DeriveBytes vaultKey, Server server)
        {
            byte[] encryptedVault = server.GetEncryptedVault();

            // Try to Decrypt vault with the obtained IV and vaultKey.
            string decryptedVault = server.Decrypt(encryptedVault, vaultKey);
            Console.WriteLine(decryptedVault);

            if (decryptedVault == null)
            {
                Console.WriteLine("Could not authenticate client.");
                return false;
            }
            return true;
        }
    }
}
