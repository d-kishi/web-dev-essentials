/**
 * デバウンス処理
 * ファイル: wwwroot/js/common/debouncer.js
 */

/**
 * デバウンス処理クラス
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

// グローバル公開
window.Debouncer = Debouncer;