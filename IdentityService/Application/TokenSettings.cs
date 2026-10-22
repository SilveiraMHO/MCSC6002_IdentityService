namespace Application
{
    public class TokenSettings
    {
        public string PrivateKeyPath { get; set; }
        public int TokenExpireInMinutes { get; set; }
        public int RefreshTokenExpireInDays { get; set; }
    }
}
