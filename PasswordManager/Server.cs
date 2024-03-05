
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace PasswordManager
{
    internal class Server
    {
        public string Path;
        private int _lengthOfKey = 16;
        public byte[] IV { get; set; }
        public Dictionary<string, Dictionary<string, string>> Vault;
        private Dictionary<string, string> _domainsWithPasswords;

        public Server(string path)
        {
            Path = path;
        }

        public void GenerateIV()
        {
            // Antingen via RandomNumberGenerator:
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                IV = new byte[_lengthOfKey];
                generator.GetBytes(IV);
            }

            // Eller genom att skapa ett Aes-objekt för att generera IV

        }

        public void SetIV()
        {
            FileHandler fileHandler = new FileHandler();
            string IVAsString = fileHandler.ReadValueFromJson(Path, "IV");
            byte[] IVAsBytes = Convert.FromBase64String(IVAsString);
            this.IV = IVAsBytes;
        }

        public void CreateVault()
        {
            _domainsWithPasswords = new Dictionary<string, string>();
            Vault = new Dictionary<string, Dictionary<string, string>>();
            Vault.Add("vault", _domainsWithPasswords);
        }

        public byte[] Encrypt(Rfc2898DeriveBytes vaultKey)
        {
            string textToEncrypt = JsonSerializer.Serialize(Vault["vault"]);
            Console.WriteLine("Entering Encrypt method, this is the string to encrypt: " + textToEncrypt);
            byte[] result;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = vaultKey.GetBytes(16);
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(textToEncrypt);
                        }
                        result = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            Console.WriteLine("Exiting Encrypt method, this is the result: ");
            foreach (var b in result)
            {
                Console.Write(b + " ");
            }
            return result;
        }

        public byte[] GetEncryptedVault()
        {
            FileHandler fileHandler = new FileHandler();
            string encryptedVault = fileHandler.ReadValueFromJson(this.Path, "vault");
            Console.WriteLine("GetEncryptedVault returned: " + encryptedVault);
            return Convert.FromBase64String(encryptedVault);
        }

        public void WriteIVAndEncryptedVaultToJSON(byte[] encryptedVaultValue)
        {
            // Gör om byte[] till sträng
            string encryptedVaultValueAsString = Convert.ToBase64String(encryptedVaultValue);
            string IVAsString = Convert.ToBase64String(IV);

            Console.WriteLine("Entering method WriteIVAndEncryptedVaultToJSON, this is the string getting saved to server.json: " + encryptedVaultValueAsString);

            Dictionary<string, string> serverFileKeyValuePairs = new Dictionary<string, string>();
            serverFileKeyValuePairs["vault"] = encryptedVaultValueAsString;
            serverFileKeyValuePairs["IV"] = IVAsString;

            string serverFileAsJsonText = JsonSerializer.Serialize(serverFileKeyValuePairs);
            //string serverFileAsJsonText = JsonSerializer.Serialize(serverFileKeyValuePairs, new JsonSerializerOptions()
            //{
            //    /* Using UnsafeRelaxedJsonEscaped to fix that the "+" character was getting converted to "\u00-something"
            //     * which caused an error when trying to decrypt with the secret key.
            //     */
            //    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            //}
            //);

            File.WriteAllText(Path, serverFileAsJsonText);
        }

        public string Decrypt(byte[] encryptedVaultAsBytes, Rfc2898DeriveBytes vaultKey)
        {
            try
            {
                Console.WriteLine("Entering Decrypt method!!! EncryptedVaultAsBytes is ");
                foreach ( var v in encryptedVaultAsBytes)
                {
                    Console.Write(v + " ");
                }
                Console.WriteLine();
                string plaintext = null;

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Key = vaultKey.GetBytes(16);
                    aesAlg.IV = IV;

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for decryption.
                    using (MemoryStream msDecrypt = new MemoryStream(encryptedVaultAsBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {

                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                                Console.WriteLine("This is the plaintext in decrypt method: " + plaintext);
                            }
                        }
                    }
                }

                return plaintext;
            }
            catch 
            {
                Console.WriteLine("Could not decrypt data.");
                return null;
            }
        }
    }
}
