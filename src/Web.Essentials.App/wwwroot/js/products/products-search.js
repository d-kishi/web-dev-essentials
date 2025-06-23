/**
 * 商品検索専用サービス
 * ファイル: wwwroot/js/products/products-search.js
 */

/**
 * 商品検索専用サービス
 */
class ProductSearchService {
    /**
     * コンストラクタ
     */
    constructor() {
        this.apiClient = window.RxAPI || window.Ajax;
    }

    /**
     * 商品を検索する（RxJS使用）
     * @param {Object} searchParams - 検索パラメータ
     * @returns {Observable} 検索結果のObservable
     */
    searchProducts$(searchParams) {
        if (!window.rxjs) {
            throw new Error('RxJS is required for reactive search');
        }

        // パラメータの正規化
        const normalizedParams = this.normalizeSearchParams(searchParams);
        
        return this.apiClient.getProducts$(normalizedParams).pipe(
            window.rxjs.operators.map(data => ({
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
     * 商品を検索する（Promise使用）
     * @param {Object} searchParams - 検索パラメータ
     * @returns {Promise} 検索結果のPromise
     */
    async searchProducts(searchParams) {
        const normalizedParams = this.normalizeSearchParams(searchParams);
        
        try {
            const response = await this.apiClient.get(`/products?${new URLSearchParams(normalizedParams).toString()}`);
            const data = response.data || response;
            
            return {
                products: data.items || [],
                pagination: {
                    currentPage: data.currentPage || 1,
                    totalPages: data.totalPages || 1,
                    totalCount: data.totalCount || 0,
                    pageSize: data.pageSize || 10,
                    hasNextPage: data.hasNextPage || false,
                    hasPreviousPage: data.hasPreviousPage || false
                }
            };
        } catch (error) {
            console.error('商品検索エラー:', error);
            throw error;
        }
    }

    /**
     * 商品詳細を取得する
     * @param {number} productId - 商品ID
     * @returns {Promise} 商品詳細のPromise
     */
    async getProductDetails(productId) {
        try {
            const response = await this.apiClient.get(`/products/${productId}`);
            return response.data || response;
        } catch (error) {
            console.error('商品詳細取得エラー:', error);
            throw error;
        }
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

// グローバル公開
window.ProductSearchService = ProductSearchService;