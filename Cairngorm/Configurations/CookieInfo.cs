using System;
using System.Web;

namespace Cairngorm.Settings
{
    public class CookieInfo
    {
        public string Name { get; }
        public int Lifespan { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }

        public HttpCookie ToHttpCookie()
        {
            return new HttpCookie(Name)
            {
                Expires = DateTime.Now.AddDays(Lifespan),
                Domain = Domain,
                Path = Path,
                Secure = Secure,
                HttpOnly = HttpOnly,
            };
        }
    }
}
