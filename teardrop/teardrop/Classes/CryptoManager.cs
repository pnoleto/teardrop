using System.Security.Cryptography;

namespace teardrop
{
    internal sealed partial class CryptoManager(CryptoSettings settings, CancellationToken cancellationToken)
    {
        private const int ZERO = 0;
        private const int BYTE_SIZE = 8;
        private const int KEY_SIZE = 256;
        private const int BLOCK_SIZE = 128;
        private const int BUFFER_STREAM_LENGTH = 8192;

        private static void DefineCipherMode(Aes AES)
        {
            AES.KeySize = KEY_SIZE;
            AES.BlockSize = BLOCK_SIZE;
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CFB;
        }

        private void DeriveKeyBytes(byte[] passwordHash, byte[] saltBytes, Aes AES)
        {
            using (Rfc2898DeriveBytes key = new(passwordHash, saltBytes, settings.IteractionsLimit, HashAlgorithmName.SHA256))
            {
                AES.Key = key.GetBytes(AES.KeySize / BYTE_SIZE);
                AES.IV = key.GetBytes(AES.BlockSize / BYTE_SIZE);
            }
        }

        public async Task EncodeFileAsync(string actualFilePath, string newFilePath)
        {
            byte[] buffer = new byte[BUFFER_STREAM_LENGTH];

            using (FileStream newFileStream = new(newFilePath, FileMode.Create))
            {
                newFileStream.Write(settings.SaltBytes, ZERO, settings.SaltBytes.Length);

                using (FileStream actualFileStream = new(actualFilePath, FileMode.Open))
                {
                    using (Aes AES = Aes.Create())
                    {
                        DefineCipherMode(AES);

                        DeriveKeyBytes(settings.PasswordHash, settings.SaltBytes, AES);

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

        public async Task DecodeFileAsync(string actualFilePath, string newFilePath)
        {
            byte[] buffer = new byte[BUFFER_STREAM_LENGTH];

            using (FileStream actualFileStream = new(actualFilePath, FileMode.Open))
            {
                actualFileStream.Read(settings.SaltBytes, ZERO, settings.SaltBytes.Length);

                using (FileStream newFileStream = new(newFilePath, FileMode.Create))
                {
                    using (Aes AES = Aes.Create())
                    {
                        DefineCipherMode(AES);

                        DeriveKeyBytes(settings.PasswordHash, settings.SaltBytes, AES);

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
