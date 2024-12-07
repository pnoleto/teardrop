using System.Security.Cryptography;

namespace teardrop
{
    internal sealed partial class CryptoManager(CryptoSettings settings, CancellationToken cancellationToken)
    {
        private const int ZERO = 0;
        private const int BYTE_LENGTH = 8;
        private const int KEY_LENGTH = 256;
        private const int BLOCK_LENGTH = 128;
        private const int BUFFER_STREAM_LENGTH = 8192;
        private const int ITERACTIONS_LIMIT = 5000;
        private const int SALT_BYTES_LENGTH = 32;

        private static void DeriveKeyBytes(byte[] passwordHash, byte[] saltBytes, Aes AES)
        {
            using (Rfc2898DeriveBytes key = new(passwordHash, saltBytes, ITERACTIONS_LIMIT, HashAlgorithmName.SHA256))
            {
                AES.Key = key.GetBytes(AES.KeySize / BYTE_LENGTH);
                AES.IV = key.GetBytes(AES.BlockSize / BYTE_LENGTH);
            }
        }

        private static void DefineCipherMode(Aes AES)
        {
            AES.KeySize = KEY_LENGTH;
            AES.BlockSize = BLOCK_LENGTH;
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CFB;
        }


        // This will encrypt a file with a random sault.
        public async Task EncodeFileAsync(string actualFilePath, string newFilePath)
        {
            byte[] buffer = new byte[BUFFER_STREAM_LENGTH];

            using (FileStream newFileStream = new(newFilePath, FileMode.Create))
            {
                newFileStream.Write(settings.SALT_HASH, ZERO, settings.SALT_HASH.Length);

                using (FileStream actualFileStream = new(actualFilePath, FileMode.Open))
                {
                    using (Aes AES = Aes.Create())
                    {
                        DefineCipherMode(AES);

                        DeriveKeyBytes(settings.PASSWORD_HASH, settings.SALT_HASH, AES);

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
        public async Task DecodeFileAsync(string actualFilePath, string newFilePath)
        {
            byte[] saltBytes = new byte[SALT_BYTES_LENGTH];
            byte[] buffer = new byte[BUFFER_STREAM_LENGTH];

            using (FileStream actualFileStream = new(actualFilePath, FileMode.Open))
            {
                actualFileStream.Read(saltBytes, ZERO, saltBytes.Length);

                using (FileStream newFileStream = new(newFilePath, FileMode.Create))
                {
                    using (Aes AES = Aes.Create())
                    {
                        DefineCipherMode(AES);

                        DeriveKeyBytes(settings.PASSWORD_HASH, saltBytes, AES);

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
