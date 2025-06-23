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

    // 画像ファイル入力の初期化は image-upload-with-preview.js で処理
    // 重複を避けるためここでは処理しない

    // 画像関連のイベントハンドラーは image-upload-with-preview.js で処理
    // 重複を避けるため、この初期化は無効化
    // setupImageEventHandlers();

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
    
    // プレビュー削除ボタンのイベントリスナー（委譲パターン）
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('preview-remove-btn')) {
            removeImagePreview(e.target);
        }
    });
}

/**
 * 価格入力のフォーマット処理
 * @param {HTMLInputElement} input - 価格入力フィールド
 */
function formatPrice(input) {
    let value = input.value.replace(/[^\d]/g, '');
    
    if (value) {
        // 数値に変換（カンマなしで設定）
        const numValue = parseInt(value);
        if (!isNaN(numValue)) {
            input.value = numValue.toString();
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
 * 画像アップロード処理は image-upload-with-preview.js で処理
 * 重複を避けるため、この関数は使用しない
 */
// function handleImageUpload(input) - 削除済み

/**
 * 画像プレビュー作成（遅延読み込み対応）
 * @param {File} file - 画像ファイル
 * @param {number} index - インデックス
 * @param {HTMLElement} container - プレビューコンテナ
 */
// 重複処理を無効化 - image-upload-with-preview.js で処理
function createImagePreview_DISABLED(file, index, container) {
    // プレビューアイテムをまず作成（遅延読み込み用）
    const previewItem = document.createElement('div');
    previewItem.className = 'image-preview-item';
    previewItem.innerHTML = `
        <div class="preview-image-container">
            <div class="preview-placeholder" data-file-index="${index}">
                <div class="placeholder-icon">📷</div>
                <div class="placeholder-text">読み込み中...</div>
            </div>
            <button type="button" class="preview-remove-btn">
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
    
    // 遅延読み込みでファイルを読み込む
    requestIdleCallback(() => {
        // loadImagePreview(file, previewItem); // 無効化済み
    }, { timeout: 2000 });
}

/**
 * 画像プレビューの遅延読み込み
 * @param {File} file - 画像ファイル
 * @param {HTMLElement} previewItem - プレビューアイテム要素
 */
// 重複処理を無効化 - image-upload-with-preview.js で処理
function loadImagePreview_DISABLED(file, previewItem) {
    const reader = new FileReader();
    
    reader.onload = function(e) {
        const placeholder = previewItem.querySelector('.preview-placeholder');
        if (placeholder) {
            const img = document.createElement('img');
            img.src = e.target.result;
            img.alt = 'プレビュー';
            img.className = 'preview-image';
            
            // 画像読み込み完了後にプレースホルダーと置き換え
            img.onload = function() {
                placeholder.replaceWith(img);
            };
            
            // 画像読み込みエラー時の処理
            img.onerror = function() {
                placeholder.innerHTML = `
                    <div class="placeholder-icon">⚠</div>
                    <div class="placeholder-text">読み込みエラー</div>
                `;
                placeholder.classList.add('error');
            };
        }
    };
    
    reader.onerror = function() {
        const placeholder = previewItem.querySelector('.preview-placeholder');
        if (placeholder) {
            placeholder.innerHTML = `
                <div class="placeholder-icon">⚠</div>
                <div class="placeholder-text">ファイル読み込みエラー</div>
            `;
            placeholder.classList.add('error');
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

/**
 * 画像関連のイベントハンドラーを設定
 * 単一責任の原則に基づき、インラインイベントハンドラーを分離実装
 */
function setupImageEventHandlers() {
    // ファイル選択ボタン
    const selectFileBtn = document.querySelector('[data-action="select-file"]');
    if (selectFileBtn) {
        selectFileBtn.addEventListener('click', function() {
            const fileInput = document.getElementById('imageFiles');
            if (fileInput) {
                fileInput.click();
            }
        });
    }

    // すべてクリアボタン
    const clearAllBtn = document.querySelector('[data-action="clear-all"]');
    if (clearAllBtn) {
        clearAllBtn.addEventListener('click', clearAllImages);
    }

    // モーダル閉じるボタン（複数あるため全て取得）
    const closeModalBtns = document.querySelectorAll('[data-action="close-modal"]');
    closeModalBtns.forEach(btn => {
        btn.addEventListener('click', closeImageEditModal);
    });

    // 設定保存ボタン
    const saveSettingsBtn = document.querySelector('[data-action="save-settings"]');
    if (saveSettingsBtn) {
        saveSettingsBtn.addEventListener('click', saveImageSettings);
    }
}

/**
 * すべての画像をクリアする
 * 既存のグローバル関数が存在する場合はそれを使用、なければ新規実装
 */
function clearAllImages() {
    // 画像プレビューコンテナをクリア
    const previewContainer = document.getElementById('imagePreviewContainer');
    if (previewContainer) {
        previewContainer.innerHTML = '';
    }

    // ファイル入力をクリア
    const fileInput = document.getElementById('imageFiles');
    if (fileInput) {
        fileInput.value = '';
    }

    console.log('すべての画像がクリアされました');
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
 * 画像設定を保存する
 */
function saveImageSettings() {
    // 画像設定の保存処理
    const altText = document.getElementById('imageAltText');
    if (altText) {
        console.log('画像設定を保存:', altText.value);
        // 実際の保存処理をここに実装
    }
    
    // モーダルを閉じる
    closeImageEditModal();
}

// グローバル関数として公開
window.formatPrice = formatPrice;
window.validateJanCode = validateJanCode;
window.updateCharacterCount = updateCharacterCount;
window.removeImagePreview = removeImagePreview;
window.deleteExistingImage = deleteExistingImage;
window.setupImageEventHandlers = setupImageEventHandlers;
window.clearAllImages = clearAllImages;
window.closeImageEditModal = closeImageEditModal;
window.saveImageSettings = saveImageSettings;