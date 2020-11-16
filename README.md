# Cairngorm
*Cairngorm* is an easy-to-use recommender library for Sitecore. This software intends to be an advanced version of [TagBasedRecommender](https://github.com/xirtardauq/TagBasedRecommender).

**Warning: This repo is under development. The API is likely to change.**

## Installation
Download .nupkg file from [here](https://github.com/xirtardauq/Cairngorm/releases), and install it from a local package source.

## Supports
This library is tested on Sitecore XP 9.3 initial release.

## Usage
1. Apply the following configuration patch.
    ```xml
    <configuration>
      <sitecore>
        <cairngorm>
          <configuration type="Cairngorm.Configurations.RecommenderConfiguration, Cairngorm">
            <recommenders hint="list:AddRecommender">
              <recommender name="sample" type="Cairngorm.Configurations.RecommenderSetting, Cairngorm">
                <!-- Specifie recommender name. -->
                <param desc="name">$(name)</param>

                <!-- Set the page templates that is returned from the recommender. -->
                <searchTemplates hint="raw:AddSearchTemplate">
                  <SampleItem>{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}</SampleItem>
                </searchTemplates>

                <!-- Add the tag resolvers. In the following example, tags are contained in the "Tags" field, separated by '|'. -->
                <tagResolvers hint="raw:AddTagResolver">
                  <resolver type="Cairngorm.TagResolvers.TextFieldTagResolver, Cairngorm" fieldName="Tags" delimiter="|" />
                </tagResolvers>
              </recommender>
            </recommenders>
          </configuration>
        </cairngorm>
      </sitecore>
    </configuration>
    ```
1. Now you can use the recommender like this:
    ```csharp
    public class SampleRecommendationRepository
    {
        protected Recommender SampleRecommender { get; }

        public SampleRecommendationRepository(IRecommenderFactory factory)
        {
            // Get the recommender by the name specified in the configuration.
            SampleRecommender = factory.GetRecommender(name: "sample");
        }

        public IModel GetModel()
        {
            // Get recommended items by the recommender.
            var recommendation = SampleRecommender.GetRecommendation(count: 5);
            return ...;
        }
    }
    ```

### Configuration
You can see a sample configuration from [here](Cairngorm/App_Config/Include/Feature/Cairngorm/Cairngorm.SampleRecommender.config.example).

|Property Name|Type|Description|Default Value|
|:-|:-|:-|:-|
|SearchField|`string`|An index field name for search by tags.|`"_content"`|
|SearchTemplates|`List<ID>`|The template IDs to filter the recommendation items.|Empty (All templates).|
|StoredItemsCount|`int`|A length of items stored in the cookie.|`10`|
|WeightPerMatching|`float`|A value to be added to boosting when a tag is matched.|`1.0`|
|BoostGradually|`bool`|set true, the newer the item in the cookie, the more weight is given to it.|`false`|
|FilterStoredItems|`bool`|When set true, the items stored in the cookie are filtered from recommendation.|`false`|
|FilterContextItem|`bool`| When set true, a context item is filtered from recommendation.|`true`|
|SearchScope|`string`|A root item of the recommendation items (Path or ID).|No scope.|
|CookieInfo|`CookieInfo`|Cookie's information used for recommendation.||
|TagResolvers|`List<TagResolverBase>`|The tag resolvers which resolve the tags from a context item. The following three are pre-defined resolvers.|No filter.|

### For Customization
#### Tag Resolver
There are three pre-defined resolvers:

|Name|Description|
|:-|:-|
|`TextFieldTagResolver`|The tags are in the text field. When a delimiter specified, the field value is splited by it.|
|`LinkFieldTagResolver`|The tags are in the item's field refered from the link field. When a delimiter specified, the field value is splited by it.|
|`MultilistFieldTagResolver`|The tags are in the items' field refered from the multilist field. When a delimiter specified, the field value is splited by it.|

The tag resolver can be added by implementing a class derived from the `TagResolverBase` class.

```csharp
public class MetadataKeywordsTagResolver : Cairngorm.TagResolvers.TagResolverBase
{
    public override List<string> GetItemTags(Item item)
    {
        // Get tags from the Keyword field in the {context item}/Data/Metadata item.
        var metadata = item.Children["Data"].Children["Metadata"];
        return metadata["Keywords"].Split(' ').ToList();
    }
}
```

And set the resolver to configuration's `TagResolvers` section.

```xml
<tagResolvers hint="raw:AddTagResolver">
  <resolver type="Namespace.To.MetadataKeywordsTagResolver, AssemblyName" />
</tagResolvers>
```

#### Search Filter
By default, the recommended items are filtered by the context language. If you add or change the filter condition, override the `DefaultRecommender.ApplyItemsFilter` method.

```csharp
public class MySearchResultItem : SearchResultItem
{
    public string MyProperty { get; set; }
}

public class MyRecommender : Cairngorm.Services.DefaultRecommender<MySearchResultItem>
{
    public MyRecommender(RecommenderSetting recommenderSetting) : base(recommenderSetting)
    {
    }

    // Apply the additional filters.
    protected override IQueryable<MySearchResultItem> ApplyItemsFilter(IQueryable<MySearchResultItem> query) => base.ApplyItemsFilter(query)
        .Filter(item => item.MyProperty == "My Filter")
        .Filter(item => item.TemplateName == "My Template");
}
```

Then fix the recommender factory to return the custom recommender.

```csharp
public class MyRecommenderFactory : Cairngorm.Services.DefaultRecommenderFactory<MySearchResultItem>
{
    public override Recommender GetRecommender(string name)
    {
        var setting = GetRecommenderSetting(name);
        return new MyRecommender(setting);
    }
}
```

Finally, replace the factory to the default one.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/">
  <sitecore>
    <services>
      <register serviceType="Cairngorm.Services.IRecommenderFactory, Cairngorm">
        <patch:attribute name="implementationType">Namespace.To.MyRecommenderFactory, AssemblyName</patch:attribute>
      </register>
    </services>
  </sitecore>
</configuration>
```

## Author
- Takumi Yamada (xirtardauq@gmail.com)

## License
*Cairngorm* is licensed under the MIT license. See LICENSE.txt.
