using Cairngorm.Configurations;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Diagnostics;
using Sitecore.Xml;
using System.Linq;
using System.Xml;

namespace Cairngorm.ContentSearch
{
    public class RecommenderTagResolversIndexField : AbstractComputedIndexField
    {
        protected string RecommenderName { get; }

        protected RecommenderTagResolversIndexField() : base()
        {
        }

        protected RecommenderTagResolversIndexField(XmlNode configNode) : base(configNode)
        {
            RecommenderName = XmlUtil.GetAttribute("recommender", configNode);
        }

        public override object ComputeFieldValue(IIndexable indexable)
        {
            if (!(indexable is SitecoreIndexableItem indexableItem))
            {
                return null;
            }

            var item = indexableItem.Item;
            if (item == null)
            {
                return null;
            }

            var config = RecommenderConfiguration.Create();
            if (config == null)
            {
                return null;
            }

            var setting = config.GetSetting(RecommenderName);
            if (setting == null)
            {
                Log.Warn($"The specified recommender '{RecommenderName}' does not exist.", this);
                return null;
            }

            var tags = setting.TagResolvers.SelectMany(resolver => resolver.GetItemTags(item));
            return string.Join(" ", tags);
        }
    }
}
