/* ======================================
   共通JavaScript関数 - Pure JavaScript
   jQuery不使用、モダンブラウザ対応
   ====================================== */

/**
 * ユーティリティ関数群
 */
const Utils = {
    /**
     * 要素取得のヘルパー関数
     */
    element(selector) {
        if (typeof selector === 'string') {
            return document.querySelector(selector);
        }
        return selector;
    },
    
    elements(selector) {
        return document.querySelectorAll(selector);
    },
    
    /**
     * イベントリスナー追加のヘルパー関数
     */
    on(element, event, handler, options = false) {
        const el = this.element(element);
        if (el) {
            el.addEventListener(event, handler, options);
        }
    },
    
    /**
     * 複数要素にイベントリスナー追加
     */
    onAll(selector, event, handler, options = false) {
        const elements = this.elements(selector);
        elements.forEach(el => {
            el.addEventListener(event, handler, options);
        });
    },
    
    /**
     * 要素のクラス操作
     */
    addClass(element, className) {
        const el = this.element(element);
        if (el) el.classList.add(className);
    },
    
    removeClass(element, className) {
        const el = this.element(element);
        if (el) el.classList.remove(className);
    },
    
    toggleClass(element, className) {
        const el = this.element(element);
        if (el) el.classList.toggle(className);
    },
    
    hasClass(element, className) {
        const el = this.element(element);
        return el ? el.classList.contains(className) : false;
    },
    
    /**
     * 要素の表示/非表示
     */
    show(element, display = 'block') {
        const el = this.element(element);
        if (el) el.style.display = display;
    },
    
    hide(element) {
        const el = this.element(element);
        if (el) el.style.display = 'none';
    },
    
    toggle(element, display = 'block') {
        const el = this.element(element);
        if (el) {
            el.style.display = el.style.display === 'none' ? display : 'none';
        }
    },
    
    /**
     * 要素の属性操作
     */
    attr(element, name, value = null) {
        const el = this.element(element);
        if (!el) return null;
        
        if (value === null) {
            return el.getAttribute(name);
        } else {
            el.setAttribute(name, value);
            return el;
        }
    },
    
    removeAttr(element, name) {
        const el = this.element(element);
        if (el) el.removeAttribute(name);
    },
    
    /**
     * 要素のプロパティ操作
     */
    prop(element, name, value = null) {
        const el = this.element(element);
        if (!el) return null;
        
        if (value === null) {
            return el[name];
        } else {
            el[name] = value;
            return el;
        }
    },
    
    /**
     * 要素のHTML/テキスト操作
     */
    html(element, content = null) {
        const el = this.element(element);
        if (!el) return null;
        
        if (content === null) {
            return el.innerHTML;
        } else {
            el.innerHTML = content;
            return el;
        }
    },
    
    text(element, content = null) {
        const el = this.element(element);
        if (!el) return null;
        
        if (content === null) {
            return el.textContent;
        } else {
            el.textContent = content;
            return el;
        }
    },
    
    /**
     * フォーム値操作
     */
    val(element, value = null) {
        const el = this.element(element);
        if (!el) return null;
        
        if (value === null) {
            return el.value;
        } else {
            el.value = value;
            return el;
        }
    },
    
    /**
     * CSS操作
     */
    css(element, property, value = null) {
        const el = this.element(element);
        if (!el) return null;
        
        if (typeof property === 'object') {
            // オブジェクトとして渡された場合、複数のプロパティを設定
            Object.assign(el.style, property);
            return el;
        }
        
        if (value === null) {
            return getComputedStyle(el)[property];
        } else {
            el.style[property] = value;
            return el;
        }
    },
    
    /**
     * アニメーション関連
     */
    fadeIn(element, duration = 300) {
        const el = this.element(element);
        if (!el) return;
        
        el.style.opacity = '0';
        el.style.display = 'block';
        
        const start = performance.now();
        
        const animate = (currentTime) => {
            const elapsed = currentTime - start;
            const progress = Math.min(elapsed / duration, 1);
            
            el.style.opacity = progress;
            
            if (progress < 1) {
                requestAnimationFrame(animate);
            }
        };
        
        requestAnimationFrame(animate);
    },
    
    fadeOut(element, duration = 300) {
        const el = this.element(element);
        if (!el) return;
        
        const start = performance.now();
        const startOpacity = parseFloat(getComputedStyle(el).opacity);
        
        const animate = (currentTime) => {
            const elapsed = currentTime - start;
            const progress = Math.min(elapsed / duration, 1);
            
            el.style.opacity = startOpacity * (1 - progress);
            
            if (progress >= 1) {
                el.style.display = 'none';
            } else {
                requestAnimationFrame(animate);
            }
        };
        
        requestAnimationFrame(animate);
    },
    
    /**
     * DOM操作
     */
    append(parent, child) {
        const parentEl = this.element(parent);
        if (parentEl) {
            if (typeof child === 'string') {
                parentEl.insertAdjacentHTML('beforeend', child);
            } else {
                parentEl.appendChild(child);
            }
        }
    },
    
    prepend(parent, child) {
        const parentEl = this.element(parent);
        if (parentEl) {
            if (typeof child === 'string') {
                parentEl.insertAdjacentHTML('afterbegin', child);
            } else {
                parentEl.insertBefore(child, parentEl.firstChild);
            }
        }
    },
    
    remove(element) {
        const el = this.element(element);
        if (el && el.parentNode) {
            el.parentNode.removeChild(el);
        }
    },
    
    /**
     * データ操作
     */
    data(element, key, value = null) {
        const el = this.element(element);
        if (!el) return null;
        
        if (value === null) {
            return el.dataset[key];
        } else {
            el.dataset[key] = value;
            return el;
        }
    },
    
    /**
     * 位置・サイズ操作
     */
    offset(element) {
        const el = this.element(element);
        if (!el) return null;
        
        const rect = el.getBoundingClientRect();
        return {
            top: rect.top + window.pageYOffset,
            left: rect.left + window.pageXOffset,
            width: rect.width,
            height: rect.height
        };
    },
    
    /**
     * 文字列操作
     */
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },
    
    /**
     * 数値操作
     */
    formatNumber(num, locale = 'ja-JP') {
        return new Intl.NumberFormat(locale).format(num);
    },
    
    formatCurrency(amount, currency = 'JPY', locale = 'ja-JP') {
        return new Intl.NumberFormat(locale, {
            style: 'currency',
            currency: currency
        }).format(amount);
    },
    
    /**
     * 日付操作
     */
    formatDate(date, options = {}) {
        const defaultOptions = {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit'
        };
        return new Intl.DateTimeFormat('ja-JP', { ...defaultOptions, ...options }).format(new Date(date));
    },
    
    formatDateTime(date, options = {}) {
        const defaultOptions = {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        };
        return new Intl.DateTimeFormat('ja-JP', { ...defaultOptions, ...options }).format(new Date(date));
    },
    
    /**
     * デバウンス・スロットル
     */
    debounce(func, wait, immediate = false) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                timeout = null;
                if (!immediate) func.apply(this, args);
            };
            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(this, args);
        };
    },
    
    throttle(func, limit) {
        let inThrottle;
        return function executedFunction(...args) {
            if (!inThrottle) {
                func.apply(this, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }
};

/**
 * 通知・メッセージ表示
 */
const Notifications = {
    /**
     * 成功メッセージ表示
     */
    showSuccess(message, duration = 5000) {
        this.show(message, 'success', duration);
    },
    
    /**
     * エラーメッセージ表示
     * AjaxErrorオブジェクトにも対応
     */
    showError(message, duration = 8000) {
        // AjaxErrorオブジェクトの場合はユーザー向けメッセージを取得
        if (message && typeof message.getUserMessage === 'function') {
            message = message.getUserMessage();
        }
        this.show(message, 'error', duration);
    },
    
    /**
     * 警告メッセージ表示
     */
    showWarning(message, duration = 6000) {
        this.show(message, 'warning', duration);
    },
    
    /**
     * 情報メッセージ表示
     */
    showInfo(message, duration = 5000) {
        this.show(message, 'info', duration);
    },
    
    /**
     * ネットワークエラー専用表示
     */
    showNetworkError(error, duration = 10000) {
        let message = 'ネットワークエラーが発生しました';
        
        if (error && typeof error.getErrorType === 'function') {
            const errorType = error.getErrorType();
            switch (errorType) {
                case 'network':
                    message = 'ネットワーク接続に問題があります。インターネット接続を確認してください。';
                    break;
                case 'server':
                    message = 'サーバーで問題が発生しました。しばらく時間をおいて再度お試しください。';
                    break;
                case 'client':
                    message = error.getUserMessage();
                    break;
                default:
                    message = error.getUserMessage();
            }
        } else if (error && error.message) {
            message = error.message;
        }
        
        this.show(message, 'error', duration);
    },
    
    /**
     * API通信エラー専用表示
     */
    showApiError(error, duration = 8000) {
        let message = 'API通信でエラーが発生しました';
        
        if (error && typeof error.getUserMessage === 'function') {
            message = error.getUserMessage();
        } else if (error && error.message) {
            message = error.message;
        }
        
        // エラーの種類に応じて表示時間を調整
        if (error && typeof error.getErrorType === 'function') {
            const errorType = error.getErrorType();
            if (errorType === 'network') {
                duration = 12000; // ネットワークエラーは長めに表示
            } else if (errorType === 'server') {
                duration = 10000; // サーバーエラーも長めに表示
            }
        }
        
        this.show(message, 'error', duration);
    },
    
    /**
     * メッセージ表示の共通処理
     */
    show(message, type, duration) {
        const icons = {
            success: '✓',
            error: '⚠',
            warning: '⚠',
            info: 'ℹ'
        };
        
        const notification = document.createElement('div');
        notification.className = `message-container ${type}-message notification-toast`;
        notification.innerHTML = `
            <div class="message-content">
                <span class="message-icon">${icons[type]}</span>
                <span class="message-text">${Utils.escapeHtml(message)}</span>
                <button type="button" class="message-close">
                    <span>×</span>
                </button>
            </div>
        `;
        
        // トースト用のスタイル追加
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            min-width: 300px;
            max-width: 500px;
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
            animation: slideInRight 0.3s ease-out;
        `;
        
        if (document.body) {
            document.body.appendChild(notification);
        }
        
        // 自動削除
        if (duration > 0) {
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.style.animation = 'slideOutRight 0.3s ease-out';
                    setTimeout(() => {
                        if (notification.parentNode) {
                            notification.remove();
                        }
                    }, 300);
                }
            }, duration);
        }
    }
};

/**
 * ローディング表示
 */
const Loading = {
    /**
     * ローディングモーダル表示
     */
    show(message = '処理中...') {
        const existingModal = document.getElementById('loadingModal');
        if (existingModal) {
            Utils.text('#loadingMessage', message);
            Utils.show(existingModal, 'flex');
            return;
        }
        
        const modal = document.createElement('div');
        modal.id = 'loadingModal';
        modal.className = 'modal-overlay loading-modal';
        modal.style.display = 'flex';
        modal.innerHTML = `
            <div class="modal-dialog loading-dialog">
                <div class="modal-content">
                    <div class="modal-body">
                        <div class="loading-spinner">
                            <div class="spinner"></div>
                            <p id="loadingMessage">${Utils.escapeHtml(message)}</p>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        if (document.body) {
            document.body.appendChild(modal);
        }
    },
    
    /**
     * ローディング非表示
     */
    hide() {
        const modal = document.getElementById('loadingModal');
        if (modal) {
            Utils.hide(modal);
        }
    },
    
    /**
     * ローディング更新
     */
    update(message) {
        Utils.text('#loadingMessage', message);
    }
};

/**
 * Anti-Forgery Token 取得
 */
function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
}

/**
 * フォームデータをオブジェクトに変換
 */
function serializeForm(form) {
    const formData = new FormData(form);
    const data = {};
    
    for (let [key, value] of formData.entries()) {
        if (data[key]) {
            // 同じキーが複数ある場合は配列にする
            if (Array.isArray(data[key])) {
                data[key].push(value);
            } else {
                data[key] = [data[key], value];
            }
        } else {
            data[key] = value;
        }
    }
    
    return data;
}

/**
 * URL パラメータ操作
 */
const UrlParams = {
    get(name) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(name);
    },
    
    set(name, value) {
        const url = new URL(window.location);
        url.searchParams.set(name, value);
        window.history.replaceState({}, '', url);
    },
    
    remove(name) {
        const url = new URL(window.location);
        url.searchParams.delete(name);
        window.history.replaceState({}, '', url);
    },
    
    getAll() {
        const urlParams = new URLSearchParams(window.location.search);
        const params = {};
        for (let [key, value] of urlParams.entries()) {
            params[key] = value;
        }
        return params;
    }
};

