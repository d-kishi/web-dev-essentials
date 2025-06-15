/* ======================================
   Ajax通信用JavaScript - Pure JavaScript + Fetch API
   ====================================== */

/**
 * Ajax通信クラス
 * Fetch APIを使用したHTTP通信を簡素化
 */
class AjaxClient {
    constructor() {
        this.baseUrl = '';
        this.defaultHeaders = {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        };
    }
    
    /**
     * GET リクエスト
     */
    async get(url, options = {}) {
        return this.request('GET', url, null, options);
    }
    
    /**
     * POST リクエスト
     */
    async post(url, data = null, options = {}) {
        return this.request('POST', url, data, options);
    }
    
    /**
     * PUT リクエスト
     */
    async put(url, data = null, options = {}) {
        return this.request('PUT', url, data, options);
    }
    
    /**
     * DELETE リクエスト
     */
    async delete(url, options = {}) {
        return this.request('DELETE', url, null, options);
    }
    
    /**
     * ファイルアップロード用POST
     */
    async uploadFile(url, formData, options = {}) {
        const uploadOptions = {
            ...options,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': getAntiForgeryToken(),
                ...options.headers
            }
        };
        
        // Content-Typeは設定しない（FormDataの場合、ブラウザが自動設定）
        delete uploadOptions.headers['Content-Type'];
        
        return this.request('POST', url, formData, uploadOptions);
    }
    
    /**
     * 基本的なリクエスト処理
     */
    async request(method, url, data = null, options = {}) {
        const fullUrl = this.baseUrl + url;
        
        const config = {
            method: method,
            headers: {
                ...this.defaultHeaders,
                ...options.headers
            },
            ...options
        };
        
        // Anti-Forgery Token を自動追加
        if (['POST', 'PUT', 'DELETE'].includes(method)) {
            config.headers['RequestVerificationToken'] = getAntiForgeryToken();
        }
        
        // データ処理
        if (data !== null) {
            if (data instanceof FormData) {
                config.body = data;
                // FormDataの場合はContent-Typeを削除（ブラウザが自動設定）
                delete config.headers['Content-Type'];
            } else if (typeof data === 'object') {
                config.body = JSON.stringify(data);
            } else {
                config.body = data;
            }
        }
        
        try {
            const response = await fetch(fullUrl, config);
            return await this.handleResponse(response);
        } catch (error) {
            console.error('Ajax request failed:', error);
            throw new AjaxError('ネットワークエラーが発生しました', 0, error);
        }
    }
    
    /**
     * レスポンス処理
     */
    async handleResponse(response) {
        const contentType = response.headers.get('Content-Type') || '';
        let data;
        
        if (contentType.includes('application/json')) {
            data = await response.json();
        } else {
            data = await response.text();
        }
        
        if (!response.ok) {
            throw new AjaxError(
                data.message || `HTTP Error: ${response.status}`,
                response.status,
                data
            );
        }
        
        return {
            data: data,
            status: response.status,
            headers: response.headers,
            ok: response.ok
        };
    }
}

/**
 * Ajax エラークラス
 */
class AjaxError extends Error {
    constructor(message, status, data) {
        super(message);
        this.name = 'AjaxError';
        this.status = status;
        this.data = data;
    }
}

/**
 * Ajax インスタンス作成
 */
const Ajax = new AjaxClient();

/**
 * API 通信専用クラス
 */
class ApiClient extends AjaxClient {
    constructor() {
        super();
        this.baseUrl = '/api';
    }
    
    /**
     * 商品一覧取得
     */
    async getProducts(params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = queryString ? `/product?${queryString}` : '/product';
        
        try {
            const response = await this.get(url);
            return response.data;
        } catch (error) {
            console.error('商品一覧取得エラー:', error);
            throw error;
        }
    }
    
    /**
     * 商品詳細取得
     */
    async getProduct(id) {
        try {
            const response = await this.get(`/product/${id}`);
            return response.data;
        } catch (error) {
            console.error('商品詳細取得エラー:', error);
            throw error;
        }
    }
    
