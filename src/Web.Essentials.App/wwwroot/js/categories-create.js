/**
 * カテゴリ登録画面用JavaScript
 * フォームバリデーション、階層レベル表示、プレビュー機能を提供
 */

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeCategoryCreateForm();
});

/**
 * カテゴリ登録フォームの初期化
 * 各種機能の初期化を行う
 */
function initializeCategoryCreateForm() {
    // フォームバリデーションの設定
    setupFormValidation();
    
    // 階層レベル表示の初期化
    updateCategoryLevel();
    
    // フォーム変更検知の設定
    setupFormChangeDetection();
    
    // フォーム送信イベントの設定
    setupFormSubmitHandler();
    
    // カテゴリ名変更時の階層パス更新
    setupNameChangeHandler();
}

/**
 * フォーム送信ハンドラーの設定
 */
function setupFormSubmitHandler() {
    const form = document.getElementById('categoryCreateForm');
    if (form) {
        form.addEventListener('submit', function(event) {
            const validateBeforeSave = document.getElementById('validateBeforeSave');
            
            if (validateBeforeSave && validateBeforeSave.checked) {
                event.preventDefault();
                
                if (validateCategoryForm()) {
                    submitCategoryForm();
                }
            }
        });
    }
}

/**
 * カテゴリ名変更ハンドラーの設定
 */
