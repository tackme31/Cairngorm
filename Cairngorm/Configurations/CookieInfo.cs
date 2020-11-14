using Sitecore.Diagnostics;
using System;
using System.Web;

namespace Cairngorm.Configurations
{
    public class CookieInfo
    {
        public CookieInfo(string name)
        {
            Assert.ArgumentNotNullOrEmpty(name, nameof(name));

            Name = name;
        }

        public string Name { get; }
        public int Lifespan { get; set; } = 30;
        public string Domain { get; set; } = string.Empty;
        public string Path { get; set; } = "/";
        public bool Secure { get; set; } = true;
        public bool HttpOnly { get; set; } = true;

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
