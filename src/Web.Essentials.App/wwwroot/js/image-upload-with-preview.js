/**
 * 画像アップロードとプレビュー機能
 * 重複表示問題を解決したバージョン
 */

// 設定
const MAX_FILES = 5;
const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/gif'];

// 選択されたファイルを管理
let selectedFiles = [];
let selectedFileMetadata = []; // 代替テキストとメイン画像フラグを管理
let currentEditingIndex = -1;

/**
 * 初期化
 */
function initializeImageUploadWithPreview() {
    const fileInput = document.getElementById('imageFiles');
    const uploadZone = document.getElementById('imageUploadZone');
    
    if (!fileInput) return;

    // ファイル選択イベント
    fileInput.addEventListener('change', handleFileSelection);
    
    // ドラッグ&ドロップイベント
    if (uploadZone) {
        setupDragAndDrop(uploadZone);
    }
}

/**
 * ドラッグ&ドロップの設定
 */
function setupDragAndDrop(uploadZone) {
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
        
        const files = e.dataTransfer.files;
        processFiles(Array.from(files));
    });
    
    // クリックでファイル選択
    uploadZone.addEventListener('click', function(e) {
        if (e.target.tagName !== 'BUTTON') {
            document.getElementById('imageFiles').click();
        }
    });
}

/**
 * ファイル選択処理
 */
function handleFileSelection(event) {
    const files = Array.from(event.target.files);
    processFiles(files);
}

/**
 * ファイル処理
 */
function processFiles(files) {
    // 現在の選択をクリア
    selectedFiles = [];
    selectedFileMetadata = [];
    
    // ファイル数チェック
    if (files.length > MAX_FILES) {
        showError(`画像は最大${MAX_FILES}枚まで選択可能です`);
        clearFileInput();
        return;
    }
    
    // ファイルバリデーションと処理
    const validFiles = [];
    const errors = [];
    
    files.forEach((file) => {
        const validation = validateFile(file);
        if (validation.isValid) {
            validFiles.push(file);
        } else {
            errors.push(`${file.name}: ${validation.error}`);
        }
    });
    
    // エラーがある場合は表示
    if (errors.length > 0) {
        showError('以下のファイルに問題があります：\n' + errors.join('\n'));
        if (validFiles.length === 0) {
            clearFileInput();
            return;
        }
    }
    
    // 有効なファイルを処理
    selectedFiles = validFiles;
    
    // メタデータを初期化
    selectedFileMetadata = selectedFiles.map((file, index) => ({
        altText: '',
        isMain: index === 0 // 最初の画像をメインに設定
    }));
    
    if (selectedFiles.length > 0) {
        displayImagePreviews();
        showSuccess(`${selectedFiles.length}枚の画像が選択されました`);
    } else {
        hideImagePreviews();
    }
}

/**
 * ファイルバリデーション
 */
function validateFile(file) {
    // ファイルタイプチェック
    if (!ALLOWED_TYPES.includes(file.type)) {
        return {
            isValid: false,
            error: '対応していないファイル形式です（JPEG、PNG、GIFのみ対応）'
        };
    }
    
    // ファイルサイズチェック
    if (file.size > MAX_FILE_SIZE) {
        return {
            isValid: false,
            error: `ファイルサイズが大きすぎます（最大${formatFileSize(MAX_FILE_SIZE)}）`
        };
    }
    
    return { isValid: true };
}

/**
 * 画像プレビュー表示
 */
function displayImagePreviews() {
    const previewArea = document.getElementById('imagePreviewArea');
    const previewContainer = document.getElementById('imagePreviewContainer');
    
    if (!previewArea || !previewContainer) return;
    
    previewArea.style.display = 'block';
    previewContainer.innerHTML = '';
    
    selectedFiles.forEach((file, index) => {
        createImagePreview(file, index, previewContainer);
    });
}

/**
 * 個別画像プレビュー作成
 */
