/**
 * カテゴリフォーム用JavaScript
 * カテゴリ登録・編集フォームの入力処理・バリデーション機能を提供
 */

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeCategoryForm();
});

/**
 * カテゴリフォームの初期化
 */
function initializeCategoryForm() {
    // 説明文入力フィールドの初期化
    const descriptionInput = document.querySelector('textarea[name="Description"]');
    if (descriptionInput) {
        descriptionInput.addEventListener('input', function() {
            updateCharacterCount(this, 'categoryDescriptionCount');
        });
        // 初期文字数カウント
        updateCharacterCount(descriptionInput, 'categoryDescriptionCount');
    }
}


/**
 * 文字数カウント更新
 * @param {HTMLTextAreaElement} textarea - テキストエリア
 * @param {string} counterId - カウンター要素のID
 */
function updateCharacterCount(textarea, counterId) {
    const counter = document.getElementById(counterId);
    if (counter) {
        counter.textContent = textarea.value.length;
    }
}

// グローバル関数として公開
window.updateCharacterCount = updateCharacterCount;