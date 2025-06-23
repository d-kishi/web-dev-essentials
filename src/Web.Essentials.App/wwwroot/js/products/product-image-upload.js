/**
 * 商品画像アップロード機能
 * ドラッグ&ドロップ、プレビュー、編集、削除機能
 */

// 画像管理変数
let uploadedImages = [];
let existingImages = [];
let currentEditingIndex = -1;
let maxImages = 5;
let maxFileSize = 5 * 1024 * 1024; // 5MB

/**
 * 画像アップロード初期化
 */
function initializeImageUpload() {
    setupDropZone();
    setupFileValidation();
    loadExistingImages();
}

/**
 * ドロップゾーンの設定
 */
function setupDropZone() {
    const uploadZone = document.getElementById('imageUploadZone');
    if (!uploadZone) return;
    
    // ドラッグ&ドロップイベント
    uploadZone.addEventListener('dragover', function(e) {
        e.preventDefault();
        uploadZone.classList.add('drag-over');
    });
    
    uploadZone.addEventListener('dragleave', function(e) {
        e.preventDefault();
        uploadZone.classList.remove('drag-over');
    });
    
    uploadZone.addEventListener('drop', function(e) {
        e.preventDefault();
        uploadZone.classList.remove('drag-over');
        handleFileSelect(e.dataTransfer.files);
    });
    
    // クリックでファイル選択
    uploadZone.addEventListener('click', function() {
        triggerFileInput();
    });
}

/**
 * ファイルバリデーション設定
 */
function setupFileValidation() {
    const fileInput = document.getElementById('imageFileInput');
    if (fileInput) {
        fileInput.addEventListener('change', function() {
            handleFileSelect(this.files);
        });
    }
}

/**
 * ファイル入力トリガー
 */
function triggerFileInput() {
    const fileInput = document.getElementById('imageFileInput');
    if (fileInput) {
        fileInput.click();
    }
}

/**
 * ファイル選択処理
 */
function handleFileSelect(files) {
    const fileArray = Array.from(files);
    
    // 最大枚数チェック
    if (uploadedImages.length + fileArray.length > maxImages) {
        if (typeof showError === 'function') {
            showError(`画像は最大${maxImages}枚まで登録可能です`);
        } else {
            alert(`画像は最大${maxImages}枚まで登録可能です`);
        }
        return;
    }
    
    fileArray.forEach(file => {
        if (validateFile(file)) {
            processImageFile(file);
        }
    });
}

/**
 * ファイルバリデーション
 */
function validateFile(file) {
    // ファイルタイプチェック
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif'];
    if (!allowedTypes.includes(file.type)) {
        if (typeof showError === 'function') {
            showError(`対応していないファイル形式です: ${file.name}`);
        } else {
            alert(`対応していないファイル形式です: ${file.name}`);
        }
        return false;
    }
    
    // ファイルサイズチェック
    if (file.size > maxFileSize) {
        if (typeof showError === 'function') {
            showError(`ファイルサイズが大きすぎます: ${file.name} (最大5MB)`);
        } else {
            alert(`ファイルサイズが大きすぎます: ${file.name} (最大5MB)`);
        }
        return false;
    }
    
    return true;
}

/**
 * 画像ファイル処理
 */
function processImageFile(file) {
    const reader = new FileReader();
    
    reader.onload = function(e) {
        const imageData = {
            file: file,
            dataUrl: e.target.result,
            name: file.name,
            size: file.size,
            altText: '',
            displayOrder: uploadedImages.length + 1,
            isMain: uploadedImages.length === 0 // 最初の画像をメインに設定
        };
        
        uploadedImages.push(imageData);
        updateImagePreview();
    };
    
    reader.readAsDataURL(file);
}

/**
 * 画像プレビュー更新
 */
function updateImagePreview() {
    const previewArea = document.getElementById('imagePreviewArea');
    const previewContainer = document.getElementById('imagePreviewContainer');
    
    if (!previewArea || !previewContainer) return;
    
    if (uploadedImages.length === 0) {
        previewArea.style.display = 'none';
        return;
    }
    
    previewArea.style.display = 'block';
    
    const previewHtml = uploadedImages.map((image, index) => `
        <div class="image-preview-item ${image.isMain ? 'main-image' : ''}" data-index="${index}">
            <div class="preview-image-container">
                <img src="${image.dataUrl}" alt="${image.altText || image.name}" class="preview-image" />
                <div class="preview-overlay">
                    <button type="button" class="overlay-btn edit-btn" data-index="${index}" data-action="edit" title="編集">
                        ✏️
                    </button>
                    <button type="button" class="overlay-btn view-btn" data-src="${image.dataUrl}" data-name="${image.name}" data-action="view" title="拡大表示">
                        🔍
                    </button>
                    <button type="button" class="overlay-btn delete-btn" data-index="${index}" data-action="delete" title="削除">
                        🗑️
                    </button>
                </div>
                ${image.isMain ? '<div class="main-badge">メイン</div>' : ''}
            </div>
            <div class="preview-info">
                <div class="preview-name">${image.name}</div>
                <div class="preview-details">
                    <span class="preview-size">${formatFileSize(image.size)}</span>
                    <span class="preview-order">順序: ${image.displayOrder}</span>
                </div>
            </div>
        </div>
    `).join('');
    
    previewContainer.innerHTML = previewHtml;
    
    // ソート可能にする
    setupImageSorting();
    // ボタンイベント設定
    setupImageActions();
}

