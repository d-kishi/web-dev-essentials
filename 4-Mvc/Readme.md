# .Net MVCとしてWebアプリケーションを作成する

このディレクトリに作成したサンプルコードは、.NET9のMVCで作成しています。  
本稿では、MVCのView構成や属性ヘルパの使い方について学習します。

また、`../src/Web.Essentials.App`に実装されたカテゴリ管理アプリケーションを実例として、実践的なMVCアプリケーション開発で必要な知識を解説します。

## 目次

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
- [FormとModel](#formとmodel)
- [実践的なMVCアプリケーション開発](#実践的なmvcアプリケーション開発)
  - [MVCパターンの実装例](#mvcパターンの実装例)
  - [フロントエンド連携](#フロントエンド連携)
  - [実用的なUI/UX実装](#実用的なuiux実装)
  - [セキュリティ実装](#セキュリティ実装)
  - [エラーハンドリング](#エラーハンドリング)
  - [状態管理とデータフロー](#状態管理とデータフロー)
  - [Web標準準拠の意義](#web標準準拠の意義)

## プロジェクト構成

Web.Essentials.Appは、Clean Architectureパターンを採用した多層アーキテクチャで構成されています：

```
src/
├── Web.Essentials.App/          # プレゼンテーション層
│   ├── Controllers/
│   │   ├── Api/                # WebAPI用コントローラー
│   │   │   ├── CategoryApiController.cs
│   │   │   └── ProductApiController.cs
│   │   ├── Mvc/                # MVC用コントローラー
│   │   │   ├── CategoriesController.cs
│   │   │   └── ProductsController.cs
│   │   └── HomeController.cs
│   ├── Views/                  # Razorビューテンプレート
│   ├── ViewModels/             # View専用データ構造
│   ├── Services/               # アプリケーションサービス
│   ├── wwwroot/                # 静的ファイル
│   └── Program.cs              # アプリケーションエントリーポイント
├── Web.Essentials.Domain/       # ドメイン層
│   ├── Entities/               # エンティティ（Category, Product等）
│   └── Repositories/           # リポジトリインターフェース
├── Web.Essentials.Infrastructure/ # インフラ層
│   ├── Data/                   # データベースコンテキスト
│   └── Repositories/           # リポジトリ実装
└── Web.Essentials.sln          # ソリューションファイル
```

### 主要コンポーネント

- **Controllers**: ユーザーリクエストの処理とレスポンス制御
- **Views**: ユーザーインターフェースの定義（Razorテンプレート）
- **ViewModels**: View専用のデータ転送オブジェクト
- **Services**: ビジネスロジックとドメインサービス
- **Entities**: ドメインオブジェクト（Category, Product等）
- **Repositories**: データアクセス抽象化
- **wwwroot**: 静的ファイル（CSS、JavaScript、画像等）

## 実行方法

### 前提条件

- .NET 8.0 SDK以降
- VSCode（推奨拡張機能：C# Dev Kit）

### 起動手順

1. **プロジェクトのクローン/ダウンロード**
   ```bash
   # リポジトリをクローンした場合
   cd web-dev-essentials/src
   ```

2. **依存関係の復元**
   ```bash
   dotnet restore
   ```

3. **アプリケーションの起動**
   ```bash
   dotnet run --project Web.Essentials.App
   ```

4. **ブラウザでアクセス**
   ```
   http://localhost:5017
   ```

### 開発時の便利なコマンド

```bash
# ビルドのみ実行
dotnet build

# ホットリロード付きで起動（ファイル変更時に自動再起動）
dotnet watch --project Web.Essentials.App

# 特定のポートで起動
dotnet run --project Web.Essentials.App --urls "http://localhost:8080"
```

### VSCodeでの開発

1. VSCodeでsrcフォルダーを開く
2. C# Dev Kit拡張機能をインストール
3. `Ctrl+Shift+P` → "C#: Select Project" でWeb.Essentials.Appを選択
4. `F5`キーでデバッグ実行

## Viewの構成

Web.Essentials.AppのViewsディレクトリの構成を解説します。

### ディレクトリ構造

```
Views/
├── _ViewStart.cshtml         # 全Viewの初期設定
├── _ViewImports.cshtml       # 共通の名前空間とディレクティブ
├── Shared/                   # 共通ビューとパーシャルビュー
│   ├── _Layout.cshtml        # アプリケーション共通レイアウト
│   ├── _ConfirmationModal.cshtml  # 確認モーダル
│   ├── _Messages.cshtml      # メッセージ表示
│   ├── _Pagination.cshtml    # ページング
│   ├── _SearchForm.cshtml    # 検索フォーム
│   ├── _CategoryForm.cshtml  # カテゴリフォーム
│   ├── _ProductForm.cshtml   # 商品フォーム
│   └── Error.cshtml          # エラーページ
├── Home/                     # ホーム画面
│   ├── Index.cshtml          # トップページ
│   └── Privacy.cshtml        # プライバシーページ
├── Categories/               # カテゴリ管理画面
│   ├── Index.cshtml          # カテゴリ一覧
│   ├── Create.cshtml         # カテゴリ登録
│   └── Edit.cshtml           # カテゴリ編集
└── Products/                 # 商品管理画面
    ├── Index.cshtml          # 商品一覧
    ├── Create.cshtml         # 商品登録
    ├── Edit.cshtml           # 商品編集
    └── Details.cshtml        # 商品詳細
```

### 実装されている主要機能

- **カテゴリ管理**: 階層構造のカテゴリCRUD操作
- **商品管理**: 画像アップロード付き商品CRUD操作
- **検索機能**: リアルタイム検索とフィルタリング
- **モーダル操作**: 削除確認などのユーザーインタラクション
- **レスポンシブ対応**: マルチデバイス対応UI

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
Web.Essentials.Appの実装例を交えて、.NET Core/.NET 5+での現代的な書き方を解説します。

### .NET Framework 4.8との主な違い

**.NET Framework 4.8時代**：
```cshtml
@using (Html.BeginForm("Create", "Categories")) {
    @Html.LabelFor(m => m.Name)
    @Html.TextBoxFor(m => m.Name)
    @Html.ValidationMessageFor(m => m.Name)
}
```

**.NET Core/.NET 5+（現代）**：
```cshtml
<form asp-controller="Categories" asp-action="Create">
    <label asp-for="Name"></label>
    <input asp-for="Name" />
    <span asp-validation-for="Name"></span>
</form>
```

**なぜ変わったのか**：
- **直感性**: HTMLらしい記述
- **IntelliSense**: より良い補完サポート  
- **保守性**: HTMLとC#の役割分離

### 基本的な構文

- **C#コードの埋め込み**

  ```cshtml
  <!-- Web.Essentials.App/Views/Categories/Index.cshtml -->
  <p>最終更新: @DateTime.Now.ToString("yyyy/MM/dd HH:mm")</p>
  <p>カテゴリ数: @Model.Categories.Count()個</p>
  ```

- **変数の宣言と使用**

  ```cshtml
  @{
      var pageTitle = $"カテゴリ編集 - {Model.Name}";
      var isRootCategory = Model.ParentCategoryId == null;
      var hasChildren = Model.ChildCategories?.Any() == true;
  }
  
  <title>@pageTitle</title>
  <div class="category-info @(isRootCategory ? "root-category" : "child-category")">
      <!-- コンテンツ -->
  </div>
  ```

- **条件分岐（実装例）**

  ```cshtml
  <!-- Web.Essentials.App/Views/Categories/Edit.cshtml -->
  @if (Model.ChildCategories?.Any() == true)
  {
      <div class="warning-message">
          <div class="warning-icon">⚠️</div>
          <div class="warning-content">
              <strong>階層変更の制限</strong>
              <p>このカテゴリには子カテゴリが存在するため、親カテゴリの変更はできません。</p>
          </div>
      </div>
  }
  else
  {
      <div class="field-help">
          <small>親カテゴリを変更すると、階層構造が変更されます。</small>
      </div>
  }
  ```

- **ループ処理（実装例）**

  ```cshtml
  <!-- Web.Essentials.App/Views/Categories/Edit.cshtml -->
  @if (Model.ChildCategories?.Any() == true)
  {
      <div class="child-categories-list">
          @foreach (var child in Model.ChildCategories)
          {
              <div class="child-category-item">
                  <span class="child-category-name">@child.Name</span>
                  <span class="child-category-path">(@child.FullPath)</span>
                  <a asp-action="Edit" asp-route-id="@child.Id" class="btn btn-sm btn-outline">編集</a>
              </div>
          }
      </div>
  }
  
  <!-- forループでの配列インデックス活用 -->
  @for (int i = 0; i < Model.MaxLevel; i++)
  {
      <div class="level-indicator level-@i">
          <span class="level-badge">レベル @(i + 1)</span>
      </div>
  }
  ```

- **モデルの利用（型安全性）**

  ```cshtml
  <!-- Web.Essentials.App/Views/Categories/Index.cshtml -->
  @model Web.Essentials.App.ViewModels.CategoryIndexViewModel
  
  <h1>@ViewData["Title"]</h1>
  <p>検索キーワード: @Model.SearchKeyword</p>
  <p>現在のページ: @Model.CurrentPage / @Model.TotalPages</p>
  
  <!-- .NET Framework 4.8との違い: null条件演算子が使用可能 -->
  <p>商品数: @(Model.ProductCount?.ToString() ?? "不明")</p>
  ```

### .NET Core/.NET 5+の新機能

- **null条件演算子とnull合体演算子**

  ```cshtml
  <!-- .NET Framework 4.8では使用不可、.NET Core/.NET 5+で使用可能 -->
  <p>説明: @(Model.Description ?? "説明なし")</p>
  <p>更新日: @Model.UpdatedAt?.ToString("yyyy/MM/dd")</p>
  <p>子カテゴリ数: @Model.ChildCategories?.Count()</p>
  ```

- **文字列補間（String Interpolation）**

  ```cshtml
  @{
      var categoryInfo = $"カテゴリ「{Model.Name}」（レベル{Model.Level}）";
      var statusClass = $"status-{(Model.IsActive ? "active" : "inactive")}";
  }
  
  <div class="@statusClass">
      <h2>@categoryInfo</h2>
  </div>
  ```

- **パターンマッチング（C# 7.0+）**

  ```cshtml
  @{
      var displayClass = Model.ProductCount switch
      {
          0 => "empty-category",
          > 0 and <= 10 => "small-category", 
          > 10 and <= 100 => "medium-category",
          > 100 => "large-category",
          _ => "unknown-category"
      };
  }
  
  <div class="category-item @displayClass">
      <!-- カテゴリ表示 -->
  </div>
  ```

### 実用的な応用例

- **HTMLエスケープと生HTML**

  ```cshtml
  <!-- 自動エスケープ（安全） -->
  <p>カテゴリ名: @Model.Name</p>
  
  <!-- 生HTML出力（注意して使用） -->
  @if (!string.IsNullOrEmpty(Model.RichDescription))
  {
      <div class="rich-content">
          @Html.Raw(Model.RichDescription)
      </div>
  }
  
  <!-- .NET Core/.NET 5+でのHTMLエンコード -->
  <div data-category-name="@Html.Encode(Model.Name)">
      <!-- JavaScriptで安全に使用可能 -->
  </div>
  ```

- **セクションの定義（現代的な書き方）**

  ```cshtml
  <!-- Web.Essentials.App/Views/Categories/Index.cshtml -->
  @section Styles {
      <link rel="stylesheet" href="~/css/category-hierarchy.css" asp-append-version="true" />
      <style>
          .category-tree {
              /* ページ固有スタイル */
          }
      </style>
  }
  
  @section Scripts {
      <script src="~/js/categories-index.js" asp-append-version="true" defer></script>
      <script>
          // ページ固有のJavaScript初期化
          document.addEventListener('DOMContentLoaded', function() {
              setInitialSearchParams({
                  searchKeyword: '@Model.SearchKeyword',
                  page: @Model.CurrentPage
              });
          });
      </script>
  }
  ```

- **パーシャルビューの活用**

  ```cshtml
  <!-- Web.Essentials.App/Views/Categories/Index.cshtml -->
  <!-- .NET Framework 4.8: @Html.Partial("_SearchForm", Model) -->
  <!-- .NET Core/.NET 5+: -->
  <partial name="_SearchForm" model="Model.SearchParams" />
  
  <!-- 条件付きパーシャルビュー -->
  @if (Model.Categories?.Any() == true)
  {
      <partial name="_CategoryList" model="Model.Categories" />
  }
  else
  {
      <partial name="_NoDataMessage" />
  }
  ```

### ViewDataとViewBagの使い分け

```cshtml
<!-- Controller側 -->
ViewData["Title"] = "カテゴリ一覧";
ViewBag.Categories = categoryList;

<!-- View側 -->
<title>@ViewData["Title"]</title>

<!-- .NET Framework 4.8とほぼ同じだが、型安全性の観点でViewModelを推奨 -->
@model CategoryIndexViewModel  <!-- 推奨 -->
@* ViewBag.Categories の使用は最小限に *@
```

### 重要なポイント

**なぜこれらの機能が重要なのか**：
- **型安全性**: コンパイル時エラー検出
- **null安全性**: null条件演算子によるランタイムエラー回避  
- **保守性**: IntelliSenseサポートとリファクタリング安全性
- **パフォーマンス**: 文字列補間による効率的な文字列操作

**.NET Framework 4.8からの移行時の注意点**：
- `@Html.ActionLink`より`<a asp-action>`を推奨
- `@Html.BeginForm`より`<form asp-action>`を推奨
- `ViewBag`より強く型付けされたViewModelを推奨
- null条件演算子（`?.`）とnull合体演算子（`??`）の積極活用

**実装時のベストプラクティス**：
- TagHelperを使用せずにリテラル出力する場合のみRazor構文を使用
- 複雑なロジックはControllerまたはViewModelに移動
- セキュリティを考慮したHTMLエスケープの適切な使用
- パーシャルビューによるコードの再利用と可読性向上

この現代的なRazor構文により、.NET Framework時代より安全で保守しやすいViewを作成できます。

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
しかし、これは見ての通り実際に生成されるHTMLタグとcshtml上の記載が異なってしまいます。

これではRazor⇒HTMLへの変換パターンをエンジニアが覚えなければならず、直感的に製造できるものではありませんでした。  
この問題への解決として、.NET Core以降は前述のMicrosoft.AspNetCore.Mvc.TagHelpersが導入され、通常のHTMLタグに`asp-○○`と拡張属性で制御することができるようになりました。  
現代では、下記のTagHelperでレイアウトを構築するのが推奨されています。

## TagHelper

TagHelperは、.NET Coreで導入された機能で、通常のHTMLタグに`asp-`で始まる属性を追加することで、Razorエンジンが自動的に適切なHTMLを生成してくれる仕組みです。Web.Essentials.Appの実装例を交えて解説します。

### 基本的な使い方

#### フォーム関連

**フォームタグ**

```cshtml
<!-- Web.Essentials.App/Views/Categories/Create.cshtml -->
<form asp-controller="Categories" asp-action="Create" method="post">
  <!-- フォーム内容 -->
</form>
```

**なぜTagHelperのformなのか**：
- **CSRF対策自動化**: `@Html.AntiForgeryToken()`の記述が不要
- **URL生成**: ルーティング変更時の自動対応
- **型安全性**: コントローラー名とアクション名のコンパイル時チェック

**入力フィールド（基本）**

```cshtml
@model CategoryCreateViewModel

<!-- テキスト入力 -->
<input asp-for="Name" class="form-input" />
<!-- 自動生成: name="Name" id="Name" value="@Model.Name" -->

<!-- 数値入力 -->
<input asp-for="Price" type="number" step="0.01" class="form-input" />

<!-- テキストエリア -->
<textarea asp-for="Description" class="form-textarea" rows="3"></textarea>
```

モデルのプロパティに基づいて、name、id、value属性が自動設定されます。  
asp-forのvalueはモデルのプロパティになりますが、このプロパティに定義したRequiredやStringLength、DataTypeといったC#の属性に基づいてHTMLの属性も自動設定されます。

**入力フィールド（高度）**

```cshtml
<!-- ファイルアップロード（商品画像） -->
<input type="file" 
       name="ImageFiles" 
       multiple 
       accept="image/*" 
       class="form-file" 
       data-max-files="5" />

<!-- パスワード -->
<input asp-for="Password" type="password" class="form-input" />

<!-- 隠しフィールド -->
<input asp-for="Id" type="hidden" />
```

**ラベルとバリデーション**

```cshtml
<!-- Web.Essentials.App/Views/Categories/Edit.cshtml -->
<div class="form-group">
    <label asp-for="Name" class="form-label"></label>
    <!-- 自動生成: for="Name">カテゴリ名</label> (Display属性から取得) -->
    
    <input asp-for="Name" class="form-input" />
    
    <span asp-validation-for="Name" class="validation-error"></span>
    <!-- バリデーションエラー時に自動表示 -->
</div>
```

モデルの[Display(Name="...")]属性やプロパティ名からラベルテキストが生成されます。  
また、モデルのバリデーション属性に基づいてエラーメッセージが表示されます。

#### 選択系（実装例）

**ドロップダウンリスト（階層カテゴリ選択）**

```cshtml
<!-- Web.Essentials.App/Views/Categories/Create.cshtml -->
<select asp-for="ParentCategoryId" 
        asp-items="@(new SelectList(Model.ParentCategories, "Id", "FullPath"))"
        class="form-select">
    <option value="">ルートカテゴリとして作成</option>
</select>
```

asp-itemsにリスト・配列を設定する事により、選択肢を自動生成します。

**複数選択（商品のカテゴリ選択）**

```cshtml
<!-- Web.Essentials.App/Views/Products/Create.cshtml -->
<select asp-for="SelectedCategoryIds" 
        asp-items="@ViewBag.AllCategories"
        multiple 
        class="form-select category-multi-select">
</select>
```

対応するController側：
```csharp
ViewBag.AllCategories = new SelectList(categories, "Id", "FullPath");
```

**チェックボックス**

```cshtml
<!-- 単一チェックボックス -->
<div class="checkbox-group">
    <label class="checkbox-label">
        <input asp-for="IsActive" class="checkbox-input" />
        <span class="checkbox-custom"></span>
        アクティブ
    </label>
</div>

<!-- 複数チェックボックス（カテゴリ選択） -->
@for (int i = 0; i < Model.AvailableCategories.Count; i++)
{
    <div class="checkbox-group">
        <input asp-for="@Model.AvailableCategories[i].IsSelected" type="hidden" />
        <input asp-for="@Model.AvailableCategories[i].CategoryId" type="hidden" />
        <label class="checkbox-label">
            <input asp-for="@Model.AvailableCategories[i].IsSelected" class="checkbox-input" />
            <span class="checkbox-custom"></span>
            @Model.AvailableCategories[i].CategoryName
        </label>
    </div>
}
```

#### リンク関連

**アクションリンク（実装例）**

```cshtml
<!-- Web.Essentials.App/Views/Categories/Index.cshtml -->
<!-- 編集リンク -->
<a asp-controller="Categories" asp-action="Edit" asp-route-id="@category.Id" 
   class="btn btn-sm btn-warning">編集</a>

<!-- カテゴリ一覧に戻る -->
<a asp-controller="Categories" asp-action="Index" 
   class="btn btn-secondary">カテゴリ一覧に戻る</a>

<!-- 商品詳細（複数パラメータ） -->
<a asp-controller="Products" asp-action="Details" 
   asp-route-id="@product.Id" 
   asp-route-categoryId="@product.CategoryId"
   class="product-link">@product.Name</a>
```

**静的ファイルとキャッシュバスティング**

```cshtml
<!-- Web.Essentials.App/Views/Shared/_Layout.cshtml -->
<!-- CSS -->
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
<link rel="stylesheet" href="~/css/category-hierarchy.css" asp-append-version="true" />

<!-- JavaScript -->
<script src="~/js/common.js" asp-append-version="true" defer></script>
<script src="~/js/categories-index.js" asp-append-version="true" defer></script>

<!-- 画像 -->
<img asp-append-version="true" src="~/images/no-image.png" alt="画像なし" class="product-image" />
```

#### 条件付き表示とパーシャルビュー

**条件付きTagHelper**

```cshtml
<!-- Web.Essentials.App/Views/Categories/Edit.cshtml -->
<!-- 子カテゴリがある場合は親カテゴリ変更を無効化 -->
<select asp-for="ParentCategoryId" 
        asp-items="@(new SelectList(Model.ParentCategories, "Id", "FullPath"))"
        class="form-select"
        disabled="@(Model.ChildCategories?.Any() == true)">
    <option value="">ルートカテゴリとして設定</option>
</select>

@if (Model.ChildCategories?.Any() == true)
{
    <div class="field-help">
        <small class="text-warning">子カテゴリが存在するため、親カテゴリの変更は無効化されています。</small>
    </div>
}
```

**パーシャルビューでのTagHelper**

```cshtml
<!-- Web.Essentials.App/Views/Shared/_SearchForm.cshtml -->
<partial name="_SearchForm" model="Model.SearchParams" />

<!-- パーシャルビュー内部 -->
@model SearchFormViewModel
<form asp-controller="Categories" asp-action="Index" method="get" id="searchForm">
    <input asp-for="SearchKeyword" class="form-input search-input" placeholder="カテゴリ名で検索..." />
    
    <select asp-for="DeletableOnly" class="form-select">
        <option value="false">全て表示</option>
        <option value="true">削除可能のみ</option>
    </select>
    
    <button type="submit" class="btn btn-primary" data-action="perform-search">検索</button>
</form>
```

### TagHelperの高度な活用

**カスタム属性との組み合わせ**

```cshtml
<!-- Web.Essentials.App/Views/Categories/Index.cshtml -->
<button type="button" 
        class="btn btn-sm btn-danger category-delete-button" 
        asp-for="@category.Id"
        data-category-id="@category.Id" 
        data-category-name="@category.Name" 
        data-product-count="@category.ProductCount"
        data-has-children="@category.HasChildren">
    削除
</button>
```

**フォーム内の動的コンテンツ**

```cshtml
<!-- 階層レベルに応じた表示制御 -->
@for (int i = 0; i < Model.MaxLevel; i++)
{
    <div class="level-indicator level-@i" 
         style="display: @(Model.CurrentLevel >= i ? "block" : "none")">
        <label asp-for="Levels[i].Name" class="form-label">レベル @(i + 1)</label>
        <input asp-for="Levels[i].Name" class="form-input" />
        <span asp-validation-for="Levels[i].Name" class="validation-error"></span>
    </div>
}
```

### TagHelperの利点

1. **HTMLの直感性**: 通常のHTMLタグとほぼ同じ書き方
2. **型安全性**: モデルのプロパティと強く結合し、コンパイル時にエラー検出
3. **自動生成**: name、id、value属性やルーティングURLの自動生成
4. **バリデーション統合**: モデルのData Annotationsと自動連携
5. **IntelliSense**: Visual StudioやVS Codeでの補完サポート
6. **保守性**: ルーティング変更時の自動対応

### 実装時のベストプラクティス

**なぜこれらの機能が重要なのか**：
- **開発効率**: 手動でのHTML属性設定が不要
- **バグ削減**: タイプミスやリンク切れの防止
- **リファクタリング安全性**: コントローラー名変更時の自動追従
- **一貫性**: 統一されたフォーム処理パターン

**注意点**：
- `asp-for`は強く型付けされているため、プロパティ名変更時は自動で追従
- 複雑な選択肢はControllerまたはViewModelで準備
- ファイルアップロードは`enctype="multipart/form-data"`を忘れずに
- 動的なフォーム要素はJavaScriptとの連携が必要

この方法により、HTMLの可読性を保ちながら、MVCフレームワークの強力な機能を最大限活用できます。

## FormとModel

MVCにおけるFormとModelの連携は、フォームデータとC#オブジェクトの自動的なバインディングを実現します。Web.Essentials.Appのカテゴリ管理機能を例に実装パターンを解説します。

### ViewModelとEntityの違い

**Entity（ドメインオブジェクト）**：
```csharp
// Web.Essentials.Domain/Entities/Category.cs
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**ViewModel（View専用データ）**：
```csharp
// Web.Essentials.App/ViewModels/CategoryCreateViewModel.cs
public class CategoryCreateViewModel
{
    [Required(ErrorMessage = "カテゴリ名は必須です")]
    [StringLength(50, ErrorMessage = "カテゴリ名は50文字以内で入力してください")]
    [Display(Name = "カテゴリ名")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "説明は500文字以内で入力してください")]
    [Display(Name = "説明")]
    public string? Description { get; set; }

    [Display(Name = "親カテゴリ")]
    public int? ParentCategoryId { get; set; }

    // View専用のプロパティ
    public IEnumerable<CategorySelectItem> ParentCategories { get; set; } = new List<CategorySelectItem>();
}
```

**なぜViewModelが必要なのか**：
- **セキュリティ**: 内部IDやタイムスタンプなど、ユーザーが変更すべきでないデータを除外
- **バリデーション**: 画面固有の検証ルール
- **表示制御**: View専用のデータ（選択肢リストなど）

### フォームとモデルバインディング

**Controllerの実装**：
```csharp
// Web.Essentials.App/Controllers/Mvc/CategoriesController.cs
public class CategoriesController : Controller
{
    // 登録画面表示
    public async Task<IActionResult> Create()
    {
        var viewModel = await _categoryService.GetCategoryForCreateAsync();
        return View(viewModel);
    }

    // 登録処理（モデルバインディング）
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryCreateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            // バリデーションエラー時の処理
            var errorViewModel = await _categoryService.GetCategoryForCreateAsync();
            errorViewModel.Name = viewModel.Name;
            errorViewModel.Description = viewModel.Description;
            return View(errorViewModel);
        }

        var categoryId = await _categoryService.CreateCategoryAsync(viewModel);
        return RedirectToAction(nameof(Index));
    }
}
```

**Viewの実装**：
```cshtml
<!-- Web.Essentials.App/Views/Categories/Create.cshtml -->
@model Web.Essentials.App.ViewModels.CategoryCreateViewModel

<form asp-controller="Categories" asp-action="Create" method="post">
    <div class="form-group">
        <label asp-for="Name" class="form-label"></label>
        <input asp-for="Name" class="form-input" />
        <span asp-validation-for="Name" class="validation-error"></span>
    </div>

    <div class="form-group">
        <label asp-for="Description" class="form-label"></label>
        <textarea asp-for="Description" class="form-textarea" rows="3"></textarea>
        <span asp-validation-for="Description" class="validation-error"></span>
    </div>

    <div class="form-group">
        <label asp-for="ParentCategoryId" class="form-label"></label>
        <select asp-for="ParentCategoryId" 
                asp-items="@(new SelectList(Model.ParentCategories, "Id", "FullPath"))"
                class="form-select">
            <option value="">ルートカテゴリとして作成</option>
        </select>
        <span asp-validation-for="ParentCategoryId" class="validation-error"></span>
    </div>

    <button type="submit" class="btn btn-primary">登録</button>
</form>
```

### モデルバインディングの仕組み

1. **フォーム送信時**：
   - `<input asp-for="Name" />` → `viewModel.Name`
   - `<select asp-for="ParentCategoryId" />` → `viewModel.ParentCategoryId`

2. **自動バインディング**：
   - HTMLフォームのname属性とViewModelプロパティ名が一致
   - ASP.NET Coreが自動的にオブジェクトを構築

3. **バリデーション実行**：
   - `[Required]`, `[StringLength]`等の属性に基づく検証
   - `ModelState.IsValid`で結果確認

### バリデーション属性の実装例

```csharp
public class CategoryEditViewModel : ICategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "カテゴリ名は必須です")]
    [StringLength(50, ErrorMessage = "カテゴリ名は50文字以内で入力してください")]
    [Display(Name = "カテゴリ名")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "説明は500文字以内で入力してください")]
    [Display(Name = "説明")]
    public string? Description { get; set; }

    [Display(Name = "親カテゴリ")]
    public int? ParentCategoryId { get; set; }

    // 編集時に必要な追加情報
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ProductCount { get; set; }
    public IEnumerable<CategorySelectItem> ChildCategories { get; set; } = new List<CategorySelectItem>();
}
```

### 複雑なフォームの例（商品登録）

```cshtml
@model Web.Essentials.App.ViewModels.ProductCreateViewModel

<form asp-controller="Products" asp-action="Create" method="post" enctype="multipart/form-data">
    <!-- 基本情報 -->
    <input asp-for="Name" class="form-input" />
    <textarea asp-for="Description" class="form-textarea"></textarea>
    <input asp-for="Price" type="number" step="0.01" class="form-input" />

    <!-- カテゴリ選択（複数選択） -->
    <select asp-for="SelectedCategoryIds" asp-items="ViewBag.Categories" multiple class="form-select">
    </select>

    <!-- ファイルアップロード -->
    <input type="file" name="ImageFiles" multiple accept="image/*" class="form-file" />

    <button type="submit" class="btn btn-primary">登録</button>
</form>
```

**対応するViewModel**：
```csharp
public class ProductCreateViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "価格は0より大きい値を入力してください")]
    public decimal Price { get; set; }

    public List<int> SelectedCategoryIds { get; set; } = new();
    
    // ファイルアップロード用
    public List<IFormFile> ImageFiles { get; set; } = new();
}
```

### 重要なポイント

**なぜこの設計なのか**：
- **自動バインディング**: フォームデータとオブジェクトの手動マッピングが不要
- **型安全性**: コンパイル時のプロパティ名検証
- **バリデーション統合**: 属性ベースの検証ルール
- **セキュリティ**: ViewModelによる入力データ制限

**実装時の注意点**：
- ViewModelには画面で入力可能なプロパティのみ定義
- 内部ID、作成日時などはControllerで設定
- ファイルアップロードは `enctype="multipart/form-data"` が必須
- CSRF対策は `[ValidateAntiForgeryToken]` で自動化

この仕組みにより、HTMLフォームとC#オブジェクトが緊密に連携し、保守性の高いWebアプリケーションを構築できます。

---

# 実践的なMVCアプリケーション開発

`../src/Web.Essentials.App`のカテゴリ管理アプリケーションを実例として、実際のWebアプリケーション開発で必要な知識を解説します。

## MVCパターンの実装例

### なぜMVCパターンなのか

MVCパターンは**責任の分離**により、アプリケーションの保守性と拡張性を向上させます。

- **Model**: データとビジネスロジック
- **View**: ユーザーインターフェース
- **Controller**: ユーザー入力の処理とModelとViewの仲介

この分離により、画面の変更がビジネスロジックに影響しない、テストしやすいコードになります。

### 実装例：CRUDパターン

`CategoriesController.cs`では、以下のCRUD操作を実装しています：

```csharp
public class CategoriesController : Controller
{
    // 一覧表示
    public async Task<IActionResult> Index() { ... }
    
