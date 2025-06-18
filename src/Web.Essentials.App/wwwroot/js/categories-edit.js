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
}

/**
 * フォーム送信ハンドラーの設定
 */
function setupFormSubmitHandler() {
    const form = document.getElementById('categoryEditForm');
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
    const currentData = collectFormData();
    const changes = compareFormData(originalFormData, currentData);
    updateChangeHistory(changes);
    
    // 階層パス更新
    const nameInput = document.querySelector('input[name="Name"]');
    if (nameInput) {
        updateCategoryLevel();
    }
}

/**
 * フォームデータ比較
 * @param {Object} original - 元のフォームデータ
 * @param {Object} current - 現在のフォームデータ
 * @returns {Array} 変更内容の配列
 */
function compareFormData(original, current) {
    const changes = [];
    
    for (let key in current) {
        if (original[key] !== current[key]) {
            changes.push({
                field: key,
                from: original[key],
                to: current[key],
                timestamp: new Date()
            });
        }
    }
    
    return changes;
}

/**
 * 変更履歴更新
 * @param {Array} changes - 変更内容の配列
 */
function updateChangeHistory(changes) {
    const historyContainer = document.getElementById('changeHistory');
    if (!historyContainer) return;
    
    if (changes.length === 0) {
        historyContainer.innerHTML = '<p class="no-changes">変更はありません</p>';
        return;
    }
    
    const historyHtml = changes.map(change => `
        <div class="history-item">
            <div class="history-field">${getFieldDisplayName(change.field)}</div>
            <div class="history-change">
                <span class="change-from">${change.from || '(空)'}</span>
                <span class="change-arrow">→</span>
                <span class="change-to">${change.to || '(空)'}</span>
            </div>
            <div class="history-time">${change.timestamp.toLocaleTimeString()}</div>
        </div>
    `).join('');
    
    historyContainer.innerHTML = historyHtml;
}

/**
 * フィールド表示名取得
 * @param {string} fieldName - フィールド名
 * @returns {string} 表示名
 */
function getFieldDisplayName(fieldName) {
    const fieldNames = {
        'Name': 'カテゴリ名',
        'Description': 'カテゴリ説明',
        'ParentCategoryId': '親カテゴリ'
    };
    
    return fieldNames[fieldName] || fieldName;
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
            body: formData
        });
        
        hideLoadingModal();
        
        if (response.ok) {
            const keepEditing = document.getElementById('keepEditingAfterSave');
            
            showSuccess('カテゴリが正常に更新されました');
            
            if (keepEditing && keepEditing.checked) {
                // 初期値を更新
                originalFormData = collectFormData();
                updateChangeHistory([]);
            } else {
                // カテゴリ詳細に戻る
                setTimeout(() => {
                    const categoryId = document.querySelector('input[name="Id"]').value;
                    window.location.href = `/Categories/Details/${categoryId}`;
                }, 1500);
            }
        } else {
            const errorText = await response.text();
            showError('カテゴリの更新に失敗しました: ' + errorText);
        }
    } catch (error) {
        hideLoadingModal();
        console.error('カテゴリ更新エラー:', error);
        showError('カテゴリ更新中にエラーが発生しました');
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
 * プレビューから保存
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
 * 別カテゴリとして保存モーダル表示
 */
function showSaveAsNewModal() {
    const modalElement = document.getElementById('saveAsNewModal');
    if (modalElement) {
        modalElement.style.display = 'block';
    }
}

/**
 * 別カテゴリとして保存モーダルを閉じる
 */
function closeSaveAsNewModal() {
    const modalElement = document.getElementById('saveAsNewModal');
    if (modalElement) {
        modalElement.style.display = 'none';
    }
}

/**
 * 別カテゴリとして保存実行
 */
async function saveAsNewCategory() {
    try {
        const newCategoryName = document.getElementById('newCategoryName');
        const newParentCategory = document.getElementById('newParentCategory');
        
        if (!newCategoryName || !newCategoryName.value.trim()) {
            showError('新しいカテゴリ名を入力してください');
            return;
        }
        
        showLoadingModal('新しいカテゴリとして保存しています...');
        
        const form = document.getElementById('categoryEditForm');
        const formData = new FormData(form);
        formData.set('Name', newCategoryName.value); // カテゴリ名を更新
        if (newParentCategory) {
            formData.set('ParentCategoryId', newParentCategory.value); // 親カテゴリを更新
        }
        formData.delete('Id'); // IDを削除（新規作成）
        
        const response = await fetch('/Categories/Create', {
            method: 'POST',
            body: formData
        });
        
        hideLoadingModal();
        
        if (response.ok) {
            showSuccess(`新しいカテゴリ「${newCategoryName.value}」として保存されました`);
            closeSaveAsNewModal();
            
            // カテゴリ一覧に戻る
            setTimeout(() => {
                window.location.href = '/Categories';
            }, 1500);
        } else {
            const errorText = await response.text();
            showError('新しいカテゴリの保存に失敗しました: ' + errorText);
        }
    } catch (error) {
        hideLoadingModal();
        console.error('新しいカテゴリ保存エラー:', error);
        showError('新しいカテゴリ保存中にエラーが発生しました');
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
        updateChangeHistory([]);
        hideValidationErrors();
    }
}

/**
 * ページ離脱警告の設定
 */
function setupUnloadWarning() {
    window.addEventListener('beforeunload', function(event) {
        const currentData = collectFormData();
        const changes = compareFormData(originalFormData, currentData);
        
        if (changes.length > 0) {
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

// HTMLのonchange属性から呼び出されるグローバル関数
window.updateCategoryLevel = updateCategoryLevel;
window.previewCategory = previewCategory;
window.submitFromPreview = submitFromPreview;
window.closePreviewModal = closePreviewModal;
window.showSaveAsNewModal = showSaveAsNewModal;
window.closeSaveAsNewModal = closeSaveAsNewModal;
window.saveAsNewCategory = saveAsNewCategory;
window.resetForm = resetForm;