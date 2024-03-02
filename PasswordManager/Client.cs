﻿
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PasswordManager
{
    internal class Client
    {
        private string _path;
        private int _lengthOfKey = 16;
        public byte[] SecretKeyAsBytes { get; private set; }

        public Client(string path)
        {
            _path = path;
        }

        public string GenerateSecretKey()
        {
            RandomNumberGenerator generator = RandomNumberGenerator.Create();
            SecretKeyAsBytes = new byte[_lengthOfKey];
            generator.GetBytes(SecretKeyAsBytes);
            return Convert.ToBase64String(SecretKeyAsBytes);
        }

        public Rfc2898DeriveBytes DeriveVaultKey()
        {
            Console.WriteLine("Enter your master password: ");
            return new Rfc2898DeriveBytes(Console.ReadLine(), SecretKeyAsBytes, 10000, HashAlgorithmName.SHA256);
        }

        public Rfc2898DeriveBytes DeriveVaultKey(string secretKey)
        {
            SecretKeyAsBytes = Encoding.UTF8.GetBytes(secretKey);
            return DeriveVaultKey();
        }
    }
}