    // 新規作成画面表示
    public async Task<IActionResult> Create() { ... }
    
    // 新規作成処理
    [HttpPost]
    public async Task<IActionResult> Create(CategoryCreateViewModel viewModel) { ... }
    
    // 編集画面表示
    public async Task<IActionResult> Edit(int id) { ... }
    
    // 編集処理
    [HttpPost]
    public async Task<IActionResult> Edit(int id, CategoryEditViewModel viewModel) { ... }
    
    // 削除処理
    [HttpPost]
    [Route("Categories/Delete/{id}")]
    public async Task<IActionResult> Delete(int id) { ... }
}
```

**なぜこの構造なのか**：
- **GET/POSTの分離**: 表示と処理を明確に分ける
- **RESTful設計**: 直感的なURL構造
- **ViewModelの活用**: Viewに特化したデータ構造

### ViewModelの目的

`CategoryEditViewModel.cs`のようなViewModelは、以下の目的で使用されます：

**なぜViewModelが必要か**：
- **View特化**: 画面表示に必要なデータのみを含む
- **セキュリティ**: エンティティの内部構造を隠蔽
- **検証**: 画面固有のバリデーションルール

### ルーティング戦略

```csharp
[Route("Categories/Delete/{id}")]
public async Task<IActionResult> Delete(int id)
```

**なぜカスタムルートなのか**：
- **Ajax対応**: JavaScript から呼び出しやすいURL
- **RESTful設計**: `/Categories/Delete/123` のような直感的なパス

## フロントエンド連携

### Pure JavaScript選択の理由

`wwwroot/js/categories-index.js`では、jQueryを使わずにPure JavaScriptで実装しています。

**なぜjQuery脱却なのか**：
- **軽量性**: バンドルサイズの削減
- **標準準拠**: Web標準APIの活用
- **依存関係削減**: ライブラリ管理の簡素化
- **学習効率**: ブラウザネイティブAPIの理解

### Fetch APIによる非同期通信

```javascript
const response = await fetch('/Categories/Delete/' + categoryId, {
    method: 'POST',
    headers: {
        'X-Requested-With': 'XMLHttpRequest',
        'Accept': 'application/json'
    },
    body: formData
});
```

**なぜFetch APIなのか**：
- **Promise基盤**: async/awaitとの親和性
- **標準API**: XMLHttpRequestからの進化
- **柔軟性**: レスポンス処理の細かな制御

### RxJSによるリアクティブプログラミング

`_Layout.cshtml`ではRxJSライブラリを読み込んでいます：

```html
<script src="https://unpkg.com/rxjs@7.8.1/dist/bundles/rxjs.umd.min.js"></script>
```

**なぜRxJSなのか**：
- **非同期処理の宣言的記述**: ストリーム処理による直感的なコード
- **イベント管理**: 複雑なユーザーインタラクションの簡素化
- **データフロー制御**: リアクティブなUI更新

RxJSは検索機能のデバウンス処理やリアルタイム更新などで威力を発揮します。実装例は`categories-index.js`の`setupRealtimeSearch`関数を参照してください。

### モジュール設計

```
wwwroot/js/
├── common.js           # 共通ユーティリティ
├── categories-index.js # カテゴリ一覧画面専用
└── categories-edit.js  # カテゴリ編集画面専用
```

**なぜ責任分離なのか**：
- **保守性**: 機能ごとの独立性
- **再利用性**: 共通機能の活用
- **パフォーマンス**: 必要な機能のみ読み込み

## 実用的なUI/UX実装

### モーダル実装の目的

`Index.cshtml`の削除確認モーダル：

```html
<div id="deleteCategoryModal" class="modal">
    <div class="modal-content">
        <!-- モーダル内容 -->
    </div>