/**
 * ローカルストレージ操作
 */
const Storage = {
    set(key, value) {
        try {
            localStorage.setItem(key, JSON.stringify(value));
            return true;
        } catch (e) {
            console.error('localStorage set error:', e);
            return false;
        }
    },
    
    get(key, defaultValue = null) {
        try {
            const item = localStorage.getItem(key);
            return item ? JSON.parse(item) : defaultValue;
        } catch (e) {
            console.error('localStorage get error:', e);
            return defaultValue;
        }
    },
    
    remove(key) {
        try {
            localStorage.removeItem(key);
            return true;
        } catch (e) {
            console.error('localStorage remove error:', e);
            return false;
        }
    },
    
    clear() {
        try {
            localStorage.clear();
            return true;
        } catch (e) {
            console.error('localStorage clear error:', e);
            return false;
        }
    }
};

/**
 * グローバル関数（後方互換性のため）
 */
window.showSuccess = (message, duration) => Notifications.showSuccess(message, duration);
window.showError = (message, duration) => Notifications.showError(message, duration);
window.showWarning = (message, duration) => Notifications.showWarning(message, duration);
window.showInfo = (message, duration) => Notifications.showInfo(message, duration);
window.showNetworkError = (error, duration) => Notifications.showNetworkError(error, duration);
window.showApiError = (error, duration) => Notifications.showApiError(error, duration);
window.showLoadingModal = (message) => Loading.show(message);
window.hideLoadingModal = () => Loading.hide();

