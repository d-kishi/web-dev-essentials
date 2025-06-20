/**
 * 商品編集画面用JavaScript
 * フォームバリデーション、変更検知、変更履歴、別商品保存機能を提供
 */

// 初期値を保存（変更検知用）
let originalFormData = {};

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeProductEditForm();
});

/**
 * 商品編集フォームの初期化
 * 各種機能の初期化を行う
 */
function initializeProductEditForm() {
    // 初期値を保存
    originalFormData = collectFormData();
    
    // フォームバリデーションの設定
    setupFormValidation();
    
    // 画像アップロードの設定
    setupImageUpload();
    
    // 変更検知の設定
    setupChangeDetection();
    
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
    const form = document.getElementById('productEditForm');
    if (form) {
        form.addEventListener('submit', function(event) {
            const validateBeforeSave = document.getElementById('validateBeforeSave');
            
            if (validateBeforeSave && validateBeforeSave.checked) {
                event.preventDefault();
                
                if (validateProductForm()) {
                    submitProductForm();
                }
            }
        });
    }
}

/**
 * 変更検知の設定
 */
function setupChangeDetection() {
    const form = document.getElementById('productEditForm');
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
}

/**
 * フォームデータ比較
 * @param {Object} original - 元のデータ
 * @param {Object} current - 現在のデータ
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
        'Name': '商品名',
        'Description': '商品説明',
        'Price': '価格',
        'CategoryId': 'カテゴリ',
        'JanCode': 'JANコード'
    };
    
    return fieldNames[fieldName] || fieldName;
}

/**
 * 商品フォームのバリデーション
 * @returns {boolean} バリデーション結果
 */
