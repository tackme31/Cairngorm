using Sitecore.Data.Items;
using System.Collections.Generic;
using System.Xml;

namespace Cairngorm.TagResolvers
{
    public abstract class TagResolverBase
    {
        public TagResolverBase (XmlNode node)
        {
        }

        public abstract List<string> GetItemTags(Item item);
    }
}
