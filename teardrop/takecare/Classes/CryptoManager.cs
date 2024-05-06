using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace takecare
{
    public class CryptoManager
    {
        private const int BYTES_LENGTH = 10;
        private const int SALT_BYTES_SIZE = 32;
        private const int ITERACTIONS_LIMIT = 50000;
        private const int KEY_SIZE = 256;
        private const int BLOCK_SIZE = 128;
        private const int BYTE_SIZE = 8;
        private const int BUFFER_STREAM_SIZE = 8048;
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
            char[]randomChars = new char[length];

            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                randomChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(randomChars.ToArray());
        }

        // This code can be used to delete the encryption key from memory!
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);


        // This will generate a salt for the encryption process
        public static byte[] GenerateRandomSaltBytes()
        {
            byte[] data = new byte[SALT_BYTES_SIZE];

            using (RNGCryptoServiceProvider service = new RNGCryptoServiceProvider())
            {
                for (uint i = 0; i < BYTES_LENGTH; i++)
                {
                    service.GetBytes(data);
                }
            }

            return data;
        }

        // This will encrypt a file with a random sault.
        public static void EncodeFile(string actualFilePath, string newFilePath, string password)
        {
            byte[] passwordBytes = GetPasswordBytes(password);
            byte[] passwordHash = GetPasswordHash(passwordBytes);

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = KEY_SIZE;
                AES.BlockSize = BLOCK_SIZE;
                AES.Padding = PaddingMode.PKCS7;
                AES.Mode = CipherMode.CFB;

                byte[] saltBytes = GenerateRandomSaltBytes();

                using (Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordHash, saltBytes, ITERACTIONS_LIMIT))
                {
                    AES.Key = key.GetBytes(AES.KeySize / BYTE_SIZE);
                    AES.IV = key.GetBytes(AES.BlockSize / BYTE_SIZE);
                }

                using (FileStream actualFileStream = new FileStream(actualFilePath, FileMode.Open))
                {
                    using (FileStream newFileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        newFileStream.Write(saltBytes, ZERO, saltBytes.Length);

                        using (CryptoStream cryptoStream = new CryptoStream(newFileStream, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            try
                            {
                                int read = ZERO;
                                byte[] buffer = new byte[BUFFER_STREAM_SIZE];

                                while ((read = actualFileStream.Read(buffer, ZERO, buffer.Length)) > ZERO)
                                {
                                    cryptoStream.Write(buffer, ZERO, read);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            finally
                            {
                                cryptoStream.Close();
                                newFileStream.Close();
                                actualFileStream.Close();
                            }
                        }
                    }
                }
            }
        }

        private static byte[] GetPasswordHash(byte[] passwordBytes)
        {
            return SHA1.Create().ComputeHash(passwordBytes);
        }

        private static byte[] GetPasswordBytes(string password)
        {
            return System.Text.Encoding.UTF8.GetBytes(password);
        }

        // This will decrypt a file
        public static void DecodeFile(string actualFilePath, string newFilePath, string password)
        {
            byte[] passwordBytes = GetPasswordBytes(password);
            byte[] passwordHash = GetPasswordHash(passwordBytes);

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = KEY_SIZE;
                AES.BlockSize = BLOCK_SIZE;
                AES.Padding = PaddingMode.PKCS7;
                AES.Mode = CipherMode.CFB;

                byte[] saltBytes = GenerateRandomSaltBytes();

                using (Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordHash, saltBytes, ITERACTIONS_LIMIT))
                {
                    AES.Key = key.GetBytes(AES.KeySize / BYTE_SIZE);
                    AES.IV = key.GetBytes(AES.BlockSize / BYTE_SIZE);
                }

                using (FileStream actualFileStream = new FileStream(actualFilePath, FileMode.Open))
                {
                    using (FileStream newFileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        actualFileStream.Read(saltBytes, ZERO, saltBytes.Length);

                        using (CryptoStream cryptoStream = new CryptoStream(actualFileStream, AES.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            try
                            {
                                int read = ZERO;
                                byte[] buffer = new byte[BUFFER_STREAM_SIZE];

                                while ((read = cryptoStream.Read(buffer, ZERO, buffer.Length)) > ZERO)
                                {
                                    newFileStream.Write(buffer, ZERO, read);
                                }
                            }
                            catch (CryptographicException ex_CryptographicException)
                            {
                                Console.WriteLine($"CryptographicException error: {ex_CryptographicException.Message}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            finally
                            {
                                cryptoStream.Close();
                                newFileStream.Close();
                                actualFileStream.Close();
                            }
                        }
                    }
                }
            }
        }
    }
}
