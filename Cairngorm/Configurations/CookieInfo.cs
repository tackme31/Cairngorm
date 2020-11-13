namespace Cairngorm.Settings
{
    public class CookieInfo
    {
        public string Name { get; }
        public int Lifespan { get; set; }
        public string Domain { get; set; }
        public string Paht { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
    }
}
