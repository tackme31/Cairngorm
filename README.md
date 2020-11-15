# Cairngorm
*Cairngorm* is an easy-to-use recommender library for Sitecore. This software intends to be an advanced ver. of [TagBasedRecommender](https://github.com/xirtardauq/TagBasedRecommender).

**Warning: This repo is under active development. The API is likely to change.**

## Getting Started
1. Apply the following configuration patch.
    ```xml
    <configuration>
      <sitecore>
        <cairngorm>
          <configuration type="Cairngorm.Configurations.RecommenderConfiguration, Cairngorm">
            <recommenders hint="list:AddRecommender">
              <!-- Add the "sample" recommender. -->
              <recommender name="sample" type="Cairngorm.Configurations.RecommenderSetting, Cairngorm">
                <param desc="name">$(name)</param>
                <!-- Add the page template that is returned from recommender. -->
                <searchTemplates hint="raw:AddSearchTemplate">
                  <SampleItem>{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}</SampleItem>
                </searchTemplates>
                <!-- Set where are tags in. The following example, the tags are contained in "Tags" field, which is separated by '|'. -->
                <tagFields hint="raw:AddTagsResolverInfo">
                  <tagField fieldName="Tags" fieldType="text" delimiter="|" />
                </tagFields>
              </recommender>
            </recommenders>
          </configuration>
        </cairngorm>
      </sitecore>
    </configuration>
    ```
1. Now you can use recommender like this:
    ```csharp
    public class SampleRecommendationRepository
    {
        protected Recommender SampleRecommender { get; }

        public SampleRecommendationRepository(IRecommenderFactory factory)
        {
            // Get recommendation by name specified in the "name" attribute.
            SampleRecommender = factory.GetRecommender(name: "sample");
        }

        public IModel GetModel()
        {
            // Get recommended items by recommender.
            var recommendation = SampleRecommender.GetRecommendation(count: 5);
            return ...;
        }
    }
    ```

## Usage
### Configuration
WIP

### For Customization
#### Tags Resolver
WIP
#### Recommendation Filter
WIP

## Author
- Takumi Yamada (xirtardauq@gmail.com)

## License
*Cairngorm* is licensed under the MIT license. See LICENSE.txt.