function validateProductForm() {
    let isValid = true;
    const errors = [];
    
    // 商品名のバリデーション
    const nameInput = document.querySelector('input[name="Name"]');
    if (nameInput && !nameInput.value.trim()) {
        errors.push('商品名は必須です');
        highlightFieldError(nameInput);
        isValid = false;
    }
    
    // 価格のバリデーション
    const priceInput = document.querySelector('input[name="Price"]');
    if (priceInput) {
        const price = parseInt(priceInput.value);
        if (isNaN(price) || price < 0) {
            errors.push('価格は0以上の数値で入力してください');
            highlightFieldError(priceInput);
            isValid = false;
        }
    }
    
    // カテゴリのバリデーション
    const categorySelect = document.querySelector('select[name="CategoryId"]');
    if (categorySelect && !categorySelect.value) {
        errors.push('カテゴリは必須です');
        highlightFieldError(categorySelect);
        isValid = false;
    }
    
    // JANコードのバリデーション（入力されている場合）
    const janCodeInput = document.querySelector('input[name="JanCode"]');
    if (janCodeInput && janCodeInput.value && !/^[0-9]{8,13}$/.test(janCodeInput.value)) {
        errors.push('JANコードは8〜13桁の数字で入力してください');
        highlightFieldError(janCodeInput);
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
 * 商品フォーム送信
 */
async function submitProductForm() {
    try {
        showLoadingModal('商品を更新しています...');
        
        const formData = new FormData(document.getElementById('productEditForm'));
        const productId = formData.get('Id');
        
        const response = await fetch(`/Products/Edit/${productId}`, {
            method: 'POST',
            body: formData
        });
        
        hideLoadingModal();
        
        if (response.ok) {
            const keepEditing = document.getElementById('keepEditingAfterSave');
            
            showSuccess('商品が正常に更新されました');
            
            if (keepEditing && keepEditing.checked) {
                // 初期値を更新
                originalFormData = collectFormData();
                updateChangeHistory([]);
            } else {
                // 商品詳細に戻る
                setTimeout(() => {
                    window.location.href = `/Products/Details/${productId}`;
                }, 1500);
            }
        } else {
            const errorText = await response.text();
            showError('商品の更新に失敗しました: ' + errorText);
        }
    } catch (error) {
        hideLoadingModal();
        console.error('商品更新エラー:', error);
        showError('商品更新中にエラーが発生しました');
    }
}

/**
 * 別商品として保存モーダル表示
 */
function showSaveAsNewModal() {
    const modalElement = document.getElementById('saveAsNewModal');
    if (modalElement) {
        modalElement.style.display = 'block';
    }
}

/**
 * 別商品として保存モーダルを閉じる
 */
function closeSaveAsNewModal() {
    const modalElement = document.getElementById('saveAsNewModal');
    if (modalElement) {
        modalElement.style.display = 'none';
    }
}

/**
 * 別商品として保存実行
 */
async function saveAsNewProduct() {
    try {
        const newProductNameInput = document.getElementById('newProductName');
        if (!newProductNameInput) return;
        
        const newProductName = newProductNameInput.value;
        
        if (!newProductName.trim()) {
            showError('新しい商品名を入力してください');
            return;
        }
        
        showLoadingModal('新しい商品として保存しています...');
        
        const formData = new FormData(document.getElementById('productEditForm'));
        formData.set('Name', newProductName); // 商品名を更新
        formData.delete('Id'); // IDを削除（新規作成）
        
        const response = await fetch('/Products/Create', {
            method: 'POST',
            body: formData
        });
        
        hideLoadingModal();
        
        if (response.ok) {
            showSuccess(`新しい商品「${newProductName}」として保存されました`);
            closeSaveAsNewModal();
            
            // 商品一覧に戻る
            setTimeout(() => {
                window.location.href = '/Products';
            }, 1500);
        } else {
            const errorText = await response.text();
            showError('新しい商品の保存に失敗しました: ' + errorText);
        }
    } catch (error) {
        hideLoadingModal();
        console.error('新しい商品保存エラー:', error);
        showError('新しい商品保存中にエラーが発生しました');
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
    const form = document.getElementById('productEditForm');
    if (!form) return {};
    
    const formData = new FormData(form);
    const data = {};
    
    for (let [key, value] of formData.entries()) {
        if (key !== 'ImageFiles' && key !== 'NewImageFiles') { // ファイルは除外
            data[key] = value;
        }
    }
    
    return data;
}

/**
 * プレビュー表示
 */
function previewProduct() {
    const formData = collectFormData();
    const previewContent = generatePreviewContent(formData);
    
    const previewElement = document.getElementById('productPreviewContent');
    const modalElement = document.getElementById('productPreviewModal');
    
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
    
    if (validateProductForm()) {
        submitProductForm();
    }
}

/**
 * プレビューモーダルを閉じる
 */
function closePreviewModal() {
    const modalElement = document.getElementById('productPreviewModal');
    if (modalElement) {
        modalElement.style.display = 'none';
    }
}

/**
 * プレビューコンテンツ生成
 * @param {Object} data - フォームデータ
 * @returns {string} プレビューHTML
 */
function generatePreviewContent(data) {
    return `
        <div class="product-preview">
            <div class="preview-header">
                <h2>${data.Name || '商品名未入力'}</h2>
                <div class="preview-price">¥${parseInt(data.Price || 0).toLocaleString()}</div>
            </div>
            <div class="preview-body">
                <div class="preview-section">
                    <h3>商品説明</h3>
                    <p>${data.Description || '説明なし'}</p>
                </div>
                <div class="preview-section">
                    <h3>商品情報</h3>
                    <dl class="preview-details">
                        <dt>カテゴリ</dt>
                        <dd>${getCategoryName(data.CategoryId) || '未選択'}</dd>
                        <dt>JANコード</dt>
                        <dd>${data.JanCode || 'なし'}</dd>
                    </dl>
                </div>
            </div>
        </div>
    `;
}

/**
 * カテゴリ名取得
 * @param {string} categoryId - カテゴリID
 * @returns {string|null} カテゴリ名
 */
function getCategoryName(categoryId) {
    const categorySelect = document.querySelector('select[name="CategoryId"]');
    if (!categorySelect) return null;
    
    const option = categorySelect.querySelector(`option[value="${categoryId}"]`);
    return option ? option.textContent : null;
}

/**
 * フォームバリデーションの設定（プレースホルダー）
 */
function setupFormValidation() {
    // 実装予定：詳細なバリデーション設定
    console.log('Form validation setup');
}

/**
 * 画像アップロードの設定（プレースホルダー）
 */
function setupImageUpload() {
    // 実装予定：画像アップロード機能の設定
    console.log('Image upload setup');
}

/**
 * ボタンイベントハンドラーの設定
 */
function setupButtonEventHandlers() {
    // 別商品として保存ボタン（IDベース）
    const saveAsNewButton = document.getElementById('saveAsNewButton');
    if (saveAsNewButton) {
        saveAsNewButton.addEventListener('click', showSaveAsNewModal);
    }
    
    // プレビューボタン（IDベース）
    const previewButton = document.getElementById('previewButton');
    if (previewButton) {
        previewButton.addEventListener('click', previewProduct);
    }
    
    // リセットボタン（IDベース）
    const resetButton = document.getElementById('resetButton');
    if (resetButton) {
        resetButton.addEventListener('click', resetForm);
    }
    
    // 別商品として保存モーダルの閉じるボタン（クラスベース）
    const closeSaveAsNewButtons = document.querySelectorAll('.close-save-as-new-modal');
    closeSaveAsNewButtons.forEach(button => {
        button.addEventListener('click', closeSaveAsNewModal);
    });
    
    // 別商品として保存実行ボタン（IDベース）
    const saveAsNewSubmitButton = document.getElementById('saveAsNewSubmitButton');
    if (saveAsNewSubmitButton) {
        saveAsNewSubmitButton.addEventListener('click', saveAsNewProduct);
    }
    
    // プレビューモーダルの閉じるボタン（クラスベース）
    const closePreviewButtons = document.querySelectorAll('.close-preview-modal');
    closePreviewButtons.forEach(button => {
        button.addEventListener('click', closePreviewModal);
    });
    
    // プレビューからの送信ボタン（IDベース）
    const submitFromPreviewButton = document.getElementById('submitFromPreviewButton');
    if (submitFromPreviewButton) {
        submitFromPreviewButton.addEventListener('click', submitFromPreview);
    }
}