/**
 * 画像アクションボタンの設定
 */
function setupImageActions() {
    const previewContainer = document.getElementById('imagePreviewContainer');
    if (!previewContainer) return;
    
    // イベント委譲でボタンクリックを処理
    previewContainer.addEventListener('click', function(e) {
        const button = e.target.closest('[data-action]');
        if (!button) return;
        
        e.preventDefault();
        e.stopPropagation();
        
        const action = button.dataset.action;
        const index = parseInt(button.dataset.index);
        
        switch (action) {
            case 'edit':
                openImageEditModal(index);
                break;
            case 'view':
                const src = button.dataset.src;
                const name = button.dataset.name;
                openImageViewModal(src, name);
                break;
            case 'delete':
                removeImage(index);
                break;
        }
    });
}

/**
 * 画像削除
 */
function removeImage(index) {
    if (confirm('この画像を削除しますか？')) {
        uploadedImages.splice(index, 1);
        
        // 表示順序を再調整
        uploadedImages.forEach((image, idx) => {
            image.displayOrder = idx + 1;
        });
        
        // メイン画像が削除された場合、最初の画像をメインに設定
        if (uploadedImages.length > 0 && !uploadedImages.some(img => img.isMain)) {
            uploadedImages[0].isMain = true;
        }
        
        updateImagePreview();
    }
}

/**
 * すべての画像をクリア
 */
function clearAllImages() {
    if (uploadedImages.length === 0) return;
    
    if (confirm('すべての画像を削除しますか？')) {
        uploadedImages = [];
        updateImagePreview();
    }
}

/**
 * 画像編集モーダルを開く
 */
function openImageEditModal(index) {
    if (index < 0 || index >= uploadedImages.length) return;
    
    currentEditingIndex = index;
    const image = uploadedImages[index];
    
    // モーダルに現在の設定を表示
    const editPreviewImage = document.getElementById('editPreviewImage');
    const imageAltText = document.getElementById('imageAltText');
    const imageDisplayOrder = document.getElementById('imageDisplayOrder');
    const imageIsMain = document.getElementById('imageIsMain');
    const modal = document.getElementById('imageEditModal');
    
    if (editPreviewImage) editPreviewImage.src = image.dataUrl;
    if (imageAltText) imageAltText.value = image.altText || '';
    if (imageDisplayOrder) imageDisplayOrder.value = image.displayOrder;
    if (imageIsMain) imageIsMain.checked = image.isMain;
    
    if (modal) {
        modal.style.display = 'block';
        document.body.style.overflow = 'hidden';
    }
}

/**
 * 画像編集モーダルを閉じる
 */
function closeImageEditModal() {
    const modal = document.getElementById('imageEditModal');
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
    currentEditingIndex = -1;
}

/**
 * 画像設定保存
 */
function saveImageSettings() {
    if (currentEditingIndex < 0) return;
    
    const image = uploadedImages[currentEditingIndex];
    const altText = document.getElementById('imageAltText')?.value || '';
    const displayOrder = parseInt(document.getElementById('imageDisplayOrder')?.value || '1');
    const isMain = document.getElementById('imageIsMain')?.checked || false;
    
    // 設定を更新
    image.altText = altText;
    image.displayOrder = displayOrder;
    
    // メイン画像設定
    if (isMain) {
        // 他の画像のメイン設定を解除
        uploadedImages.forEach(img => img.isMain = false);
        image.isMain = true;
    }
    
    // 表示順序でソート
    uploadedImages.sort((a, b) => a.displayOrder - b.displayOrder);
    
    updateImagePreview();
    closeImageEditModal();
    
    if (typeof showSuccess === 'function') {
        showSuccess('画像設定を保存しました');
    }
}

/**
 * 画像拡大表示モーダルを開く
 */
function openImageViewModal(imageSrc, imageName) {
    const viewImage = document.getElementById('viewImage');
    const imageViewTitle = document.getElementById('imageViewTitle');
    const modal = document.getElementById('imageViewModal');
    
    if (viewImage) viewImage.src = imageSrc;
    if (imageViewTitle) imageViewTitle.textContent = imageName;
    
    if (modal) {
        modal.style.display = 'block';
        document.body.style.overflow = 'hidden';
    }
}

/**
 * 画像拡大表示モーダルを閉じる
 */
function closeImageViewModal() {
    const modal = document.getElementById('imageViewModal');
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
}

/**
 * 画像並び替え機能
 */
