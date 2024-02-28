﻿using System.IO;
using System.Security.Cryptography;
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
                    // Encrypt server vault using master password
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
        }

        private static void CreateNewVault(string[] args)
        {

            Client client = new Client(args[1]);
            Server server = new Server(args[2]);

            // GenerateVaultKey(); Vault key används i Aes för att kryptera Vault.
            // master password + secret key + Rfc2898DeriveBytes = vault key
            // string secret = GetValueFromJSONFile(args[1], "secret"); // Behövs inte just nu, men kanske sedan i dekryptering?
            Console.WriteLine("Enter your master password: ");
            Rfc2898DeriveBytes vaultKey = new Rfc2898DeriveBytes(Console.ReadLine(), client.RandomBytes, 10000, HashAlgorithmName.SHA256);

            // EcryptVault();
            // Encrypt the JSON string
            // Get vault as jsonstring
            string vaultAsJsonString = JsonSerializer.Serialize(server.Vault);

            // IV + Vault key i Aes-objekt för att kryptera Vault!
            // Get IV from server: server.InitializationVector
            // Create Aes object (vault key + IV)
            // Encrypt vault by using the Aes object encryption method

            byte[] encryptedVaultAsBytes = EncryptJsonString(vaultAsJsonString, vaultKey.GetBytes(16), server.InitializationVector);
            string encryptedVaultAsString = Convert.ToBase64String(encryptedVaultAsBytes);

            // Skriv till server.json
            server.WriteEncryptedVaultToJSON(encryptedVaultAsString);
        }

        public static byte[] EncryptJsonString(string vaultAsJsonString, byte[] vaultKey, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(vaultKey, iv);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(vaultAsJsonString);
                        }
                    }

                    return msEncrypt.ToArray();
                }
            }
        }

        private static void GenerateVaultKey()
        {
            
        }

        private static string GetValueFromJSONFile(string pathToFile, string key)
        {
            string fileAsText = File.ReadAllText(pathToFile);
            Dictionary<string, string> KeyValuePairs = JsonSerializer.Deserialize<Dictionary<string, string>>(fileAsText);
            return KeyValuePairs[key];
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
    }
}
