# JavaScript・Ajax実装仕様書

## 目次

1. [基本方針](#1-基本方針)
2. [技術スタック](#2-技術スタック)
3. [ファイル構成](#3-ファイル構成)
4. [RxJS使用方針](#4-rxjs使用方針)
5. [API通信実装](#5-api通信実装)
6. [商品検索機能実装](#6-商品検索機能実装)
7. [カテゴリ検索機能実装](#7-カテゴリ検索機能実装)
8. [ページング機能実装](#8-ページング機能実装)
9. [エラーハンドリング](#9-エラーハンドリング)
10. [パフォーマンス最適化](#10-パフォーマンス最適化)
11. [デバッグとテスト](#11-デバッグとテスト)

---

## 1. 基本方針

### 1.1 技術選択理由
- **Pure JavaScript**：jQueryに依存しない、モダンな開発スキル習得
- **RxJS**：非同期処理の理解とReactive Programming概念の学習
- **Fetch API**：Promise ベースのモダンなHTTP通信方法
- **ES6+機能**：最新JavaScript機能の積極活用

### 1.2 実装対象機能
- **商品一覧の検索・ページング**（Ajax通信）
- **カテゴリ一覧の検索・ページング**（Ajax通信）
- **リアルタイム検索**（入力時の自動検索）
- **フォームバリデーション**（クライアントサイド）

### 1.3 非実装機能
- 商品・カテゴリのCRUD操作（従来のMVC方式で実装）
- 画像アップロード（フォーム送信で実装）

## 2. 技術スタック

### 2.1 コアライブラリ
```html
<!-- RxJS（CDN使用） -->
<script src="https://unpkg.com/rxjs@7.8.1/dist/bundles/rxjs.umd.min.js"></script>

<!-- Pure JavaScript（ES6+）のみ使用 -->
<!-- jQuery、Lodash等のサードパーティライブラリは使用禁止 -->
```

### 2.2 ブラウザサポート
- **Chrome 90+**
- **Firefox 88+**
- **Safari 14+**
- **Edge 90+**

### 2.3 必要な JavaScript 機能
- **Fetch API**：HTTP通信
- **Promise/async-await**：非同期処理
- **Arrow Functions**：関数記述
- **Template Literals**：文字列テンプレート
- **Destructuring**：分割代入
- **Classes**：オブジェクト指向プログラミング

## 3. ファイル構成

### 3.1 ディレクトリ構造
```
wwwroot/js/
├── common/
│   ├── api-client.js          # API通信クライアント
│   ├── pagination.js          # ページング共通処理
│   ├── validation.js          # バリデーション共通処理
│   ├── debouncer.js          # デバウンス処理
│   └── dom-utils.js          # DOM操作ユーティリティ
├── products/
│   ├── products-index.js     # 商品一覧画面
│   ├── products-search.js    # 商品検索機能
│   ├── products-create.js    # 商品登録画面
│   └── products-edit.js      # 商品編集画面
├── categories/
│   ├── categories-index.js   # カテゴリ一覧画面
│   ├── categories-search.js  # カテゴリ検索機能
│   └── categories-tree.js    # カテゴリ階層表示
└── lib/
    └── rxjs/                 # RxJS関連ファイル
```

### 3.2 ファイル読み込み順序
```html
<!-- レイアウトファイル（_Layout.cshtml） -->
<script src="~/js/lib/rxjs.umd.min.js"></script>
<script src="~/js/common/dom-utils.js"></script>
<script src="~/js/common/api-client.js"></script>

<!-- 各画面固有のスクリプト -->
@section Scripts {
    <script src="~/js/common/debouncer.js"></script>
    <script src="~/js/common/pagination.js"></script>
    <script src="~/js/products/products-search.js"></script>
    <script src="~/js/products/products-index.js"></script>
}
```

## 4. RxJS使用方針

### 4.1 使用範囲の限定
**RxJSはAPI通信処理でのみ使用し、DOM操作やイベント処理では使用しない**

#### 4.1.1 ✅ 良い例：API通信でRxJS使用
```javascript
/**
 * 商品検索API呼び出し（RxJS使用）
 */
searchProducts(params) {
    return rxjs.from(
        fetch('/api/products?' + new URLSearchParams(params))
    ).pipe(
        rxjs.operators.switchMap(response => {
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            return rxjs.from(response.json());
        }),
        rxjs.operators.map(result => {
            if (!result.success) {
                throw new Error(result.message);
            }
            return result.data;
        }),
        rxjs.operators.catchError(error => {
            console.error('API Error:', error);
            return rxjs.throwError(error);
        })
    );
}
```

#### 4.1.2 ❌ 悪い例：DOM操作でRxJS使用（避ける）
```javascript
// ❌ DOM操作でRxJSは使用しない
// rxjs.fromEvent(button, 'click').subscribe(() => { ... });

// ✅ DOM操作は通常のJavaScript
button.addEventListener('click', () => { ... });
```

### 4.2 RxJS オペレータ使用方針

#### 4.2.1 推奨オペレータ
- **switchMap**: APIリクエストの切り替え
- **map**: データ変換
- **catchError**: エラーハンドリング
- **debounceTime**: 入力遅延処理
- **distinctUntilChanged**: 重複値の除外

#### 4.2.2 使用例
```javascript
/**
 * リアルタイム検索の実装例
 */
class RealTimeSearch {
    constructor(searchInput, searchFunction) {
        this.searchInput = searchInput;
        this.searchFunction = searchFunction;
        this.setupRealTimeSearch();
    }

    setupRealTimeSearch() {
        // 入力イベントをRxJSで処理
        rxjs.fromEvent(this.searchInput, 'input').pipe(
            rxjs.operators.map(event => event.target.value),
            rxjs.operators.debounceTime(300),  // 300ms待機
            rxjs.operators.distinctUntilChanged(),  // 重複値除外
            rxjs.operators.switchMap(term => {
                if (term.length === 0) {
                    return rxjs.of([]);  // 空文字の場合は空配列
                }
                return this.searchFunction(term);
            })
        ).subscribe({
            next: (results) => this.handleSearchResults(results),
            error: (error) => this.handleSearchError(error)
        });
    }

    handleSearchResults(results) {
        // 検索結果の処理（通常のJavaScript）
        console.log('Search results:', results);
    }

    handleSearchError(error) {
        // エラー処理（通常のJavaScript）
        console.error('Search error:', error);
    }
}
```

## 5. API通信実装

### 5.1 APIクライアントクラス
```javascript
/**
 * API通信を統一管理するクライアントクラス
 * ファイル: wwwroot/js/common/api-client.js
 */
class ApiClient {
    /**
     * コンストラクタ
     * @param {string} baseUrl - APIのベースURL
     */
    constructor(baseUrl = '/api') {
        this.baseUrl = baseUrl;
        this.defaultHeaders = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
    }

    /**
     * GETリクエスト
     * @param {string} endpoint - APIエンドポイント
     * @param {Object} params - クエリパラメータ
     * @returns {Observable} APIレスポンスのObservable
     */
    get(endpoint, params = {}) {
        const url = this.buildUrl(endpoint, params);
        
        return rxjs.from(
            fetch(url, {
                method: 'GET',
                headers: this.defaultHeaders
            })
        ).pipe(
            rxjs.operators.switchMap(response => this.handleResponse(response)),
            rxjs.operators.catchError(error => this.handleError(error))
        );
    }

    /**
     * POSTリクエスト
     * @param {string} endpoint - APIエンドポイント
     * @param {Object} data - 送信データ
     * @returns {Observable} APIレスポンスのObservable
     */
    post(endpoint, data = {}) {
        const url = `${this.baseUrl}${endpoint}`;
        
        return rxjs.from(
            fetch(url, {
                method: 'POST',
                headers: this.defaultHeaders,
                body: JSON.stringify(data)
            })
        ).pipe(
            rxjs.operators.switchMap(response => this.handleResponse(response)),
            rxjs.operators.catchError(error => this.handleError(error))
        );
    }

    /**
     * URL構築
     * @param {string} endpoint - エンドポイント
     * @param {Object} params - パラメータ
     * @returns {string} 構築されたURL
     */
    buildUrl(endpoint, params) {
        const url = new URL(`${this.baseUrl}${endpoint}`, window.location.origin);
        
        // パラメータをクエリストリングに追加
        Object.keys(params).forEach(key => {
            const value = params[key];
            if (value !== null && value !== undefined && value !== '') {
                url.searchParams.append(key, value);
            }
        });
        
        return url.toString();
    }

    /**
     * レスポンス処理
     * @param {Response} response - Fetchレスポンス
     * @returns {Observable} 処理されたレスポンス
     */
    handleResponse(response) {
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        return rxjs.from(response.json()).pipe(
            rxjs.operators.map(data => {
                if (!data.success) {
                    throw new Error(data.message || 'API処理に失敗しました');
                }
                return data.data;
            })
        );
    }

    /**
     * エラーハンドリング
     * @param {Error} error - エラー
     * @returns {Observable} エラーObservable
     */
    handleError(error) {
        console.error('API Client Error:', error);
        
        // ネットワークエラーかAPIエラーかを判定
        if (error instanceof TypeError) {
            error.message = 'ネットワークエラーが発生しました';
        }
        
        return rxjs.throwError(error);
    }
}

// グローバルAPIクライアントインスタンス
window.apiClient = new ApiClient();
```

### 5.2 専用サービスクラス

#### 5.2.1 商品検索サービス
```javascript
/**
 * 商品検索専用サービス
 * ファイル: wwwroot/js/products/products-search.js
 */
class ProductSearchService {
    /**
     * コンストラクタ
     */
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * 商品を検索する
     * @param {Object} searchParams - 検索パラメータ
     * @returns {Observable} 検索結果のObservable
     */
    searchProducts(searchParams) {
        // パラメータの正規化
        const normalizedParams = this.normalizeSearchParams(searchParams);
        
        return this.apiClient.get('/products', normalizedParams).pipe(
            rxjs.operators.map(data => ({
                products: data.items || [],
                pagination: {
                    currentPage: data.currentPage || 1,
                    totalPages: data.totalPages || 1,
                    totalCount: data.totalCount || 0,
                    pageSize: data.pageSize || 10,
                    hasNextPage: data.hasNextPage || false,
                    hasPreviousPage: data.hasPreviousPage || false
                }
            }))
        );
    }

    /**
     * 商品詳細を取得する
     * @param {number} productId - 商品ID
     * @returns {Observable} 商品詳細のObservable
     */
    getProductDetails(productId) {
        return this.apiClient.get(`/products/${productId}`);
    }

    /**
     * 検索パラメータの正規化
     * @param {Object} params - 検索パラメータ
     * @returns {Object} 正規化されたパラメータ
     */
    normalizeSearchParams(params) {
        const normalized = {};
        
        // 商品名検索
        if (params.nameTerm && params.nameTerm.trim()) {
            normalized.nameTerm = params.nameTerm.trim();
        }
        
        // JANコード検索
        if (params.janCode && params.janCode.trim()) {
            normalized.janCode = params.janCode.trim();
        }
        
        // カテゴリID
        if (params.categoryId && !isNaN(params.categoryId)) {
            normalized.categoryId = parseInt(params.categoryId);
        }
        
        // 商品ステータス
        if (params.status !== null && params.status !== undefined && !isNaN(params.status)) {
            normalized.status = parseInt(params.status);
        }
        
        // 価格範囲
        if (params.minPrice && !isNaN(params.minPrice)) {
            normalized.minPrice = parseInt(params.minPrice);
        }
        if (params.maxPrice && !isNaN(params.maxPrice)) {
            normalized.maxPrice = parseInt(params.maxPrice);
        }
        
        // ページング
        normalized.page = params.page && !isNaN(params.page) ? parseInt(params.page) : 1;
        normalized.pageSize = params.pageSize && !isNaN(params.pageSize) ? parseInt(params.pageSize) : 10;
        
        // ソート
        if (params.sortBy) {
            normalized.sortBy = params.sortBy;
        }
        if (params.sortOrder) {
            normalized.sortOrder = params.sortOrder;
        }
        
        return normalized;
    }
}
```

## 6. 商品検索機能実装

### 6.1 商品一覧画面制御クラス
```javascript
/**
 * 商品一覧画面の制御クラス
 * ファイル: wwwroot/js/products/products-index.js
 */
class ProductIndexController {
    /**
     * コンストラクタ
     */
    constructor() {
        this.productSearchService = new ProductSearchService();
        this.debouncer = new Debouncer(500);
        
        this.initializeElements();
        this.setupEventListeners();
        this.loadInitialData();
    }

    /**
     * DOM要素の初期化
     */
    initializeElements() {
        this.searchForm = document.getElementById('product-search-form');
        this.searchInput = document.getElementById('search-input');
        this.categorySelect = document.getElementById('category-select');
        this.statusSelect = document.getElementById('status-select');
        this.minPriceInput = document.getElementById('min-price');
        this.maxPriceInput = document.getElementById('max-price');
        this.sortBySelect = document.getElementById('sort-by');
        this.sortOrderSelect = document.getElementById('sort-order');
        
        this.productList = document.getElementById('product-list');
        this.paginationContainer = document.getElementById('pagination');
        this.loadingIndicator = document.getElementById('loading');
        this.noResultsMessage = document.getElementById('no-results');
        this.errorMessage = document.getElementById('error-message');
    }

    /**
     * イベントリスナーの設定
     */
    setupEventListeners() {
        // 検索フォーム送信
        if (this.searchForm) {
            this.searchForm.addEventListener('submit', (event) => {
                event.preventDefault();
                this.performSearch();
            });
        }

        // リアルタイム検索（商品名）
        if (this.searchInput) {
            this.searchInput.addEventListener('input', () => {
                this.debouncer.execute(() => {
                    this.performSearch();
                });
            });
        }

        // 検索条件変更時の即座検索
        [this.categorySelect, this.statusSelect, this.sortBySelect, this.sortOrderSelect].forEach(element => {
            if (element) {
                element.addEventListener('change', () => {
                    this.performSearch();
                });
            }
        });

        // 価格範囲の入力（デバウンス処理）
        [this.minPriceInput, this.maxPriceInput].forEach(element => {
            if (element) {
                element.addEventListener('input', () => {
                    this.debouncer.execute(() => {
                        this.performSearch();
                    });
                });
            }
        });
    }

    /**
     * 初期データの読み込み
     */
    loadInitialData() {
        this.performSearch();
    }

    /**
     * 検索の実行
     * @param {number} page - ページ番号（省略時は1）
     */
    performSearch(page = 1) {
        const searchParams = this.collectSearchParams();
        searchParams.page = page;

        this.showLoading(true);
        this.hideMessages();

        this.productSearchService.searchProducts(searchParams)
            .subscribe({
                next: (result) => {
                    this.updateProductList(result.products);
                    this.updatePagination(result.pagination);
                    this.showLoading(false);
                    
                    if (result.products.length === 0) {
                        this.showNoResults();
                    }
                },
                error: (error) => {
                    this.showError(error.message);
                    this.showLoading(false);
                }
            });
    }

    /**
     * 検索パラメータの収集
     * @returns {Object} 検索パラメータ
     */
    collectSearchParams() {
        return {
            nameTerm: this.searchInput?.value || '',
            categoryId: this.categorySelect?.value || '',
            status: this.statusSelect?.value || '',
            minPrice: this.minPriceInput?.value || '',
            maxPrice: this.maxPriceInput?.value || '',
            sortBy: this.sortBySelect?.value || 'createdAt',
            sortOrder: this.sortOrderSelect?.value || 'desc',
            pageSize: 10
        };
    }

    /**
     * 商品一覧の更新
     * @param {Array} products - 商品データ配列
     */
    updateProductList(products) {
        if (!this.productList) return;

        this.productList.innerHTML = products.map(product => {
            const mainImage = product.images?.find(img => img.isMain) || product.images?.[0];
            
            return `
                <div class="product-card" data-product-id="${product.id}">
                    <div class="product-image">
                        ${mainImage 
                            ? `<img src="${mainImage.imagePath}" alt="${DomUtils.escapeHtml(mainImage.altText)}" loading="lazy">`
                            : '<div class="no-image">画像なし</div>'
                        }
                    </div>
                    <div class="product-info">
                        <h3 class="product-name">${DomUtils.escapeHtml(product.name)}</h3>
                        <p class="product-price">¥${product.price.toLocaleString()}</p>
                        <p class="product-status status-${product.status}">${DomUtils.escapeHtml(product.statusName)}</p>
                        ${product.categories?.length > 0 
                            ? `<p class="product-categories">${product.categories.map(cat => DomUtils.escapeHtml(cat.name)).join(', ')}</p>`
                            : ''
                        }
                    </div>
                    <div class="product-actions">
                        <a href="/Products/Details/${product.id}" class="btn btn-primary">詳細</a>
                        <a href="/Products/Edit/${product.id}" class="btn btn-secondary">編集</a>
                    </div>
                </div>
            `;
        }).join('');
    }

    /**
     * ページングの更新
     * @param {Object} pagination - ページング情報
     */
    updatePagination(pagination) {
        if (!this.paginationContainer) return;

        const paginationComponent = new PaginationComponent(
            this.paginationContainer,
            pagination,
            (page) => this.performSearch(page)
        );
        
        paginationComponent.render();
    }

    /**
     * ローディング表示の制御
     * @param {boolean} show - 表示するかどうか
     */
    showLoading(show) {
        if (this.loadingIndicator) {
            this.loadingIndicator.style.display = show ? 'block' : 'none';
        }
    }

    /**
     * 結果なしメッセージの表示
     */
    showNoResults() {
        if (this.noResultsMessage) {
            this.noResultsMessage.style.display = 'block';
        }
    }

    /**
     * エラーメッセージの表示
     * @param {string} message - エラーメッセージ
     */
    showError(message) {
        if (this.errorMessage) {
            this.errorMessage.textContent = message;
            this.errorMessage.style.display = 'block';
        }
    }

    /**
     * メッセージの非表示
     */
    hideMessages() {
        if (this.noResultsMessage) {
            this.noResultsMessage.style.display = 'none';
        }
        if (this.errorMessage) {
            this.errorMessage.style.display = 'none';
        }
    }
}

// 初期化
document.addEventListener('DOMContentLoaded', () => {
    new ProductIndexController();
});
```

## 7. カテゴリ検索機能実装

### 7.1 カテゴリ検索サービス
```javascript
/**
 * カテゴリ検索専用サービス
 * ファイル: wwwroot/js/categories/categories-search.js
 */
class CategorySearchService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * カテゴリを検索する
     * @param {Object} searchParams - 検索パラメータ
     * @returns {Observable} 検索結果のObservable
     */
    searchCategories(searchParams = {}) {
        const normalizedParams = this.normalizeSearchParams(searchParams);
        
        return this.apiClient.get('/categories', normalizedParams).pipe(
            rxjs.operators.map(data => {
                // 階層構造の構築
                return this.buildCategoryHierarchy(data);
            })
        );
    }

    /**
     * 検索パラメータの正規化
     * @param {Object} params - 検索パラメータ
     * @returns {Object} 正規化されたパラメータ
     */
    normalizeSearchParams(params) {
        const normalized = {};
        
        if (params.nameTerm && params.nameTerm.trim()) {
            normalized.nameTerm = params.nameTerm.trim();
        }
        
        if (params.level !== null && params.level !== undefined && !isNaN(params.level)) {
            normalized.level = parseInt(params.level);
        }
        
        if (params.parentId && !isNaN(params.parentId)) {
            normalized.parentId = parseInt(params.parentId);
        }
        
        normalized.includeProductCount = params.includeProductCount || false;
        
        return normalized;
    }

    /**
     * カテゴリ階層構造の構築
     * @param {Array} categories - フラットなカテゴリ配列
     * @returns {Array} 階層構造のカテゴリ配列
     */
    buildCategoryHierarchy(categories) {
        const categoryMap = new Map();
        const rootCategories = [];

        // マップの作成
        categories.forEach(category => {
            categoryMap.set(category.id, { ...category, children: [] });
        });

        // 階層構造の構築
        categories.forEach(category => {
            if (category.parentCategoryId) {
                const parent = categoryMap.get(category.parentCategoryId);
                if (parent) {
                    parent.children.push(categoryMap.get(category.id));
                }
            } else {
                rootCategories.push(categoryMap.get(category.id));
            }
        });

        return rootCategories;
    }
}
```

### 7.2 カテゴリツリー表示コンポーネント
```javascript
/**
 * カテゴリツリー表示コンポーネント
 * ファイル: wwwroot/js/categories/categories-tree.js
 */
class CategoryTreeComponent {
    /**
     * コンストラクタ
     * @param {HTMLElement} container - 表示コンテナ
     * @param {Array} categories - カテゴリデータ
     * @param {Object} options - オプション設定
     */
    constructor(container, categories, options = {}) {
        this.container = container;
        this.categories = categories;
        this.options = {
            showProductCount: false,
            allowSelection: false,
            collapsible: true,
            ...options
        };
        
        this.selectedCategories = new Set();
    }

    /**
     * ツリー表示のレンダリング
     */
    render() {
        if (!this.container) return;

        this.container.innerHTML = `
            <div class="category-tree">
                ${this.renderCategoryList(this.categories, 0)}
            </div>
        `;

        this.setupEventListeners();
    }

    /**
     * カテゴリリストのレンダリング
     * @param {Array} categories - カテゴリ配列
     * @param {number} level - 階層レベル
     * @returns {string} HTML文字列
     */
    renderCategoryList(categories, level = 0) {
        return categories.map(category => this.renderCategoryItem(category, level)).join('');
    }

    /**
     * カテゴリアイテムのレンダリング
     * @param {Object} category - カテゴリデータ
     * @param {number} level - 階層レベル
     * @returns {string} HTML文字列
     */
    renderCategoryItem(category, level) {
        const hasChildren = category.children && category.children.length > 0;
        const indent = '  '.repeat(level);
        const productCountDisplay = this.options.showProductCount && category.productCount !== undefined 
            ? ` (${category.productCount})` : '';

        return `
            <div class="category-item level-${level}" data-category-id="${category.id}">
                <div class="category-content">
                    ${hasChildren && this.options.collapsible 
                        ? `<button class="category-toggle" type="button" aria-expanded="true">
                             <span class="toggle-icon">▼</span>
                           </button>`
                        : '<span class="category-spacer"></span>'
                    }
                    
                    ${this.options.allowSelection 
                        ? `<input type="checkbox" class="category-checkbox" value="${category.id}" id="cat-${category.id}">`
                        : ''
                    }
                    
                    <label class="category-label" ${this.options.allowSelection ? `for="cat-${category.id}"` : ''}>
                        ${DomUtils.escapeHtml(category.name)}${productCountDisplay}
                    </label>
                    
                    <div class="category-actions">
                        <a href="/Categories/Details/${category.id}" class="btn btn-sm btn-info">詳細</a>
                        <a href="/Categories/Edit/${category.id}" class="btn btn-sm btn-secondary">編集</a>
                    </div>
                </div>
                
                ${hasChildren 
                    ? `<div class="category-children">
                         ${this.renderCategoryList(category.children, level + 1)}
                       </div>`
                    : ''
                }
            </div>
        `;
    }

    /**
     * イベントリスナーの設定
     */
    setupEventListeners() {
        // 折りたたみボタン
        this.container.querySelectorAll('.category-toggle').forEach(button => {
            button.addEventListener('click', (event) => {
                this.toggleCategory(event.target.closest('.category-item'));
            });
        });

        // チェックボックス選択
        if (this.options.allowSelection) {
            this.container.querySelectorAll('.category-checkbox').forEach(checkbox => {
                checkbox.addEventListener('change', (event) => {
                    this.handleCategorySelection(event.target);
                });
            });
        }
    }

    /**
     * カテゴリの折りたたみ切り替え
     * @param {HTMLElement} categoryItem - カテゴリアイテム要素
     */
    toggleCategory(categoryItem) {
        const toggle = categoryItem.querySelector('.category-toggle');
        const children = categoryItem.querySelector('.category-children');
        
        if (!toggle || !children) return;

        const isExpanded = toggle.getAttribute('aria-expanded') === 'true';
        
        toggle.setAttribute('aria-expanded', !isExpanded);
        toggle.querySelector('.toggle-icon').textContent = isExpanded ? '▶' : '▼';
        children.style.display = isExpanded ? 'none' : 'block';
    }

    /**
     * カテゴリ選択の処理
     * @param {HTMLInputElement} checkbox - チェックボックス要素
     */
    handleCategorySelection(checkbox) {
        const categoryId = parseInt(checkbox.value);
        
        if (checkbox.checked) {
            this.selectedCategories.add(categoryId);
        } else {
            this.selectedCategories.delete(categoryId);
        }

        // カスタムイベントの発火
        this.container.dispatchEvent(new CustomEvent('categorySelectionChanged', {
            detail: {
                selectedCategories: Array.from(this.selectedCategories),
                changedCategoryId: categoryId,
                isSelected: checkbox.checked
            }
        }));
    }

    /**
     * 選択されたカテゴリIDの取得
     * @returns {Array} 選択されたカテゴリIDの配列
     */
    getSelectedCategories() {
        return Array.from(this.selectedCategories);
    }

    /**
     * カテゴリ選択のクリア
     */
    clearSelection() {
        this.selectedCategories.clear();
        this.container.querySelectorAll('.category-checkbox').forEach(checkbox => {
            checkbox.checked = false;
        });
    }
}
```

## 8. ページング機能実装

### 8.1 ページングコンポーネント
```javascript
/**
 * ページング表示・制御コンポーネント
 * ファイル: wwwroot/js/common/pagination.js
 */
class PaginationComponent {
    /**
     * コンストラクタ
     * @param {HTMLElement} container - 表示コンテナ
     * @param {Object} paginationData - ページング情報
     * @param {Function} onPageChange - ページ変更時のコールバック
     */
    constructor(container, paginationData, onPageChange) {
        this.container = container;
        this.paginationData = paginationData;
        this.onPageChange = onPageChange;
        this.maxVisiblePages = 7; // 表示する最大ページ数
    }

    /**
     * ページングのレンダリング
     */
    render() {
        if (!this.container || !this.paginationData) return;

        const { currentPage, totalPages, totalCount, pageSize } = this.paginationData;

        if (totalPages <= 1) {
            this.container.innerHTML = '';
            return;
        }

        const pageNumbers = this.calculateVisiblePages(currentPage, totalPages);
        
        this.container.innerHTML = `
            <nav aria-label="ページネーション" class="pagination-nav">
                <div class="pagination-info">
                    <span>全 ${totalCount.toLocaleString()} 件中 ${this.getDisplayRange()} 件を表示</span>
                </div>
                <ul class="pagination">
                    ${this.renderPreviousButton(currentPage)}
                    ${pageNumbers.map(page => this.renderPageButton(page, currentPage)).join('')}
                    ${this.renderNextButton(currentPage, totalPages)}
                </ul>
            </nav>
        `;

        this.setupEventListeners();
    }

    /**
     * 表示する範囲の計算
     * @returns {string} 表示範囲の文字列
     */
    getDisplayRange() {
        const { currentPage, totalCount, pageSize } = this.paginationData;
        const startItem = (currentPage - 1) * pageSize + 1;
        const endItem = Math.min(currentPage * pageSize, totalCount);
        return `${startItem.toLocaleString()} - ${endItem.toLocaleString()}`;
    }

    /**
     * 表示するページ番号の計算
     * @param {number} currentPage - 現在のページ
     * @param {number} totalPages - 総ページ数
     * @returns {Array} 表示するページ番号の配列
     */
    calculateVisiblePages(currentPage, totalPages) {
        const maxVisible = this.maxVisiblePages;
        const halfVisible = Math.floor(maxVisible / 2);

        let startPage = Math.max(1, currentPage - halfVisible);
        let endPage = Math.min(totalPages, startPage + maxVisible - 1);

        // 終端調整
        if (endPage - startPage + 1 < maxVisible) {
            startPage = Math.max(1, endPage - maxVisible + 1);
        }

        const pages = [];
        
        // 最初のページ
        if (startPage > 1) {
            pages.push(1);
            if (startPage > 2) {
                pages.push('...');
            }
        }

        // 中間ページ
        for (let i = startPage; i <= endPage; i++) {
            pages.push(i);
        }

        // 最後のページ
        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                pages.push('...');
            }
            pages.push(totalPages);
        }

        return pages;
    }

    /**
     * 前へボタンのレンダリング
     * @param {number} currentPage - 現在のページ
     * @returns {string} HTML文字列
     */
    renderPreviousButton(currentPage) {
        const disabled = currentPage <= 1;
        return `
            <li class="page-item ${disabled ? 'disabled' : ''}">
                <button class="page-link" 
                        data-page="${currentPage - 1}" 
                        ${disabled ? 'disabled' : ''}
                        aria-label="前のページ">
                    <span aria-hidden="true">&laquo;</span>
                    <span class="sr-only">前のページ</span>
                </button>
            </li>
        `;
    }

    /**
     * 次へボタンのレンダリング
     * @param {number} currentPage - 現在のページ
     * @param {number} totalPages - 総ページ数
     * @returns {string} HTML文字列
     */
    renderNextButton(currentPage, totalPages) {
        const disabled = currentPage >= totalPages;
        return `
            <li class="page-item ${disabled ? 'disabled' : ''}">
                <button class="page-link" 
                        data-page="${currentPage + 1}" 
                        ${disabled ? 'disabled' : ''}
                        aria-label="次のページ">
                    <span aria-hidden="true">&raquo;</span>
                    <span class="sr-only">次のページ</span>
                </button>
            </li>
        `;
    }

    /**
     * ページボタンのレンダリング
     * @param {number|string} page - ページ番号または"..."
     * @param {number} currentPage - 現在のページ
     * @returns {string} HTML文字列
     */
    renderPageButton(page, currentPage) {
        if (page === '...') {
            return `
                <li class="page-item disabled">
                    <span class="page-link">...</span>
                </li>
            `;
        }

        const isActive = page === currentPage;
        return `
            <li class="page-item ${isActive ? 'active' : ''}">
                <button class="page-link" 
                        data-page="${page}"
                        ${isActive ? 'aria-current="page"' : ''}>
                    ${page}
                </button>
            </li>
        `;
    }

    /**
     * イベントリスナーの設定
     */
    setupEventListeners() {
        this.container.querySelectorAll('.page-link[data-page]').forEach(button => {
            button.addEventListener('click', (event) => {
                event.preventDefault();
                const page = parseInt(event.target.getAttribute('data-page'));
                if (page && !isNaN(page) && this.onPageChange) {
                    this.onPageChange(page);
                }
            });
        });
    }
}
```

## 9. エラーハンドリング

### 9.1 エラー分類と対応

#### 9.1.1 ネットワークエラー
```javascript
/**
 * ネットワークエラーの処理
 */
handleNetworkError(error) {
    console.error('Network Error:', error);
    
    const errorMessage = '接続に失敗しました。インターネット接続を確認してください。';
    this.showUserFriendlyError(errorMessage, 'network-error');
    
    // 再試行ボタンの表示
    this.showRetryOption();
}

/**
 * 再試行オプションの表示
 */
showRetryOption() {
    const retryButton = document.createElement('button');
    retryButton.textContent = '再試行';
    retryButton.className = 'btn btn-primary retry-button';
    retryButton.addEventListener('click', () => {
        this.performSearch();
        retryButton.remove();
    });
    
    const errorContainer = document.getElementById('error-message');
    if (errorContainer) {
        errorContainer.appendChild(retryButton);
    }
}
```

#### 9.1.2 APIエラー
```javascript
/**
 * APIエラーの処理
 */
handleApiError(error, response) {
    console.error('API Error:', error, response);
    
    // ステータスコード別の処理
    switch (response.status) {
        case 400:
            this.handleValidationError(error);
            break;
        case 404:
            this.showUserFriendlyError('データが見つかりませんでした。', 'not-found-error');
            break;
        case 500:
            this.showUserFriendlyError('サーバーエラーが発生しました。しばらく待ってからお試しください。', 'server-error');
            break;
        default:
            this.showUserFriendlyError('予期しないエラーが発生しました。', 'unknown-error');
    }
}

/**
 * バリデーションエラーの処理
 */
handleValidationError(error) {
    if (error.errors && Array.isArray(error.errors)) {
        // フィールド固有のエラー表示
        error.errors.forEach(fieldError => {
            this.showFieldError(fieldError.field, fieldError.message);
        });
    } else {
        this.showUserFriendlyError('入力内容に不備があります。', 'validation-error');
    }
}
```

### 9.2 ユーザーフレンドリーなエラー表示

```javascript
/**
 * エラー表示管理クラス
 */
class ErrorDisplayManager {
    /**
     * コンストラクタ
     */
    constructor() {
        this.errorContainer = this.createErrorContainer();
    }

    /**
     * エラーコンテナの作成
     */
    createErrorContainer() {
        let container = document.getElementById('error-display');
        if (!container) {
            container = document.createElement('div');
            container.id = 'error-display';
            container.className = 'error-display';
            document.body.appendChild(container);
        }
        return container;
    }

    /**
     * エラーメッセージの表示
     * @param {string} message - エラーメッセージ
     * @param {string} type - エラータイプ
     * @param {number} duration - 表示時間（ミリ秒）
     */
    showError(message, type = 'error', duration = 5000) {
        const errorElement = document.createElement('div');
        errorElement.className = `error-item error-${type}`;
        errorElement.innerHTML = `
            <div class="error-content">
                <span class="error-icon">${this.getErrorIcon(type)}</span>
                <span class="error-message">${DomUtils.escapeHtml(message)}</span>
                <button class="error-close" type="button" aria-label="エラーを閉じる">×</button>
            </div>
        `;

        // 閉じるボタンのイベント
        errorElement.querySelector('.error-close').addEventListener('click', () => {
            this.removeError(errorElement);
        });

        // エラーの追加
        this.errorContainer.appendChild(errorElement);

        // アニメーション
        requestAnimationFrame(() => {
            errorElement.classList.add('show');
        });

        // 自動削除
        if (duration > 0) {
            setTimeout(() => {
                this.removeError(errorElement);
            }, duration);
        }

        return errorElement;
    }

    /**
     * エラーの削除
     * @param {HTMLElement} errorElement - エラー要素
     */
    removeError(errorElement) {
        errorElement.classList.remove('show');
        errorElement.classList.add('hide');
        
        setTimeout(() => {
            if (errorElement.parentNode) {
                errorElement.parentNode.removeChild(errorElement);
            }
        }, 300);
    }

    /**
     * エラーアイコンの取得
     * @param {string} type - エラータイプ
     * @returns {string} アイコンHTML
     */
    getErrorIcon(type) {
        switch (type) {
            case 'network-error':
                return '🌐';
            case 'validation-error':
                return '⚠️';
            case 'server-error':
                return '🔥';
            case 'not-found-error':
                return '🔍';
            default:
                return '❌';
        }
    }

    /**
     * 全エラーのクリア
     */
    clearAll() {
        const errors = this.errorContainer.querySelectorAll('.error-item');
        errors.forEach(error => this.removeError(error));
    }
}

// グローバルエラー表示マネージャー
window.errorDisplayManager = new ErrorDisplayManager();
```

## 10. パフォーマンス最適化

### 10.1 デバウンス処理
```javascript
/**
 * デバウンス処理クラス
 * ファイル: wwwroot/js/common/debouncer.js
 */
class Debouncer {
    /**
     * コンストラクタ
     * @param {number} delay - 遅延時間（ミリ秒）
     */
    constructor(delay = 500) {
        this.delay = delay;
        this.timeoutId = null;
    }

    /**
     * デバウンス実行
     * @param {Function} func - 実行する関数
     * @param {...any} args - 関数の引数
     */
    execute(func, ...args) {
        this.cancel();
        
        this.timeoutId = setTimeout(() => {
            func.apply(this, args);
            this.timeoutId = null;
        }, this.delay);
    }

    /**
     * デバウンスのキャンセル
     */
    cancel() {
        if (this.timeoutId) {
            clearTimeout(this.timeoutId);
            this.timeoutId = null;
        }
    }

    /**
     * 即座実行
     * @param {Function} func - 実行する関数
     * @param {...any} args - 関数の引数
     */
    executeImmediate(func, ...args) {
        this.cancel();
        func.apply(this, args);
    }
}
```

### 10.2 仮想スクロール（大量データ対応）
```javascript
/**
 * 仮想スクロール実装
 * 大量のデータを効率的に表示
 */
class VirtualScrollList {
    /**
     * コンストラクタ
     * @param {HTMLElement} container - スクロールコンテナ
     * @param {Object} options - オプション設定
     */
    constructor(container, options = {}) {
        this.container = container;
        this.options = {
            itemHeight: 100,
            visibleItems: 10,
            bufferSize: 5,
            ...options
        };
        
        this.data = [];
        this.scrollTop = 0;
        this.startIndex = 0;
        this.endIndex = 0;
        
        this.setupContainer();
        this.setupEventListeners();
    }

    /**
     * コンテナの設定
     */
    setupContainer() {
        this.container.style.position = 'relative';
        this.container.style.overflow = 'auto';
        this.container.style.height = `${this.options.itemHeight * this.options.visibleItems}px`;
        
        // スクロール領域の作成
        this.scrollArea = document.createElement('div');
        this.scrollArea.style.position = 'absolute';
        this.scrollArea.style.top = '0';
        this.scrollArea.style.left = '0';
        this.scrollArea.style.right = '0';
        
        // アイテムコンテナの作成
        this.itemContainer = document.createElement('div');
        this.itemContainer.style.position = 'absolute';
        this.itemContainer.style.top = '0';
        this.itemContainer.style.left = '0';
        this.itemContainer.style.right = '0';
        
        this.container.appendChild(this.scrollArea);
        this.container.appendChild(this.itemContainer);
    }

    /**
     * イベントリスナーの設定
     */
    setupEventListeners() {
        this.container.addEventListener('scroll', () => {
            this.scrollTop = this.container.scrollTop;
            this.updateVisibleItems();
        });
    }

    /**
     * データの設定
     * @param {Array} data - 表示データ
     */
    setData(data) {
        this.data = data;
        this.scrollArea.style.height = `${data.length * this.options.itemHeight}px`;
        this.updateVisibleItems();
    }

    /**
     * 表示アイテムの更新
     */
    updateVisibleItems() {
        const startIndex = Math.floor(this.scrollTop / this.options.itemHeight);
        const endIndex = Math.min(
            this.data.length - 1,
            startIndex + this.options.visibleItems + this.options.bufferSize
        );

        this.startIndex = Math.max(0, startIndex - this.options.bufferSize);
        this.endIndex = endIndex;

        this.renderVisibleItems();
    }

    /**
     * 表示アイテムのレンダリング
     */
    renderVisibleItems() {
        const fragment = document.createDocumentFragment();
        
        for (let i = this.startIndex; i <= this.endIndex; i++) {
            const item = this.data[i];
            if (!item) continue;

            const itemElement = this.renderItem(item, i);
            itemElement.style.position = 'absolute';
            itemElement.style.top = `${i * this.options.itemHeight}px`;
            itemElement.style.left = '0';
            itemElement.style.right = '0';
            itemElement.style.height = `${this.options.itemHeight}px`;

            fragment.appendChild(itemElement);
        }

        this.itemContainer.innerHTML = '';
        this.itemContainer.appendChild(fragment);
    }

    /**
     * 個別アイテムのレンダリング
     * @param {Object} item - アイテムデータ
     * @param {number} index - インデックス
     * @returns {HTMLElement} アイテム要素
     */
    renderItem(item, index) {
        // サブクラスでオーバーライド
        const itemElement = document.createElement('div');
        itemElement.textContent = `Item ${index}: ${item.name || item.toString()}`;
        return itemElement;
    }
}
```

## 11. デバッグとテスト

### 11.1 デバッグ用ユーティリティ
```javascript
/**
 * デバッグ用ユーティリティクラス
 * ファイル: wwwroot/js/common/debug-utils.js
 */
class DebugUtils {
    /**
     * デバッグモードかどうか
     */
    static get isDebugMode() {
        return window.location.hostname === 'localhost' || 
               window.location.search.includes('debug=true');
    }

    /**
     * デバッグログ
     * @param {string} label - ラベル
     * @param {...any} data - ログデータ
     */
    static log(label, ...data) {
        if (this.isDebugMode) {
            console.log(`[DEBUG] ${label}:`, ...data);
        }
    }

    /**
     * パフォーマンス測定
     * @param {string} label - 測定ラベル
     * @param {Function} func - 測定対象関数
     * @returns {any} 関数の実行結果
     */
    static async measurePerformance(label, func) {
        if (!this.isDebugMode) {
            return await func();
        }

        const startTime = performance.now();
        try {
            const result = await func();
            const endTime = performance.now();
            console.log(`[PERF] ${label}: ${(endTime - startTime).toFixed(2)}ms`);
            return result;
        } catch (error) {
            const endTime = performance.now();
            console.error(`[PERF] ${label} (ERROR): ${(endTime - startTime).toFixed(2)}ms`, error);
            throw error;
        }
    }

    /**
     * API リクエストのトレース
     * @param {string} url - リクエストURL
     * @param {Object} params - リクエストパラメータ
     */
    static traceApiRequest(url, params) {
        if (this.isDebugMode) {
            console.group(`[API] ${url}`);
            console.log('Parameters:', params);
            console.log('Timestamp:', new Date().toISOString());
            console.groupEnd();
        }
    }

    /**
     * API レスポンスのトレース
     * @param {string} url - リクエストURL
     * @param {Object} response - レスポンスデータ
     * @param {number} duration - 処理時間
     */
    static traceApiResponse(url, response, duration) {
        if (this.isDebugMode) {
            console.group(`[API RESPONSE] ${url} (${duration.toFixed(2)}ms)`);
            console.log('Response:', response);
            console.groupEnd();
        }
    }
}
```

### 11.2 単体テスト例
```javascript
/**
 * JavaScript 単体テスト例
 * （実際のテストフレームワークは使用せず、シンプルな検証）
 */
class JavaScriptTests {
    /**
     * 全テストの実行
     */
    static runAllTests() {
        if (!DebugUtils.isDebugMode) return;

        console.group('[TESTS] JavaScript Unit Tests');
        
        this.testApiClient();
        this.testProductSearchService();
        this.testPaginationComponent();
        this.testDebouncer();
        
        console.groupEnd();
    }

    /**
     * APIクライアントのテスト
     */
    static testApiClient() {
        console.group('ApiClient Tests');
        
        // URL構築のテスト
        const apiClient = new ApiClient('/api');
        const url = apiClient.buildUrl('/products', { name: 'test', page: 1 });
        console.assert(
            url.includes('/api/products?name=test&page=1'),
            'URL construction failed'
        );
        
        console.log('✓ API Client tests passed');
        console.groupEnd();
    }

    /**
     * 商品検索サービスのテスト
     */
    static testProductSearchService() {
        console.group('ProductSearchService Tests');
        
        const service = new ProductSearchService();
        
        // パラメータ正規化のテスト
        const normalized = service.normalizeSearchParams({
            nameTerm: '  test  ',
            page: '2',
            invalidParam: null
        });
        
        console.assert(normalized.nameTerm === 'test', 'Name term normalization failed');
        console.assert(normalized.page === 2, 'Page normalization failed');
        console.assert(!normalized.hasOwnProperty('invalidParam'), 'Invalid param filtering failed');
        
        console.log('✓ ProductSearchService tests passed');
        console.groupEnd();
    }

    /**
     * ページングコンポーネントのテスト
     */
    static testPaginationComponent() {
        console.group('PaginationComponent Tests');
        
        const mockContainer = document.createElement('div');
        const pagination = new PaginationComponent(
            mockContainer,
            { currentPage: 5, totalPages: 10, totalCount: 100, pageSize: 10 },
            () => {}
        );
        
        const visiblePages = pagination.calculateVisiblePages(5, 10);
        console.assert(visiblePages.includes(5), 'Current page not in visible pages');
        console.assert(visiblePages.length <= 7, 'Too many visible pages');
        
        console.log('✓ PaginationComponent tests passed');
        console.groupEnd();
    }

    /**
     * デバウンサーのテスト
     */
    static testDebouncer() {
        console.group('Debouncer Tests');
        
        const debouncer = new Debouncer(100);
        let callCount = 0;
        
        const testFunction = () => callCount++;
        
        // 複数回実行してもデバウンスされることを確認
        debouncer.execute(testFunction);
        debouncer.execute(testFunction);
        debouncer.execute(testFunction);
        
        setTimeout(() => {
            console.assert(callCount === 1, 'Debouncer failed to limit calls');
            console.log('✓ Debouncer tests passed');
            console.groupEnd();
        }, 150);
    }
}

// デバッグモード時にテスト実行
if (DebugUtils.isDebugMode) {
    document.addEventListener('DOMContentLoaded', () => {
        JavaScriptTests.runAllTests();
    });
}
```

---

このJavaScript実装仕様書に従って実装することで、保守性が高く、パフォーマンスの良いフロントエンド機能を構築できます。特に**RxJSの使用範囲限定**と**単一責任の原則**を守ることで、理解しやすく保守しやすいコードベースを実現できます。