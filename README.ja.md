[English](README.md) | 日本語

<img src="img/logo-title.svg" height="120px" style="margin-left: 20px;">

# Cairngorm
*Cairngorm* は簡単に使用できるSitecore用のレコメンドライブラリです。

**注意: このライブラリは開発中なので今後APIが変わる可能性があります。**

## インストール方法
[こちら](https://github.com/xirtardauq/Cairngorm/releases)から.nupkgファイルをダウンロードし、ローカルのパッケージソースに配置してインストールしてください。

## サポート情報
このライブラリはSitecore XP 9.3.0でテストしています。検索プロバイダーはSolrを使用することを想定しています。

## 使用方法
このライブラリでは、アイテムから取得した「タグ」を元にレコメンドを作成しています。「タグ」が多く含まれているアイテムほど、レコメンドの上位に出現するようになります。

1. 以下のパッチファイルをApp_Config以下に配置します。
    ```xml
    <configuration>
      <sitecore>
        <cairngorm>
          <configuration type="Cairngorm.Configurations.RecommenderConfiguration, Cairngorm">
            <recommenders hint="list:AddRecommender">
              <recommender name="sample" type="Cairngorm.Configurations.RecommenderSetting, Cairngorm">
                <!-- レコメンダーの名前（親ノードのname属性の値を使用しています） -->
                <param desc="name">$(name)</param>

                <!-- レコメンドの対象となるアイテムのテンプレート一覧を設定しています。 -->
                <searchTemplates hint="raw:AddSearchTemplate">
                  <SampleItem>{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}</SampleItem>
                </searchTemplates>

                <!-- アイテムから「タグ」を取得するタグリゾルバーを登録しています。以下の設定では、アイテムの"Tags"フィールドの値を'|'で分割したものをタグとして扱います。 -->
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
1. これで以下のようにレコメンドを取得できるようになります。
    ```csharp
    public class SampleRecommendationRepository
    {
        protected Recommender SampleRecommender { get; }

        public SampleRecommendationRepository(IRecommenderFactory factory)
        {
            // レコメンダーを取得
            SampleRecommender = factory.GetRecommender(name: "sample");
        }

        public IModel GetModel()
        {
            // レコメンダーからレコメンドされたアイテム一覧を取得
            var recommendation = SampleRecommender.GetRecommendation(count: 5);
            return ...;
        }
    }
    ```

### 設定
サンプルのconfigファイルは[こちら](Cairngorm/App_Config/Include/Feature/Cairngorm/Cairngorm.SampleRecommender.config.example)をご覧ください。

|プロパティ名|型|説明|デフォルト値|
|:-|:-|:-|:-|
|SearchField|`string`|レコメンドを検索する際に使用するインデックスフィールドの名前です。|`"_content"`|
|SearchTemplates|`List<ID>`|レコメンドの対象となるテンプレートの一覧です。|空 (全テンプレート)|
|StoredItemsCount|`int`|クッキーに保持するアイテムの上限数です。|`10`|
|WeightPerMatching|`float`|アイテムにタグが含まれていた場合に加算される重みです。|`1.0`|
|BoostGradually|`bool`|有効化すると、最近アクセスしたアイテムほどレコメンドに反映されるようになります。|`false`|
|FilterStoredItems|`bool`|有効化すると、クッキーに含まれているアイテムがレコメンドから除外されます。|`false`|
|FilterContextItem|`bool`|有効化すると、`Context.Item`がレコメンドの対象から除外されます。|`true`|
|SearchScope|`string`|設定したアイテム以下からレコメンドを取得するようになります。|未指定（全アイテムが対象）|
|CookieInfo|`CookieInfo`|クッキーに関する情報です。詳しくは以下の表を参照してください。|テーブル参照|
|TagResolvers|`List<TagResolverBase>`|アイテムから「タグ」を取得するタグリゾルバーの一覧です。詳しくは「タグリゾルバー」サクションを参照してください。|未指定|


`CookieInfo`プロパティは以下のプロパティを設定可能です。

|プロパティ名|型|説明|デフォルト値|
|:-|:-|:-|:-|
|Name|`string`|クッキー名 (必須)|`"cairngorm"`|
|MaxAge|`int`|クッキーの`Expires`属性の設定に使用される有効期限 (秒)|`2592000` (30日)|
|Domain|`string`|クッキーの`Domain`属性の値|未指定|
|Path|`string`|クッキーのの`Path`属性の値|`"/"`|
|Secure|`bool`|クッキーの`Secure`属性の値|`true`|
|HttpOnly|`bool`|クッキーの`HttpOnly`属性の値|`true`|

### タグリゾルバー
タグリゾルバーは、アイテムから「タグ」を取得するためのクラスです。デフォルトでは以下の3つのリゾルバーが用意されています。

|名前|説明|
|:-|:-|
|`TextFieldTagResolver`|テキストフィールドからタグを取得します。`delimiter`が指定されている場合は、フィールド値を指定された文字で分割します。|
|`LinkFieldTagResolver`|リンクフィールドで参照しているアイテムのテキストフィールドからタグを取得します。`delimiter`が指定されている場合は、フィールド値を指定された文字で分割します。|
|`MultilistFieldTagResolver`|マルチリストフィールドで参照しているアイテム一覧が持つ、それぞれのテキストフィールドからタグを取得します。`delimiter`が指定されている場合は、フィールド値を指定された文字で分割します。|

以下のように`TagResolverBase`を継承したクラスを実装することで、独自のタグリゾルバーを作成することができます。

```csharp
public class MetadataKeywordsTagResolver : Cairngorm.TagResolvers.TagResolverBase
{
    public MetadataKeywordsTagResolver (XmlNode node) : base(node)
    {
    }

    public override List<string> GetItemTags(Item item)
    {
        // {itemのパス}/Data/Metadataにあるアイテムの"keywords"フィールドからタグを取得
        var metadata = item.Children["Data"].Children["Metadata"];
        return metadata["Keywords"].Split(' ').ToList();
    }
}
```

実装したタグリゾルバーは以下のようにconfigに追加することで使用できます。

```xml
<tagResolvers hint="raw:AddTagResolver">
  <resolver type="Namespace.To.MetadataKeywordsTagResolver, AssemblyName" />
</tagResolvers>
```

### カスタムフィルター
デフォルトでは、レコメンド結果は現在の言語のアイテムだけに絞り込まれます。`DefaultRecommender.ApplyItemsFilter`を変更することで、この絞込み条件を変更したり他の絞込み条件を追加したりすることができます。

```csharp
// 独自のSearchResultItemを定義（オプション）
public class MySearchResultItem : SearchResultItem
{
    public string MyProperty { get; set; }
}

public class MyRecommender : Cairngorm.Services.DefaultRecommender<MySearchResultItem>
{
    public MyRecommender(RecommenderSetting recommenderSetting) : base(recommenderSetting)
    {
    }

    // 独自の絞込み条件を追加
    protected override IQueryable<MySearchResultItem> ApplyItemsFilter(IQueryable<MySearchResultItem> query) => base.ApplyItemsFilter(query)
        .Filter(item => item.MyProperty == "My Filter")
        .Filter(item => item.TemplateName == "My Template");
}
```

次に`DefaultRecommenderFactory`がカスタマイズしたレコメンダーを返すように修正します。

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

最後にデフォルトのFactoryをカスタマイズしたものに置き換えます。これでレコメンド結果が先程追加したフィルターで絞り込まれます。

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

## 作者
- 山田 拓実 (xirtardauq@gmail.com)

## ライセンス
*Cairngorm* はMITライセンスの元でリリースされています。詳しくはLICENSE.txtファイルを御覧ください。
