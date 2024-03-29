﻿
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordManager
{
    internal class Server
    {
        public string Path;
        private readonly int _lengthOfKey = 16;
        public byte[] IV { get; set; }
        private FileHandler _fileHandler = new FileHandler();

        public Server(string path)
        {
            Path = path;

            if (File.Exists(Path))
                SetIV();
            else
                GenerateIV();
        }

        private void SetIV()
        {
            string IVAsString = _fileHandler.ReadValueFromJson(Path, "IV");
            byte[] IVAsBytes = Convert.FromBase64String(IVAsString);
            IV = IVAsBytes;
        }

        private void GenerateIV()
        {
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                IV = new byte[_lengthOfKey];
                generator.GetBytes(IV);
            }
        }

        public byte[] GetEncryptedAccounts()
        {
            try
            {
                string encryptedAccounts = _fileHandler.ReadValueFromJson(Path, "vault");
                return Convert.FromBase64String(encryptedAccounts);
            }
            catch
            {
                Console.WriteLine("Could not find server-file.");
                return null;
            }
        }

        public string FormatServerToText(byte[] encryptedAccounts)
        {
            string encryptedAccountsAsText = Convert.ToBase64String(encryptedAccounts);
            string IVAsText = Convert.ToBase64String(IV);

            Dictionary<string, string> server = new Dictionary<string, string>();
            server["vault"] = encryptedAccountsAsText;
            server["IV"] = IVAsText;
            return JsonSerializer.Serialize(server);
        }
    }
}
