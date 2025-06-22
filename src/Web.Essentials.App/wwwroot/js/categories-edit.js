/**
 * カテゴリ編集画面用JavaScript
 * フォームバリデーション、変更検知、階層レベル表示、プレビュー機能を提供
 */

// 初期値を保存（変更検知用）
let originalFormData = {};

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeCategoryEditForm();
});

/**
 * カテゴリ編集フォームの初期化
 * 各種機能の初期化を行う
 */
function initializeCategoryEditForm() {
    // 初期値を保存
    originalFormData = collectFormData();
    
    // フォームバリデーションの設定
    setupFormValidation();
    
    // 変更検知の設定
    setupChangeDetection();
    
    // 階層レベル表示の初期化
    updateCategoryLevel();
    
    // ページ離脱警告の設定
    setupUnloadWarning();
    
    // フォーム送信イベントの設定
    setupFormSubmitHandler();
    
    // ボタンイベントハンドラーの設定
    setupButtonEventHandlers();
}

/**
 * フォーム送信ハンドラーの設定
 */
function setupFormSubmitHandler() {
    const form = document.getElementById('categoryEditForm');
    if (form) {
        form.addEventListener('submit', function(event) {
            // Always validate before submitting
            event.preventDefault();
            
            if (validateCategoryForm()) {
                submitCategoryForm();
            }
        });
    }
}

/**
 * 階層レベル更新
 */
function updateCategoryLevel() {
    const parentSelect = document.getElementById('parentCategorySelect');
    if (!parentSelect) return;
    
    const selectedOption = parentSelect.options[parentSelect.selectedIndex];
    
    const levelBadge = document.getElementById('levelBadge');
    const levelDescription = document.getElementById('levelDescription');
    const pathPreview = document.getElementById('pathPreview');
    
    if (!levelBadge || !levelDescription || !pathPreview) return;
    
    if (selectedOption.value === '') {
        // ルートカテゴリ
        levelBadge.textContent = 'レベル 0';
        levelBadge.className = 'level-badge level-0';
        levelDescription.textContent = '（ルートカテゴリ）';
        
        const nameInput = document.querySelector('input[name="Name"]');
        const currentName = nameInput ? nameInput.value : '';
        pathPreview.textContent = currentName || '[カテゴリ名]';
    } else {
        // 子カテゴリ
        const parentLevel = parseInt(selectedOption.dataset.level) || 0;
        const newLevel = parentLevel + 1;
        
        levelBadge.textContent = `レベル ${newLevel}`;
        levelBadge.className = `level-badge level-${newLevel}`;
        levelDescription.textContent = `（${newLevel} 階層目）`;
        
        // 階層パス更新
        const parentPath = selectedOption.textContent;
        const nameInput = document.querySelector('input[name="Name"]');
        const currentName = nameInput ? nameInput.value : '';
        pathPreview.textContent = `${parentPath} > ${currentName || '[カテゴリ名]'}`;
    }
}

/**
 * 変更検知の設定
 */
function setupChangeDetection() {
    const form = document.getElementById('categoryEditForm');
    if (!form) return;
    
    const inputs = form.querySelectorAll('input, select, textarea');
    
    inputs.forEach(input => {
        input.addEventListener('input', detectChanges);
        input.addEventListener('change', detectChanges);
    });
}

/**
 * 変更検知
 */
function detectChanges() {
    // 階層パス更新
    const nameInput = document.querySelector('input[name="Name"]');
    if (nameInput) {
        updateCategoryLevel();
    }
}


/**
 * カテゴリフォームのバリデーション
 * @returns {boolean} バリデーション結果
 */
function validateCategoryForm() {
    let isValid = true;
    const errors = [];
    
    // カテゴリ名のバリデーション
    const nameInput = document.querySelector('input[name="Name"]');
    if (nameInput) {
        if (!nameInput.value.trim()) {
            errors.push('カテゴリ名は必須です');
            highlightFieldError(nameInput);
            isValid = false;
        } else if (nameInput.value.length > 50) {
            errors.push('カテゴリ名は50文字以内で入力してください');
            highlightFieldError(nameInput);
            isValid = false;
        }
    }
    
    // 説明のバリデーション
    const descriptionInput = document.querySelector('textarea[name="Description"]');
    if (descriptionInput && descriptionInput.value && descriptionInput.value.length > 500) {
        errors.push('カテゴリ説明は500文字以内で入力してください');
        highlightFieldError(descriptionInput);
        isValid = false;
    }
    
    // エラー表示
    if (!isValid) {
        showValidationErrors(errors);
    } else {
        hideValidationErrors();
    }
    
    return isValid;
}

/**
 * カテゴリフォーム送信
 */
async function submitCategoryForm() {
    try {
        showLoadingModal('カテゴリを更新しています...');
        
        const form = document.getElementById('categoryEditForm');
        const formData = new FormData(form);
        
        // フォームのaction属性からURLを取得
        const actionUrl = form.getAttribute('action') || window.location.pathname;
        
        const response = await fetch(actionUrl, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Accept': 'application/json'
            },
            body: formData
        });
        
        hideLoadingModal();
        
        const result = await response.json();
        
        if (result.success) {
            const keepEditing = document.getElementById('keepEditingAfterSave');
            
            showSuccess(result.message || 'カテゴリが正常に更新されました');
            
            if (keepEditing && keepEditing.checked) {
                // 初期値を更新
                originalFormData = collectFormData();
            } else {
                // カテゴリ一覧に戻る
                setTimeout(() => {
                    window.location.href = '/Categories';
                }, 1500);
            }
        } else {
            showError(result.message || 'カテゴリの更新に失敗しました');
            
            // バリデーションエラーの場合、個別エラーを表示
            if (result.errors && result.errors.length > 0) {
                result.errors.forEach(error => {
                    console.error(`${error.Field}: ${error.Message}`);
                });
            }
        }
    } catch (error) {
        hideLoadingModal();
        console.error('カテゴリ更新エラー:', error);
        showError('カテゴリ更新中にエラーが発生しました');
    }
}



/**
 * フォームリセット
 */
function resetForm() {
    if (confirm('変更内容を破棄して元の状態に戻しますか？')) {
        // 元の値に戻す
        for (let key in originalFormData) {
            const input = document.querySelector(`[name="${key}"]`);
            if (input) {
                input.value = originalFormData[key];
            }
        }
        
        updateCategoryLevel();
        hideValidationErrors();
    }
}

/**
 * ページ離脱警告の設定
 */
function setupUnloadWarning() {
    window.addEventListener('beforeunload', function(event) {
        const currentData = collectFormData();
        const hasChanges = JSON.stringify(originalFormData) !== JSON.stringify(currentData);
        
        if (hasChanges) {
            event.preventDefault();
            event.returnValue = '変更が保存されていません。ページを離れますか？';
        }
    });
}

/**
 * フォームデータ収集
 * @returns {Object} フォームデータオブジェクト
 */
function collectFormData() {
    const form = document.getElementById('categoryEditForm');
    if (!form) return {};
    
    const formData = new FormData(form);
    const data = {};
    
    for (let [key, value] of formData.entries()) {
        data[key] = value;
    }
    
    return data;
}


/**
 * ボタンイベントハンドラーの設定
 */
function setupButtonEventHandlers() {
    // リセットボタン（IDベース）
    const resetButton = document.getElementById('resetButton');
    if (resetButton) {
        resetButton.addEventListener('click', resetForm);
    }
}

// 互換性のためのグローバル関数公開（段階的に削除予定）
// 新しいID/class/data属性アプローチに移行済み
window.updateCategoryLevel = updateCategoryLevel;
window.resetForm = resetForm;