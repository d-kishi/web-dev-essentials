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
    
    // 画像表示・編集機能の設定
    setupImageViewerAndEditor();
    
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
 * 画像表示・編集機能の設定
 */
function setupImageViewerAndEditor() {
    // イベントリスナーの設定
    document.addEventListener('click', function(e) {
        const target = e.target.closest('[data-action]');
        if (!target) return;
        
        const action = target.dataset.action;
        
        switch (action) {
            case 'open-image-viewer':
                e.preventDefault();
                openImageViewer(target.dataset.imagePath, target.dataset.imageAlt);
                break;
                
            case 'edit-image':
                e.preventDefault();
                openImageEditModal(target);
                break;
                
            case 'close-modal':
                e.preventDefault();
                closeImageEditModal();
                break;
                
            case 'save-settings':
                e.preventDefault();
                saveImageSettings();
                break;
        }
    });
}

/**
 * 画像ビューアーを開く
 * @param {string} imagePath - 画像パス
 * @param {string} altText - 代替テキスト
 */
function openImageViewer(imagePath, altText) {
    const modal = document.createElement('div');
    modal.className = 'image-view-modal';
    modal.style.cssText = `
        position: fixed; top: 0; left: 0; width: 100%; height: 100%;
        background-color: rgba(0,0,0,0.8); z-index: 1000;
        display: flex; align-items: center; justify-content: center;
        cursor: pointer;
    `;
    
    modal.innerHTML = `
        <div style="max-width: 90%; max-height: 90%; position: relative;">
            <img src="${imagePath}" alt="${altText}" style="max-width: 100%; max-height: 100%; border-radius: 8px;">
            <div style="position: absolute; top: -40px; right: 0; color: white; font-size: 24px; cursor: pointer;">&times;</div>
        </div>
    `;
    
    modal.addEventListener('click', function() {
        document.body.removeChild(modal);
        document.body.style.overflow = 'auto';
    });
    
    document.body.appendChild(modal);
    document.body.style.overflow = 'hidden';
}

/**
 * 画像編集モーダルを開く
 * @param {HTMLElement} editButton - 編集ボタン要素
 */
function openImageEditModal(editButton) {
    const modal = document.getElementById('imageEditModal');
    const previewImg = document.getElementById('editPreviewImage');
    const altTextInput = document.getElementById('imageAltText');
    const isMainCheckbox = document.getElementById('imageIsMain');
    
    if (!modal || !previewImg || !altTextInput || !isMainCheckbox) return;
    
    // データをモーダルに設定
    previewImg.src = editButton.dataset.imagePath;
    previewImg.alt = editButton.dataset.imageAlt || '商品画像';
    altTextInput.value = editButton.dataset.imageAlt || '';
    
    // 現在編集中の画像IDを保存
    modal.dataset.currentImageId = editButton.dataset.imageId;
    
    // モーダルを表示
    modal.style.display = 'block';
}

/**
 * 画像編集モーダルを閉じる
 */
function closeImageEditModal() {
    const modal = document.getElementById('imageEditModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

/**
 * 画像設定を保存
 */
function saveImageSettings() {
    const modal = document.getElementById('imageEditModal');
    const altTextInput = document.getElementById('imageAltText');
    const isMainCheckbox = document.getElementById('imageIsMain');
    
    if (!modal || !altTextInput) return;
    
    const imageId = modal.dataset.currentImageId;
    const newAltText = altTextInput.value;
    const isMain = isMainCheckbox ? isMainCheckbox.checked : false;
    
    // 表示されている画像情報を更新
    const imageItem = document.querySelector(`[data-image-id="${imageId}"]`);
    if (imageItem) {
        // 代替テキストを更新
        const editButton = imageItem.querySelector('[data-action="edit-image"]');
        if (editButton) {
            editButton.dataset.imageAlt = newAltText;
        }
        
        const img = imageItem.querySelector('img');
        if (img) {
            img.alt = newAltText;
            img.dataset.imageAlt = newAltText;
        }
        
        // 情報表示を更新
        let infoElement = imageItem.querySelector('.alt-text');
        if (newAltText) {
            if (!infoElement) {
                const imageInfo = imageItem.querySelector('.image-info');
                if (imageInfo) {
                    infoElement = document.createElement('small');
                    infoElement.className = 'alt-text';
                    imageInfo.appendChild(infoElement);
                }
            }
            if (infoElement) {
                infoElement.textContent = `代替テキスト: ${newAltText}`;
            }
        } else if (infoElement) {
            infoElement.remove();
        }
    }
    
    showSuccess('画像設定を更新しました');
    closeImageEditModal();
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