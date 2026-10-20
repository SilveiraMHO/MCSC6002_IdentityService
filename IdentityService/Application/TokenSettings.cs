namespace Application
{
    public class TokenSettings
    {
        public string Secret { get; set; }
        public int TokenExpireInMinutes { get; set; }
        public int RefreshTokenExpireInDays { get; set; }
    }
}
