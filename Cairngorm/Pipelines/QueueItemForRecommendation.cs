using Cairngorm.Configurations;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.HttpRequest;
using System.Collections.Generic;
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

                if (!IsTargetTemplate(setting.SearchTemplates, Context.Item))
                {
                    continue;
                }

                if (!IsInTargetScope(setting.SearchScope, Context.Item))
                {
                    continue;
                }

                setting.ItemsStore.AddItem(Context.Item);
            }
        }

        private static bool IsTargetTemplate(List<string> templates, Item item)
            => !templates.Any()
            || templates.Any(template => ID.TryParse(template, out var id) ? item.TemplateID == id : item.TemplateName == template);

        private bool IsInTargetScope(string pathOrId, Item item)
        {
            if (string.IsNullOrWhiteSpace(pathOrId))
            {
                return true;
            }

            var scope = ID.TryParse(pathOrId, out var id)
                ? item.Database.GetItem(id)
                : item.Database.GetItem(pathOrId);
            if (scope == null)
            {
                Log.Warn($"The specified search scope '{pathOrId}' does not exist. No scopes are used instead.", this);
                return true;
            }

            return item.Axes.IsDescendantOf(scope);
        }
    }
}
