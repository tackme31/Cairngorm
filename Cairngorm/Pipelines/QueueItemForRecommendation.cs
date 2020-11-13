using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Pipelines.HttpRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cairngorm.Pipelines
{
    public class QueueItemForRecommendation : HttpRequestProcessor
    {
        public QueueItemForRecommendation()
        {
        }

        public override void Process(HttpRequestArgs args)
        {
            if (Context.Item == null)
            {
                return;
            }

            if (KnownSettings.SearchTemplates.Any() && KnownSettings.SearchTemplates.All(id => id != Context.Item.TemplateID))
            {
                return;
            }

            if (!(Context.PageMode.IsPreview || Context.PageMode.IsNormal))
            {
                return;
            }

            var cookie = GetCookie();
            var cookieItems = new Queue<string>(cookie.Value?.Split('|') ?? Array.Empty<string>());
            cookieItems.Enqueue(Context.Item.ID.ToShortID().ToString());

            while (cookieItems.Count > KnownSettings.StoredItemCount)
            {
                cookieItems.Dequeue();
            }

            HttpContext.Current.Response.Cookies[KnownSettings.Cookie.Name].Value = string.Join("|", cookieItems);
        }

        private HttpCookie GetCookie()
        {
            var cookieName = KnownSettings.Cookie.Name;
            var cookie = HttpContext.Current.Request.Cookies.Get(cookieName);
            if (cookie == null)
            {
                // Ensure cookie exists
                cookie = new HttpCookie(cookieName)
                {
                    Expires = DateTime.Now.AddDays(KnownSettings.Cookie.LifeSpan),
                    Domain = KnownSettings.Cookie.Domain,
                    Path = KnownSettings.Cookie.Path,
                    Secure = KnownSettings.Cookie.Secure,
                    HttpOnly = KnownSettings.Cookie.HttpOnly,
                };

                HttpContext.Current.Response.Cookies.Add(cookie);
            }

            return cookie;
        }
    }
}
