/**
 * 商品フォーム用JavaScript
 * 商品登録・編集フォームの入力処理・バリデーション機能を提供
 */

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeProductForm();
});

/**
 * 商品フォームの初期化
 */
function initializeProductForm() {
    // 価格入力フィールドの初期化
    const priceInput = document.querySelector('input[name="Price"]');
    if (priceInput) {
        priceInput.addEventListener('input', function() {
            formatPrice(this);
        });
    }

    // JANコード入力フィールドの初期化
    const janCodeInput = document.querySelector('input[name="JanCode"]');
    if (janCodeInput) {
        janCodeInput.addEventListener('input', function() {
            validateJanCode(this);
        });
    }

    // 説明文入力フィールドの初期化
    const descriptionTextarea = document.querySelector('textarea[name="Description"]');
    if (descriptionTextarea) {
        descriptionTextarea.addEventListener('input', function() {
            updateCharacterCount(this, 'descriptionCount');
        });
        // 初期文字数カウント
        updateCharacterCount(descriptionTextarea, 'descriptionCount');
    }

    // 画像ファイル入力の初期化
    const imageInput = document.querySelector('input[name="ImageFiles"]');
    if (imageInput) {
        imageInput.addEventListener('change', function() {
            handleImageUpload(this);
        });
    }

    // 既存画像削除ボタンの初期化
    document.querySelectorAll('.delete-image-btn').forEach(button => {
        button.addEventListener('click', function() {
            const imageId = this.getAttribute('data-image-id') || 
                           this.closest('.existing-image-item')?.getAttribute('data-image-id');
            if (imageId) {
                deleteExistingImage(parseInt(imageId));
            }
        });
    });
}

/**
 * 価格入力のフォーマット処理
 * @param {HTMLInputElement} input - 価格入力フィールド
 */
function formatPrice(input) {
    let value = input.value.replace(/[^\d]/g, '');
    
    if (value) {
        // 数値に変換してフォーマット
        const numValue = parseInt(value);
        if (!isNaN(numValue)) {
            input.value = numValue.toLocaleString();
        }
    }
}

/**
 * JANコードバリデーション
 * @param {HTMLInputElement} input - JANコード入力フィールド
 */
function validateJanCode(input) {
    const value = input.value.trim();
    const validationDiv = document.getElementById('janCodeValidation');
    
    if (!validationDiv) return;

    if (!value) {
        validationDiv.style.display = 'none';
        return;
    }

    let isValid = false;
    let message = '';

    // 8桁または13桁の数字のみ許可
    if (!/^\d{8}$|^\d{13}$/.test(value)) {
        message = 'JANコードは8桁または13桁の数字で入力してください';
    } else {
        // チェックディジット検証
        isValid = validateJanCheckDigit(value);
        if (!isValid) {
            message = 'JANコードのチェックディジットが正しくありません';
        } else {
            message = '✓ 有効なJANコードです';
        }
    }

    validationDiv.textContent = message;
    validationDiv.className = isValid ? 'field-validation success' : 'field-validation error';
    validationDiv.style.display = 'block';
}

/**
 * JANコードのチェックディジット検証
 * @param {string} jan - JANコード
 * @returns {boolean} 有効かどうか
 */
function validateJanCheckDigit(jan) {
    if (jan.length !== 8 && jan.length !== 13) return false;

    const digits = jan.split('').map(Number);
    const checkDigit = digits.pop();
    
    let sum = 0;
    for (let i = 0; i < digits.length; i++) {
        if (jan.length === 13) {
            sum += digits[i] * (i % 2 === 0 ? 1 : 3);
        } else {
            sum += digits[i] * (i % 2 === 0 ? 3 : 1);
        }
    }
    
    const calculatedCheckDigit = (10 - (sum % 10)) % 10;
    return calculatedCheckDigit === checkDigit;
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
 * 画像アップロード処理
 * @param {HTMLInputElement} input - ファイル入力フィールド
 */
function handleImageUpload(input) {
    const files = input.files;
    const previewArea = document.getElementById('imagePreviewArea');
    const previewList = document.getElementById('imagePreviewList');

    if (!files.length) {
        if (previewArea) previewArea.style.display = 'none';
        return;
    }

    if (previewArea) previewArea.style.display = 'block';
    if (previewList) previewList.innerHTML = '';

    Array.from(files).forEach((file, index) => {
        if (file.type.startsWith('image/')) {
            createImagePreview(file, index, previewList);
        }
    });
}

/**
 * 画像プレビュー作成
 * @param {File} file - 画像ファイル
 * @param {number} index - インデックス
 * @param {HTMLElement} container - プレビューコンテナ
 */
function createImagePreview(file, index, container) {
    const reader = new FileReader();
    
    reader.onload = function(e) {
        const previewItem = document.createElement('div');
        previewItem.className = 'image-preview-item';
        previewItem.innerHTML = `
            <div class="preview-image-container">
                <img src="${e.target.result}" alt="プレビュー" class="preview-image" />
                <button type="button" class="preview-remove-btn" onclick="removeImagePreview(this)">
                    ×
                </button>
            </div>
            <div class="preview-info">
                <div class="preview-filename">${file.name}</div>
                <div class="preview-filesize">${formatFileSize(file.size)}</div>
            </div>
        `;
        
        if (container) {
            container.appendChild(previewItem);
        }
    };
    
    reader.readAsDataURL(file);
}

/**
 * プレビュー画像削除
 * @param {HTMLButtonElement} button - 削除ボタン
 */
function removeImagePreview(button) {
    const previewItem = button.closest('.image-preview-item');
    if (previewItem) {
        previewItem.remove();
    }
    
    // プレビューが空になったら非表示
    const previewList = document.getElementById('imagePreviewList');
    const previewArea = document.getElementById('imagePreviewArea');
    if (previewList && !previewList.children.length && previewArea) {
        previewArea.style.display = 'none';
    }
}

/**
 * ファイルサイズフォーマット
 * @param {number} bytes - バイト数
 * @returns {string} フォーマットされたサイズ
 */
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

/**
 * 既存画像削除
 * @param {number} imageId - 画像ID
 */
async function deleteExistingImage(imageId) {
    if (!confirm('この画像を削除しますか？')) {
        return;
    }

    try {
        showLoadingModal('画像を削除しています...');

        const response = await fetch(`/Products/DeleteImage/${imageId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });

        hideLoadingModal();

        if (response.ok) {
            // 画像要素を削除
            const imageItem = document.querySelector(`[data-image-id="${imageId}"]`);
            if (imageItem) {
                imageItem.remove();
            }
            showSuccess('画像が削除されました');
        } else {
            const errorText = await response.text();
            showError('画像の削除に失敗しました: ' + errorText);
        }
    } catch (error) {
        hideLoadingModal();
        console.error('画像削除エラー:', error);
        showError('画像削除中にエラーが発生しました');
    }
}

// グローバル関数として公開
window.formatPrice = formatPrice;
window.validateJanCode = validateJanCode;
window.updateCharacterCount = updateCharacterCount;
window.handleImageUpload = handleImageUpload;
window.removeImagePreview = removeImagePreview;
window.deleteExistingImage = deleteExistingImage;