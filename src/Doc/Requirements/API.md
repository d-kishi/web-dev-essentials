# API・コントローラー設計仕様書

## 目次

1. [概要](#1-概要)
   - [1.1 基本情報](#11-基本情報)
   - [1.2 アーキテクチャ分離](#12-アーキテクチャ分離)
   - [1.3 共通ヘッダー（API部分）](#13-共通ヘッダーapi部分)
2. [非同期API（Ajax通信対象）](#2-非同期apiajax通信対象)
   - [2.1 商品一覧取得 (検索・ページング対応)](#21-商品一覧取得-検索ページング対応)
   - [2.2 カテゴリ一覧取得 (階層構造・検索対応)](#22-カテゴリ一覧取得-階層構造検索対応)
3. [同期MVCコントローラー（HTMLビューレスポンス）](#3-同期mvcコントローラーhtmlビューレスポンス)
   - [3.1 商品管理コントローラー (ProductsController)](#31-商品管理コントローラー-productscontroller)
   - [3.2 カテゴリ管理コントローラー (CategoriesController)](#32-カテゴリ管理コントローラー-categoriescontroller)
   - [3.3 ホームコントローラー (HomeController)](#33-ホームコントローラー-homecontroller)
4. [共通仕様](#4-共通仕様)
   - [4.1 ビューモデル共通プロパティ](#41-ビューモデル共通プロパティ)
   - [4.2 ファイルアップロード仕様](#42-ファイルアップロード仕様)
   - [4.3 バリデーションルール](#43-バリデーションルール)
5. [非同期APIレスポンス形式](#5-非同期apiレスポンス形式)
   - [5.1 成功レスポンス](#51-成功レスポンス)
   - [5.2 エラーレスポンス](#52-エラーレスポンス)
6. [HTTPステータスコード（API部分）](#6-httpステータスコードapi部分)
7. [バリデーションエラー例（API部分）](#7-バリデーションエラー例api部分)
   - [7.1 APIバリデーションエラー例](#71-apiバリデーションエラー例)
8. [JavaScript実装例（非同期部分）](#8-javascript実装例非同期部分)
   - [8.1 商品検索機能（Fetch API + RxJS）](#81-商品検索機能fetch-api--rxjs)
   - [8.2 カテゴリ検索機能](#82-カテゴリ検索機能)
9. [MVCコントローラーエラーハンドリング](#9-mvcコントローラーエラーハンドリング)
   - [9.1 ModelStateバリデーションエラー](#91-modelstateバリデーションエラー)
   - [9.2 ビジネスロジックエラー](#92-ビジネスロジックエラー)
   - [9.3 TempDataでのメッセージ表示](#93-tempdataでのメッセージ表示)
10. [エラーハンドリング戦略](#10-エラーハンドリング戦略)
    - [10.1 クライアント側エラーハンドリング（Ajax部分のみ）](#101-クライアント側エラーハンドリングajax部分のみ)
    - [10.2 サーバー側エラーハンドリング](#102-サーバー側エラーハンドリング)

---

## 1. 概要

### 1.1 基本情報
- **非同期API**: 商品一覧取得、カテゴリ一覧取得のみ
- **同期MVC**: その他すべての機能
- **データ形式**: JSON（API）、HTML（MVC）
- **文字エンコーディング**: UTF-8
- **HTTP通信**: Fetch API + RxJS使用（非同期部分のみ）

### 1.2 アーキテクチャ分離
- **APIコントローラー** (`Microsoft.AspNetCore.Mvc.ControllerBase`継承)
  - **配置場所**: `Web.Essentials.App.Controllers.Api`
  - `/api`ベースURL
  - JSON レスポンス
  - 商品一覧、カテゴリ一覧のみ
- **MVCコントローラー** (`Microsoft.AspNetCore.Mvc.Controller`継承)
  - **配置場所**: `Web.Essentials.App.Controllers.Mvc`
  - 通常のMVCパターン
  - HTMLビュー レスポンス
  - CRUD操作全般

### 1.3 共通ヘッダー（API部分）
```http
Content-Type: application/json
Accept: application/json
```

## 2. 非同期API（Ajax通信対象）

### 2.1 商品一覧取得 (検索・ページング対応)

#### エンドポイント
```http
GET /api/products
```

#### クエリパラメータ
| パラメータ名 | 型 | 必須 | 説明 | デフォルト値 |
|-------------|---|------|------|-------------|
| page | int | ✗ | ページ番号 (1から開始) | 1 |
| pageSize | int | ✗ | 1ページあたりの件数 | 10 |
| nameTerm | string | ✗ | 商品名での部分一致検索 | - |
| janCode | string | ✗ | JANコードでの部分一致検索 | - |
| categoryId | int | ✗ | カテゴリIDでの絞り込み（階層検索対応） | - |
| status | int | ✗ | 商品ステータスでの絞り込み (0,1,2) | - |
| minPrice | uint | ✗ | 最低価格 | - |
| maxPrice | uint | ✗ | 最高価格 | - |
| sortBy | string | ✗ | ソート項目 (name, price, createdAt) | createdAt |
| sortOrder | string | ✗ | ソート順 (asc, desc) | desc |

#### リクエスト例
```http
GET /api/products?page=1&pageSize=10&nameTerm=ランニング&categoryId=33&status=1&minPrice=5000&maxPrice=20000&sortBy=price&sortOrder=asc
```

#### レスポンス例
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 15,
        "name": "中長距離用ランニングシューズ",
        "description": "中距離・長距離対応ランニングシューズ　26.5cm",
        "price": 12800,
        "janCode": "4901005005001",
        "status": 1,
        "statusName": "販売中",
        "categories": [
          {
            "id": 34,
            "name": "中距離用",
            "fullPath": "ランニング > シューズ > 中距離用"
          },
          {
            "id": 38,
            "name": "長距離用",
            "fullPath": "ランニング > シューズ > 長距離用"
          }
        ],
        "images": [
          {
            "id": 21,
            "imagePath": "/uploads/products/running_road_main.jpg",
            "altText": "ロードランニングシューズメイン",
            "isMain": true,
            "displayOrder": 1
          }
        ],
        "createdAt": "2024-01-15T10:30:00Z",
        "updatedAt": "2024-01-15T10:30:00Z"
      }
    ],
    "totalCount": 8,
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "message": null,
  "errors": null
}
```

### 2.2 カテゴリ一覧取得 (階層構造・検索対応)

#### エンドポイント
```http
GET /api/categories
```

#### クエリパラメータ
| パラメータ名 | 型 | 必須 | 説明 | デフォルト値 |
|-------------|---|------|------|-------------|
| level | int | ✗ | 階層レベルでの絞り込み (0,1,2) | - |
| parentId | int | ✗ | 親カテゴリIDでの絞り込み | - |
| nameTerm | string | ✗ | カテゴリ名での部分一致検索 | - |
| includeProductCount | bool | ✗ | 関連商品数を含むか | false |

#### リクエスト例
```http
GET /api/categories?level=2&parentId=20&includeProductCount=true
```

#### レスポンス例
```json
{
  "success": true,
  "data": [
    {
      "id": 33,
      "name": "短距離用",
      "description": "短距離ランニング用シューズ",
      "parentCategoryId": 20,
      "level": 2,
      "sortOrder": 1,
      "fullPath": "ランニング > シューズ > 短距離用",
      "productCount": 1,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    },
    {
      "id": 34,
      "name": "中距離用",
      "description": "中距離ランニング用シューズ",
      "parentCategoryId": 20,
      "level": 2,
      "sortOrder": 2,
      "fullPath": "ランニング > シューズ > 中距離用",
      "productCount": 1,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
  ],
  "message": null,
  "errors": null
}
```

## 3. 同期MVCコントローラー（HTMLビューレスポンス）

### 3.1 商品管理コントローラー (ProductsController)

#### 3.1.1 商品一覧画面
```http
GET /Products
GET /Products/Index
```
- **機能**: 商品一覧表示（Ajax検索機能付き）
- **レスポンス**: HTMLビュー
- **ビューモデル**: `ProductIndexViewModel`

#### 3.1.2 商品詳細画面
```http
GET /Products/Details/{id}
```
- **機能**: 商品詳細情報表示
- **パラメータ**: id (int) - 商品ID
- **レスポンス**: HTMLビュー
- **ビューモデル**: `ProductDetailsViewModel`

#### 3.1.3 商品登録画面 (GET)
```http
GET /Products/Create
```
- **機能**: 商品登録フォーム表示
- **レスポンス**: HTMLビュー
- **ビューモデル**: `ProductCreateViewModel`

#### 3.1.4 商品登録処理 (POST)
```http
POST /Products/Create
```
- **機能**: 商品登録処理（画像アップロード含む）
- **リクエスト**: `ProductCreateViewModel` (FormData)
- **成功時**: `RedirectToAction("Index")`
- **失敗時**: ビュー再表示（バリデーションエラー付き）

#### 3.1.5 商品編集画面 (GET)
```http
GET /Products/Edit/{id}
```
- **機能**: 商品編集フォーム表示
- **パラメータ**: id (int) - 商品ID
- **レスポンス**: HTMLビュー
- **ビューモデル**: `ProductEditViewModel`

#### 3.1.6 商品編集処理 (POST)
```http
POST /Products/Edit/{id}
```
- **機能**: 商品更新処理（画像アップロード含む）
- **パラメータ**: id (int) - 商品ID
- **リクエスト**: `ProductEditViewModel` (FormData)
- **成功時**: `RedirectToAction("Details", new { id })`
- **失敗時**: ビュー再表示

#### 3.1.7 商品削除確認画面
```http
GET /Products/Delete/{id}
```
- **機能**: 商品削除確認画面表示
- **パラメータ**: id (int) - 商品ID
- **レスポンス**: HTMLビュー
- **ビューモデル**: `ProductDetailsViewModel`

#### 3.1.8 商品削除処理 (POST)
```http
POST /Products/Delete/{id}
```
- **機能**: 商品削除処理
- **パラメータ**: id (int) - 商品ID
- **成功時**: `RedirectToAction("Index")`
- **失敗時**: エラーメッセージ付きで確認画面再表示

### 3.2 カテゴリ管理コントローラー (CategoriesController)

#### 3.2.1 カテゴリ一覧画面
```http
GET /Categories
GET /Categories/Index
```
- **機能**: カテゴリ一覧表示（階層構造、Ajax検索機能付き）
- **レスポンス**: HTMLビュー
- **ビューモデル**: `CategoryIndexViewModel`

#### 3.2.2 カテゴリ詳細画面
```http
GET /Categories/Details/{id}
```
- **機能**: カテゴリ詳細情報、関連商品一覧表示
- **パラメータ**: id (int) - カテゴリID
- **レスポンス**: HTMLビュー
- **ビューモデル**: `CategoryDetailsViewModel`

#### 3.2.3 カテゴリ登録画面 (GET)
```http
GET /Categories/Create
```
- **機能**: カテゴリ登録フォーム表示（親カテゴリ選択含む）
- **レスポンス**: HTMLビュー
- **ビューモデル**: `CategoryCreateViewModel`

#### 3.2.4 カテゴリ登録処理 (POST)
```http
POST /Categories/Create
```
- **機能**: カテゴリ登録処理
- **リクエスト**: `CategoryCreateViewModel`
- **成功時**: `RedirectToAction("Index")`
- **失敗時**: ビュー再表示

#### 3.2.5 カテゴリ編集画面・処理
```http
GET /Categories/Edit/{id}
POST /Categories/Edit/{id}
```
- **機能**: カテゴリ情報編集
- **制約**: 子カテゴリが存在する場合の親カテゴリ変更制限

#### 3.2.6 カテゴリ削除確認・処理
```http
GET /Categories/Delete/{id}
POST /Categories/Delete/{id}
```
- **機能**: カテゴリ削除処理
- **制約**: 子カテゴリまたは関連商品が存在する場合は削除不可

### 3.3 ホームコントローラー (HomeController)

#### 3.3.1 トップページ
```http
GET /
GET /Home
GET /Home/Index
```
- **機能**: サイトトップページ（新着商品、おすすめ商品表示）
- **レスポンス**: HTMLビュー
- **ビューモデル**: `HomeIndexViewModel`

## 4. 共通仕様

### 4.1 ビューモデル共通プロパティ

#### 4.1.1 ページング情報 (PagingInfo)
```csharp
public class PagingInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
```

#### 4.1.2 カテゴリ選択用 (CategorySelectItem)
```csharp
public class CategorySelectItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FullPath { get; set; }
    public int Level { get; set; }
}
```

### 4.2 ファイルアップロード仕様
- **最大ファイルサイズ**: 5MB
- **対応形式**: JPEG, PNG, GIF
- **保存先**: `/wwwroot/uploads/products/`
- **ファイル名**: `{ProductId}_{DisplayOrder}_{GUID}.{extension}`

### 4.3 バリデーションルール
- **商品名**: 必須、1-100文字
- **商品説明**: 任意、最大1000文字
- **価格**: 必須、0以上の数値
- **JANコード**: 任意、13桁の数字、重複不可
- **カテゴリ名**: 必須、1-50文字、重複不可

## 5. 非同期APIレスポンス形式

### 5.1 成功レスポンス
```json
{
  "success": true,
  "data": {}, // 実際のデータ
  "message": "成功メッセージ",
  "errors": null
}
```

### 5.2 エラーレスポンス
```json
{
  "success": false,
  "data": null,
  "message": "エラーメッセージ",
  "errors": [
    {
      "field": "フィールド名",
      "message": "エラーの詳細"
    }
  ]
}
```

## 6. HTTPステータスコード（API部分）

| ステータスコード | 説明 | 使用場面 |
|----------------|------|----------|
| 200 OK | 成功 | GET, PUT, DELETE成功時 |
| 201 Created | 作成成功 | POST成功時 |
| 400 Bad Request | リクエストエラー | バリデーションエラー |
| 404 Not Found | リソースが見つからない | 存在しないID指定時 |
| 409 Conflict | 競合エラー | 重複データ登録時 |
| 500 Internal Server Error | サーバーエラー | 予期しないエラー |

## 7. バリデーションエラー例（API部分）

### 7.1 APIバリデーションエラー例
```json
{
  "success": false,
  "data": null,
  "message": "入力データに不正があります。",
  "errors": [
    {
      "field": "Name",
      "message": "商品名は必須です。"
    },
    {
      "field": "Price",
      "message": "価格は0以上の数値を入力してください。"
    },
    {
      "field": "CategoryId",
      "message": "存在しないカテゴリが指定されました。"
    }
  ]
}
```

## 8. JavaScript実装例（非同期部分）

### 8.1 商品検索機能（Fetch API + RxJS）
```javascript
// 商品検索サービス
class ProductSearchService {
    // 商品一覧取得（検索・ページング対応）
    searchProducts(params) {
        const queryString = new URLSearchParams(params).toString();
        return from(
            fetch(`/api/products?${queryString}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            })
        ).pipe(
            switchMap(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return from(response.json());
            }),
            map(result => {
                if (!result.success) {
                    throw new Error(result.message || '検索に失敗しました');
                }
                return result.data;
            }),
            catchError(error => {
                console.error('Product Search Error:', error);
                return throwError(error);
            })
        );
    }
}

// 使用例
const productService = new ProductSearchService();
const searchParams = {
    page: 1,
    pageSize: 10,
    nameTerm: 'ランニング',
    categoryId: 33,
    status: 1,
    sortBy: 'price',
    sortOrder: 'asc'
};

productService.searchProducts(searchParams)
    .subscribe({
        next: (data) => {
            // 検索結果を画面に反映
            updateProductList(data.items);
            updatePagination(data);
        },
        error: (error) => {
            // エラーハンドリング
            showErrorMessage(error.message);
        }
    });
```

### 8.2 カテゴリ検索機能
```javascript
// カテゴリ検索サービス
class CategorySearchService {
    // カテゴリ一覧取得（階層構造対応）
    searchCategories(params) {
        const queryString = new URLSearchParams(params).toString();
        return from(
            fetch(`/api/categories?${queryString}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            })
        ).pipe(
            switchMap(response => from(response.json())),
            map(result => {
                if (!result.success) {
                    throw new Error(result.message || 'カテゴリ取得に失敗しました');
                }
                return result.data;
            }),
            catchError(error => {
                console.error('Category Search Error:', error);
                return throwError(error);
            })
        );
    }
}
```

## 9. MVCコントローラーエラーハンドリング

### 9.1 ModelStateバリデーションエラー
```csharp
// コントローラー内でのエラーハンドリング
if (!ModelState.IsValid)
{
    // バリデーションエラーがある場合はフォームを再表示
    return View(viewModel);
}
```

### 9.2 ビジネスロジックエラー
```csharp
// カテゴリ削除時のエラー例
try
{
    await categoryService.DeleteAsync(id);
    return RedirectToAction("Index");
}
catch (InvalidOperationException ex)
{
    // ビジネスルール違反エラー
    TempData["ErrorMessage"] = ex.Message;
    return RedirectToAction("Details", new { id });
}
```

### 9.3 TempDataでのメッセージ表示
```csharp
// 成功メッセージ
TempData["SuccessMessage"] = "商品が正常に登録されました。";

// エラーメッセージ
TempData["ErrorMessage"] = "商品の登録に失敗しました。";
```

## 10. エラーハンドリング戦略

### 10.1 クライアント側エラーハンドリング（Ajax部分のみ）
- **ネットワークエラー**: 再試行機能
- **APIバリデーションエラー**: 検索フォームへのエラー表示
- **サーバーエラー**: ユーザーフレンドリーなエラーメッセージ表示

### 10.2 サーバー側エラーハンドリング
- **MVCバリデーション**: ModelState + TempDataでのメッセージ表示
- **APIバリデーション**: JSONエラーレスポンス
- **ビジネスルール違反**: 異なるエラーハンドリング（MVCはTempData、APIはJSON）
- **データベースエラー**: ログ出力 + コンテキスト別エラーレスポンス