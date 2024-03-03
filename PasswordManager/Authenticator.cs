
using System.Security.Cryptography;

namespace PasswordManager
{
    internal class Authenticator
    {
        FileHandler fileHandler = new FileHandler();

        public bool TryAuthenticateClient(Rfc2898DeriveBytes vaultKey, string serverPath)
        {
            // Get IV and vault from server.json, convert from string to byte[] format
            string IV = fileHandler.ReadValueFromJson(serverPath, "IV");
            string encryptedVault = fileHandler.ReadValueFromJson(serverPath, "vault");
            byte[] IVAsBytes = Convert.FromBase64String(IV);
            byte[] encryptedVaultAsBytes = Convert.FromBase64String(encryptedVault);

            Server server = new Server(serverPath);
            server.IV = IVAsBytes;

            // Try to Decrypt vault with the obtained IV and vaultKey.
            string decryptedVault = server.Decrypt(encryptedVaultAsBytes, vaultKey);

            if (decryptedVault == null)
            {
                Console.WriteLine("Could not authenticate client.");
                return false;
            }
            return true;
        }
    }
}