</div>
```

**なぜモーダルなのか**：
- **ユーザー体験**: 破壊的操作の確認
- **情報保持**: 現在のコンテキストを維持
- **誤操作防止**: 意図しない削除の回避

### CSS設計思想

`wwwroot/css/category-hierarchy.css`では、コンポーネント指向の設計を採用：

```css
.tree-item { /* ツリーアイテムの基本スタイル */ }
.modal { /* モーダルの基本スタイル */ }
.btn { /* ボタンの基本スタイル */ }
```

**なぜコンポーネント指向なのか**：
- **再利用性**: 同じスタイルを複数箇所で活用
- **保守性**: スタイル変更の影響範囲を限定
- **一貫性**: UI/UXの統一

### レスポンシブ対応

**なぜレスポンシブ設計なのか**：
- **マルチデバイス対応**: スマートフォン、タブレット、PC
- **ユーザビリティ**: デバイスに最適な表示
- **保守効率**: 単一コードベースでの管理

## セキュリティ実装

### CSRF攻撃対策

`_Layout.cshtml`でのAntiforgeryToken：

```html
@Html.AntiForgeryToken()
```

JavaScript側でのトークン送信：

```javascript
const formData = new FormData();
const token = getAntiForgeryToken();
if (token) {
    formData.append('__RequestVerificationToken', token);
}
```

**なぜCSRF対策が必要なのか**：
- **攻撃防止**: 悪意のあるサイトからの不正リクエスト阻止
- **信頼性確保**: 正当なユーザーのアクションのみ受け付け
- **データ保護**: 意図しないデータ変更の防止

### 入力検証の二重化

**クライアント側検証**：
```javascript
if (!nameInput.value.trim()) {
    errors.push('カテゴリ名は必須です');
    isValid = false;
}
```

**サーバー側検証**：
```csharp
if (!ModelState.IsValid) {
    return Json(new { success = false, errors = errors });
}
```

**なぜ二重化するのか**：
- **UX向上**: 即座のフィードバック（クライアント側）
- **セキュリティ**: 確実な検証（サーバー側）
- **信頼性**: JavaScriptが無効でも動作

## エラーハンドリング

### 統一されたエラー処理

`common.js`の`Notifications`オブジェクト：

```javascript
const Notifications = {
    showError(message, duration = 8000) { ... },
    showSuccess(message, duration = 5000) { ... },
    showApiError(error, duration = 8000) { ... }
}
```

**なぜ統一するのか**：
- **一貫性**: 同じ方法でのエラー表示
- **保守性**: エラー処理ロジックの中央管理
- **ユーザー体験**: 予測可能なフィードバック

### 非同期処理のエラー設計

```javascript
try {
    const response = await fetch(apiUrl);
    const result = await response.json();
    // 成功処理
} catch (error) {
    console.error('API通信エラー:', error);
    showApiError(error);
}
```

**なぜtry-catchなのか**：
- **エラー捕捉**: 予期しないエラーの適切な処理
- **ユーザー通知**: 技術的エラーをユーザーフレンドリーに変換
- **ログ記録**: 開発者向けの詳細情報保持

## 状態管理とデータフロー

### クライアント側状態管理

`categories-index.js`の状態管理：

```javascript
let currentSearchParams = {
    searchKeyword: '',
    deletableOnly: false
};
```

**なぜ状態管理が重要なのか**：
- **整合性維持**: UI状態とデータの同期
- **ユーザー体験**: 操作状態の保持
- **デバッグ**: 状態変化の追跡可能性

### サーバー・クライアント責任分離

**サーバー側責任**：
- データの永続化
- ビジネスロジック
- セキュリティ検証

**クライアント側責任**：
- UI状態管理
- ユーザーインタラクション
- 表示ロジック

**なぜ分離するのか**：
- **スケーラビリティ**: 各層の独立した拡張
- **保守性**: 関心事の分離
- **パフォーマンス**: 適切な処理分散

## Web標準準拠の意義

### ネイティブAPI活用

jQueryの代わりにネイティブAPIを使用：

```javascript
// jQuery: $('.className')
// Native: document.querySelectorAll('.className')

// jQuery: $.ajax()
// Native: fetch()

// jQuery: $(element).on('click', handler)
// Native: element.addEventListener('click', handler)
```

**なぜWeb標準準拠なのか**：
- **長期保守性**: ブラウザ標準による安定性
- **学習効率**: フレームワーク固有の記法が不要
- **互換性**: 標準APIによる高い互換性
- **パフォーマンス**: 余分なライブラリ層の除去

### ブラウザ標準APIの利点

**学習コスト削減**：
- フレームワーク固有の記法を覚える必要がない
- Web標準の知識は他のプロジェクトでも活用可能

**長期保守性**：
- ブラウザベンダーによる標準仕様のサポート
- フレームワークのバージョンアップ依存からの解放

**パフォーマンス向上**：
- ライブラリのダウンロード時間削減
- メモリ使用量の最適化

これらの実装パターンを理解することで、保守性が高く、ユーザビリティに優れたWebアプリケーションを開発できるようになります。コード詳細は `../src/Web.Essentials.App` を参照して、実際の実装を確認してください。
