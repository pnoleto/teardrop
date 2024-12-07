namespace teardrop
{
    internal sealed class CryptoSettings
    {
        public CryptoSettings(byte[] PASSWORD_HASH, byte[] SALT_HASH)
        {
            this.PASSWORD_HASH = PASSWORD_HASH;
            this.SALT_HASH = SALT_HASH;
        }
        public byte[] PASSWORD_HASH { get; set; }
        public byte[] SALT_HASH { get; set; }
    }
}