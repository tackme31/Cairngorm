English | [日本語](README.ja.md)

<img src="img/logo-title.svg" height="120px" style="margin-left: 20px;">

# Cairngorm
*Cairngorm* is an easy-to-use recommender library for Sitecore. This library has the following features:

- The simple API.
- Work with a small configuration.
- No machine learning needed.
- Customizable.

**Warning: This repo is under development. The API is likely to change.**

## Installation
You can install the package via NuGet.

```powershell
PM> Install-Package Cairngorm
```

## Usage
The recommender searches recommended items based on "Tag", resolved from an item. The more "Tag"s an item contains, the more likely the item is recommended.

1. Install the *Cairngorm* package to your project.
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
1. Deploy the project with the configurations.
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
|TagResolvers|`List<TagResolverBase>`|Tag resolvers which resolve the tags from an item in ItemsStore. See the Tag Resolver section for more information.|No resolver.|
|ItemsStore|`ItemsStoreBase`|An items store to keep a context item in. See the Items Store section.|`CookieItemsStore`|
|WeightPerMatching|`float`|A value to be added to boosting when a tag is matched.|`1.0`|
|BoostGradually|`bool`|When enabled, the more items that are newly added to the ItemsStore, the more they will affect the recommendation results.|`false`|
|FilterStoredItems|`bool`|When enabled, items contained in the ItemsStore will be excluded from the recommendation results.|`false`|
|FilterContextItem|`bool`|When enabled, a context item is filtered from results.|`true`|
|SearchScope|`string`|A root item where it's searched from (Path or ID).|No scope.|

### Items Store
By default, a context item is stored in a cookie. This behavior is due to the `CookieItemsStore` class, which has the following properties.

|Property Name|Type|Description|Default Value|
|:-|:-|:-|:-|
|StoredItemsCount|`int`|A length of items stored in the cookie.|`10`|
|CookieInfo|`CookieInfo`|Cookie's information used for recommendation.|See the next table.|

And the `CookieInfo` property has the following properties.

|Property Name|Type|Description|Default Value|
|:-|:-|:-|:-|
|Name|`string`|Cookie's name (required).|`"recommend_items"`|
|MaxAge|`int`|A Cookie's lifetime used to set the `Expires` attribute.|`2592000` (30 days)|
|Domain|`string`|A value of the cookie's `Domain` attribute.|Empty.|
|Path|`string`|A value of the cookie's `Path` attribute.|`"/"`|
|Secure|`bool`|A value of the cookie's `Secure` attribute.|`true`|
|HttpOnly|`bool`|A value of the cookie's `HttpOnly` attribute.|`true`|

You can create a custom items store by implementing the `ItemsStoreBase` class.

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
    public MyRecommender(RecommenderSetting setting) : base(setting)
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

Finally, replace the default factory to the customized one.

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

## Compatibility
This library is only tested on Sitecore 9.3 with Solr. It should also work on Sitecore 9.x, but if it doesn't please let me know.

## Author
- Takumi Yamada (xirtardauq@gmail.com)

## License
*Cairngorm* is licensed under the MIT license. See LICENSE.txt.