function createImagePreview(file, index, container) {
    const reader = new FileReader();
    
    reader.onload = function(e) {
        const metadata = selectedFileMetadata[index];
        const previewItem = document.createElement('div');
        previewItem.className = 'image-preview-item';
        previewItem.dataset.index = index;
        
        previewItem.innerHTML = `
            <div class="preview-image-container">
                <img src="${e.target.result}" alt="${metadata.altText || file.name}" class="preview-image" />
                <div class="preview-overlay">
                    <button type="button" class="overlay-btn edit-btn" onclick="openImageEditModal(${index})" title="編集">
                        ✏️
                    </button>
                    <button type="button" class="overlay-btn delete-btn" onclick="removeImage(${index})" title="削除">
                        🗑️
                    </button>
                    <button type="button" class="overlay-btn view-btn" onclick="viewImage('${e.target.result}', '${metadata.altText || file.name}')" title="拡大表示">
                        🔍
                    </button>
                </div>
                ${metadata.isMain ? '<div class="main-badge">メイン</div>' : ''}
            </div>
            <div class="preview-info">
                <div class="preview-name">${file.name}</div>
                <div class="preview-details">
                    <span class="preview-size">${formatFileSize(file.size)}</span>
                    <span class="preview-order">順序: ${index + 1}</span>
                </div>
                ${metadata.altText ? `<div class="preview-alt-text" title="${metadata.altText}">説明: ${metadata.altText}</div>` : ''}
            </div>
        `;
        
        container.appendChild(previewItem);
    };
    
    reader.readAsDataURL(file);
}

/**
 * 画像削除
 */
function removeImage(index) {
    if (confirm('この画像を削除しますか？')) {
        selectedFiles.splice(index, 1);
        selectedFileMetadata.splice(index, 1);
        
        // メイン画像が削除された場合、最初の画像をメインに設定
        if (selectedFileMetadata.length > 0 && !selectedFileMetadata.some(meta => meta.isMain)) {
            selectedFileMetadata[0].isMain = true;
        }
        
        if (selectedFiles.length > 0) {
            displayImagePreviews();
            updateFileInput();
            showInfo('画像を削除しました');
        } else {
            clearAllImages();
        }
    }
}

/**
 * 画像拡大表示
 */
