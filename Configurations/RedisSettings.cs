namespace Chinese_sale_api.Configurations
{
    public class RedisSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 6379;
        public string Password { get; set; } = "";
        public string InstanceName { get; set; } = "ChineseSaleApi_";
        public int DefaultCacheMinutes { get; set; } = 30;
    }
}