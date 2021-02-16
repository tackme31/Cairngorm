using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cairngorm.ItemsStores
{
    public class CookieItemsStore : ItemsStoreBase
    {
        public int StoredItemsCount { get; set; } = 10;
        public CookieInfo CookieInfo { get; set; } = new CookieInfo("recommend_items");

        public override void AddItem(Item item)
        {
            var cookie = GetCookie();
            var cookieItems = new Queue<string>(cookie.Value?.Split('|') ?? new string[0] { });
            cookieItems.Enqueue(item.ID.ToShortID().ToString());

            while (cookieItems.Count > StoredItemsCount)
            {
                cookieItems.Dequeue();
            }

            HttpContext.Current.Response.Cookies[CookieInfo.Name].Value = string.Join("|", cookieItems);
        }

        public override List<Item> GetItems()
        {
            var cookie = HttpContext.Current.Response.Cookies.AllKeys.Contains(CookieInfo.Name)
                ? HttpContext.Current.Response.Cookies[CookieInfo.Name]
                : HttpContext.Current.Request.Cookies[CookieInfo.Name];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
            {
                return new List<Item>();
            }

            var ids = cookie.Value.Split('|').Select(idStr => ID.Parse(idStr, ID.Null));
            var items = ids.Select(Context.Database.GetItem).Where(item => item != null);
            return items.Reverse().Take(10).ToList();
        }

        protected virtual HttpCookie GetCookie()
        {
            var cookie = HttpContext.Current.Request.Cookies.Get(CookieInfo.Name);
            if (cookie == null)
            {
                // Ensure cookie exists
                cookie = CookieInfo.ToHttpCookie();
                HttpContext.Current.Response.Cookies.Add(cookie);
            }

            return cookie;
        }
    }
}
