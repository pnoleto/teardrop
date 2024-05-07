using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace teardrop
{
    public partial class CryptoManager
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
            '4','5','6','7','8','9'
        };

        public static string GetRandomString(int length)
        {
            char[] randomChars = new char[length];

            Random random = new();

            for (int i = 0; i < length; i++)
            {
                randomChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(randomChars.ToArray());
        }

        // This code can be used to delete the encryption key from memory!
        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static partial bool ZeroMemory(IntPtr Destination, int Length);


        // This will generate a salt for the encryption process
        public static byte[] GenerateRandomSaltBytes()
        {
            byte[] data = new byte[SALT_BYTES_SIZE];

            using (RandomNumberGenerator service = RandomNumberGenerator.Create())
            {
                for (uint i = 0; i < BYTES_LENGTH; i++)
                {
                    service.GetBytes(data);
                }
            }

            return data;
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

        private static void DefineCypherMode(Aes AES)
        {
            AES.KeySize = KEY_SIZE;
            AES.BlockSize = BLOCK_SIZE;
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CFB;
        }


        // This will encrypt a file with a random sault.
        public static void EncodeFile(string actualFilePath, string newFilePath, string password)
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
                        DefineCypherMode(AES);

                        DeriveKeyBytes(passwordHash, saltBytes, AES);

                        using (CryptoStream cryptoStream = new(newFileStream, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            int read = ZERO;

                            while ((read = actualFileStream.Read(buffer, ZERO, buffer.Length)) > ZERO)
                            {
                                cryptoStream.Write(buffer, ZERO, read);
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
        public static void DecodeFile(string actualFilePath, string newFilePath, string password)
        {
            byte[] passwordBytes = GetKeyBytes(password);
            byte[] passwordHash = GetHashBytes(passwordBytes);
            byte[] saltBytes = new byte[SALT_BYTES_SIZE];
            byte[] buffer = new byte[BUFFER_STREAM_SIZE];

            using (FileStream newFileStream = new(newFilePath, FileMode.Create))
            {
                using (FileStream actualFileStream = new(actualFilePath, FileMode.Open))
                {
                    actualFileStream.Read(saltBytes, ZERO, saltBytes.Length);

                    using (Aes AES = Aes.Create())
                    {
                        DefineCypherMode(AES);

                        DeriveKeyBytes(passwordHash, saltBytes, AES);

                        using (CryptoStream cryptoStream = new(actualFileStream, AES.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            int read = ZERO;

                            while ((read = cryptoStream.Read(buffer, ZERO, buffer.Length)) > ZERO)
                            {
                                newFileStream.Write(buffer, ZERO, read);
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
    }
}
