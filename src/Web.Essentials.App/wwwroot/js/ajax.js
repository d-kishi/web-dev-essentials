/* ======================================
   Ajax通信用JavaScript - Fetch API + RxJS
   ====================================== */

// RxJSオペレーターの取得
const { fromFetch } = rxjs.fetch;
const { from, of, throwError } = rxjs;
const { switchMap, map, catchError, retry, debounceTime, distinctUntilChanged, shareReplay } = rxjs.operators;

/**
 * RxJS対応 Ajax通信クラス
 * Fetch API + RxJSを使用したリアクティブなHTTP通信
 */
class RxAjaxClient {
    constructor() {
        this.baseUrl = '';
        this.defaultHeaders = {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        };
    }
    
    /**
     * GET リクエスト（Observable）
     */
    get$(url, options = {}) {
        return this.request$('GET', url, null, options);
    }
    
    /**
     * POST リクエスト（Observable）
     */
    post$(url, data = null, options = {}) {
        return this.request$('POST', url, data, options);
    }
    
    /**
     * PUT リクエスト（Observable）
     */
    put$(url, data = null, options = {}) {
        return this.request$('PUT', url, data, options);
    }
    
    /**
     * DELETE リクエスト（Observable）
     */
    delete$(url, options = {}) {
        return this.request$('DELETE', url, null, options);
    }
    
    /**
     * 基本的なリクエスト処理（Observable）
     */
    request$(method, url, data = null, options = {}) {
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
        
        return fromFetch(fullUrl, config).pipe(
            switchMap(response => {
                if (!response.ok) {
                    return throwError(new AjaxError(`HTTP Error: ${response.status}`, response.status, null));
                }
                return from(response.json());
            }),
            catchError(error => {
                console.error('Ajax request failed:', error);
                return throwError(new AjaxError('ネットワークエラーが発生しました', 0, error));
            })
        );
    }
    
    /**
     * 従来のPromise版（後方互換性）
     */
    async get(url, options = {}) {
        return this.get$(url, options).toPromise();
    }
    
    async post(url, data = null, options = {}) {
        return this.post$(url, data, options).toPromise();
    }
    
    async put(url, data = null, options = {}) {
        return this.put$(url, data, options).toPromise();
    }
    
    async delete(url, options = {}) {
        return this.delete$(url, options).toPromise();
    }
}

/**
 * RxJS対応 API通信専用クラス
 */
class RxApiClient extends RxAjaxClient {
    constructor() {
        super();
        this.baseUrl = '/api';
    }
    
    /**
     * 商品一覧取得（Observable）
     */
    getProducts$(params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = queryString ? `/products?${queryString}` : '/products';
        
        return this.get$(url).pipe(
            map(result => {
                if (!result.success) {
                    throw new Error(result.message || '商品一覧の取得に失敗しました');
                }
                return result.data;
            }),
            catchError(error => {
                console.error('商品一覧取得エラー:', error);
                return throwError(error);
            })
        );
    }
    
    /**
     * 商品検索（デバウンス付き）
     */
    searchProducts$(searchTerm$, categoryId$ = of(null)) {
        return searchTerm$.pipe(
            debounceTime(300), // 300ms待機
            distinctUntilChanged(), // 重複排除
            switchMap(term => {
                if (!term || term.length < 2) {
                    return of({ items: [], totalCount: 0 });
                }
                
                return categoryId$.pipe(
                    switchMap(categoryId => {
                        const params = { nameTerm: term };
                        if (categoryId) params.categoryId = categoryId;
                        return this.getProducts$(params);
                    })
                );
            }),
            shareReplay(1) // 結果をキャッシュ
        );
    }
    
    /**
     * カテゴリ一覧取得（Observable）
     */
    getCategories$(params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = queryString ? `/categories?${queryString}` : '/categories';
        
        return this.get$(url).pipe(
            map(result => {
                if (!result.success) {
                    throw new Error(result.message || 'カテゴリ一覧の取得に失敗しました');
                }
                return result.data;
            }),
            catchError(error => {
                console.error('カテゴリ一覧取得エラー:', error);
                return throwError(error);
            })
        );
    }
    
