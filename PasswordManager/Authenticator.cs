
using System.Security.Cryptography;

namespace PasswordManager
{
    internal class Authenticator
    {
        public bool TryAuthenticateClient(Rfc2898DeriveBytes vaultKey, Server server)
        {
            byte[] encryptedVault = server.GetEncryptedVault();
            string decryptedVault = server.Decrypt(encryptedVault, vaultKey);

            if (decryptedVault == null)
            {
                Console.WriteLine("Could not authenticate client.");
                return false;
            }
            return true;
        }
    }
}