/**
 * CSS アニメーション追加
 */
document.addEventListener('DOMContentLoaded', function() {
    const styles = `
        @keyframes slideInRight {
            from {
                opacity: 0;
                transform: translateX(100%);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }
        
        @keyframes slideOutRight {
            from {
                opacity: 1;
                transform: translateX(0);
            }
            to {
                opacity: 0;
                transform: translateX(100%);
            }
        }
        
        .notification-toast {
            animation: slideInRight 0.3s ease-out !important;
        }
    `;
    
    const styleSheet = document.createElement('style');
    styleSheet.textContent = styles;
    if (document.head) {
        document.head.appendChild(styleSheet);
    }
});

/**
 * 共通イベントリスナー設定
 */
document.addEventListener('DOMContentLoaded', function() {
    // メッセージクローズボタン（委譲パターンで動的要素にも対応）
    document.addEventListener('click', function(e) {
        if (e.target.closest('.message-close')) {
            const button = e.target.closest('.message-close');
            const messageId = button.dataset.messageId;
            if (messageId) {
                // データ属性で指定されたメッセージIDの場合
                const messageElement = document.getElementById(messageId);
                if (messageElement) {
                    messageElement.remove();
                }
            } else {
                // 通常のメッセージクローズ処理
                const messageContainer = button.closest('.message-container');
                if (messageContainer) {
                    messageContainer.remove();
                }
            }
        }
    });
    
    // ドロップダウンメニューの制御
    Utils.onAll('.dropdown-toggle', 'click', function(e) {
        e.preventDefault();
        const dropdown = this.closest('.dropdown');
        Utils.toggleClass(dropdown, 'active');
    });
    
    // ドロップダウン外クリックで閉じる
    document.addEventListener('click', function(e) {
        if (!e.target.closest('.dropdown')) {
            Utils.elements('.dropdown.active').forEach(dropdown => {
                Utils.removeClass(dropdown, 'active');
            });
        }
    });
    
    // ページネーション機能
    Utils.onAll('[data-action="load-page"]', 'click', function(e) {
        e.preventDefault();
        const page = parseInt(this.dataset.page);
        if (page && typeof loadPage === 'function') {
            loadPage(page);
        }
    });
    
    Utils.onAll('[data-action="change-page-size"]', 'change', function() {
        const pageSize = this.value;
        if (pageSize && typeof changePageSize === 'function') {
            changePageSize(pageSize);
        }
    });
    
    // 確認モーダル機能
    Utils.onAll('[data-action="close-confirmation"]', 'click', function() {
        if (typeof closeConfirmationModal === 'function') {
            closeConfirmationModal();
        }
    });
    
    Utils.onAll('[data-action="execute-confirmation"]', 'click', function() {
        if (typeof executeConfirmationAction === 'function') {
            executeConfirmationAction();
        }
    });
    
    // 検索フォーム機能
    Utils.onAll('[data-action="perform-search"]', 'click', function() {
        if (typeof performSearch === 'function') {
            performSearch();
        }
    });
    
    Utils.onAll('[data-action="reset-search"]', 'click', function() {
        if (typeof resetSearch === 'function') {
            resetSearch();
        }
    });
    
    Utils.onAll('[data-action="toggle-advanced-search"]', 'click', function() {
        if (typeof toggleAdvancedSearch === 'function') {
            toggleAdvancedSearch();
        }
    });
    
    // 検索フォーム送信防止
    Utils.onAll('#searchForm', 'submit', function(e) {
        e.preventDefault();
        if (typeof performSearch === 'function') {
            performSearch();
        }
        return false;
    });
    
    // ソート順変更
    Utils.onAll('[data-action="change-sort-order"]', 'change', function() {
        const sortValue = this.value;
        if (sortValue && typeof changeSortOrder === 'function') {
            changeSortOrder(sortValue);
        }
    });
    
    // カテゴリレベル更新
    Utils.onAll('[data-action="update-category-level"]', 'change', function() {
        if (typeof updateCategoryLevel === 'function') {
            updateCategoryLevel();
        }
    });
});

// Utils をグローバルに公開
window.Utils = Utils;
window.Notifications = Notifications;
window.Loading = Loading;
window.UrlParams = UrlParams;
window.Storage = Storage;