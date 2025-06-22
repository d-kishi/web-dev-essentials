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
                    // エラーレスポンスの詳細を取得
                    return from(response.text()).pipe(
                        map(errorText => {
                            let errorMessage = `HTTP Error: ${response.status}`;
                            try {
                                const errorData = JSON.parse(errorText);
                                errorMessage = errorData.message || errorMessage;
                            } catch (e) {
                                errorMessage = errorText || errorMessage;
                            }
                            throw new AjaxError(errorMessage, response.status, errorText);
                        })
                    );
                }
                return from(response.json()).pipe(
                    catchError(parseError => {
                        console.warn('JSON parse failed, returning text response:', parseError);
                        return from(response.text());
                    })
                );
            }),
            catchError(error => {
                console.error('Ajax request failed:', error);
                
                // エラーの種類に応じて適切なメッセージを設定
                let errorMessage = 'ネットワークエラーが発生しました';
                let errorStatus = 0;
                
                if (error instanceof AjaxError) {
                    return throwError(error);
                } else if (error.name === 'TypeError' && error.message.includes('fetch')) {
                    errorMessage = 'サーバーに接続できませんでした。ネットワーク接続を確認してください。';
                } else if (error.name === 'AbortError') {
                    errorMessage = 'リクエストがキャンセルされました。';
                } else if (error.message) {
                    errorMessage = error.message;
                }
                
                return throwError(new AjaxError(errorMessage, errorStatus, error));
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
     * デバウンス機能付き関数を作成
     * Pure JavaScript実装（RxJS使用禁止）
     */
    createDebouncedFunction(func, delay = 300) {
        let timeoutId;
        return function (...args) {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(() => func.apply(this, args), delay);
        };
    }
    
    /**
     * リアルタイム検索機能
     * 入力要素に対してデバウンス付き検索を設定
     */
    setupRealtimeSearch(inputElement, searchCallback, options = {}) {
        const defaultOptions = {
            debounceTime: 300,
            minLength: 2,
            ...options
        };
        
        const debouncedSearch = this.createDebouncedFunction(async (term) => {
            if (term.length < defaultOptions.minLength) {
                searchCallback([]);
                return;
            }
            
            try {
                const response = await this.get(`/api/products?nameTerm=${encodeURIComponent(term)}&pageSize=10`);
                searchCallback(response.data || []);
            } catch (error) {
                console.error('検索エラー:', error);
                searchCallback([]);
            }
        }, defaultOptions.debounceTime);
        
        inputElement.addEventListener('input', (event) => {
            const term = event.target.value.trim();
            debouncedSearch(term);
        });
    }
}

/**
 * Ajax エラークラス
 * API通信で発生するエラーの統一処理用
 */
class AjaxError extends Error {
    constructor(message, status, data) {
        super(message);
        this.name = 'AjaxError';
        this.status = status;
        this.data = data;
        this.timestamp = new Date().toISOString();
    }
    
    /**
     * エラーの種類を判定
     * @returns {string} エラータイプ
     */
    getErrorType() {
        if (this.status === 0) {
            return 'network';
        } else if (this.status >= 400 && this.status < 500) {
            return 'client';
        } else if (this.status >= 500) {
            return 'server';
        }
        return 'unknown';
    }
    
    /**
     * ユーザー向けメッセージを取得
     * @returns {string} ユーザー向けエラーメッセージ
     */
    getUserMessage() {
        const errorType = this.getErrorType();
        
        switch (errorType) {
            case 'network':
                return 'ネットワーク接続に問題があります。インターネット接続を確認してください。';
            case 'client':
                return this.message || '入力内容に問題があります。確認して再度お試しください。';
            case 'server':
                return 'サーバーで問題が発生しました。しばらく時間をおいて再度お試しください。';
            default:
                return this.message || '予期しないエラーが発生しました。';
        }
    }
    
    /**
     * エラー情報をログ用にシリアライズ
     * @returns {object} ログ用エラー情報
     */
    toLogObject() {
        return {
            message: this.message,
            status: this.status,
            type: this.getErrorType(),
            timestamp: this.timestamp,
            stack: this.stack,
            data: this.data
        };
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
            
            // レスポンスデータの解析
            try {
                if (contentType.includes('application/json')) {
                    responseData = await response.json();
                } else {
                    responseData = await response.text();
                }
            } catch (parseError) {
                console.warn('Response parse failed:', parseError);
                responseData = await response.text();
            }
            
            if (!response.ok) {
                // エラーレスポンスの詳細メッセージを取得
                let errorMessage = `HTTP Error: ${response.status}`;
                
                if (typeof responseData === 'object' && responseData.message) {
                    errorMessage = responseData.message;
                } else if (typeof responseData === 'string' && responseData.trim()) {
                    errorMessage = responseData;
                } else {
                    // HTTPステータスコードに基づくデフォルトメッセージ
                    switch (response.status) {
                        case 400:
                            errorMessage = 'リクエストが正しくありません。入力内容を確認してください。';
                            break;
                        case 401:
                            errorMessage = '認証が必要です。ログインしてください。';
                            break;
                        case 403:
                            errorMessage = 'アクセス権限がありません。';
                            break;
                        case 404:
                            errorMessage = '要求されたリソースが見つかりません。';
                            break;
                        case 500:
                            errorMessage = 'サーバー内部エラーが発生しました。';
                            break;
                        case 503:
                            errorMessage = 'サービスが一時的に利用できません。';
                            break;
                        default:
                            errorMessage = `サーバーエラーが発生しました (${response.status})`;
                    }
                }
                
                throw new AjaxError(errorMessage, response.status, responseData);
            }
            
            return {
                data: responseData,
                status: response.status,
                headers: response.headers,
                ok: response.ok
            };
        } catch (error) {
            console.error('Ajax request failed:', error);
            
            // AjaxErrorの場合はそのまま再スロー
            if (error instanceof AjaxError) {
                throw error;
            }
            
            // ネットワークエラーの詳細分類
            let errorMessage = 'ネットワークエラーが発生しました';
            
            if (error.name === 'TypeError' && error.message.includes('fetch')) {
                errorMessage = 'サーバーに接続できませんでした。ネットワーク接続を確認してください。';
            } else if (error.name === 'AbortError') {
                errorMessage = 'リクエストがタイムアウトしました。';
            } else if (error.message) {
                errorMessage = `通信エラー: ${error.message}`;
            }
            
            throw new AjaxError(errorMessage, 0, error);
        }
    }
}

/**
 * ファイルアップロード用（進捗表示付き）
 * Pure JavaScript実装（Promise + XMLHttpRequest）
 */
function uploadWithProgress(url, formData, progressCallback) {
    return new Promise((resolve, reject) => {
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
                    resolve(response);
                } catch (error) {
                    resolve(xhr.responseText);
                }
            } else {
                reject(new AjaxError(`HTTP Error: ${xhr.status}`, xhr.status, xhr.responseText));
            }
        });
        
        xhr.addEventListener('error', () => {
            reject(new AjaxError('Network Error', 0, null));
        });
        
        xhr.addEventListener('abort', () => {
            reject(new AjaxError('Upload Aborted', 0, null));
        });
        
        xhr.open('POST', url);
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        xhr.setRequestHeader('RequestVerificationToken', getAntiForgeryToken());
        xhr.send(formData);
        
        // キャンセル機能は別途実装が必要な場合はAbortControllerを使用
        xhr._abort = () => xhr.abort();
        return xhr;
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