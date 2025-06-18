/**
 * 商品登録画面用JavaScript
 * フォームバリデーション、画像アップロード、プレビュー、下書き保存機能を提供
 */

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeProductCreateForm();
});

/**
 * 商品登録フォームの初期化
 * 各種機能の初期化を行う
 */
function initializeProductCreateForm() {
    // フォームバリデーションの設定
    setupFormValidation();
    
    // 画像アップロードの設定
    setupImageUpload();
    
    // オートセーブの設定
    setupAutoSave();
    
    // フォーム変更検知の設定
    setupFormChangeDetection();
    
    // フォーム送信イベントの設定
    setupFormSubmitHandler();
}

/**
 * フォーム送信ハンドラーの設定
 */
function setupFormSubmitHandler() {
    const form = document.getElementById('productCreateForm');
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
        showLoadingModal('商品を登録しています...');
        
        const formData = new FormData(document.getElementById('productCreateForm'));
        
        const response = await fetch('/Products/Create', {
            method: 'POST',
            body: formData
        });
        
        hideLoadingModal();
        
        if (response.ok) {
            const saveAndContinue = document.getElementById('saveAndContinue');
            
            showSuccess('商品が正常に登録されました');
            
            if (saveAndContinue && saveAndContinue.checked) {
                // フォームをリセットして続行
                resetForm();
            } else {
                // 商品一覧に戻る
                setTimeout(() => {
                    window.location.href = '/Products';
                }, 1500);
            }
        } else {
            const errorText = await response.text();
            showError('商品の登録に失敗しました: ' + errorText);
        }
    } catch (error) {
        hideLoadingModal();
        console.error('商品登録エラー:', error);
        showError('商品登録中にエラーが発生しました');
    }
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
 * プレビューから登録
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
 * 下書き保存モーダル表示
 */
function showSaveAsDraftModal() {
    const nameInput = document.querySelector('input[name="Name"]');
    const productName = nameInput ? nameInput.value : '';
    const draftName = productName ? `${productName}_下書き_${new Date().toLocaleDateString()}` : `商品下書き_${new Date().toLocaleDateString()}`;
    
    const draftNameInput = document.getElementById('draftName');
    const modalElement = document.getElementById('saveAsDraftModal');
    
    if (draftNameInput && modalElement) {
        draftNameInput.value = draftName;
        modalElement.style.display = 'block';
    }
}

/**
 * 下書き保存モーダルを閉じる
 */
function closeSaveAsDraftModal() {
    const modalElement = document.getElementById('saveAsDraftModal');
    if (modalElement) {
        modalElement.style.display = 'none';
    }
}

/**
 * 下書き保存実行
 */
function saveAsDraft() {
    const draftNameInput = document.getElementById('draftName');
    if (!draftNameInput) return;
    
    const draftName = draftNameInput.value;
    const formData = collectFormData();
    
    // ローカルストレージに保存
    const draftData = {
        name: draftName,
        data: formData,
        savedAt: new Date().toISOString()
    };
    
    localStorage.setItem(`product_draft_${Date.now()}`, JSON.stringify(draftData));
    
    showSuccess(`下書き「${draftName}」を保存しました`);
    closeSaveAsDraftModal();
}

/**
 * フォームリセット
 */
function resetForm() {
    if (confirm('入力内容をすべてリセットしますか？')) {
        const form = document.getElementById('productCreateForm');
        if (form) {
            form.reset();
            clearImagePreviews();
            hideValidationErrors();
        }
    }
}

/**
 * フォームデータ収集
 * @returns {Object} フォームデータオブジェクト
 */
function collectFormData() {
    const form = document.getElementById('productCreateForm');
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
 * オートセーブの設定（プレースホルダー）
 */
function setupAutoSave() {
    // 実装予定：オートセーブ機能の設定
    console.log('Auto save setup');
}

/**
 * フォーム変更検知の設定（プレースホルダー）
 */
function setupFormChangeDetection() {
    // 実装予定：フォーム変更検知の設定
    console.log('Form change detection setup');
}

/**
 * 画像プレビューのクリア（プレースホルダー）
 */
function clearImagePreviews() {
    // 実装予定：画像プレビューのクリア処理
    console.log('Clear image previews');
}

// HTMLのonclick属性から呼び出されるグローバル関数
window.previewProduct = previewProduct;
window.submitFromPreview = submitFromPreview;
window.closePreviewModal = closePreviewModal;
window.showSaveAsDraftModal = showSaveAsDraftModal;
window.closeSaveAsDraftModal = closeSaveAsDraftModal;
window.saveAsDraft = saveAsDraft;
window.resetForm = resetForm;