    /**
     * 商品検索候補取得
     */
    async getProductSuggestions(term, limit = 10) {
        try {
            const response = await this.get(`/product/suggestions?term=${encodeURIComponent(term)}&limit=${limit}`);
            return response.data;
        } catch (error) {
            console.error('商品検索候補取得エラー:', error);
            throw error;
        }
    }
    
    /**
     * カテゴリ一覧取得
     */
    async getCategories(params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = queryString ? `/category?${queryString}` : '/category';
        
        try {
            const response = await this.get(url);
            return response.data;
        } catch (error) {
            console.error('カテゴリ一覧取得エラー:', error);
            throw error;
        }
    }
    
    /**
     * カテゴリ詳細取得
     */
    async getCategory(id) {
        try {
            const response = await this.get(`/category/${id}`);
            return response.data;
        } catch (error) {
            console.error('カテゴリ詳細取得エラー:', error);
            throw error;
        }
    }
    
    /**
     * 全カテゴリ取得（選択肢用）
     */
    async getAllCategories() {
        try {
            const response = await this.get('/category/all');
            return response.data;
        } catch (error) {
            console.error('全カテゴリ取得エラー:', error);
            throw error;
        }
    }
    
    /**
     * カテゴリ検索候補取得
     */
    async getCategorySuggestions(term, limit = 10) {
        try {
            const response = await this.get(`/category/suggestions?term=${encodeURIComponent(term)}&limit=${limit}`);
            return response.data;
        } catch (error) {
            console.error('カテゴリ検索候補取得エラー:', error);
            throw error;
        }
    }
    
    /**
     * JANコード重複チェック
     */
    async checkJanCodeDuplicate(janCode, excludeId = null) {
        try {
            const params = { janCode };
            if (excludeId) params.excludeId = excludeId;
            
            const queryString = new URLSearchParams(params).toString();
            const response = await this.get(`/product/check-jan?${queryString}`);
            return response.data;
        } catch (error) {
            console.error('JANコード重複チェックエラー:', error);
            throw error;
        }
    }
    
    /**
     * カテゴリ名重複チェック
     */
    async checkCategoryNameDuplicate(name, excludeId = null) {
        try {
            const params = { name };
            if (excludeId) params.excludeId = excludeId;
            
            const queryString = new URLSearchParams(params).toString();
            const response = await this.get(`/category/check-duplicate?${queryString}`);
            return response.data;
        } catch (error) {
            console.error('カテゴリ名重複チェックエラー:', error);
            throw error;
        }
    }
}

/**
 * API インスタンス作成
 */
const API = new ApiClient();

/**
 * 汎用Ajax関数（後方互換性のため）
 */
window.ajaxGet = async function(url, options = {}) {
    try {
        const response = await Ajax.get(url, options);
        return response.data;
    } catch (error) {
        console.error('Ajax GET error:', error);
        throw error;
    }
};

window.ajaxPost = async function(url, data = null, options = {}) {
    try {
        const response = await Ajax.post(url, data, options);
        return response.data;
    } catch (error) {
        console.error('Ajax POST error:', error);
        throw error;
    }
};

window.ajaxPut = async function(url, data = null, options = {}) {
    try {
        const response = await Ajax.put(url, data, options);
        return response.data;
    } catch (error) {
        console.error('Ajax PUT error:', error);
        throw error;
    }
};

window.ajaxDelete = async function(url, options = {}) {
    try {
        const response = await Ajax.delete(url, options);
        return response.data;
    } catch (error) {
        console.error('Ajax DELETE error:', error);
        throw error;
    }
};

/**
 * インターセプター機能
 */
class AjaxInterceptor {
    constructor() {
        this.requestInterceptors = [];
        this.responseInterceptors = [];
    }
    
    /**
     * リクエストインターセプター追加
     */
    addRequestInterceptor(interceptor) {
        this.requestInterceptors.push(interceptor);
    }
    
    /**
     * レスポンスインターセプター追加
     */
    addResponseInterceptor(interceptor) {
        this.responseInterceptors.push(interceptor);
    }
    
