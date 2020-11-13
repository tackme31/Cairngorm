using Cairngorm.Settings;
using Sitecore;
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

            if (!(Context.PageMode.IsPreview || Context.PageMode.IsNormal))
            {
                return;
            }

            var config = RecommenderConfiguration.Create();
            if (config == null)
            {
                return;
            }

            foreach (var settingName in config.SettingNames)
            {
                var setting = config.GetSetting(settingName);
                if (setting == null)
                {
                    continue;
                }

                if (setting.SearchTemplates.Any() && setting.SearchTemplates.All(id => id != Context.Item.TemplateID))
                {
                    continue;
                }

                var cookie = GetCookie(setting.CookieInfo);
                var cookieItems = new Queue<string>(cookie.Value?.Split('|') ?? Array.Empty<string>());
                cookieItems.Enqueue(Context.Item.ID.ToShortID().ToString());

                while (cookieItems.Count > setting.StoredItemsCount)
                {
                    cookieItems.Dequeue();
                }

                HttpContext.Current.Response.Cookies[setting.CookieInfo.Name].Value = string.Join("|", cookieItems);
            }
        }

        private HttpCookie GetCookie(CookieInfo info)
        {
            var cookie = HttpContext.Current.Request.Cookies.Get(info.Name);
            if (cookie == null)
            {
                // Ensure cookie exists
                cookie = info.ToHttpCookie();
                HttpContext.Current.Response.Cookies.Add(cookie);
            }

            return cookie;
        }
    }
}
