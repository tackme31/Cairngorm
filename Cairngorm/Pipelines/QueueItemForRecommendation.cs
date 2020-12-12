using Cairngorm.Configurations;
using Sitecore;
using Sitecore.Pipelines.HttpRequest;
using System.Linq;

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

                setting.ItemsStore.AddItem(Context.Item);
            }
        }
    }
}