function viewImage(imageSrc, imageName) {
    // シンプルなモーダル表示
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
            <img src="${imageSrc}" alt="${imageName}" style="max-width: 100%; max-height: 100%; border-radius: 8px;">
            <div style="position: absolute; top: -40px; right: 0; color: white; font-size: 24px; cursor: pointer;">&times;</div>
        </div>
    `;
    
    modal.addEventListener('click', function() {
        document.body.removeChild(modal);
    });
    
    document.body.appendChild(modal);
}

/**
 * 画像編集モーダルを開く
 */
function openImageEditModal(index) {
    if (index < 0 || index >= selectedFiles.length) return;
    
    currentEditingIndex = index;
    const file = selectedFiles[index];
    const metadata = selectedFileMetadata[index];
    
    // 画像プレビューを設定
    const reader = new FileReader();
    reader.onload = function(e) {
        const editPreviewImage = document.getElementById('editPreviewImage');
        if (editPreviewImage) {
            editPreviewImage.src = e.target.result;
        }
    };
    reader.readAsDataURL(file);
    
    // フォームに現在の設定を表示
    const imageAltText = document.getElementById('imageAltText');
    const imageIsMain = document.getElementById('imageIsMain');
    
    if (imageAltText) imageAltText.value = metadata.altText || '';
    if (imageIsMain) imageIsMain.checked = metadata.isMain || false;
    
    // モーダルを表示
    const modal = document.getElementById('imageEditModal');
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
 * 画像設定を保存
 */
function saveImageSettings() {
    if (currentEditingIndex < 0 || currentEditingIndex >= selectedFileMetadata.length) return;
    
    const imageAltText = document.getElementById('imageAltText');
    const imageIsMain = document.getElementById('imageIsMain');
    
    const altText = imageAltText ? imageAltText.value.trim() : '';
    const isMain = imageIsMain ? imageIsMain.checked : false;
    
    // メタデータを更新
    selectedFileMetadata[currentEditingIndex].altText = altText;
    
    // メイン画像設定
    if (isMain) {
        // 他の画像のメイン設定を解除
        selectedFileMetadata.forEach(meta => meta.isMain = false);
        selectedFileMetadata[currentEditingIndex].isMain = true;
    } else if (selectedFileMetadata[currentEditingIndex].isMain) {
        // 現在のメイン画像のチェックを外した場合、最初の画像をメインに設定
        selectedFileMetadata[currentEditingIndex].isMain = false;
        if (!selectedFileMetadata.some(meta => meta.isMain)) {
            selectedFileMetadata[0].isMain = true;
        }
    }
    
    // プレビューを更新
    displayImagePreviews();
    closeImageEditModal();
    showSuccess('画像設定を保存しました');
}

/**
 * すべての画像をクリア
 */
function clearAllImages() {
    selectedFiles = [];
    selectedFileMetadata = [];
    hideImagePreviews();
    clearFileInput();
    showInfo('すべての画像を削除しました');
}

/**
 * プレビューエリアを非表示
 */
function hideImagePreviews() {
    const previewArea = document.getElementById('imagePreviewArea');
    if (previewArea) {
        previewArea.style.display = 'none';
    }
}

/**
 * ファイル入力を更新（選択されたファイルに同期）
 */
function updateFileInput() {
    const fileInput = document.getElementById('imageFiles');
    if (!fileInput) return;
    
    // FileListは直接編集できないため、新しいDataTransferを使用
    const dt = new DataTransfer();
    selectedFiles.forEach(file => dt.items.add(file));
    fileInput.files = dt.files;
}

/**
 * ファイル入力をクリア
 */
function clearFileInput() {
    const fileInput = document.getElementById('imageFiles');
    if (fileInput) {
        fileInput.value = '';
    }
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
 * 成功メッセージ表示
 */
function showSuccess(message) {
    console.log('Success:', message);
    if (typeof window.showToast === 'function') {
        window.showToast(message, 'success');
    }
}

/**
 * エラーメッセージ表示
 */
function showError(message) {
    console.error('Error:', message);
    alert(message);
}

/**
 * 情報メッセージ表示
 */
function showInfo(message) {
    console.info('Info:', message);
    if (typeof window.showToast === 'function') {
        window.showToast(message, 'info');
    }
}

// 初期化
document.addEventListener('DOMContentLoaded', function() {
    initializeImageUploadWithPreview();
    setupFormSubmissionHandler();
});

/**
 * フォーム送信前に隠しフィールドを設定
 */
function prepareFormSubmission() {
    // 既存の隠しフィールドを削除
    const existingFields = document.querySelectorAll('input[name^="ImageAltTexts"], input[name^="ImageIsMainFlags"]');
    existingFields.forEach(field => field.remove());
    
    // 新しい隠しフィールドを作成
    const form = document.querySelector('form');
    if (!form) return;
    
    selectedFileMetadata.forEach((metadata, index) => {
        // 代替テキスト用隠しフィールド
        const altTextInput = document.createElement('input');
        altTextInput.type = 'hidden';
        altTextInput.name = `ImageAltTexts[${index}]`;
        altTextInput.value = metadata.altText || '';
        form.appendChild(altTextInput);
        
        // メイン画像フラグ用隠しフィールド
        const isMainInput = document.createElement('input');
        isMainInput.type = 'hidden';
        isMainInput.name = `ImageIsMainFlags[${index}]`;
        isMainInput.value = metadata.isMain ? 'true' : 'false';
        form.appendChild(isMainInput);
    });
}

/**
 * フォーム送信イベントの設定
 */
function setupFormSubmissionHandler() {
    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function(e) {
            prepareFormSubmission();
        });
    }
}

// グローバル関数として公開
window.clearAllImages = clearAllImages;
window.removeImage = removeImage;
window.viewImage = viewImage;
window.openImageEditModal = openImageEditModal;
window.closeImageEditModal = closeImageEditModal;
window.saveImageSettings = saveImageSettings;