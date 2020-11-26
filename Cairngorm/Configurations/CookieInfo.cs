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
        public int MaxAge { get; set; } = 30 * 24 * 60 * 60;
        public string Domain { get; set; } = string.Empty;
        public string Path { get; set; } = "/";
        public bool Secure { get; set; } = true;
        public bool HttpOnly { get; set; } = true;

        public HttpCookie ToHttpCookie()
        {
            var cookie = new HttpCookie(Name)
            {
                Expires = DateTime.Now.AddSeconds(MaxAge),
                Domain = Domain,
                Path = Path,
                Secure = Secure,
                HttpOnly = HttpOnly,
            };

            cookie.Values.Add("max-age", MaxAge.ToString());

            return cookie;
        }
    }
}
