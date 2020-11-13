using Sitecore;
using Sitecore.Data;
using System.Linq;

namespace Cairngorm
{
    public static class KnownSettings
    {
        private static string GetSetting(string name, string @default = null) => Sitecore.Configuration.Settings.GetSetting($"Cairngorm.{name}", @default);

        public static string SearchField = GetSetting("SearchField");
        public static ID[] SearchTemplates = GetSetting("SearchTemplates", string.Empty).Split('|').Select(id => MainUtil.GetID(id, ID.Null)).ToArray();
        public static int StoredItemCount = MainUtil.GetInt(GetSetting("StoredItemCount"), 20);
        public static float BoostMultiplicand = MainUtil.GetFloat(GetSetting("BoostMultiplicand"), 1.0f);
        public static bool FilterStoredItems = MainUtil.GetBool(GetSetting("FilterStoredItems"), false);
        public static bool FilterContextItem = MainUtil.GetBool(GetSetting("FilterContextItem"), true);
        public static class Cookie
        {
            public static string Name = GetSetting("Cookie.Name");
            public static int LifeSpan = MainUtil.GetInt(GetSetting("Cookie.Lifespan"), 30);
            public static string Domain = GetSetting("Cookie.Domain", string.Empty);
            public static string Path = GetSetting("Cookie.Path", "/");
            public static bool Secure = MainUtil.GetBool(GetSetting("Cookie.Secure"), false);
            public static bool HttpOnly = MainUtil.GetBool(GetSetting("Cookie.HttpOnly"), false);
        }
    }
}