    /**
     * カテゴリ検索（デバウンス付き）
     */
    searchCategories$(searchTerm$, level$ = of(null)) {
        return searchTerm$.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            switchMap(term => {
                return level$.pipe(
                    switchMap(level => {
                        const params = {};
                        if (term && term.length >= 1) params.nameTerm = term;
                        if (level !== null) params.level = level;
                        return this.getCategories$(params);
                    })
                );
            }),
            shareReplay(1)
        );
    }
    
    /**
     * JANコード重複チェック（Observable）
     */
    checkJanCodeDuplicate$(janCode, excludeId = null) {
        const params = { janCode };
        if (excludeId) params.excludeId = excludeId;
        
        const queryString = new URLSearchParams(params).toString();
        return this.get$(`/products/check-jan?${queryString}`).pipe(
            map(result => result.isDuplicate),
            catchError(error => {
                console.error('JANコード重複チェックエラー:', error);
                return of(false); // エラー時は重複なしとして処理
            })
        );
    }
    
    /**
     * カテゴリ名重複チェック（Observable）
     */
    checkCategoryNameDuplicate$(name, excludeId = null) {
        const params = { name };
        if (excludeId) params.excludeId = excludeId;
        
        const queryString = new URLSearchParams(params).toString();
        return this.get$(`/categories/check-duplicate?${queryString}`).pipe(
            map(result => result.isDuplicate),
            catchError(error => {
                console.error('カテゴリ名重複チェックエラー:', error);
                return of(false);
            })
        );
    }
    
    /**
     * リアルタイム検索用のオペレーター
     */
    createRealtimeSearch$(inputElement, options = {}) {
        const defaultOptions = {
            debounceTime: 300,
            minLength: 2,
            ...options
        };
        
        return from(inputElement, 'input').pipe(
            map(event => event.target.value.trim()),
            debounceTime(defaultOptions.debounceTime),
            distinctUntilChanged(),
            switchMap(term => {
                if (term.length < defaultOptions.minLength) {
                    return of([]);
                }
                return this.getProducts$({ nameTerm: term, pageSize: 10 });
            })
        );
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
 * レガシー Ajax クラス（後方互換性用）
 */
class AjaxClient {
    constructor() {
        this.baseUrl = '';
        this.defaultHeaders = {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        };
    }
    
    async get(url, options = {}) {
        return this.request('GET', url, null, options);
    }
    
    async post(url, data = null, options = {}) {
        return this.request('POST', url, data, options);
    }
    
    async put(url, data = null, options = {}) {
        return this.request('PUT', url, data, options);
    }
    
    async delete(url, options = {}) {
        return this.request('DELETE', url, null, options);
    }
    
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
        
        if (['POST', 'PUT', 'DELETE'].includes(method)) {
            config.headers['RequestVerificationToken'] = getAntiForgeryToken();
        }
        
        if (data !== null) {
            if (data instanceof FormData) {
                config.body = data;
                delete config.headers['Content-Type'];
            } else if (typeof data === 'object') {
                config.body = JSON.stringify(data);
            } else {
                config.body = data;
            }
        }
        
        try {
            const response = await fetch(fullUrl, config);
            const contentType = response.headers.get('Content-Type') || '';
            let responseData;
            
            if (contentType.includes('application/json')) {
                responseData = await response.json();
            } else {
                responseData = await response.text();
            }
            
            if (!response.ok) {
                throw new AjaxError(
                    responseData.message || `HTTP Error: ${response.status}`,
                    response.status,
                    responseData
                );
            }
            
            return {
                data: responseData,
                status: response.status,
                headers: response.headers,
                ok: response.ok
            };
        } catch (error) {
            console.error('Ajax request failed:', error);
            throw new AjaxError('ネットワークエラーが発生しました', 0, error);
        }
    }
}

/**
 * ファイルアップロード用（進捗表示付き）
 */
function uploadWithProgress$(url, formData, progressCallback) {
    return new rxjs.Observable(observer => {
        const xhr = new XMLHttpRequest();
        
        if (progressCallback && typeof progressCallback === 'function') {
            xhr.upload.addEventListener('progress', (e) => {
                if (e.lengthComputable) {
                    const percentComplete = (e.loaded / e.total) * 100;
                    progressCallback(percentComplete, e.loaded, e.total);
                }
            });
        }
        
        xhr.addEventListener('load', () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                try {
                    const response = JSON.parse(xhr.responseText);
                    observer.next(response);
                    observer.complete();
                } catch (error) {
                    observer.next(xhr.responseText);
                    observer.complete();
                }
            } else {
                observer.error(new AjaxError(`HTTP Error: ${xhr.status}`, xhr.status, xhr.responseText));
            }
        });
        
        xhr.addEventListener('error', () => {
            observer.error(new AjaxError('Network Error', 0, null));
        });
        
        xhr.addEventListener('abort', () => {
            observer.error(new AjaxError('Upload Aborted', 0, null));
        });
        
        xhr.open('POST', url);
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        xhr.setRequestHeader('RequestVerificationToken', getAntiForgeryToken());
        xhr.send(formData);
        
        // キャンセル処理
        return () => {
            xhr.abort();
        };
    });
}

/**
 * インスタンス作成
 */
const RxAjax = new RxAjaxClient();
const RxAPI = new RxApiClient();
const Ajax = new AjaxClient(); // 後方互換性

/**
 * 汎用関数（後方互換性のため）
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

// グローバル公開
window.RxAjax = RxAjax;
window.RxAPI = RxAPI;
window.Ajax = Ajax;
window.AjaxError = AjaxError;
window.uploadWithProgress$ = uploadWithProgress$;

// RxJSオペレーターのエクスポート
window.rxjs = rxjs;
window.rxjsOperators = rxjs.operators;