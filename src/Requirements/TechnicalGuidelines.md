# 技術ガイドライン・コーディング方針

## 目次

1. [基本方針](#1-基本方針)
2. [単一責任の原則](#2-単一責任の原則)
3. [C# コーディング規約](#3-c-コーディング規約)
4. [JavaScript コーディング規約](#4-javascript-コーディング規約)
5. [CSS コーディング規約](#5-css-コーディング規約)
6. [ファイル分離の原則](#6-ファイル分離の原則)
7. [コメント記述規約](#7-コメント記述規約)
8. [バリデーション実装方針](#8-バリデーション実装方針)
9. [エラーハンドリング方針](#9-エラーハンドリング方針)
10. [テスト実装方針](#10-テスト実装方針)
11. [パフォーマンス考慮事項](#11-パフォーマンス考慮事項)

---

## 1. 基本方針

### 1.1 教育目標
- **C# 8.0～11の新機能**を積極的に使用（.NET 8対応）
- **フロントエンド技術**（HTML, JavaScript, CSS）の基礎習得
- **クリーンアーキテクチャ**の理解と実践
- **保守性の高いコード**の作成

### 1.2 技術選択指針
- **Pure JavaScript**使用（jQueryは使用禁止）
- **RxJS**使用（API通信のみ）
- **Fetch API**使用（HTTP通信）
- **.NET Core標準タグヘルパー**を最大限活用

### 1.3 品質基準
- **初級者〜中級者向け**の理解しやすいコード
- **神経質なレベル**での詳細コメント記載
- **WSL2環境**での開発に対応

## 2. 単一責任の原則

### 2.1 基本概念
**各クラス、メソッド、ファイルは一つの責務のみを持つ**

### 2.2 クラス設計での適用

#### 2.2.1 良い例
```csharp
/// <summary>
/// 商品価格に関するビジネスロジックを担当
/// </summary>
public class ProductPriceCalculator
{
    /// <summary>
    /// 税込み価格を計算する
    /// </summary>
    public decimal CalculateIncludingTax(decimal basePrice, decimal taxRate)
    {
        return basePrice * (1 + taxRate);
    }
}

/// <summary>
/// 商品データの永続化を担当
/// </summary>
public class ProductRepository
{
    /// <summary>
    /// 商品を保存する
    /// </summary>
    public async Task SaveProductAsync(Product product)
    {
        // データベース保存処理
    }
}
```

#### 2.2.2 悪い例（修正が必要）
```csharp
/// <summary>
/// ❌ 複数の責務を持つため修正が必要
/// </summary>
public class ProductManager
{
    // ❌ 価格計算とデータ保存が混在
    public decimal CalculatePrice(decimal basePrice, decimal taxRate) { }
    public async Task SaveProduct(Product product) { }
    public string FormatProductName(string name) { }
    public void SendNotification(string message) { }
}
```

### 2.3 メソッド設計での適用

#### 2.3.1 良い例
```csharp
/// <summary>
/// 商品名の妥当性を検証する
/// </summary>
/// <param name="name">検証対象の商品名</param>
/// <returns>妥当性検証結果</returns>
private bool ValidateProductName(string name)
{
    return !string.IsNullOrWhiteSpace(name) && name.Length <= 100;
}

/// <summary>
/// 商品名をフォーマットする
/// </summary>
/// <param name="name">フォーマット対象の商品名</param>
/// <returns>フォーマット済み商品名</returns>
private string FormatProductName(string name)
{
    return name?.Trim().ToUpperInvariant();
}
```

### 2.4 ファイル分離での適用

#### 2.4.1 JavaScript ファイル分離
```
wwwroot/js/
├── products/
│   ├── products-index.js      # 商品一覧画面専用
│   ├── products-create.js     # 商品登録画面専用
│   ├── products-edit.js       # 商品編集画面専用
│   └── products-search.js     # 商品検索機能専用
├── categories/
│   ├── categories-index.js    # カテゴリ一覧画面専用
│   └── categories-create.js   # カテゴリ登録画面専用
└── common/
    ├── api-client.js          # API通信専用
    ├── pagination.js          # ページング専用
    └── validation.js          # バリデーション専用
```

#### 2.4.2 CSS ファイル分離
```
wwwroot/css/
├── products/
│   ├── products-index.css     # 商品一覧画面専用
│   ├── products-form.css      # 商品フォーム専用
│   └── products-details.css   # 商品詳細画面専用
├── categories/
│   ├── categories-index.css   # カテゴリ一覧画面専用
│   └── categories-form.css    # カテゴリフォーム専用
└── common/
    ├── layout.css             # レイアウト専用
    ├── pagination.css         # ページング専用
    └── forms.css              # フォーム共通スタイル
```

## 3. C# コーディング規約

### 3.1 命名規約

#### 3.1.1 基本規則
- **PascalCase**: クラス名、メソッド名、プロパティ名
- **camelCase**: フィールド名、ローカル変数名、パラメータ名
- **UPPER_CASE**: 定数名

#### 3.1.2 具体例
```csharp
/// <summary>
/// 商品エンティティクラス
/// </summary>
public class Product
{
    /// <summary>
    /// 商品ID（主キー）
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 商品名
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 最大商品名長
    /// </summary>
    private const int MAX_PRODUCT_NAME_LENGTH = 100;
    
    /// <summary>
    /// 商品名を検証する
    /// </summary>
    /// <param name="productName">検証対象の商品名</param>
    /// <returns>検証結果</returns>
    public bool ValidateProductName(string productName)
    {
        var trimmedName = productName?.Trim();
        return !string.IsNullOrEmpty(trimmedName) && 
               trimmedName.Length <= MAX_PRODUCT_NAME_LENGTH;
    }
}
```

### 3.2 C# 8.0～11の新機能活用

#### 3.2.1 Switch式の使用
```csharp
/// <summary>
/// 商品ステータスに応じた表示名を取得
/// </summary>
/// <param name="status">商品ステータス</param>
/// <returns>表示名</returns>
public string GetStatusDisplayName(ProductStatus status) => status switch
{
    ProductStatus.PreSale => "販売開始前",
    ProductStatus.OnSale => "販売中",
    ProductStatus.Discontinued => "取扱終了",
    _ => "不明"
};
```

#### 3.2.2 レコード型の使用
```csharp
/// <summary>
/// 商品検索条件を表すレコード
/// </summary>
/// <param name="NameTerm">商品名検索語</param>
/// <param name="CategoryId">カテゴリID</param>
/// <param name="MinPrice">最低価格</param>
/// <param name="MaxPrice">最高価格</param>
public record ProductSearchCriteria(
    string NameTerm,
    int? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice
);
```

#### 3.2.3 Null合体演算子の活用
```csharp
/// <summary>
/// 商品説明のデフォルト値設定
/// </summary>
/// <param name="description">商品説明</param>
/// <returns>設定された商品説明</returns>
public string SetDefaultDescription(string description)
{
    return description ?? "商品説明なし";
}
```

### 3.3 非同期処理の実装

#### 3.3.1 async/awaitの適切な使用
```csharp
/// <summary>
/// 商品を非同期で保存する
/// </summary>
/// <param name="product">保存対象商品</param>
/// <param name="cancellationToken">キャンセレーショントークン</param>
/// <returns>保存されたか否か</returns>
public async Task<bool> SaveProductAsync(Product product, CancellationToken cancellationToken = default)
{
    try
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
        var result = await _dbContext.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
    catch (OperationCanceledException)
    {
        // キャンセレーション処理
        throw;
    }
    catch (Exception ex)
    {
        // エラーログ出力
        _logger.LogError(ex, "商品保存処理でエラーが発生しました。ProductId: {ProductId}", product.Id);
        return false;
    }
}
```

## 4. JavaScript コーディング規約

### 4.1 基本方針
- **Pure JavaScript**のみ使用（jQueryは使用禁止）
- **ES6+**の機能を積極的に活用
- **関数型プログラミング**のスタイルを採用

### 4.2 ファイル構成例

#### 4.2.1 商品検索機能（products-index.js）
```javascript
/**
 * 商品一覧画面のJavaScript
 * 商品検索とページング機能を提供
 */

/**
 * 商品検索サービスクラス
 * API通信とデータ処理を担当
 */
class ProductSearchService {
    /**
     * コンストラクタ
     * @param {string} baseUrl - APIのベースURL
     */
    constructor(baseUrl = '/api') {
        this.baseUrl = baseUrl;
    }

    /**
     * 商品を検索する
     * @param {Object} searchParams - 検索パラメータ
     * @returns {Observable} 検索結果のObservable
     */
    searchProducts(searchParams) {
        const queryString = new URLSearchParams(searchParams).toString();
        const url = `${this.baseUrl}/products?${queryString}`;
        
        return rxjs.from(
            fetch(url, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            })
        ).pipe(
            rxjs.operators.switchMap(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return rxjs.from(response.json());
            }),
            rxjs.operators.map(result => {
                if (!result.success) {
                    throw new Error(result.message || '検索に失敗しました');
                }
                return result.data;
            }),
            rxjs.operators.catchError(error => {
                console.error('Product Search Error:', error);
                return rxjs.throwError(error);
            })
        );
    }
}

/**
 * 商品一覧画面のUI制御クラス
 * DOM操作とイベント処理を担当
 */
class ProductIndexController {
    /**
     * コンストラクタ
     */
    constructor() {
        this.productService = new ProductSearchService();
        this.searchForm = document.getElementById('search-form');
        this.productList = document.getElementById('product-list');
        this.pagination = document.getElementById('pagination');
        this.loadingIndicator = document.getElementById('loading');
        
        this.initializeEventListeners();
    }

    /**
     * イベントリスナーを初期化する
     */
    initializeEventListeners() {
        // 検索フォームの送信イベント
        this.searchForm?.addEventListener('submit', (event) => {
            event.preventDefault();
            this.performSearch();
        });

        // リアルタイム検索（入力後500ms後に実行）
        const searchInput = document.getElementById('search-input');
        if (searchInput) {
            rxjs.fromEvent(searchInput, 'input').pipe(
                rxjs.operators.debounceTime(500),
                rxjs.operators.distinctUntilChanged(),
                rxjs.operators.switchMap(() => this.performSearch())
            ).subscribe();
        }
    }

    /**
     * 検索を実行する
     * @returns {Observable} 検索処理のObservable
     */
    performSearch() {
        const formData = new FormData(this.searchForm);
        const searchParams = Object.fromEntries(formData.entries());
        
        this.showLoading(true);
        
        return this.productService.searchProducts(searchParams).pipe(
            rxjs.operators.tap(data => {
                this.updateProductList(data.items);
                this.updatePagination(data);
                this.showLoading(false);
            }),
            rxjs.operators.catchError(error => {
                this.showError(error.message);
                this.showLoading(false);
                return rxjs.EMPTY;
            })
        );
    }

    /**
     * 商品一覧を更新する
     * @param {Array} products - 商品データ配列
     */
    updateProductList(products) {
        if (!this.productList) return;

        this.productList.innerHTML = products.map(product => `
            <div class="product-item" data-product-id="${product.id}">
                <div class="product-image">
                    ${product.images && product.images.length > 0 
                        ? `<img src="${product.images[0].imagePath}" alt="${product.images[0].altText}">`
                        : '<div class="no-image">画像なし</div>'
                    }
                </div>
                <div class="product-info">
                    <h3 class="product-name">${this.escapeHtml(product.name)}</h3>
                    <p class="product-price">¥${product.price.toLocaleString()}</p>
                    <p class="product-status">${product.statusName}</p>
                </div>
                <div class="product-actions">
                    <a href="/Products/Details/${product.id}" class="btn btn-primary">詳細</a>
                    <a href="/Products/Edit/${product.id}" class="btn btn-secondary">編集</a>
                </div>
            </div>
        `).join('');
    }

    /**
     * HTMLエスケープ処理
     * @param {string} text - エスケープ対象テキスト
     * @returns {string} エスケープ済みテキスト
     */
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    /**
     * ローディング表示を制御する
     * @param {boolean} show - 表示するかどうか
     */
    showLoading(show) {
        if (this.loadingIndicator) {
            this.loadingIndicator.style.display = show ? 'block' : 'none';
        }
    }

    /**
     * エラーメッセージを表示する
     * @param {string} message - エラーメッセージ
     */
    showError(message) {
        // エラー表示処理
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger';
        errorDiv.textContent = message;
        
        const container = document.querySelector('.content-container');
        if (container) {
            container.insertBefore(errorDiv, container.firstChild);
            
            // 5秒後に自動削除
            setTimeout(() => {
                errorDiv.remove();
            }, 5000);
        }
    }
}

// DOM読み込み完了後に初期化
document.addEventListener('DOMContentLoaded', () => {
    new ProductIndexController();
});
```

### 4.3 RxJS使用方針

#### 4.3.1 API通信でのみ使用
```javascript
/**
 * RxJSはAPI通信処理でのみ使用する
 * DOM操作やその他の処理では通常のJavaScriptを使用
 */

// ✅ 良い例：API通信でRxJS使用
const searchProducts = (params) => {
    return rxjs.from(fetch('/api/products', { ... })).pipe(
        rxjs.operators.switchMap(response => rxjs.from(response.json())),
        rxjs.operators.map(data => data.items)
    );
};

// ❌ 悪い例：DOM操作でRxJS使用（避ける）
// rxjs.fromEvent(button, 'click').subscribe(() => { ... });

// ✅ 良い例：DOM操作は通常のJavaScript
button.addEventListener('click', () => { ... });
```

## 5. CSS コーディング規約

### 5.1 BEM記法の採用
```css
/* 商品カード用スタイル（products-index.css） */

/**
 * 商品カードのベーススタイル
 */
.product-card {
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 16px;
    margin-bottom: 16px;
    background-color: #fff;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

/**
 * 商品画像エリア
 */
.product-card__image {
    width: 100%;
    height: 200px;
    object-fit: cover;
    border-radius: 4px;
    margin-bottom: 12px;
}

/**
 * 商品情報エリア
 */
.product-card__info {
    padding: 8px 0;
}

/**
 * 商品名
 */
.product-card__name {
    font-size: 1.2em;
    font-weight: bold;
    margin-bottom: 8px;
    color: #333;
}

/**
 * 商品価格
 */
.product-card__price {
    font-size: 1.1em;
    color: #e74c3c;
    font-weight: bold;
}

/**
 * アクションボタンエリア
 */
.product-card__actions {
    display: flex;
    gap: 8px;
    margin-top: 12px;
}

/**
 * 商品カード：ホバー状態
 */
.product-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
    transition: all 0.2s ease-in-out;
}
```

### 5.2 レスポンシブデザイン
```css
/* レスポンシブ対応（products-index.css） */

/**
 * デスクトップ用スタイル（1200px以上）
 */
@media (min-width: 1200px) {
    .product-grid {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 20px;
    }
}

/**
 * タブレット用スタイル（768px-1199px）
 */
@media (min-width: 768px) and (max-width: 1199px) {
    .product-grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 16px;
    }
}

/**
 * スマートフォン用スタイル（767px以下）
 */
@media (max-width: 767px) {
    .product-grid {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 12px;
    }
    
    .product-card__name {
        font-size: 1em;
    }
    
    .product-card__actions {
        flex-direction: column;
    }
}
```

## 6. ファイル分離の原則

### 6.1 JavaScript分離規則

#### 6.1.1 画面別分離
```html
<!-- 商品一覧画面（Products/Index.cshtml） -->
@section Scripts {
    <!-- ❌ インライン記述は禁止 -->
    <!-- <script>function searchProducts() { ... }</script> -->
    
    <!-- ✅ 外部ファイルに分離 -->
    <script src="~/js/products/products-index.js"></script>
}
```

#### 6.1.2 機能別分離
```javascript
// ❌ 悪い例：すべての機能を1ファイルに記述
// products.js (避けるべき構成)
// - 商品検索
// - 商品登録
// - 商品編集
// - 画像アップロード
// - バリデーション

// ✅ 良い例：機能別に分離
// products-index.js    (商品一覧・検索専用)
// products-create.js   (商品登録専用)
// products-edit.js     (商品編集専用)
// image-upload.js      (画像アップロード専用)
// validation.js        (バリデーション専用)
```

### 6.2 CSS分離規則

#### 6.2.1 画面別分離
```html
<!-- 商品登録画面（Products/Create.cshtml） -->
@section Styles {
    <!-- ✅ 画面専用スタイル -->
    <link rel="stylesheet" href="~/css/products/products-form.css" />
    <link rel="stylesheet" href="~/css/common/image-upload.css" />
}
```

#### 6.2.2 コンポーネント別分離
```css
/* ❌ 悪い例：すべてのスタイルをmain.cssに記述 */

/* ✅ 良い例：コンポーネント別に分離 */
/* pagination.css - ページング専用 */
.pagination { ... }
.pagination__item { ... }
.pagination__link { ... }

/* modal.css - モーダル専用 */
.modal { ... }
.modal__content { ... }
.modal__close { ... }

/* form.css - フォーム専用 */
.form-group { ... }
.form-control { ... }
.form-error { ... }
```

## 7. コメント記述規約

### 7.1 C# XMLドキュメントコメント

#### 7.1.1 クラスコメント
```csharp
/// <summary>
/// 商品管理のアプリケーションサービスクラス
/// 商品のCRUD操作と検索機能を提供します。
/// ドメインサービスとリポジトリを組み合わせてビジネスロジックを実行します。
/// </summary>
/// <remarks>
/// このクラスは以下の責務を担います：
/// - 商品の登録、更新、削除
/// - 商品の検索とページング
/// - 商品画像の管理
/// - バリデーション処理
/// </remarks>
public class ProductApplicationService
{
    // ...
}
```

#### 7.1.2 メソッドコメント
```csharp
/// <summary>
/// 商品を検索し、ページングされた結果を返します
/// </summary>
/// <param name="searchCriteria">検索条件オブジェクト</param>
/// <param name="pageNumber">ページ番号（1から開始）</param>
/// <param name="pageSize">1ページあたりの表示件数</param>
/// <param name="cancellationToken">処理のキャンセレーション用トークン</param>
/// <returns>検索結果とページング情報を含むオブジェクト</returns>
/// <exception cref="ArgumentNullException">searchCriteriaがnullの場合</exception>
/// <exception cref="ArgumentException">pageNumberまたはpageSizeが不正な値の場合</exception>
/// <example>
/// <code>
/// var criteria = new ProductSearchCriteria("ランニング", null, 1000, 10000);
/// var result = await service.SearchProductsAsync(criteria, 1, 10);
/// </code>
/// </example>
public async Task<PagedResult<ProductDto>> SearchProductsAsync(
    ProductSearchCriteria searchCriteria,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
{
    // 引数の妥当性検証
    if (searchCriteria == null)
        throw new ArgumentNullException(nameof(searchCriteria));
        
    if (pageNumber < 1)
        throw new ArgumentException("ページ番号は1以上である必要があります", nameof(pageNumber));
        
    if (pageSize < 1 || pageSize > 100)
        throw new ArgumentException("ページサイズは1以上100以下である必要があります", nameof(pageSize));

    // 検索処理の実行
    var products = await _productRepository.SearchAsync(searchCriteria, cancellationToken);
    
    // ページング処理
    var pagedProducts = products
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToList();
    
    // DTOへの変換
    var productDtos = pagedProducts.Select(p => _mapper.Map<ProductDto>(p)).ToList();
    
    // 結果オブジェクトの作成
    return new PagedResult<ProductDto>
    {
        Items = productDtos,
        TotalCount = products.Count(),
        CurrentPage = pageNumber,
        PageSize = pageSize
    };
}
```

#### 7.1.3 インターフェース継承時のコメント
```csharp
/// <summary>
/// 商品リポジトリのインターフェース
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// 商品を非同期で保存します
    /// </summary>
    /// <param name="product">保存対象の商品エンティティ</param>
    /// <param name="cancellationToken">キャンセレーション用トークン</param>
    /// <returns>保存結果を表すタスク</returns>
    Task SaveAsync(Product product, CancellationToken cancellationToken = default);
}

/// <summary>
/// 商品リポジトリの実装クラス
/// </summary>
public class ProductRepository : IProductRepository
{
    /// <inheritdoc />
    public async Task SaveAsync(Product product, CancellationToken cancellationToken = default)
    {
        // 実装処理
        // インターフェースのコメントを継承するため、/// <inheritdoc />を使用
    }
}
```

### 7.2 JavaScript JSDocコメント

#### 7.2.1 関数コメント
```javascript
/**
 * 商品データをAPIから取得する
 * @async
 * @function fetchProducts
 * @param {Object} searchParams - 検索パラメータオブジェクト
 * @param {string} [searchParams.nameTerm] - 商品名での部分一致検索
 * @param {number} [searchParams.categoryId] - カテゴリIDでの絞り込み
 * @param {number} [searchParams.page=1] - ページ番号
 * @param {number} [searchParams.pageSize=10] - 1ページあたりの件数
 * @returns {Promise<Object>} 検索結果を含むPromise
 * @throws {Error} API通信でエラーが発生した場合
 * @example
 * // 基本的な使用例
 * const result = await fetchProducts({ nameTerm: 'ランニング', page: 1 });
 * console.log(result.items); // 商品配列
 * 
 * @example
 * // エラーハンドリング付きの使用例
 * try {
 *     const result = await fetchProducts({ categoryId: 5 });
 *     updateProductList(result.items);
 * } catch (error) {
 *     showErrorMessage(error.message);
 * }
 */
async function fetchProducts(searchParams) {
    // APIエンドポイントの構築
    const queryString = new URLSearchParams(searchParams).toString();
    const url = `/api/products?${queryString}`;
    
    try {
        // APIリクエストの実行
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            }
        });
        
        // レスポンスの妥当性確認
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        // JSONデータの取得
        const data = await response.json();
        
        // APIレスポンスの構造確認
        if (!data.success) {
            throw new Error(data.message || 'API処理に失敗しました');
        }
        
        return data.data;
        
    } catch (error) {
        // エラーログの出力
        console.error('商品取得処理でエラーが発生:', error);
        throw error;
    }
}
```

#### 7.2.2 クラスコメント
```javascript
/**
 * 商品一覧画面の制御を行うクラス
 * DOM操作、イベント処理、API通信を統合管理します
 * 
 * @class ProductIndexController
 * @example
 * // 基本的な使用例
 * const controller = new ProductIndexController();
 * 
 * @example
 * // カスタム設定での使用例
 * const controller = new ProductIndexController({
 *     apiBaseUrl: '/custom-api',
 *     pageSize: 20
 * });
 */
class ProductIndexController {
    /**
     * ProductIndexControllerのコンストラクタ
     * @param {Object} [options={}] - 設定オプション
     * @param {string} [options.apiBaseUrl='/api'] - APIのベースURL
     * @param {number} [options.pageSize=10] - デフォルトページサイズ
     * @param {boolean} [options.enableRealTimeSearch=true] - リアルタイム検索の有効/無効
     */
    constructor(options = {}) {
        // デフォルト設定の適用
        this.options = {
            apiBaseUrl: '/api',
            pageSize: 10,
            enableRealTimeSearch: true,
            ...options
        };
        
        // DOM要素の取得
        this.searchForm = document.getElementById('search-form');
        this.productList = document.getElementById('product-list');
        this.pagination = document.getElementById('pagination');
        
        // サービスクラスの初期化
        this.productService = new ProductSearchService(this.options.apiBaseUrl);
        
        // 初期化処理の実行
        this.initialize();
    }
    
    // ... その他のメソッド
}
```

## 8. バリデーション実装方針

### 8.1 サーバーサイドバリデーション

#### 8.1.1 ViewModelでのバリデーション
```csharp
/// <summary>
/// 商品作成用ビューモデル
/// フォーム入力値の検証とバインディングを担当
/// </summary>
public class ProductCreateViewModel
{
    /// <summary>
    /// 商品名
    /// </summary>
    [Required(ErrorMessage = "商品名は必須です。")]
    [StringLength(100, ErrorMessage = "商品名は100文字以内で入力してください。")]
    [Display(Name = "商品名")]
    public string Name { get; set; }

    /// <summary>
    /// 商品説明
    /// </summary>
    [StringLength(1000, ErrorMessage = "商品説明は1000文字以内で入力してください。")]
    [Display(Name = "商品説明")]
    public string Description { get; set; }

    /// <summary>
    /// 価格
    /// </summary>
    [Required(ErrorMessage = "価格は必須です。")]
    [Range(0, uint.MaxValue, ErrorMessage = "価格は0以上の値を入力してください。")]
    [Display(Name = "価格")]
    public uint Price { get; set; }

    /// <summary>
    /// JANコード
    /// </summary>
    [RegularExpression(@"^\d{13}$", ErrorMessage = "JANコードは13桁の数字で入力してください。")]
    [Display(Name = "JANコード")]
    public string JanCode { get; set; }

    /// <summary>
    /// アップロード画像ファイル
    /// </summary>
    [Display(Name = "商品画像")]
    public List<IFormFile> ImageFiles { get; set; } = new List<IFormFile>();
}
```

#### 8.1.2 カスタムバリデーション属性
```csharp
/// <summary>
/// 画像ファイルの妥当性を検証するカスタムバリデーション属性
/// </summary>
public class ImageFileValidationAttribute : ValidationAttribute
{
    /// <summary>
    /// 最大ファイルサイズ（バイト）
    /// </summary>
    public long MaxFileSize { get; set; } = 5 * 1024 * 1024; // 5MB

    /// <summary>
    /// 許可されるファイル拡張子
    /// </summary>
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif" };

    /// <summary>
    /// バリデーション処理を実行
    /// </summary>
    /// <param name="value">検証対象の値</param>
    /// <param name="validationContext">バリデーションコンテキスト</param>
    /// <returns>バリデーション結果</returns>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // null値は許可（RequiredAttributeで別途チェック）
        if (value == null)
            return ValidationResult.Success;

        // IFormFileリストとして処理
        if (value is List<IFormFile> files)
        {
            foreach (var file in files)
            {
                // ファイルサイズの検証
                if (file.Length > MaxFileSize)
                {
                    return new ValidationResult($"ファイルサイズは{MaxFileSize / (1024 * 1024)}MB以下にしてください。");
                }

                // ファイル拡張子の検証
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    return new ValidationResult($"許可されるファイル形式は{string.Join(", ", AllowedExtensions)}です。");
                }
            }
        }

        return ValidationResult.Success;
    }
}
```

### 8.2 クライアントサイドバリデーション

#### 8.2.1 リアルタイムバリデーション
```javascript
/**
 * フォームバリデーションクラス
 * リアルタイムでの入力値検証を提供
 */
class FormValidator {
    /**
     * コンストラクタ
     * @param {HTMLFormElement} form - 対象フォーム
     */
    constructor(form) {
        this.form = form;
        this.rules = new Map();
        this.errors = new Map();
        
        this.setupEventListeners();
    }

    /**
     * バリデーションルールを追加
     * @param {string} fieldName - フィールド名
     * @param {Array} rules - バリデーションルール配列
     * @example
     * validator.addRule('name', [
     *     { type: 'required', message: '商品名は必須です' },
     *     { type: 'maxLength', value: 100, message: '100文字以内で入力してください' }
     * ]);
     */
    addRule(fieldName, rules) {
        this.rules.set(fieldName, rules);
    }

    /**
     * イベントリスナーのセットアップ
     */
    setupEventListeners() {
        // フォーム内の全入力要素に対してバリデーションを設定
        const inputs = this.form.querySelectorAll('input, textarea, select');
        
        inputs.forEach(input => {
            // フォーカスアウト時のバリデーション
            input.addEventListener('blur', () => {
                this.validateField(input.name, input.value);
            });

            // 入力時のリアルタイムバリデーション（デバウンス処理）
            let timeoutId;
            input.addEventListener('input', () => {
                clearTimeout(timeoutId);
                timeoutId = setTimeout(() => {
                    this.validateField(input.name, input.value);
                }, 300);
            });
        });
    }

    /**
     * 単一フィールドのバリデーション
     * @param {string} fieldName - フィールド名
     * @param {string} value - 検証値
     * @returns {boolean} バリデーション結果
     */
    validateField(fieldName, value) {
        const rules = this.rules.get(fieldName);
        if (!rules) return true;

        const errors = [];

        // 各ルールの検証実行
        for (const rule of rules) {
            const result = this.executeRule(rule, value);
            if (!result.isValid) {
                errors.push(result.message);
            }
        }

        // エラー状態の更新
        if (errors.length > 0) {
            this.errors.set(fieldName, errors);
            this.showFieldError(fieldName, errors[0]); // 最初のエラーのみ表示
            return false;
        } else {
            this.errors.delete(fieldName);
            this.clearFieldError(fieldName);
            return true;
        }
    }

    /**
     * バリデーションルールの実行
     * @param {Object} rule - バリデーションルール
     * @param {string} value - 検証値
     * @returns {Object} バリデーション結果
     */
    executeRule(rule, value) {
        switch (rule.type) {
            case 'required':
                return {
                    isValid: value.trim() !== '',
                    message: rule.message || 'この項目は必須です'
                };

            case 'maxLength':
                return {
                    isValid: value.length <= rule.value,
                    message: rule.message || `${rule.value}文字以内で入力してください`
                };

            case 'minLength':
                return {
                    isValid: value.length >= rule.value,
                    message: rule.message || `${rule.value}文字以上で入力してください`
                };

            case 'pattern':
                const regex = new RegExp(rule.pattern);
                return {
                    isValid: regex.test(value),
                    message: rule.message || '入力形式が正しくありません'
                };

            case 'numeric':
                return {
                    isValid: !isNaN(value) && !isNaN(parseFloat(value)),
                    message: rule.message || '数値を入力してください'
                };

            default:
                return { isValid: true, message: '' };
        }
    }

    /**
     * フィールドエラーの表示
     * @param {string} fieldName - フィールド名
     * @param {string} errorMessage - エラーメッセージ
     */
    showFieldError(fieldName, errorMessage) {
        const field = this.form.querySelector(`[name="${fieldName}"]`);
        if (!field) return;

        // 既存のエラーメッセージを削除
        this.clearFieldError(fieldName);

        // エラースタイルの適用
        field.classList.add('is-invalid');

        // エラーメッセージの表示
        const errorElement = document.createElement('div');
        errorElement.className = 'invalid-feedback';
        errorElement.textContent = errorMessage;
        errorElement.dataset.field = fieldName;

        field.parentNode.appendChild(errorElement);
    }

    /**
     * フィールドエラーのクリア
     * @param {string} fieldName - フィールド名
     */
    clearFieldError(fieldName) {
        const field = this.form.querySelector(`[name="${fieldName}"]`);
        if (!field) return;

        // エラースタイルの削除
        field.classList.remove('is-invalid');

        // エラーメッセージの削除
        const errorElement = field.parentNode.querySelector(`[data-field="${fieldName}"]`);
        if (errorElement) {
            errorElement.remove();
        }
    }

    /**
     * フォーム全体のバリデーション
     * @returns {boolean} バリデーション結果
     */
    validateForm() {
        let isValid = true;
        const formData = new FormData(this.form);

        // 全フィールドのバリデーション実行
        for (const [fieldName] of this.rules) {
            const value = formData.get(fieldName) || '';
            if (!this.validateField(fieldName, value)) {
                isValid = false;
            }
        }

        return isValid;
    }
}

// 使用例
document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('productForm');
    if (form) {
        const validator = new FormValidator(form);
        
        // バリデーションルールの設定
        validator.addRule('Name', [
            { type: 'required', message: '商品名は必須です' },
            { type: 'maxLength', value: 100, message: '商品名は100文字以内で入力してください' }
        ]);
        
        validator.addRule('Price', [
            { type: 'required', message: '価格は必須です' },
            { type: 'numeric', message: '価格は数値で入力してください' }
        ]);
        
        validator.addRule('JanCode', [
            { type: 'pattern', pattern: '^\\d{13}$', message: 'JANコードは13桁の数字で入力してください' }
        ]);

        // フォーム送信時のバリデーション
        form.addEventListener('submit', (event) => {
            if (!validator.validateForm()) {
                event.preventDefault();
                event.stopPropagation();
            }
        });
    }
});
```

## 9. エラーハンドリング方針

### 9.1 例外処理の階層化

#### 9.1.1 Domain層での例外定義
```csharp
/// <summary>
/// ドメイン固有の例外基底クラス
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">例外メッセージ</param>
    protected DomainException(string message) : base(message) { }
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">例外メッセージ</param>
    /// <param name="innerException">内部例外</param>
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// ビジネスルール違反例外
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">ビジネスルール違反の詳細</param>
    public BusinessRuleViolationException(string message) : base(message) { }
}

/// <summary>
/// エンティティが見つからない場合の例外
/// </summary>
public class EntityNotFoundException : DomainException
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="entityType">エンティティ型</param>
    /// <param name="entityId">エンティティID</param>
    public EntityNotFoundException(string entityType, object entityId) 
        : base($"{entityType} (ID: {entityId}) が見つかりませんでした。") { }
}
```

#### 9.1.2 Application層でのエラーハンドリング
```csharp
/// <summary>
/// 商品アプリケーションサービス
/// </summary>
public class ProductApplicationService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductApplicationService> _logger;

    public ProductApplicationService(
        IProductRepository productRepository,
        ILogger<ProductApplicationService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// 商品を作成する
    /// </summary>
    /// <param name="createModel">商品作成モデル</param>
    /// <returns>作成された商品のID</returns>
    /// <exception cref="BusinessRuleViolationException">ビジネスルール違反の場合</exception>
    public async Task<int> CreateProductAsync(ProductCreateViewModel createModel)
    {
        try
        {
            // ビジネスルールの検証
            await ValidateProductBusinessRules(createModel);

            // エンティティの作成
            var product = new Product
            {
                Name = createModel.Name,
                Description = createModel.Description,
                Price = createModel.Price,
                JanCode = createModel.JanCode,
                Status = ProductStatus.PreSale,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 保存処理
            var productId = await _productRepository.CreateAsync(product);
            
            _logger.LogInformation("商品が正常に作成されました。ProductId: {ProductId}, Name: {ProductName}", 
                productId, product.Name);
            
            return productId;
        }
        catch (BusinessRuleViolationException)
        {
            // ビジネスルール違反は再スロー
            throw;
        }
        catch (Exception ex)
        {
            // 予期しないエラーをログ出力してから再スロー
            _logger.LogError(ex, "商品作成処理で予期しないエラーが発生しました。ProductName: {ProductName}", 
                createModel.Name);
            throw;
        }
    }

    /// <summary>
    /// 商品のビジネスルールを検証
    /// </summary>
    /// <param name="createModel">商品作成モデル</param>
    /// <exception cref="BusinessRuleViolationException">ビジネスルール違反の場合</exception>
    private async Task ValidateProductBusinessRules(ProductCreateViewModel createModel)
    {
        // JANコードの重複チェック
        if (!string.IsNullOrEmpty(createModel.JanCode))
        {
            var existingProduct = await _productRepository.FindByJanCodeAsync(createModel.JanCode);
            if (existingProduct != null)
            {
                throw new BusinessRuleViolationException($"JANコード '{createModel.JanCode}' は既に使用されています。");
            }
        }

        // その他のビジネスルール検証...
    }
}
```

#### 9.1.3 Controller層でのエラーハンドリング
```csharp
/// <summary>
/// 商品管理コントローラー
/// </summary>
public class ProductsController : Controller
{
    private readonly IProductApplicationService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductApplicationService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// 商品作成処理
    /// </summary>
    /// <param name="model">商品作成ビューモデル</param>
    /// <returns>処理結果</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel model)
    {
        // モデルバリデーションの確認
        if (!ModelState.IsValid)
        {
            // バリデーションエラーがある場合はフォームを再表示
            return View(model);
        }

        try
        {
            // 商品作成処理の実行
            var productId = await _productService.CreateProductAsync(model);
            
            // 成功メッセージの設定
            TempData["SuccessMessage"] = "商品が正常に登録されました。";
            
            // 詳細画面にリダイレクト
            return RedirectToAction(nameof(Details), new { id = productId });
        }
        catch (BusinessRuleViolationException ex)
        {
            // ビジネスルール違反エラーをModelStateに追加
            ModelState.AddModelError(string.Empty, ex.Message);
            
            _logger.LogWarning("商品作成でビジネスルール違反: {ErrorMessage}, ProductName: {ProductName}", 
                ex.Message, model.Name);
            
            return View(model);
        }
        catch (Exception ex)
        {
            // 予期しないエラーの処理
            _logger.LogError(ex, "商品作成処理で予期しないエラーが発生しました");
            
            // エラーメッセージの設定
            TempData["ErrorMessage"] = "商品の登録処理中にエラーが発生しました。時間をおいて再度お試しください。";
            
            return View(model);
        }
    }
}
```

### 9.2 API用エラーハンドリング

#### 9.2.1 APIレスポンス用エラーハンドリング
```csharp
/// <summary>
/// 商品API コントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsApiController : ControllerBase
{
    private readonly IProductApplicationService _productService;
    private readonly ILogger<ProductsApiController> _logger;

    public ProductsApiController(
        IProductApplicationService productService,
        ILogger<ProductsApiController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// 商品検索API
    /// </summary>
    /// <param name="searchParams">検索パラメータ</param>
    /// <returns>検索結果</returns>
    [HttpGet]
    public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchParams searchParams)
    {
        try
        {
            // 検索処理の実行
            var result = await _productService.SearchProductsAsync(searchParams);
            
            // 成功レスポンス
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = result,
                Message = null,
                Errors = null
            });
        }
        catch (ArgumentException ex)
        {
            // パラメータエラー
            _logger.LogWarning("商品検索でパラメータエラー: {ErrorMessage}", ex.Message);
            
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Data = null,
                Message = "検索パラメータに不正な値が含まれています。",
                Errors = new[] { new ApiError { Field = "parameter", Message = ex.Message } }
            });
        }
        catch (Exception ex)
        {
            // 予期しないエラー
            _logger.LogError(ex, "商品検索処理で予期しないエラーが発生しました");
            
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Data = null,
                Message = "検索処理中にエラーが発生しました。",
                Errors = null
            });
        }
    }
}

/// <summary>
/// API レスポンス用クラス
/// </summary>
/// <typeparam name="T">データ型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 処理成功フラグ
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// レスポンスデータ
    /// </summary>
    public T Data { get; set; }
    
    /// <summary>
    /// メッセージ
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// エラー詳細
    /// </summary>
    public ApiError[] Errors { get; set; }
}

/// <summary>
/// API エラー詳細クラス
/// </summary>
public class ApiError
{
    /// <summary>
    /// エラー対象フィールド
    /// </summary>
    public string Field { get; set; }
    
    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string Message { get; set; }
}
```

## 10. テスト実装方針

### 10.1 単体テスト基本方針

#### 10.1.1 テスト対象の優先順位
1. **ドメインサービス** - 最重要（ビジネスロジックの核心）
2. **アプリケーションサービス** - 重要（ユースケースの実現）
3. **リポジトリ** - 重要（データアクセスの信頼性）
4. **コントローラー** - 中程度（統合テストでカバー）

#### 10.1.2 テストプロジェクト構成
```
Web.Essentials.Tests/
├── Domain/
│   ├── Services/
│   │   ├── ProductDomainServiceTests.cs
│   │   └── CategoryDomainServiceTests.cs
│   └── Entities/
│       ├── ProductTests.cs
│       └── CategoryTests.cs
├── Application/
│   ├── Services/
│   │   ├── ProductApplicationServiceTests.cs
│   │   └── CategoryApplicationServiceTests.cs
│   └── Common/
│       └── TestHelpers/
├── Infrastructure/
│   └── Repositories/
│       ├── ProductRepositoryTests.cs
│       └── CategoryRepositoryTests.cs
└── Web/
    └── Controllers/
        ├── ProductsControllerTests.cs
        └── CategoriesControllerTests.cs
```

### 10.2 ドメインサービステスト例

```csharp
/// <summary>
/// 商品ドメインサービスの単体テスト
/// </summary>
public class ProductDomainServiceTests
{
    private readonly ProductDomainService _domainService;
    private readonly Mock<IProductRepository> _mockProductRepository;

    public ProductDomainServiceTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _domainService = new ProductDomainService(_mockProductRepository.Object);
    }

    /// <summary>
    /// JANコードの重複チェック：重複なしの場合
    /// </summary>
    [Fact]
    public async Task ValidateJanCodeUniqueness_WhenJanCodeIsUnique_ShouldReturnTrue()
    {
        // Arrange
        var janCode = "1234567890123";
        _mockProductRepository
            .Setup(x => x.FindByJanCodeAsync(janCode))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _domainService.ValidateJanCodeUniquenessAsync(janCode);

        // Assert
        Assert.True(result);
        _mockProductRepository.Verify(x => x.FindByJanCodeAsync(janCode), Times.Once);
    }

    /// <summary>
    /// JANコードの重複チェック：重複ありの場合
    /// </summary>
    [Fact]
    public async Task ValidateJanCodeUniqueness_WhenJanCodeIsDuplicate_ShouldReturnFalse()
    {
        // Arrange
        var janCode = "1234567890123";
        var existingProduct = new Product { Id = 1, JanCode = janCode };
        _mockProductRepository
            .Setup(x => x.FindByJanCodeAsync(janCode))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _domainService.ValidateJanCodeUniquenessAsync(janCode);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// 商品価格計算：税込み価格の計算
    /// </summary>
    [Theory]
    [InlineData(1000, 0.1, 1100)]  // 1000円 + 10%税
    [InlineData(1980, 0.08, 2138.4)]  // 1980円 + 8%税
    [InlineData(0, 0.1, 0)]  // 0円の場合
    public void CalculateIncludingTax_WithVariousPrices_ShouldReturnCorrectAmount(
        decimal basePrice, decimal taxRate, decimal expectedPrice)
    {
        // Act
        var result = _domainService.CalculateIncludingTax(basePrice, taxRate);

        // Assert
        Assert.Equal(expectedPrice, result);
    }
}
```

## 11. パフォーマンス考慮事項

### 11.1 データベースアクセス最適化

#### 11.1.1 N+1問題の回避
```csharp
/// <summary>
/// 商品とカテゴリを効率的に取得する
/// N+1問題を回避するためIncludeを使用
/// </summary>
/// <returns>商品リスト</returns>
public async Task<List<Product>> GetProductsWithCategoriesAsync()
{
    return await _dbContext.Products
        .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
        .Include(p => p.ProductImages)
        .ToListAsync();
}
```

#### 11.1.2 非同期処理の適切な使用
```csharp
/// <summary>
/// 複数の非同期処理を並列実行
/// </summary>
public async Task<ProductDetailsViewModel> GetProductDetailsAsync(int productId)
{
    // 複数のクエリを並列実行
    var productTask = _productRepository.GetByIdAsync(productId);
    var categoriesTask = _categoryRepository.GetByProductIdAsync(productId);
    var imagesTask = _productImageRepository.GetByProductIdAsync(productId);

    // 全ての処理完了を待機
    await Task.WhenAll(productTask, categoriesTask, imagesTask);

    // 結果を取得
    var product = await productTask;
    var categories = await categoriesTask;
    var images = await imagesTask;

    // ViewModelの構築
    return new ProductDetailsViewModel
    {
        Product = product,
        Categories = categories,
        Images = images
    };
}
```

### 11.2 フロントエンド最適化

#### 11.2.1 デバウンス処理による API リクエスト制限
```javascript
/**
 * 検索入力のデバウンス処理
 * 連続入力時のAPI呼び出しを制限
 */
class SearchDebouncer {
    /**
     * コンストラクタ
     * @param {number} delay - 遅延時間（ミリ秒）
     */
    constructor(delay = 500) {
        this.delay = delay;
        this.timeoutId = null;
    }

    /**
     * デバウンス処理付きで関数を実行
     * @param {Function} func - 実行する関数
     * @param {...any} args - 関数の引数
     */
    execute(func, ...args) {
        // 既存のタイマーをクリア
        if (this.timeoutId) {
            clearTimeout(this.timeoutId);
        }

        // 新しいタイマーを設定
        this.timeoutId = setTimeout(() => {
            func.apply(this, args);
            this.timeoutId = null;
        }, this.delay);
    }
}

// 使用例
const searchDebouncer = new SearchDebouncer(300);
const searchInput = document.getElementById('search-input');

searchInput.addEventListener('input', (event) => {
    searchDebouncer.execute(() => {
        performSearch(event.target.value);
    });
});
```

#### 11.2.2 画像遅延読み込み
```javascript
/**
 * 画像の遅延読み込み（Lazy Loading）
 * 表示領域に入った時点で画像を読み込む
 */
class LazyImageLoader {
    /**
     * コンストラクタ
     */
    constructor() {
        this.observer = new IntersectionObserver(
            this.handleIntersection.bind(this),
            {
                root: null,
                rootMargin: '50px',
                threshold: 0.1
            }
        );
        
        this.initializeLazyImages();
    }

    /**
     * 遅延読み込み対象の画像を初期化
     */
    initializeLazyImages() {
        const lazyImages = document.querySelectorAll('img[data-src]');
        lazyImages.forEach(img => {
            this.observer.observe(img);
        });
    }

    /**
     * 表示領域との交差を処理
     * @param {Array} entries - 交差エントリー配列
     */
    handleIntersection(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                
                // 実際の画像を読み込み
                img.src = img.dataset.src;
                img.classList.add('loaded');
                
                // 監視を停止
                this.observer.unobserve(img);
                
                // data-src属性を削除
                delete img.dataset.src;
            }
        });
    }
}

// 初期化
document.addEventListener('DOMContentLoaded', () => {
    new LazyImageLoader();
});
```

---

この技術ガイドラインに従って実装することで、保守性が高く、教育効果のあるコードベースを構築できます。特に**単一責任の原則**と**ファイル分離**を徹底することで、コードの理解しやすさと保守性を向上させることができます。