function setupImageSorting() {
    const previewContainer = document.getElementById('imagePreviewContainer');
    if (!previewContainer) return;
    
    let draggedElement = null;
    
    previewContainer.addEventListener('dragstart', function(e) {
        draggedElement = e.target.closest('.image-preview-item');
        if (draggedElement) {
            draggedElement.classList.add('dragging');
        }
    });
    
    previewContainer.addEventListener('dragover', function(e) {
        e.preventDefault();
    });
    
    previewContainer.addEventListener('drop', function(e) {
        e.preventDefault();
        const targetElement = e.target.closest('.image-preview-item');
        
        if (draggedElement && targetElement && draggedElement !== targetElement) {
            const draggedIndex = parseInt(draggedElement.dataset.index);
            const targetIndex = parseInt(targetElement.dataset.index);
            
            // 配列内で要素を入れ替え
            const temp = uploadedImages[draggedIndex];
            uploadedImages[draggedIndex] = uploadedImages[targetIndex];
            uploadedImages[targetIndex] = temp;
            
            // 表示順序を更新
            uploadedImages.forEach((image, idx) => {
                image.displayOrder = idx + 1;
            });
            
            updateImagePreview();
        }
        
        if (draggedElement) {
            draggedElement.classList.remove('dragging');
            draggedElement = null;
        }
    });
    
    // ドラッグ可能に設定
    const previewItems = previewContainer.querySelectorAll('.image-preview-item');
    previewItems.forEach(item => {
        item.draggable = true;
    });
}

/**
 * 並び順変更ボタン
 */
function reorderImages() {
    if (uploadedImages.length <= 1) {
        if (typeof showInfo === 'function') {
            showInfo('並び替える画像がありません');
        } else {
            alert('並び替える画像がありません');
        }
        return;
    }
    
    if (typeof showInfo === 'function') {
        showInfo('画像をドラッグ&ドロップして並び順を変更できます');
    } else {
        alert('画像をドラッグ&ドロップして並び順を変更できます');
    }
}

/**
 * 既存画像読み込み（Edit画面用）
 */
function loadExistingImages() {
    // Edit画面の場合、既存画像情報を読み込む
    // この関数は、編集画面で呼び出される想定
    // 実装は画面固有の処理で行う
}

/**
 * ファイルサイズフォーマット
 */
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

/**
 * フォーム送信時の画像データ処理
 */
function getImageFormData() {
    const formData = new FormData();
    
    uploadedImages.forEach((image, index) => {
        formData.append(`ImageFiles`, image.file);
        formData.append(`ImageAltTexts[${index}]`, image.altText || '');
        formData.append(`ImageDisplayOrders[${index}]`, image.displayOrder);
        formData.append(`ImageIsMain[${index}]`, image.isMain);
    });
    
    return formData;
}

/**
 * モーダルクローズボタンの設定
 */
function setupModalCloseButtons() {
    // 画像編集モーダルのクローズボタン
    const editModalCloseButtons = document.querySelectorAll('#imageEditModal .modal-close, #imageEditModal .btn-secondary');
    editModalCloseButtons.forEach(button => {
        button.addEventListener('click', closeImageEditModal);
    });
    
    // 画像表示モーダルのクローズボタン
    const viewModalCloseButtons = document.querySelectorAll('#imageViewModal .modal-close');
    viewModalCloseButtons.forEach(button => {
        button.addEventListener('click', closeImageViewModal);
    });
    
    // モーダル外クリックで閉じる
    const viewModal = document.getElementById('imageViewModal');
    if (viewModal) {
        viewModal.addEventListener('click', function(e) {
            if (e.target === this) {
                closeImageViewModal();
            }
        });
    }
    
    // 保存ボタン
    const saveButton = document.querySelector('#imageEditModal .btn-primary');
    if (saveButton) {
        saveButton.addEventListener('click', saveImageSettings);
    }
}

/**
 * その他のボタンイベント設定
 */
function setupOtherButtons() {
    // すべてクリアボタン
    const clearButton = document.querySelector('[data-action="clear-all"]');
    if (clearButton) {
        clearButton.addEventListener('click', clearAllImages);
    }
    
    // 並び順変更ボタン
    const reorderButton = document.querySelector('[data-action="reorder"]');
    if (reorderButton) {
        reorderButton.addEventListener('click', reorderImages);
    }
    
    // ファイル選択ボタン
    const selectFileButton = document.querySelector('[data-action="select-file"]');
    if (selectFileButton) {
        selectFileButton.addEventListener('click', triggerFileInput);
    }
}

/**
 * 初期化（ページ読み込み時に呼び出し）
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeImageUpload();
    setupModalCloseButtons();
    setupOtherButtons();
});

// グローバル関数として公開（後方互換性のため）
window.triggerFileInput = triggerFileInput;
window.handleFileSelect = handleFileSelect;
window.clearAllImages = clearAllImages;
window.reorderImages = reorderImages;
window.openImageEditModal = openImageEditModal;
window.closeImageEditModal = closeImageEditModal;
window.saveImageSettings = saveImageSettings;
window.openImageViewModal = openImageViewModal;
window.closeImageViewModal = closeImageViewModal;
window.removeImage = removeImage;
window.getImageFormData = getImageFormData;