# Cairngorm
*Cairngorm* is an easy-to-use recommender library for Sitecore. This software intends to be an advanced version of [TagBasedRecommender](https://github.com/xirtardauq/TagBasedRecommender).

**Warning: This repo is under development. The API is likely to change.**

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
                <!-- Set the page template that is returned from the recommender. -->
                <searchTemplates hint="raw:AddSearchTemplate">
                  <SampleItem>{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}</SampleItem>
                </searchTemplates>
                <!-- Add the tag resolvers. In the following example, the tags are contained in "Tags" field, separated by '|'. -->
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
            // Get the recommender by the name specified in the "name" attribute.
            SampleRecommender = factory.GetRecommender(name: "sample");
        }

        public IModel GetModel()
        {
            // Get the recommended items by the recommender.
            var recommendation = SampleRecommender.GetRecommendation(count: 5);
            return ...;
        }
    }
    ```

## Usage
You can see a sample configuration from [here](Cairngorm/App_Config/Include/Feature/Cairngorm/Cairngorm.config.example).

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