function setupNameChangeHandler() {
    const nameInput = document.querySelector('input[name="Name"]');
    if (nameInput) {
        nameInput.addEventListener('input', function() {
            updateCategoryLevel();
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
    const hierarchyPathPreview = document.getElementById('hierarchyPathPreview');
    const pathPreview = document.getElementById('pathPreview');
    
    if (!levelBadge || !levelDescription) return;
    
    if (selectedOption.value === '') {
        // ルートカテゴリ
        levelBadge.textContent = 'レベル 0';
        levelBadge.className = 'level-badge level-0';
        levelDescription.textContent = '（ルートカテゴリ）';
        if (hierarchyPathPreview) {
            hierarchyPathPreview.style.display = 'none';
        }
    } else {
        // 子カテゴリ
        const parentLevel = parseInt(selectedOption.dataset.level) || 0;
        const newLevel = parentLevel + 1;
        
        levelBadge.textContent = `レベル ${newLevel}`;
        levelBadge.className = `level-badge level-${newLevel}`;
        levelDescription.textContent = `（${newLevel} 階層目）`;
        
        // 階層パスプレビュー
        if (hierarchyPathPreview && pathPreview) {
            const parentPath = selectedOption.textContent;
            const nameInput = document.querySelector('input[name="Name"]');
            const currentName = nameInput ? nameInput.value || '[新しいカテゴリ名]' : '[新しいカテゴリ名]';
            pathPreview.textContent = `${parentPath} > ${currentName}`;
            hierarchyPathPreview.style.display = 'block';
        }
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
        showLoadingModal('カテゴリを登録しています...');
        
        const formData = new FormData(document.getElementById('categoryCreateForm'));
        
        const response = await fetch('/Categories/Create', {
            method: 'POST',
            body: formData
        });
        
        hideLoadingModal();
        
        if (response.ok) {
            const result = await response.json();
            const saveAndContinue = document.getElementById('saveAndContinue');
            const createAsChild = document.getElementById('createAsChild');
            
            showSuccess('カテゴリが正常に登録されました');
            
            if (createAsChild && createAsChild.checked && result.categoryId) {
                // 子カテゴリ作成画面に移動
                setTimeout(() => {
                    window.location.href = `/Categories/Create?parentId=${result.categoryId}`;
                }, 1500);
            } else if (saveAndContinue && saveAndContinue.checked) {
                // フォームをリセットして続行
                resetForm();
            } else {
                // カテゴリ一覧に戻る
                setTimeout(() => {
                    window.location.href = '/Categories';
                }, 1500);
            }
        } else {
            const errorText = await response.text();
            showError('カテゴリの登録に失敗しました: ' + errorText);
        }
    } catch (error) {
        hideLoadingModal();
        console.error('カテゴリ登録エラー:', error);
        showError('カテゴリ登録中にエラーが発生しました');
    }
}

/**
 * プレビュー表示
 */
function previewCategory() {
    const formData = collectFormData();
    const previewContent = generatePreviewContent(formData);
    
    const previewElement = document.getElementById('categoryPreviewContent');
    const modalElement = document.getElementById('categoryPreviewModal');
    
    if (previewElement && modalElement) {
        previewElement.innerHTML = previewContent;
        modalElement.style.display = 'block';
    }
}

/**
 * プレビューから登録
 */
function submitFromPreview() {
    closePreviewModal();
    
    if (validateCategoryForm()) {
        submitCategoryForm();
    }
}

/**
 * プレビューモーダルを閉じる
 */
function closePreviewModal() {
    const modalElement = document.getElementById('categoryPreviewModal');
    if (modalElement) {
        modalElement.style.display = 'none';
    }
}

/**
 * フォームリセット
 */
function resetForm() {
    if (confirm('入力内容をすべてリセットしますか？')) {
        const form = document.getElementById('categoryCreateForm');
        if (form) {
            form.reset();
            updateCategoryLevel();
            hideValidationErrors();
        }
    }
}

/**
 * フォームデータ収集
 * @returns {Object} フォームデータオブジェクト
 */
function collectFormData() {
    const form = document.getElementById('categoryCreateForm');
    if (!form) return {};
    
    const formData = new FormData(form);
    const data = {};
    
    for (let [key, value] of formData.entries()) {
        data[key] = value;
    }
    
    return data;
}

/**
 * プレビューコンテンツ生成
 * @param {Object} data - フォームデータ
 * @returns {string} プレビューHTML
 */
function generatePreviewContent(data) {
    const parentSelect = document.getElementById('parentCategorySelect');
    if (!parentSelect) return '<p>エラー: 親カテゴリ選択が見つかりません</p>';
    
    const selectedOption = parentSelect.options[parentSelect.selectedIndex];
    const parentPath = selectedOption.value ? selectedOption.textContent : '';
    const fullPath = parentPath ? `${parentPath} > ${data.Name || '[カテゴリ名]'}` : (data.Name || '[カテゴリ名]');
    const level = selectedOption.value ? (parseInt(selectedOption.dataset.level) + 1) : 0;
    
    return `
        <div class="category-preview">
            <div class="preview-header">
                <h2>${data.Name || 'カテゴリ名未入力'}</h2>
                <span class="level-badge level-${level}">レベル ${level}</span>
            </div>
            <div class="preview-body">
                <div class="preview-section">
                    <h3>階層パス</h3>
                    <p class="hierarchy-path">${fullPath}</p>
                </div>
                <div class="preview-section">
                    <h3>カテゴリ説明</h3>
                    <p>${data.Description || '説明なし'}</p>
                </div>
                <div class="preview-section">
                    <h3>階層情報</h3>
                    <dl class="preview-details">
                        <dt>階層レベル</dt>
                        <dd>${level} ${level === 0 ? '（ルートカテゴリ）' : '（子カテゴリ）'}</dd>
                        <dt>親カテゴリ</dt>
                        <dd>${parentPath || 'なし（ルートカテゴリ）'}</dd>
                    </dl>
                </div>
            </div>
        </div>
    `;
}

/**
 * フォームバリデーションの設定（プレースホルダー）
 */
function setupFormValidation() {
    // 実装予定：詳細なバリデーション設定
    console.log('Form validation setup');
}

/**
 * フォーム変更検知の設定（プレースホルダー）
 */
function setupFormChangeDetection() {
    // 実装予定：フォーム変更検知の設定
    console.log('Form change detection setup');
}

// HTMLのonchange属性から呼び出されるグローバル関数
window.updateCategoryLevel = updateCategoryLevel;
window.previewCategory = previewCategory;
window.submitFromPreview = submitFromPreview;
window.closePreviewModal = closePreviewModal;
window.resetForm = resetForm;