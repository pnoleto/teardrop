﻿using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System;

namespace teardrop
{
    internal sealed partial class CryptoManager
    {
        private const int BYTES_LENGTH = 10;
        private const int SALT_BYTES_SIZE = 32;
        private const int ITERACTIONS_LIMIT = 5000;
        private const int KEY_SIZE = 256;
        private const int BLOCK_SIZE = 128;
        private const int BYTE_SIZE = 8;
        private const int BUFFER_STREAM_SIZE = 8192;
        private const int ZERO = 0;

        private static readonly char[] chars = new[]
        {
            'A','B','C','D','E','F',
            'G','H','I','J','K','L',
            'M','N','O','P','Q','R',
            'S','T','U','V','W','X',
            'Y','Z','0','1','2','3',
            '4','5','6','7','8','9',
            '$','%','#','@','*','&',
            '+','-','[',']','?','/'
        };

        public static string GetRandomString(uint length)
        {
            char[] randomChars = new char[length];

            Random random = new();

            for (int index = ZERO; index < length; index++)
            {
                randomChars[index] = chars[random.Next(chars.Length)];
            }

            return new string(randomChars);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static partial bool ZeroMemory(IntPtr Destination, int Length);

        public static byte[] GenerateRandomSaltBytes()
        {
            byte[] saltBytes = new byte[SALT_BYTES_SIZE];

            using (RandomNumberGenerator service = RandomNumberGenerator.Create())
            {
                for (uint index = ZERO; index < BYTES_LENGTH; index++)
                {
                    service.GetBytes(saltBytes);
                }
            }

            return saltBytes;
        }

        private static byte[] GetHashBytes(byte[] passwordBytes)
        {
            return SHA256.HashData(passwordBytes);
        }

        private static byte[] GetKeyBytes(string password)
        {
            return System.Text.Encoding.Default.GetBytes(password);
        }

        private static void DeriveKeyBytes(byte[] passwordHash, byte[] saltBytes, Aes AES)
        {
            using (Rfc2898DeriveBytes key = new(passwordHash, saltBytes, ITERACTIONS_LIMIT, HashAlgorithmName.SHA256))
            {
                AES.Key = key.GetBytes(AES.KeySize / BYTE_SIZE);
                AES.IV = key.GetBytes(AES.BlockSize / BYTE_SIZE);
            }
        }

        private static void DefineCipherMode(Aes AES)
        {
            AES.KeySize = KEY_SIZE;
            AES.BlockSize = BLOCK_SIZE;
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CFB;
        }


        // This will encrypt a file with a random sault.
        public static async Task EncodeFileAsync(string actualFilePath, string newFilePath, string password, CancellationToken cancellationToken)
        {
            byte[] passwordBytes = GetKeyBytes(password);
            byte[] passwordHash = GetHashBytes(passwordBytes);
            byte[] saltBytes = GenerateRandomSaltBytes();
            byte[] buffer = new byte[BUFFER_STREAM_SIZE];

            using (FileStream newFileStream = new(newFilePath, FileMode.Create))
            {
                newFileStream.Write(saltBytes, ZERO, saltBytes.Length);

                using (FileStream actualFileStream = new(actualFilePath, FileMode.Open))
                {
                    using (Aes AES = Aes.Create())
                    {
                        DefineCipherMode(AES);

                        DeriveKeyBytes(passwordHash, saltBytes, AES);

                        using (CryptoStream cryptoStream = new(newFileStream, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            int read = ZERO;

                            while ((read = await actualFileStream.ReadAsync(buffer.AsMemory(ZERO, buffer.Length), cancellationToken)) > ZERO)
                            {
                                await cryptoStream.WriteAsync(buffer.AsMemory(ZERO, read), cancellationToken);
                            }

                            cryptoStream.Close();
                        }

                        AES.Clear();
                    }

                    actualFileStream.Close();
                }

                newFileStream.Close();
            }
        }

        // This will decrypt a file
        public static async Task DecodeFileAsync(string actualFilePath, string newFilePath, string password, CancellationToken cancellationToken)
        {
            byte[] passwordBytes = GetKeyBytes(password);
            byte[] passwordHash = GetHashBytes(passwordBytes);
            byte[] saltBytes = new byte[SALT_BYTES_SIZE];
            byte[] buffer = new byte[BUFFER_STREAM_SIZE];

            using (FileStream actualFileStream = new(actualFilePath, FileMode.Open))
            {
                actualFileStream.Read(saltBytes, ZERO, saltBytes.Length);

                using (FileStream newFileStream = new(newFilePath, FileMode.Create))
                {
                    using (Aes AES = Aes.Create())
                    {
                        DefineCipherMode(AES);

                        DeriveKeyBytes(passwordHash, saltBytes, AES);

                        using (CryptoStream cryptoStream = new(actualFileStream, AES.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            int read = ZERO;

                            while ((read = await cryptoStream.ReadAsync(buffer.AsMemory(ZERO, buffer.Length), cancellationToken)) > ZERO)
                            {
                                await newFileStream.WriteAsync(buffer.AsMemory(ZERO, read), cancellationToken);
                            }

                            cryptoStream.Close();
                        }

                        AES.Clear();
                    }

                    newFileStream.Close();
                }

                actualFileStream.Close();
            }
        }
    }
}
