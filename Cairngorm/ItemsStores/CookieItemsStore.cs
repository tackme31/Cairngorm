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
            var cookieItems = new Queue<string>(cookie.Value?.Split('|') ?? Array.Empty<string>());
            cookieItems.Enqueue(item.ID.ToShortID().ToString());

            while (cookieItems.Count > StoredItemsCount)
            {
                cookieItems.Dequeue();
            }

            HttpContext.Current.Response.Cookies[CookieInfo.Name].Value = string.Join("|", cookieItems);
        }

        public override List<Item> GetItems()
        {
            var cookieValue = HttpContext.Current.Request.Cookies[CookieInfo.Name]?.Value ?? string.Empty;
            return cookieValue.Split('|')
                .Select(idStr => ID.Parse(idStr, ID.Null))
                .Where(id => !id.IsNull)
                .Select(Context.Database.GetItem)
                .Where(item => item != null).
                ToList();
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
