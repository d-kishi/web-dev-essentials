/**
 * カテゴリフォーム用JavaScript
 * カテゴリ登録・編集フォームの入力処理・バリデーション機能を提供
 */

// グローバル変数
let categoryNameCheckTimeout;

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
    // カテゴリ名入力フィールドの初期化
    const nameInput = document.querySelector('input[name="Name"]');
    if (nameInput) {
        nameInput.addEventListener('input', function() {
            validateCategoryName(this);
        });
    }

    // 説明文入力フィールドの初期化
    const descriptionInput = document.querySelector('textarea[name="Description"]');
    if (descriptionInput) {
        descriptionInput.addEventListener('input', function() {
            updateCharacterCount(this, 'categoryDescriptionCount');
            updateCategoryPreview();
        });
        // 初期文字数カウント
        updateCharacterCount(descriptionInput, 'categoryDescriptionCount');
    }

    // プレビューの初期化
    updateCategoryPreview();
}

/**
 * カテゴリ名のリアルタイムバリデーション
 * @param {HTMLInputElement} input - カテゴリ名入力フィールド
 */
function validateCategoryName(input) {
    // プレビュー更新
    updateCategoryPreview();
    
    // 重複チェック（デバウンス）
    clearTimeout(categoryNameCheckTimeout);
    categoryNameCheckTimeout = setTimeout(() => {
        checkCategoryNameDuplicate(input.value);
    }, 500);
}

/**
 * カテゴリ名重複チェック
 * @param {string} categoryName - チェックするカテゴリ名
 */
async function checkCategoryNameDuplicate(categoryName) {
    if (!categoryName || categoryName.trim() === '') {
        hideValidationMessage('categoryNameValidation');
        return;
    }
    
    try {
        // モデルIDをグローバル変数から取得（各ページで設定される）
        const excludeId = window.categoryModelId || 0;
        const response = await fetch(`/api/category/check-duplicate?name=${encodeURIComponent(categoryName)}&excludeId=${excludeId}`);
        const result = await response.json();
        
        if (result.isDuplicate) {
            showValidationMessage('categoryNameValidation', 'このカテゴリ名は既に使用されています', 'error');
        } else {
            showValidationMessage('categoryNameValidation', 'このカテゴリ名は使用可能です', 'success');
        }
    } catch (error) {
        console.error('カテゴリ名重複チェックエラー:', error);
    }
}

/**
 * プレビュー更新
 */
function updateCategoryPreview() {
    const nameInput = document.querySelector('input[name="Name"]');
    const descriptionInput = document.querySelector('textarea[name="Description"]');
    
    const previewName = document.getElementById('previewCategoryName');
    const previewDescription = document.getElementById('previewCategoryDescription');
    
    if (nameInput && previewName) {
        previewName.textContent = nameInput.value || 'カテゴリ名がここに表示されます';
    }
    
    if (descriptionInput && previewDescription) {
        previewDescription.textContent = descriptionInput.value || 'カテゴリ説明がここに表示されます';
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

/**
 * バリデーションメッセージ表示
 * @param {string} elementId - 表示要素のID
 * @param {string} message - メッセージ
 * @param {string} type - メッセージタイプ（success, error）
 */
function showValidationMessage(elementId, message, type) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = message;
        element.className = `field-validation ${type}`;
        element.style.display = 'block';
    }
}

/**
 * バリデーションメッセージ非表示
 * @param {string} elementId - 非表示にする要素のID
 */
function hideValidationMessage(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.style.display = 'none';
    }
}

// グローバル関数として公開
window.validateCategoryName = validateCategoryName;
window.updateCharacterCount = updateCharacterCount;
window.updateCategoryPreview = updateCategoryPreview;
window.checkCategoryNameDuplicate = checkCategoryNameDuplicate;
window.showValidationMessage = showValidationMessage;
window.hideValidationMessage = hideValidationMessage;