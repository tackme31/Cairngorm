English | [日本語](README.ja.md)

# Cairngorm
*Cairngorm* is an easy-to-use recommender library for Sitecore.

**Warning: This repo is under development. The API is likely to change.**

## Installation
Download .nupkg file from [here](https://github.com/xirtardauq/Cairngorm/releases), and install it from a local package source.

## Supports
This library is tested on Sitecore XP 9.3 initial release with Solr.

## Usage
The recommender searches recommended items based on "Tag", resolved from an item. The more "Tag"s an item contains, the more likely the item is recommended.

1. Apply the following configuration patch.
    ```xml
    <configuration>
      <sitecore>
        <cairngorm>
          <configuration type="Cairngorm.Configurations.RecommenderConfiguration, Cairngorm">
            <recommenders hint="list:AddRecommender">
              <recommender name="sample" type="Cairngorm.Configurations.RecommenderSetting, Cairngorm">
                <!-- Specify recommender name. -->
                <param desc="name">$(name)</param>

                <!-- Add template IDs to be the target of the recommender. -->
                <searchTemplates hint="raw:AddSearchTemplate">
                  <SampleItem>{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}</SampleItem>
                </searchTemplates>

                <!-- Add tag resolvers. In the following example, tags are contained in the "Tags" field, separated by '|'. -->
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
|SearchTemplates|`List<ID>`|Template IDs to be the target of the recommender.|Empty (All templates).|
|StoredItemsCount|`int`|A length of items stored in the cookie.|`10`|
|WeightPerMatching|`float`|A value to be added to boosting when a tag is matched.|`1.0`|
|BoostGradually|`bool`|If enabled, the newer the item in the cookie, the more weight is given to it.|`false`|
|FilterStoredItems|`bool`|If enabled, items stored in the cookie are filtered from results.|`false`|
|FilterContextItem|`bool`|If enabled, a context item is filtered from results.|`true`|
|SearchScope|`string`|A root item where it's searched from (Path or ID).|No scope.|
|CookieInfo|`CookieInfo`|Cookie's information used for recommendation.|See the next table.|
|TagResolvers|`List<TagResolverBase>`|Tag resolvers which resolve the tags from an item in the cookie. See the Tag Resolver section for more information.|No resolver.|

The `CookieInfo` property has the following properties.

|Property Name|Type|Description|Default Value|
|:-|:-|:-|:-|
|Name|`string`|Cookie's name (required).|`"recommend_items"`|
|Lifespan|`int`|Cookie's lifespan to set to Expire attribute (in days).|`30`|
|Domain|`string`|A value of the cookie's `Domain` attribute.|Empty.|
|Path|`string`|A value of the cookie's `Path` attribute.|`"/"`|
|Secure|`bool`|A value of the cookie's `Secure` attribute.|`true`|
|HttpOnly|`bool`|A value of the cookie's `HttpOnly` attribute.|`true`|

### Tag Resolver
There are three pre-defined resolvers:

|Name|Description|
|:-|:-|
|`TextFieldTagResolver`|Tags are in the text field. When a delimiter is specified, the field value is split by it.|
|`LinkFieldTagResolver`|Tags are in the item's field referred from the link field. When a delimiter is specified, the field value is split by it.|
|`MultilistFieldTagResolver`|Tags are in the items' field referred from the multilist field. When a delimiter is specified, the field value is split by it.|

The tag resolver can be added by implementing a class derived from the `TagResolverBase` class.

```csharp
public class MetadataKeywordsTagResolver : Cairngorm.TagResolvers.TagResolverBase
{
    public MetadataKeywordsTagResolver (XmlNode node) : base(node)
    {
    }

    public override List<string> GetItemTags(Item item)
    {
        // Get tags from the Keyword field in the {item's path}/Data/Metadata item.
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

### Search Filter
The recommender filters items that have non-context languages from results. If you want to add or change filters, override the `DefaultRecommender.ApplyItemsFilter` method.

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

    // Apply additional filters.
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
