namespace teardrop
{
    internal sealed class CryptoSettings(byte[] PasswordHash, byte[] SaltBytes, int IteractionsLimit)
    {
        public byte[] PasswordHash { get; set; } = PasswordHash;
        public byte[] SaltBytes { get; set; } = SaltBytes;
        public int IteractionsLimit { get; set; } = IteractionsLimit;
    }
}