    /**
     * リクエスト実行前処理
     */
    async executeRequestInterceptors(config) {
        let modifiedConfig = config;
        
        for (const interceptor of this.requestInterceptors) {
            try {
                modifiedConfig = await interceptor(modifiedConfig);
            } catch (error) {
                console.error('Request interceptor error:', error);
            }
        }
        
        return modifiedConfig;
    }
    
    /**
     * レスポンス受信後処理
     */
    async executeResponseInterceptors(response) {
        let modifiedResponse = response;
        
        for (const interceptor of this.responseInterceptors) {
            try {
                modifiedResponse = await interceptor(modifiedResponse);
            } catch (error) {
                console.error('Response interceptor error:', error);
            }
        }
        
        return modifiedResponse;
    }
}

/**
 * 進捗表示機能付きアップロード
 */
async function uploadWithProgress(url, formData, progressCallback) {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        
        // 進捗表示
        if (progressCallback && typeof progressCallback === 'function') {
            xhr.upload.addEventListener('progress', (e) => {
                if (e.lengthComputable) {
                    const percentComplete = (e.loaded / e.total) * 100;
                    progressCallback(percentComplete, e.loaded, e.total);
                }
            });
        }
        
        // 完了処理
        xhr.addEventListener('load', () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                try {
                    const response = JSON.parse(xhr.responseText);
                    resolve(response);
                } catch (error) {
                    resolve(xhr.responseText);
                }
            } else {
                reject(new AjaxError(`HTTP Error: ${xhr.status}`, xhr.status, xhr.responseText));
            }
        });
        
        // エラー処理
        xhr.addEventListener('error', () => {
            reject(new AjaxError('Network Error', 0, null));
        });
        
        // 中断処理
        xhr.addEventListener('abort', () => {
            reject(new AjaxError('Upload Aborted', 0, null));
        });
        
        // リクエスト送信
        xhr.open('POST', url);
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        xhr.setRequestHeader('RequestVerificationToken', getAntiForgeryToken());
        xhr.send(formData);
    });
}

/**
 * バッチ処理（複数のAPIリクエストを並行実行）
 */
async function batchRequest(requests) {
    try {
        const promises = requests.map(async (request) => {
            try {
                return await request();
            } catch (error) {
                return { error: error };
            }
        });
        
        return await Promise.all(promises);
    } catch (error) {
        console.error('Batch request error:', error);
        throw error;
    }
}

/**
 * キャッシュ機能付きGet
 */
class CachedAjax {
    constructor(ttl = 5 * 60 * 1000) { // デフォルト5分
        this.cache = new Map();
        this.ttl = ttl;
    }
    
    async get(url, options = {}) {
        const cacheKey = this.generateCacheKey(url, options);
        const cachedData = this.cache.get(cacheKey);
        
        if (cachedData && (Date.now() - cachedData.timestamp) < this.ttl) {
            return cachedData.data;
        }
        
        try {
            const response = await Ajax.get(url, options);
            this.cache.set(cacheKey, {
                data: response,
                timestamp: Date.now()
            });
            return response;
        } catch (error) {
            console.error('Cached Ajax GET error:', error);
            throw error;
        }
    }
    
    generateCacheKey(url, options) {
        return `${url}_${JSON.stringify(options)}`;
    }
    
    clear() {
        this.cache.clear();
    }
    
    remove(url, options = {}) {
        const cacheKey = this.generateCacheKey(url, options);
        this.cache.delete(cacheKey);
    }
}

/**
 * リトライ機能付きAjax
 */
async function ajaxWithRetry(requestFn, maxRetries = 3, delay = 1000) {
    let lastError;
    
    for (let i = 0; i <= maxRetries; i++) {
        try {
            return await requestFn();
        } catch (error) {
            lastError = error;
            
            if (i < maxRetries) {
                // 指数バックオフでリトライ
                await new Promise(resolve => setTimeout(resolve, delay * Math.pow(2, i)));
            }
        }
    }
    
    throw lastError;
}

// グローバル公開
window.Ajax = Ajax;
window.API = API;
window.AjaxError = AjaxError;
window.uploadWithProgress = uploadWithProgress;
window.batchRequest = batchRequest;
window.CachedAjax = CachedAjax;
window.ajaxWithRetry = ajaxWithRetry;