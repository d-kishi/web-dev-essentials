/**
 * カテゴリ検索専用サービス
 * ファイル: wwwroot/js/categories/categories-search.js
 */

/**
 * カテゴリ検索専用サービス
 */
class CategorySearchService {
    constructor() {
        this.apiClient = window.RxAPI || window.Ajax;
    }

    /**
     * カテゴリを検索する（RxJS使用）
     * @param {Object} searchParams - 検索パラメータ
     * @returns {Observable} 検索結果のObservable
     */
    searchCategories$(searchParams = {}) {
        if (!window.rxjs) {
            throw new Error('RxJS is required for reactive search');
        }

        const normalizedParams = this.normalizeSearchParams(searchParams);
        
        return this.apiClient.getCategories$(normalizedParams).pipe(
            window.rxjs.operators.map(data => {
                // 階層構造の構築
                return this.buildCategoryHierarchy(data);
            })
        );
    }

    /**
     * カテゴリを検索する（Promise使用）
     * @param {Object} searchParams - 検索パラメータ
     * @returns {Promise} 検索結果のPromise
     */
    async searchCategories(searchParams = {}) {
        const normalizedParams = this.normalizeSearchParams(searchParams);
        
        try {
            const response = await this.apiClient.get(`/categories?${new URLSearchParams(normalizedParams).toString()}`);
            const data = response.data || response;
            
            return this.buildCategoryHierarchy(Array.isArray(data) ? data : []);
        } catch (error) {
            console.error('カテゴリ検索エラー:', error);
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
        if (!Array.isArray(categories)) {
            return [];
        }

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

// グローバル公開
window.CategorySearchService = CategorySearchService;