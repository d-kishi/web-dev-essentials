# .Net MVCとしてWebアプリケーションを作成する

このディレクトリに作成したサンプルコードは、.NET9のMVCで作成しています。  
本稿では、MVCのView構成や属性ヘルパの使い方について学習します。

バックエンドの処理はControllerとModelを用意しただけになりますので、設計や実装方法としては参考になるものではありません。

## 目次
- [.Net MVCとしてWebアプリケーションを作成する](#net-mvcとしてwebアプリケーションを作成する)
  - [目次](#目次)
  - [プロジェクト構成](#プロジェクト構成)
  - [実行方法](#実行方法)
  - [Viewの構成](#viewの構成)
    - [ディレクトリ構造](#ディレクトリ構造)
      - [読み込み順序と役割](#読み込み順序と役割)
  - [Razor構文](#razor構文)
    - [基本的な構文](#基本的な構文)
    - [注意点](#注意点)
  - [TagHelper](#taghelper)
    - [基本的な使い方](#基本的な使い方)
      - [フォーム関連](#フォーム関連)
      - [リンク関連](#リンク関連)
      - [選択系](#選択系)
    - [TagHelperの利点](#taghelperの利点)
    - [完全な例](#完全な例)

## プロジェクト構成
- **Controllers**: ユーザーからのリクエストを処理し、適切なViewを返す
- **Views**: ユーザーインターフェースを定義するRazorテンプレート
- **Models**: データ構造を定義
- **wwwroot**: 静的ファイル（CSS、JavaScript、画像など）を格納

## 実行方法

.NETは.Net Frameworkとは異なり、VSCodeで開発することができますが今回はVisual Studioで作成しました。  
ルートディレクトリの`WebEssentials.sln`をVisualStudioで読み込み、`4-Mvc\4-Mvc.csproj`を起動すると実行できます。

## Viewの構成

Viewsディレクトリの構成を解説します。

### ディレクトリ構造
```
Views/
├─ _ViewStart.cshtml      # 全Viewの初期設定
├─ _ViewImports.cshtml    # 共通の名前空間とディレクティブ
├── Shared/
│   └── _Layout.cshtml    # 共通レイアウト
└── User/
  └── Create.cshtml       # UserコントローラのCreateアクション用View
```

#### 読み込み順序と役割

1. **_ViewImports.cshtml** (最初に読み込み)
   - 全Viewで使用する名前空間やタグヘルパーを定義
   - `@using` や `@addTagHelper` ディレクティブを記述
     - `Microsoft.AspNetCore.Mvc.Razor`は、Razorビューエンジンの機能を提供するタグヘルパ  
     これにより、`@Model`や`@Html`解いたタグヘルパの利用を宣言している
     - `Microsoft.AspNetCore.Mvc.TagHelpers`は、.Net Coreの標準タグヘルパ  
     これにより、`<input asp-for"..." />`など、`asp-○○`の属性ヘルパの利用を宣言している

2. **_ViewStart.cshtml** (各View実行前)
   - 全Viewに共通する初期設定を定義
   - 一般的には `Layout = "_Layout";` を設定してレイアウトを指定

3. **_Layout.cshtml** (レイアウトテンプレート)
   - ページの共通構造（HTML骨格、ヘッダー、フッターなど）を定義
     - Reactでいうところの共通レイアウトコンポーネント、Angularでいうところのapp.component、レガシASP.Netのマスターページに相当する
   - `@RenderBody()` でコンテンツ部分を指定
   - `<head>`内の`@await RenderSectionAsync(string name)`は、コンテンツの.cshtmlから動的に埋め込むセクションを定義している
     - コンテンツ側からは`@section {name} { ... }`と記載することで、`{ ... }`の記述を埋め込むことができる
     - 例えばUser/Create.cshtmlでは、`@section Styles { }`で"Styles"セクションにCreateページが使用するスタイルシート読み込み宣言を埋め込んでいる
     - `<head>`だけでなく、`<body>`内部や様々な場所に埋め込み可能

4. **個別のView** (実装例：User/Create.cshtml)
   - 各アクション固有のコンテンツを定義
   - レイアウトの `@RenderBody()` 部分に挿入される

この順序により、共通設定→レイアウト→個別コンテンツの階層構造が構築されます。

## Razor構文

Razorは、C#コードとHTMLをシームレスに組み合わせて記述できるテンプレートエンジンです。  
`.cshtml`ファイル内で、`@`記号を使ってC#コードを埋め込むことができます。  
これにより、動的なWebページを簡単に作成できます。

### 基本的な構文

- **C#コードの埋め込み**
  ```cshtml
  <p>@DateTime.Now</p>
  ```
  → 現在日時を表示

- **変数の宣言と使用**
  ```cshtml
  @{
      var message = "Hello, Razor!";
  }
  <p>@message</p>
  ```

- **条件分岐**
  ```cshtml
  @if (User.Identity.IsAuthenticated)
  {
      <p>ログイン中</p>
  }
  else
  {
      <p>ゲストユーザー</p>
  }
  ```

- **ループ処理**
  ```cshtml
  @for (int i = 0; i < 3; i++)
  {
      <li>Item @i</li>
  }
  ```

- **モデルの利用**
  ```cshtml
  @model UserModel
  <p>ユーザー名: @Model.UserName</p>
  ```

- **HTMLエスケープ**
  - `@`で出力した値は自動的にHTMLエスケープされます。
  - 生のHTMLを出力したい場合は`@Html.Raw()`を使います。

- **セクションの定義**
  ```cshtml
  @section Styles {
      <link rel="stylesheet" href="custom.css" />
  }
  ```

Razor構文を使うことで、C#のロジックとHTMLを直感的に組み合わせて記述できます。

### 注意点

.Net Frameworkの時代は、以下のようにRazor構文を使用して画面レイアウトを構築する必要がありました。

``` cshtml
@using (Html.BeginForm("Register", "Account", FormMethod.Post)) {
    @Html.TextBoxFor(m => m.UserName)
    @Html.TextBoxFor(m => m.Email)
    <button type="submit">登録</button>
}
```

`Html.BeginForm`は`<form>`タグに、`@Html.TextBoxFor`は`<input type="text">`タグが最終的に生成されます。  
しかし、これは見ての通り実際に生成されるHTMLタグとは異なります。

これではRazor⇒HTMLへの変換パターンをエンジニアが覚えなければならず、直感的に製造できるものではありませんでした。  
この問題への解決として、.NET Core以降は前述のMicrosoft.AspNetCore.Mvc.TagHelpersが導入され、通常のHTMLタグに`asp-○○`と拡張属性で制御することができるようになりました。  
現代では、下記のTagHelperでレイアウトを構築するのが推奨されています。

## TagHelper

TagHelperは、.NET Coreで導入された機能で、通常のHTMLタグに`asp-`で始まる属性を追加することで、Razorエンジンが自動的に適切なHTMLを生成してくれる仕組みです。

### 基本的な使い方

#### フォーム関連

**フォームタグ**
```cshtml
<form asp-controller="User" asp-action="Create" method="post">
  <!-- フォーム内容 -->
</form>
```
ポイントとして、TagHelperでformを定義した場合は適切なaction属性とCSRFトークンが自動生成されます。  
これにより、従来のようにCSRF対策で`@Html.AntiForgeryToken()`を記述する必要はありません。

**入力フィールド**
```cshtml
@model UserModel

<input asp-for="UserName" class="form-control" />
<input asp-for="Email" type="email" class="form-control" />
<input asp-for="Age" type="number" class="form-control" />
```
モデルのプロパティに基づいて、name、id、value属性が自動設定されます。
`asp-for`のvalueはモデルのプロパティになりますが、このプロパティに定義した`Required`や`StringLength`、`DataType`といったC#の属性に基づいてHTMLの属性も自動設定されます。  

**ラベル**
```cshtml
<label asp-for="UserName"></label>
<label asp-for="Email"></label>
```
モデルの`[Display(Name="...")]`属性やプロパティ名からラベルテキストが生成されます

**バリデーションメッセージ**
```cshtml
<span asp-validation-for="UserName" class="text-danger"></span>
<span asp-validation-for="Email" class="text-danger"></span>
```
モデルのバリデーション属性に基づいてエラーメッセージが表示されます

#### リンク関連

**アクションリンク**
```cshtml
<a asp-controller="User" asp-action="Index">ユーザー一覧</a>
<a asp-controller="User" asp-action="Edit" asp-route-id="123">編集</a>
```
適切なURL（/User/Index、/User/Edit/123）が生成されます

**画像タグ**
```cshtml
<img asp-append-version="true" src="~/images/logo.png" alt="ロゴ" />
```
`asp-append-version="true"`とすることで、キャッシュバスティングのためのバージョン番号が自動付与されます  
これは特に、CDN有効なWebアプリケーションでファイルを更新する場合にCDNキャッシュのクリアを気にする必要が無くなります

#### 選択系

**ドロップダウンリスト**
```cshtml
<select asp-for="CategoryId" asp-items="ViewBag.Categories" class="form-control">
  <option value="">選択してください</option>
</select>
```
→ ViewBagやモデルのSelectListItemsから選択肢が自動生成されます

### TagHelperの利点

1. **HTMLの直感性**: 通常のHTMLタグとほぼ同じ書き方
2. **型安全性**: モデルのプロパティと強く結合し、コンパイル時にエラー検出
3. **自動生成**: name、id、value属性やルーティングURLの自動生成
4. **バリデーション統合**: モデルのData Annotationsと自動連携
5. **IntelliSense**: Visual StudioやVS Codeでの補完サポート

### 完全な例

```cshtml
@model UserCreateViewModel

<form asp-controller="User" asp-action="Create" method="post">
  <div class="form-group">
    <label asp-for="UserName"></label>
    <input asp-for="UserName" class="form-control" />
    <span asp-validation-for="UserName" class="text-danger"></span>
  </div>
  
  <div class="form-group">
    <label asp-for="Email"></label>
    <input asp-for="Email" type="email" class="form-control" />
    <span asp-validation-for="Email" class="text-danger"></span>
  </div>
  
  <div class="form-group">
    <label asp-for="CategoryId"></label>
    <select asp-for="CategoryId" asp-items="ViewBag.Categories" class="form-control">
      <option value="">選択してください</option>
    </select>
    <span asp-validation-for="CategoryId" class="text-danger"></span>
  </div>
  
  <button type="submit" class="btn btn-primary">登録</button>
  <a asp-controller="User" asp-action="Index" class="btn btn-secondary">キャンセル</a>
</form>
```

この方法により、HTMLの可読性を保ちながら、MVCフレームワークの強力な機能を活用できます。
