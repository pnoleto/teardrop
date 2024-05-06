using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace teardrop
{
    class Crypto
    {
        private const int BYTES_LIMIT = 10;
        private const int SALT_BYTES_SIZE = 32;
        private const int ITERACTIONS_LIMIT = 50000;
        private const int KEY_SIZE = 256;
        private const int BLOCK_SIZE = 128;
        private const int BYTE_SIZE = 8;
        private const int BUFFER_STREAM_SIZE = 8048;
        private const int ZERO = 0;

        // This is used to generate a random string. Can be used to generate the encryption key
        private static Random random = new Random();

        public static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
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
                for (uint i = 0; i < BYTES_LIMIT; i++)
                {
                    service.GetBytes(data);
                }
            }

            return data;
        }

        // This will encrypt a file with a random sault.
        public static void EncodeFile(string filePath, string newFilePath, string password)
        {
            byte[] passwordBytes = GetPasswordBytes(password);

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = KEY_SIZE;
                AES.BlockSize = BLOCK_SIZE;
                AES.Padding = PaddingMode.PKCS7;
                AES.Mode = CipherMode.CFB;

                byte[] saltBytes = GenerateRandomSaltBytes();

                using (Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, ITERACTIONS_LIMIT))
                {
                    AES.Key = key.GetBytes(AES.KeySize / BYTE_SIZE);
                    AES.IV = key.GetBytes(AES.BlockSize / BYTE_SIZE);
                }

                using (FileStream actualFileStream = new FileStream(filePath, FileMode.Open))
                {
                    using (FileStream newFileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        newFileStream.Write(saltBytes, 0, saltBytes.Length);

                        using (CryptoStream cryptoStream = new CryptoStream(newFileStream, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            try
                            {
                                int read = 0;
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

        private static byte[] GetPasswordBytes(string password)
        {
            return System.Text.Encoding.UTF8.GetBytes(password);
        }

        // This will decrypt a file
        public static void DecodeFile(string actualFilePath, string newFilePath, string password)
        {
            byte[] salt = new byte[SALT_BYTES_SIZE];
            byte[] passwordBytes = GetPasswordBytes(password);

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = KEY_SIZE;
                AES.BlockSize = BLOCK_SIZE;
                AES.Padding = PaddingMode.PKCS7;
                AES.Mode = CipherMode.CFB;

                using (Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordBytes, salt, ITERACTIONS_LIMIT))
                {
                    AES.Key = key.GetBytes(AES.KeySize / BYTE_SIZE);
                    AES.IV = key.GetBytes(AES.BlockSize / BYTE_SIZE);
                }

                using (FileStream actualFileStream = new FileStream(actualFilePath, FileMode.Open))
                {
                    actualFileStream.Read(salt, ZERO, salt.Length);

                    using (FileStream newFileStream = new FileStream(newFilePath, FileMode.Create))
                    {
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
